using System.Net;
using Codium.Template.IntegrationTests.Infrastructure;

namespace Codium.Template.IntegrationTests;

/// <summary>Authentication parity with the pre-Identity template (see tests/parity/baseline).</summary>
[Collection(ApiCollection.Name)]
public class AuthTests(ApiFixture fixture)
{
    private ApiClient NewClient() => new(fixture.CreateClient());

    [Fact]
    public async Task Login_ReturnsTokenPair()
    {
        var api = NewClient();
        var response = await api.LoginAsync(ApiClient.AdminEmail, ApiClient.AdminPassword);

        Assert.Equal(HttpStatusCode.OK, response.Status);
        Assert.True(response.Body.GetProperty("success").GetBoolean());
        Assert.False(string.IsNullOrEmpty(response.Data.GetProperty("accessToken").GetString()));
        Assert.False(string.IsNullOrEmpty(response.Data.GetProperty("refreshToken").GetString()));
        Assert.True(response.Data.GetProperty("expiryTime").GetInt64() > 0);
    }

    [Fact]
    public async Task Login_IsCaseInsensitiveOnEmail()
    {
        var response = await NewClient().LoginAsync("ADMIN@Codium.com", ApiClient.AdminPassword);
        Assert.Equal(HttpStatusCode.OK, response.Status);
    }

    [Fact]
    public async Task Login_WrongPasswordOrUnknownEmail_Is401()
    {
        var api = NewClient();

        var wrongPassword = await api.LoginAsync(ApiClient.AdminEmail, "Wrong1234*");
        var unknown = await api.LoginAsync($"nobody-{ApiClient.Suffix()}@example.com", "Wrong1234*");

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.Status);
        Assert.Equal("APP:UNAUTHORIZED", wrongPassword.ErrorCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.Status);
        Assert.Equal("APP:UNAUTHORIZED", unknown.ErrorCode);
    }

    [Fact]
    public async Task Login_InvalidPayload_Is400()
    {
        var response = await NewClient().LoginAsync("bad", "x");
        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("APP:VALIDATION", response.ErrorCode);
    }

    [Fact]
    public async Task RefreshToken_IssuesNewPair_AndRejectsReuse()
    {
        var api = NewClient();
        var login = await api.LoginAsync(ApiClient.AdminEmail, ApiClient.AdminPassword);
        var refreshToken = login.Data.GetProperty("refreshToken").GetString();

        var refreshed = await api.PostAsync("/api/v1/auth/refresh-token", new { refreshToken });
        Assert.Equal(HttpStatusCode.OK, refreshed.Status);
        Assert.NotEqual(refreshToken, refreshed.Data.GetProperty("refreshToken").GetString());

        var reused = await api.PostAsync("/api/v1/auth/refresh-token", new { refreshToken });
        Assert.Equal(HttpStatusCode.Unauthorized, reused.Status);
    }

    [Fact]
    public async Task Logout_InvalidatesTheAccessToken()
    {
        var api = NewClient();
        var token = await api.AdminTokenAsync();

        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/v1/profile", token)).Status);
        Assert.Equal(HttpStatusCode.NoContent, (await api.PostAsync("/api/v1/auth/logout", null, token)).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/v1/profile", token)).Status);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_Is401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await NewClient().GetAsync("/api/v1/profile")).Status);
    }

    [Fact]
    public async Task InactiveUser_CannotLogin()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"i-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email, isActive: false);

        var response = await api.LoginAsync(email, ApiClient.DefaultPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
        Assert.Equal("APP:FORBIDDEN", response.ErrorCode);
    }

    [Fact]
    public async Task UnconfirmedEmail_CannotLogin()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"c-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email, emailConfirmed: false);

        var response = await api.LoginAsync(email, ApiClient.DefaultPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.Status);
    }

    [Fact]
    public async Task RepeatedFailures_LockTheAccount_AndAdminUnlockRestoresLogin()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"l-{ApiClient.Suffix()}@example.com";
        var id = await api.CreateUserAsync(admin, email);

        // Baseline: four 401s, the fifth failure already answers "locked" (403).
        for (var attempt = 1; attempt <= 4; attempt++)
        {
            Assert.Equal(HttpStatusCode.Unauthorized, (await api.LoginAsync(email, "Wrong1234*")).Status);
        }

        Assert.Equal(HttpStatusCode.Forbidden, (await api.LoginAsync(email, "Wrong1234*")).Status);
        Assert.Equal(HttpStatusCode.Forbidden, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);

        Assert.Equal(HttpStatusCode.NoContent, (await api.PatchAsync($"/api/v1/users/{id}/unlock", null, admin)).Status);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
    }

    [Fact]
    public async Task ChangePassword_ValidatesOldPassword_AndSwitchesCredentials()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"p-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email);
        var token = await api.LoginTokenAsync(email, ApiClient.DefaultPassword);

        var wrongOld = await api.PostAsync("/api/v1/profile/change-password",
            new { oldPassword = "Nope12345*", newPassword = "Changed1234*", confirmNewPassword = "Changed1234*" }, token);
        Assert.Equal(HttpStatusCode.BadRequest, wrongOld.Status);

        var mismatch = await api.PostAsync("/api/v1/profile/change-password",
            new { oldPassword = ApiClient.DefaultPassword, newPassword = "Changed1234*", confirmNewPassword = "Other1234*" }, token);
        Assert.Equal(HttpStatusCode.BadRequest, mismatch.Status);

        var changed = await api.PostAsync("/api/v1/profile/change-password",
            new { oldPassword = ApiClient.DefaultPassword, newPassword = "Changed1234*", confirmNewPassword = "Changed1234*" }, token);
        Assert.Equal(HttpStatusCode.NoContent, changed.Status);

        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, "Changed1234*")).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
    }

    [Fact]
    public async Task ChangePassword_WithWeakNewPassword_IsRejected_AndKeepsTheOldPassword()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"w-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email);
        var token = await api.LoginTokenAsync(email, ApiClient.DefaultPassword);

        // 8+ characters so only the template's password policy (no uppercase/digit/symbol) can reject it
        var weak = await api.PostAsync("/api/v1/profile/change-password",
            new { oldPassword = ApiClient.DefaultPassword, newPassword = "alllowercase", confirmNewPassword = "alllowercase" }, token);

        Assert.Equal(HttpStatusCode.BadRequest, weak.Status);
        Assert.Equal("APP:VALIDATION", weak.ErrorCode);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
    }

    [Fact]
    public async Task SixthLogin_IsAccepted_AndOldestSessionStaysValid()
    {
        // Recorded in the baseline: the per-user session limit does not block the sixth login or revoke the first session.
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"s-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email);

        var firstToken = await api.LoginTokenAsync(email, ApiClient.DefaultPassword);
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
        }

        Assert.Equal(HttpStatusCode.OK, (await api.GetAsync("/api/v1/profile", firstToken)).Status);
    }
}

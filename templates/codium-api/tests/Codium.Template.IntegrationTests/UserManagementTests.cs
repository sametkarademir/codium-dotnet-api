using System.Net;
using Codium.Template.IntegrationTests.Infrastructure;

namespace Codium.Template.IntegrationTests;

[Collection(ApiCollection.Name)]
public class UserManagementTests(ApiFixture fixture)
{
    private ApiClient NewClient() => new(fixture.CreateClient());

    [Fact]
    public async Task CreateListGetUpdateDelete_RoundTrip()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"u-{ApiClient.Suffix()}@example.com";
        var id = await api.CreateUserAsync(admin, email);

        var get = await api.GetAsync($"/api/v1/users/{id}", admin);
        Assert.Equal(HttpStatusCode.OK, get.Status);
        Assert.Equal(email, get.Data.GetProperty("email").GetString());
        Assert.True(get.Data.GetProperty("isActive").GetBoolean());

        // Search is case-insensitive
        var search = await api.GetAsync($"/api/v1/users/paged?search={email.ToUpperInvariant()}", admin);
        Assert.Single(search.Data.GetProperty("data").EnumerateArray());

        var update = await api.PutAsync($"/api/v1/users/{id}", new
        {
            phoneNumber = "+905554445566",
            firstName = "Changed",
            lastName = "Name",
            isActive = true,
            emailConfirmed = true,
            phoneNumberConfirmed = true,
            twoFactorEnabled = false
        }, admin);
        Assert.Equal(HttpStatusCode.NoContent, update.Status);

        var updated = await api.GetAsync($"/api/v1/users/{id}", admin);
        Assert.Equal("Changed", updated.Data.GetProperty("firstName").GetString());
        Assert.Equal("+905554445566", updated.Data.GetProperty("phoneNumber").GetString());

        Assert.Equal(HttpStatusCode.OK, (await api.DeleteAsync($"/api/v1/users/{id}", admin)).Status);
        var afterDelete = await api.GetAsync($"/api/v1/users/{id}", admin);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.Status);
        Assert.Equal("APP:ENTITY:NOT_FOUND", afterDelete.ErrorCode);
    }

    [Fact]
    public async Task DuplicateEmail_IgnoringCase_Is409()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"d-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email);

        var duplicate = await api.PostAsync("/api/v1/users", new
        {
            email = email.ToUpperInvariant(),
            password = ApiClient.DefaultPassword,
            confirmPassword = ApiClient.DefaultPassword,
            emailConfirmed = true
        }, admin);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.Status);
        Assert.Equal("APP:CONFLICT", duplicate.ErrorCode);
    }

    [Fact]
    public async Task EmailOfDeletedUser_CanBeReused()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"r-{ApiClient.Suffix()}@example.com";
        var id = await api.CreateUserAsync(admin, email);

        Assert.Equal(HttpStatusCode.OK, (await api.DeleteAsync($"/api/v1/users/{id}", admin)).Status);

        var again = await api.PostAsync("/api/v1/users", new
        {
            email,
            password = ApiClient.DefaultPassword,
            confirmPassword = ApiClient.DefaultPassword,
            emailConfirmed = true
        }, admin);
        Assert.Equal(HttpStatusCode.NoContent, again.Status);
    }

    [Fact]
    public async Task CreateUser_InvalidPayload_Is400()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();

        var response = await api.PostAsync("/api/v1/users",
            new { email = "bad", password = "short", confirmPassword = "different" }, admin);

        Assert.Equal(HttpStatusCode.BadRequest, response.Status);
        Assert.Equal("APP:VALIDATION", response.ErrorCode);
    }

    [Fact]
    public async Task WeakPasswords_AreRejectedByRequestValidation_WithTheFieldName()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"wp-{ApiClient.Suffix()}@example.com";

        // Long enough, but no uppercase, digit or special character
        var create = await api.PostAsync("/api/v1/users",
            new { email, password = "alllowercase", confirmPassword = "alllowercase", emailConfirmed = true }, admin);
        Assert.Equal(HttpStatusCode.BadRequest, create.Status);
        Assert.Equal("APP:VALIDATION", create.ErrorCode);
        Assert.Equal("Password", create.Body.GetProperty("Details")[0].GetProperty("Property").GetString());
        Assert.Equal(3, create.Body.GetProperty("Details")[0].GetProperty("Errors").GetArrayLength());

        var id = await api.CreateUserAsync(admin, email);
        var reset = await api.PatchAsync($"/api/v1/users/{id}/reset-password",
            new { newPassword = "alllowercase", confirmNewPassword = "alllowercase" }, admin);
        Assert.Equal(HttpStatusCode.BadRequest, reset.Status);
        Assert.Equal("NewPassword", reset.Body.GetProperty("Details")[0].GetProperty("Property").GetString());

        // The user keeps the original password
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
    }

    [Fact]
    public async Task ResetPassword_ReplacesCredentials()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"x-{ApiClient.Suffix()}@example.com";
        var id = await api.CreateUserAsync(admin, email);

        var reset = await api.PatchAsync($"/api/v1/users/{id}/reset-password",
            new { newPassword = "Reset1234*", confirmNewPassword = "Reset1234*" }, admin);
        Assert.Equal(HttpStatusCode.NoContent, reset.Status);

        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(email, "Reset1234*")).Status);
        Assert.Equal(HttpStatusCode.Unauthorized, (await api.LoginAsync(email, ApiClient.DefaultPassword)).Status);
    }

    [Fact]
    public async Task Unlock_ForUsersWithoutLockout_IsANoOp()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var adminId = await api.FindUserIdAsync(admin, ApiClient.AdminEmail);

        // The seeded admin has lockout disabled: there is nothing to unlock.
        Assert.Equal(HttpStatusCode.NoContent, (await api.PatchAsync($"/api/v1/users/{adminId}/unlock", null, admin)).Status);
        Assert.Equal(HttpStatusCode.OK, (await api.LoginAsync(ApiClient.AdminEmail, ApiClient.AdminPassword)).Status);
    }

    [Fact]
    public async Task UnknownAndDeletedUsers_Are404_ForEveryOperation()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var id = await api.CreateUserAsync(admin, $"gone-{ApiClient.Suffix()}@example.com");
        await api.DeleteAsync($"/api/v1/users/{id}", admin);

        foreach (var path in new[] { "email-confirmation", "phone-number-confirmation", "two-factor-enabled", "is-active", "unlock" })
        {
            Assert.Equal(HttpStatusCode.NotFound, (await api.PatchAsync($"/api/v1/users/{id}/{path}", null, admin)).Status);
        }

        Assert.Equal(HttpStatusCode.NotFound, (await api.DeleteAsync($"/api/v1/users/{id}", admin)).Status);
        Assert.Equal(HttpStatusCode.NotFound,
            (await api.PatchAsync($"/api/v1/users/{id}/sync-roles", new { roleIds = Array.Empty<Guid>() }, admin)).Status);
    }

    [Fact]
    public async Task SyncRoles_WithUnknownRole_Is404_AndChangesNothing()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var suffix = ApiClient.Suffix();
        var userId = await api.CreateUserAsync(admin, $"mr-{suffix}@example.com");
        var roleId = await api.CreateRoleAsync(admin, $"MissingRole-{suffix}");

        var response = await api.PatchAsync($"/api/v1/users/{userId}/sync-roles",
            new { roleIds = new[] { roleId, Guid.NewGuid() } }, admin);

        Assert.Equal(HttpStatusCode.NotFound, response.Status);
        var user = await api.GetAsync($"/api/v1/users/{userId}", admin);
        Assert.Empty(user.Data.GetProperty("roles").EnumerateArray());
    }

    [Fact]
    public async Task Toggles_FlipTheirFlags()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"t-{ApiClient.Suffix()}@example.com";
        var id = await api.CreateUserAsync(admin, email);

        foreach (var (path, property, expected) in new[]
                 {
                     ("email-confirmation", "emailConfirmed", false),
                     ("phone-number-confirmation", "phoneNumberConfirmed", true),
                     ("two-factor-enabled", "twoFactorEnabled", true),
                     ("is-active", "isActive", false)
                 })
        {
            Assert.Equal(HttpStatusCode.NoContent, (await api.PatchAsync($"/api/v1/users/{id}/{path}", null, admin)).Status);
            var user = await api.GetAsync($"/api/v1/users/{id}", admin);
            Assert.Equal(expected, user.Data.GetProperty(property).GetBoolean());
        }
    }

    [Fact]
    public async Task SyncRoles_ChangesRolesAndPermissionsInTheNextLogin()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var suffix = ApiClient.Suffix();
        var email = $"sr-{suffix}@example.com";
        var userId = await api.CreateUserAsync(admin, email);
        var roleId = await api.CreateRoleAsync(admin, $"SyncRole-{suffix}");

        var permissions = await api.GetAsync("/api/v1/permissions/options", admin);
        var permissionIds = permissions.Data.GetProperty("data").EnumerateArray()
            .Take(2).Select(p => p.GetProperty("id").GetGuid()).ToArray();
        Assert.Equal(HttpStatusCode.NoContent,
            (await api.PatchAsync($"/api/v1/roles/{roleId}/sync-permissions", new { permissionIds }, admin)).Status);

        Assert.Equal(HttpStatusCode.NoContent,
            (await api.PatchAsync($"/api/v1/users/{userId}/sync-roles", new { roleIds = new[] { roleId } }, admin)).Status);

        var user = await api.GetAsync($"/api/v1/users/{userId}", admin);
        Assert.Equal($"SyncRole-{suffix}", Assert.Single(user.Data.GetProperty("roles").EnumerateArray()).GetProperty("name").GetString());

        var token = await api.LoginTokenAsync(email, ApiClient.DefaultPassword);
        var payload = ApiClient.ReadJwtPayload(token);
        Assert.Equal($"SyncRole-{suffix}", payload.GetProperty("role").GetString());
        Assert.Equal(2, payload.GetProperty("permission").GetArrayLength());

        // Removing every role drops the link (hard delete of the Identity user-role row)
        Assert.Equal(HttpStatusCode.NoContent,
            (await api.PatchAsync($"/api/v1/users/{userId}/sync-roles", new { roleIds = Array.Empty<Guid>() }, admin)).Status);
        var withoutRole = await api.GetAsync($"/api/v1/users/{userId}", admin);
        Assert.Empty(withoutRole.Data.GetProperty("roles").EnumerateArray());
    }

    [Fact]
    public async Task UserWithoutPermission_IsForbidden_AndAnonymousIsUnauthorized()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var email = $"f-{ApiClient.Suffix()}@example.com";
        await api.CreateUserAsync(admin, email);
        var token = await api.LoginTokenAsync(email, ApiClient.DefaultPassword);

        var forbidden = await api.GetAsync("/api/v1/users/paged", token);
        Assert.Equal(HttpStatusCode.Forbidden, forbidden.Status);
        Assert.Equal("APP:FORBIDDEN", forbidden.ErrorCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await api.GetAsync("/api/v1/users/paged")).Status);
    }

    [Fact]
    public async Task Sessions_CanBeListed()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();

        var sessions = await api.GetAsync("/api/v1/sessions/paged", admin);
        Assert.Equal(HttpStatusCode.OK, sessions.Status);
        Assert.NotEmpty(sessions.Data.GetProperty("data").EnumerateArray());
    }
}

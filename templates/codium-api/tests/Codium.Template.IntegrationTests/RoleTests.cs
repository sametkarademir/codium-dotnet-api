using System.Net;
using Codium.Template.IntegrationTests.Infrastructure;

namespace Codium.Template.IntegrationTests;

[Collection(ApiCollection.Name)]
public class RoleTests(ApiFixture fixture)
{
    private ApiClient NewClient() => new(fixture.CreateClient());

    [Fact]
    public async Task CreateGetUpdateDelete_RoundTrip()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var name = $"Role-{ApiClient.Suffix()}";
        var id = await api.CreateRoleAsync(admin, name);

        var get = await api.GetAsync($"/api/v1/roles/{id}", admin);
        Assert.Equal(name, get.Data.GetProperty("name").GetString());
        Assert.Equal("test role", get.Data.GetProperty("description").GetString());

        var renamed = $"{name}-2";
        Assert.Equal(HttpStatusCode.NoContent,
            (await api.PutAsync($"/api/v1/roles/{id}", new { name = renamed, description = "updated" }, admin)).Status);
        Assert.Equal(renamed, (await api.GetAsync($"/api/v1/roles/{id}", admin)).Data.GetProperty("name").GetString());

        Assert.Equal(HttpStatusCode.NoContent, (await api.DeleteAsync($"/api/v1/roles/{id}", admin)).Status);
        Assert.Equal(HttpStatusCode.NotFound, (await api.GetAsync($"/api/v1/roles/{id}", admin)).Status);
    }

    [Fact]
    public async Task DuplicateName_IgnoringCase_Is409()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var name = $"Dup-{ApiClient.Suffix()}";
        await api.CreateRoleAsync(admin, name);

        var duplicate = await api.PostAsync("/api/v1/roles", new { name = name.ToUpperInvariant(), description = "dup" }, admin);

        Assert.Equal(HttpStatusCode.Conflict, duplicate.Status);
    }

    [Fact]
    public async Task NameOfDeletedRole_CanBeReused()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var name = $"Reuse-{ApiClient.Suffix()}";
        var id = await api.CreateRoleAsync(admin, name);

        Assert.Equal(HttpStatusCode.NoContent, (await api.DeleteAsync($"/api/v1/roles/{id}", admin)).Status);

        var again = await api.PostAsync("/api/v1/roles", new { name, description = "again" }, admin);
        Assert.Equal(HttpStatusCode.NoContent, again.Status);
    }

    [Fact]
    public async Task SyncPermissions_AreReturnedWithTheRole()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var id = await api.CreateRoleAsync(admin, $"Perm-{ApiClient.Suffix()}");
        var permissions = await api.GetAsync("/api/v1/permissions/options", admin);
        var permissionIds = permissions.Data.GetProperty("data").EnumerateArray()
            .Take(3).Select(p => p.GetProperty("id").GetGuid()).ToArray();

        Assert.Equal(HttpStatusCode.NoContent,
            (await api.PatchAsync($"/api/v1/roles/{id}/sync-permissions", new { permissionIds }, admin)).Status);

        var role = await api.GetAsync($"/api/v1/roles/{id}", admin);
        Assert.Equal(3, role.Data.GetProperty("permissions").GetArrayLength());
    }

    [Fact]
    public async Task Roles_RequireAuthentication()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await NewClient().GetAsync("/api/v1/roles/paged")).Status);
    }
}

using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Roles;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Codium.Template.IntegrationTests;

/// <summary>New-database behavior: seed data, Identity mapping, audit and soft-delete.</summary>
[Collection(ApiCollection.Name)]
public class FreshDatabaseTests(ApiFixture fixture)
{
    private ApiClient NewClient() => new(fixture.CreateClient());

    [Fact]
    public async Task SeededAdmin_CanLogin_WithAdminRoleAndAllPermissions()
    {
        var api = NewClient();
        var token = await api.AdminTokenAsync();
        var payload = ApiClient.ReadJwtPayload(token);

        Assert.Equal(RoleConsts.Admin, payload.GetProperty("role").GetString());
        Assert.True(payload.GetProperty("permission").GetArrayLength() > 0);
    }

    [Fact]
    public async Task SeededAdmin_UsesTheIdentityColumns()
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var admin = await context.Users.AsNoTracking().SingleAsync(u => u.Email == ApiClient.AdminEmail);

        Assert.Equal(ApiClient.AdminEmail, admin.UserName);
        Assert.Equal("ADMIN@CODIUM.COM", admin.NormalizedEmail);
        Assert.Equal("ADMIN@CODIUM.COM", admin.NormalizedUserName);
        Assert.False(string.IsNullOrEmpty(admin.SecurityStamp));
        Assert.False(string.IsNullOrEmpty(admin.PasswordHash));
        Assert.False(admin.LockoutEnabled, "the seeded admin has never been lockable");
        Assert.True(admin.EmailConfirmed);

        var adminRole = await context.Roles.AsNoTracking().SingleAsync(r => r.Name == RoleConsts.Admin);
        Assert.Equal(RoleConsts.Admin.ToUpperInvariant(), adminRole.NormalizedName);
        Assert.True(await context.Set<UserRole>().AnyAsync(ur => ur.UserId == admin.Id && ur.RoleId == adminRole.Id));
    }

    [Fact]
    public async Task Seeder_IsIdempotent()
    {
        using var scope = fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var seeder = scope.ServiceProvider.GetRequiredService<Codium.Template.Domain.DevelopmentDataSeederContributor>();

        var before = (await context.Users.CountAsync(u => u.Email == ApiClient.AdminEmail),
            await context.Roles.CountAsync(r => r.Name == RoleConsts.Admin),
            await context.Set<UserRole>().CountAsync());

        await seeder.SeedAsync();

        Assert.Equal(before, (await context.Users.CountAsync(u => u.Email == ApiClient.AdminEmail),
            await context.Roles.CountAsync(r => r.Name == RoleConsts.Admin),
            await context.Set<UserRole>().CountAsync()));
        Assert.Equal(1, before.Item1);
        Assert.Equal(1, before.Item2);
    }

    [Fact]
    public async Task AuditFields_AndSoftDelete_AreRecordedForUsersAndRoles()
    {
        var api = NewClient();
        var admin = await api.AdminTokenAsync();
        var suffix = ApiClient.Suffix();
        var email = $"audit-{suffix}@example.com";
        var userId = await api.CreateUserAsync(admin, email);
        var roleId = await api.CreateRoleAsync(admin, $"Audit-{suffix}");
        await api.PatchAsync($"/api/v1/users/{userId}/sync-roles", new { roleIds = new[] { roleId } }, admin);

        await api.PutAsync($"/api/v1/users/{userId}", new
        {
            phoneNumber = (string?)null,
            firstName = "Audited",
            lastName = "User",
            isActive = true,
            emailConfirmed = true,
            phoneNumberConfirmed = false,
            twoFactorEnabled = false
        }, admin);

        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.AsNoTracking().SingleAsync(u => u.Id == userId);
            Assert.NotEqual(default, user.CreationTime);
            Assert.NotNull(user.LastModificationTime);
            Assert.False(user.IsDeleted);

            var userRole = await context.Set<UserRole>().AsNoTracking().SingleAsync(ur => ur.UserId == userId);
            Assert.NotEqual(default, userRole.CreationTime);
        }

        await api.DeleteAsync($"/api/v1/users/{userId}", admin);
        await api.DeleteAsync($"/api/v1/roles/{roleId}", admin);

        using (var scope = fixture.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var user = await context.Users.IgnoreQueryFilters().AsNoTracking().SingleAsync(u => u.Id == userId);
            Assert.True(user.IsDeleted);
            Assert.NotNull(user.DeletionTime);

            var role = await context.Roles.IgnoreQueryFilters().AsNoTracking().SingleAsync(r => r.Id == roleId);
            Assert.True(role.IsDeleted);
            Assert.NotNull(role.DeletionTime);
        }
    }
}

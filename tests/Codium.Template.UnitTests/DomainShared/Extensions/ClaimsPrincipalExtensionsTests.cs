using System.Security.Claims;
using Codium.Template.Domain.Shared.Extensions;

namespace Codium.Template.UnitTests.DomainShared.Extensions;

public class ClaimsPrincipalExtensionsTests
{
    private static ClaimsPrincipal PrincipalWith(Action<ClaimsIdentity> configure)
    {
        var identity = new ClaimsIdentity("test");
        configure(identity);
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void UserId_RoundTripsThroughAddAndGet()
    {
        var id = Guid.NewGuid();

        var principal = PrincipalWith(i => i.AddUserId(id));

        Assert.Equal(id, principal.GetUserId());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("not-a-guid")]
    public void UserId_MissingOrMalformed_IsNull(string? value)
    {
        var principal = PrincipalWith(i =>
        {
            if (value != null) i.AddClaim(new Claim(ClaimTypes.NameIdentifier, value));
        });

        Assert.Null(principal.GetUserId());
    }

    [Fact]
    public void SessionAndTenantIds_RoundTrip_AndDefaultToNull()
    {
        var sessionId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        var principal = PrincipalWith(i => i.AddSessionId(sessionId).AddTenantId(tenantId));
        var empty = PrincipalWith(_ => { });

        Assert.Equal(sessionId, principal.GetSessionId());
        Assert.Equal(tenantId, principal.GetTenantId());
        Assert.Null(empty.GetSessionId());
        Assert.Null(empty.GetTenantId());
    }

    [Fact]
    public void Email_RoundTrips()
    {
        var principal = PrincipalWith(i => i.AddUserEmail("user@example.com"));

        Assert.Equal("user@example.com", principal.GetUserEmail());
    }

    [Fact]
    public void Roles_AreReadBackInOrder_AndRoleChecksWork()
    {
        var principal = PrincipalWith(i => i.AddRoles(["Admin", "Editor"]));

        Assert.Equal(["Admin", "Editor"], principal.GetRoles());
        Assert.True(principal.HasRole("Admin"));
        Assert.False(principal.HasRole("Viewer"));
        Assert.True(principal.HasAnyRole("Viewer", "Editor"));
        Assert.False(principal.HasAnyRole("Viewer"));
        Assert.True(principal.HasAllRoles("Admin", "Editor"));
        Assert.False(principal.HasAllRoles("Admin", "Viewer"));
    }

    [Fact]
    public void Permissions_AreCaseInsensitive()
    {
        var principal = PrincipalWith(i => i.AddPermissions(["User.View", "Role.Create"]));

        Assert.Equal(["User.View", "Role.Create"], principal.GetPermissions());
        Assert.True(principal.HasPermission("user.view"));
        Assert.False(principal.HasPermission("User.Delete"));
        Assert.True(principal.HasAnyPermission("User.Delete", "ROLE.CREATE"));
        Assert.False(principal.HasAnyPermission("User.Delete"));
        Assert.True(principal.HasAllPermissions("user.view", "role.create"));
        Assert.False(principal.HasAllPermissions("user.view", "User.Delete"));
    }

    [Fact]
    public void CustomProperty_ReadsAnyClaimByKey()
    {
        var principal = PrincipalWith(i => i.AddClaim(new Claim("department", "R&D")));

        Assert.Equal("R&D", principal.GetUserCustomProperty("department"));
        Assert.Null(principal.GetUserCustomProperty("missing"));
    }

    [Fact]
    public void IsAuthenticated_DependsOnTheAuthenticationType()
    {
        Assert.True(new ClaimsPrincipal(new ClaimsIdentity("test")).IsAuthenticated());
        Assert.False(new ClaimsPrincipal(new ClaimsIdentity()).IsAuthenticated());
    }
}

using System.Security.Claims;
using Codium.Template.Application.Users;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Codium.Template.UnitTests.Application.Users;

public class CurrentUserTests
{
    private static CurrentUser CreateFor(ClaimsPrincipal? principal)
    {
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(principal == null ? null : new DefaultHttpContext { User = principal });
        return new CurrentUser(accessor);
    }

    [Fact]
    public void AuthenticatedPrincipal_ExposesItsClaims()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var identity = new ClaimsIdentity("test");
        identity.AddUserId(userId).AddUserEmail("user@example.com").AddSessionId(sessionId)
            .AddRoles(["Admin"]).AddPermissions(["User.View"]);

        var currentUser = CreateFor(new ClaimsPrincipal(identity));

        Assert.True(currentUser.IsAuthenticated);
        Assert.Equal(userId, currentUser.Id);
        Assert.Equal("user@example.com", currentUser.Email);
        Assert.Equal(sessionId, currentUser.SessionId);
        Assert.Equal(["Admin"], currentUser.Roles);
        Assert.Equal(["User.View"], currentUser.Permissions);
        Assert.True(currentUser.HasRole("Admin"));
        Assert.False(currentUser.HasRole("Editor"));
        Assert.True(currentUser.HasPermission("user.view"));
        Assert.False(currentUser.HasPermission("Role.Create"));
    }

    [Fact]
    public void AnonymousPrincipal_IsNotAuthenticated_AndHasNoIdentityData()
    {
        var currentUser = CreateFor(new ClaimsPrincipal(new ClaimsIdentity()));

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.Id);
        Assert.Null(currentUser.Email);
        Assert.Null(currentUser.SessionId);
        Assert.Empty(currentUser.Roles!);
        Assert.False(currentUser.HasRole("Admin"));
        Assert.False(currentUser.HasPermission("User.View"));
    }

    [Fact]
    public void WithoutAnHttpContext_NothingIsAvailable()
    {
        var currentUser = CreateFor(null);

        Assert.False(currentUser.IsAuthenticated);
        Assert.Null(currentUser.Id);
        Assert.Null(currentUser.Roles);
        Assert.Null(currentUser.Permissions);
        Assert.Null(currentUser.SessionId);
        Assert.Null(currentUser.User());
        Assert.False(currentUser.HasRole("Admin"));
        Assert.False(currentUser.HasPermission("User.View"));
    }
}

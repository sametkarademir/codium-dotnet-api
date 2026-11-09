using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Codium.Template.UnitTests.Infrastructure;

/// <summary>Builds Identity's manager classes with substituted stores/dependencies for tests that need them.</summary>
public static class IdentityMocks
{
    public static UserManager<User> UserManager(IdentityOptions? options = null)
    {
        return Substitute.For<UserManager<User>>(
            Substitute.For<IUserStore<User>>(),
            Options.Create(options ?? new IdentityOptions()),
            null, null, null, null, null, null, null);
    }

    public static SignInManager<User> SignInManager(UserManager<User> userManager)
    {
        return Substitute.For<SignInManager<User>>(
            userManager,
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IUserClaimsPrincipalFactory<User>>(),
            Options.Create(new IdentityOptions()),
            Substitute.For<ILogger<SignInManager<User>>>(),
            Substitute.For<IAuthenticationSchemeProvider>(),
            Substitute.For<IUserConfirmation<User>>());
    }
}

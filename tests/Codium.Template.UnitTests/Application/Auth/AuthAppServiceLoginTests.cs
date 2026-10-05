using Codium.Template.Application.Auth;
using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Users;
using Codium.Template.UnitTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;

namespace Codium.Template.UnitTests.Application.Auth;

/// <summary>
/// Decision matrix of LoginAsync: which Identity sign-in outcome becomes which API error. Only the branches that end in
/// an exception are covered; the success path (session, refresh token) is a database flow covered by the integration tests.
/// </summary>
public class AuthAppServiceLoginTests
{
    private readonly UserManager<User> _userManager = IdentityMocks.UserManager();
    private readonly SignInManager<User> _signInManager;
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly AuthAppService _service;

    private readonly LoginRequestDto _request = new() { Email = "user@example.com", Password = "Abcdef1!" };

    public AuthAppServiceLoginTests()
    {
        _signInManager = IdentityMocks.SignInManager(_userManager);
        _service = new AuthAppService(
            _userManager,
            _signInManager,
            Substitute.For<IUserRoleRepository>(),
            Substitute.For<ISessionRepository>(),
            Substitute.For<IRefreshTokenRepository>(),
            _unitOfWork,
            Substitute.For<ICurrentUser>(),
            Substitute.For<IJwtTokenAppService>(),
            Substitute.For<IHttpContextAccessor>(),
            new KeyLocalizer());
    }

    private User GivenUser(bool emailConfirmed = true, bool isActive = true)
    {
        var user = new User { Id = Guid.NewGuid(), Email = _request.Email, EmailConfirmed = emailConfirmed, IsActive = isActive };
        _userManager.FindByEmailAsync(_request.Email).Returns(user);
        return user;
    }

    private void GivenSignInResult(SignInResult result) =>
        _signInManager.CheckPasswordSignInAsync(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(result);

    [Fact]
    public async Task UnknownEmail_IsUnauthorized_WithoutTouchingSignIn()
    {
        _userManager.FindByEmailAsync(_request.Email).Returns((User?)null);

        var exception = await Assert.ThrowsAsync<AppUnauthorizedException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:InvalidCredentials", exception.Message);
        await _signInManager.DidNotReceiveWithAnyArgs().CheckPasswordSignInAsync(default!, default!, default);
    }

    [Fact]
    public async Task PasswordIsCheckedWithLockoutOnFailureEnabled()
    {
        var user = GivenUser();
        GivenSignInResult(SignInResult.Failed);

        await Assert.ThrowsAsync<AppUnauthorizedException>(() => _service.LoginAsync(_request));

        await _signInManager.Received(1).CheckPasswordSignInAsync(user, _request.Password, true);
    }

    [Fact]
    public async Task LockedOut_IsForbidden()
    {
        GivenUser();
        GivenSignInResult(SignInResult.LockedOut);

        var exception = await Assert.ThrowsAsync<AppForbiddenException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:LockedOut", exception.Message);
    }

    [Fact]
    public async Task NotAllowed_WithUnconfirmedEmail_ReportsTheEmail()
    {
        GivenUser(emailConfirmed: false);
        GivenSignInResult(SignInResult.NotAllowed);

        var exception = await Assert.ThrowsAsync<AppForbiddenException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:EmailNotConfirmed", exception.Message);
    }

    [Fact]
    public async Task NotAllowed_WithConfirmedEmail_ReportsThePhoneNumber()
    {
        GivenUser(emailConfirmed: true);
        GivenSignInResult(SignInResult.NotAllowed);

        var exception = await Assert.ThrowsAsync<AppForbiddenException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:PhoneNumberNotConfirmed", exception.Message);
    }

    [Fact]
    public async Task WrongPassword_IsUnauthorized()
    {
        GivenUser();
        GivenSignInResult(SignInResult.Failed);

        var exception = await Assert.ThrowsAsync<AppUnauthorizedException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:InvalidCredentials", exception.Message);
    }

    [Fact]
    public async Task InactiveUser_WithCorrectPassword_IsForbidden_AndNoSessionIsStarted()
    {
        GivenUser(isActive: false);
        GivenSignInResult(SignInResult.Success);

        var exception = await Assert.ThrowsAsync<AppForbiddenException>(() => _service.LoginAsync(_request));

        Assert.Equal("AuthAppService:LoginAsync:UserInactive", exception.Message);
        await _unitOfWork.DidNotReceiveWithAnyArgs().BeginTransactionAsync(default);
    }
}

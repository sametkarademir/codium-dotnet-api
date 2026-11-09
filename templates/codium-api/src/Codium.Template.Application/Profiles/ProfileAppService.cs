using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Profiles;

public class ProfileAppService : IProfileAppService
{
    private readonly UserManager<User> _userManager;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundJobExecutor _backgroundJobExecutor;
    private readonly IStringLocalizer<ApplicationResource> _localizer;


    public ProfileAppService(
        UserManager<User> userManager,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor,
        IBackgroundJobExecutor backgroundJobExecutor,
        IStringLocalizer<ApplicationResource> localizer)
    {
        _userManager = userManager;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
        _backgroundJobExecutor = backgroundJobExecutor;
        _localizer = localizer;
        
        if (!_currentUser.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }
    }

    public async Task<Result<ProfileResponseDto>> GetProfileAsync(CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userManager.FindByIdAsync(_currentUser.Id.ToString()!);
        if (matchedUser == null)
        {
            throw new AppEntityNotFoundException(typeof(User));
        }

        return Result<ProfileResponseDto>.Ok(new ProfileResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email!,
            EmailConfirmed = matchedUser.EmailConfirmed,
            ShouldChangePasswordOnNextLogin = matchedUser.ShouldChangePasswordOnNextLogin,
            PhoneNumber = matchedUser.PhoneNumber,
            PhoneNumberConfirmed = matchedUser.PhoneNumberConfirmed,
            TwoFactorEnabled = matchedUser.TwoFactorEnabled,
            FirstName = matchedUser.FirstName,
            LastName = matchedUser.LastName,
            PasswordChangedTime = matchedUser.PasswordChangedTime,
        });
    }

    public async Task ChangePasswordAsync(ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userManager.FindByIdAsync(_currentUser.Id.ToString()!);
        if (matchedUser == null)
        {
            throw new AppEntityNotFoundException(typeof(User));
        }

        matchedUser.PasswordChangedTime = DateTime.UtcNow;
        matchedUser.ShouldChangePasswordOnNextLogin = false;
        var result = await _userManager.ChangePasswordAsync(matchedUser, request.OldPassword, request.NewPassword);
        if (result.Errors.Any(e => e.Code == nameof(IdentityErrorDescriber.PasswordMismatch)))
        {
            throw new AppValidationException(_localizer["ProfileAppService:ChangePasswordAsync:InvalidOldPassword"]);
        }

        result.ThrowIfFailed();

        _backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
            new InvalidateAllSessionsBackgroundJobArgs
            {
                UserId = matchedUser.Id,
                Reason = "Password changed by user",
                CorrelationId = _httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid()
            }
        );
    }
}
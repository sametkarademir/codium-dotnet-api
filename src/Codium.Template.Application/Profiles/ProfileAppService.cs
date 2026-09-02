using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Profiles;

public class ProfileAppService : IProfileAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IPasswordValidator _passwordValidator;
    private readonly ICurrentUser _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundJobExecutor _backgroundJobExecutor;
    private readonly IStringLocalizer<ApplicationResource> _localizer;


    public ProfileAppService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher<User> passwordHasher,
        IPasswordValidator passwordValidator,
        ICurrentUser currentUser,
        IHttpContextAccessor httpContextAccessor,
        IBackgroundJobExecutor backgroundJobExecutor,
        IStringLocalizer<ApplicationResource> localizer)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _passwordValidator = passwordValidator;
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
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == _currentUser.Id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        return Result<ProfileResponseDto>.Ok(new ProfileResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email,
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
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == _currentUser.Id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            matchedUser,
            matchedUser.PasswordHash,
            request.OldPassword
        );
        
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new AppValidationException(_localizer["ProfileAppService:ChangePasswordAsync:InvalidOldPassword"]);
        }
        
        var passwordValidationResult = _passwordValidator.Validate(request.NewPassword);
        if (!passwordValidationResult.Succeeded)
        {
            throw new AppValidationException(passwordValidationResult.Errors);
        }
        
        matchedUser.PasswordHash = _passwordHasher.HashPassword(matchedUser, request.NewPassword);
        matchedUser.PasswordChangedTime = DateTime.UtcNow;
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
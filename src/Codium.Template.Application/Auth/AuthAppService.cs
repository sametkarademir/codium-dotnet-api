using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using SignInResult = Codium.Template.Domain.Shared.Result.SignInResult;

namespace Codium.Template.Application.Auth;

public class AuthAppService : IAuthAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly IJwtTokenAppService _jwtTokenAppService;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IStringLocalizer<ApplicationResource> _localizer;

    public AuthAppService(
        IUserRepository userRepository,
        IUserRoleRepository userRoleRepository,
        ISessionRepository sessionRepository, 
        IRefreshTokenRepository refreshTokenRepository,
        IUnitOfWork unitOfWork, ICurrentUser currentUser, 
        IJwtTokenAppService jwtTokenAppService, 
        IPasswordHasher<User> passwordHasher,
        IHttpContextAccessor httpContextAccessor,
        IStringLocalizer<ApplicationResource> localizer)
    {
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _sessionRepository = sessionRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _jwtTokenAppService = jwtTokenAppService;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
        _localizer = localizer;
    }

    public async Task<Result<LoginResponseDto>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.FindByEmailAsync(request.Email, cancellationToken);
        if (matchedUser == null)
        {
            throw new AppUnauthorizedException(_localizer["AuthAppService:LoginAsync:InvalidCredentials"]);
        }

        var signInResult = await CheckPasswordSignInAsync(
            matchedUser,
            request.Password,
            true,
            cancellationToken
        );

        if (signInResult.IsLockedOut)
        {
            throw new AppForbiddenException(_localizer["AuthAppService:LoginAsync:LockedOut"]);
        }

        if (!signInResult.Succeeded)
        {
            throw new AppUnauthorizedException(_localizer["AuthAppService:LoginAsync:InvalidCredentials"]);
        }
        
        if (!matchedUser.IsActive)
        {
            throw new AppForbiddenException(_localizer["AuthAppService:LoginAsync:UserInactive"]);
        }
        
        if (!matchedUser.EmailConfirmed && UserConsts.RequireConfirmedEmail)
        {
            throw new AppForbiddenException(_localizer["AuthAppService:LoginAsync:EmailNotConfirmed"]);
        }
        
        if (!matchedUser.PhoneNumberConfirmed && UserConsts.RequireConfirmedPhoneNumber) 
        {
            throw new AppForbiddenException(_localizer["AuthAppService:LoginAsync:PhoneNumberNotConfirmed"]);
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var newUserSessionId = await CreateSessionAsync(matchedUser.Id, cancellationToken);

            var rolesAndPermissions = await _userRoleRepository.GetRolesAndPermissionsByUserIdAsync(matchedUser.Id, cancellationToken);

            var tokenResponse = _jwtTokenAppService.GenerateJwt(new GenerateJwtTokenRequestDto
            {
                Id = matchedUser.Id,
                Email = matchedUser.Email,
                Roles = rolesAndPermissions.Roles,
                Permissions = rolesAndPermissions.Permissions,
                SessionId = newUserSessionId
            });

            await CreateRefreshTokenAsync(
                matchedUser.Id,
                newUserSessionId,
                tokenResponse.RefreshToken,
                null,
                tokenResponse.RefreshTokenExpiryTime,
                cancellationToken
            );
            
            await transaction.CommitAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<LoginResponseDto>.Ok(new LoginResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiryTime = tokenResponse.AccessTokenExpiryTime,
                RefreshToken = tokenResponse.RefreshToken
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task<Result<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var matchedRefreshToken = await _refreshTokenRepository.ValidateAndUseRefreshTokenAsync(request.RefreshToken, cancellationToken);
            if (matchedRefreshToken == null)
            {
                throw new AppUnauthorizedException();
            }
            
            var matchedUser = await _userRepository.SingleOrDefaultAsync(
                predicate: u => u.Id == matchedRefreshToken.UserId,
                cancellationToken: cancellationToken
            );
            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            var rolesAndPermissions = await _userRoleRepository.GetRolesAndPermissionsByUserIdAsync(matchedUser.Id, cancellationToken);

            var tokenResponse = _jwtTokenAppService.GenerateJwt(new GenerateJwtTokenRequestDto
            {
                Id = matchedUser.Id,
                Email = matchedUser.Email,
                Roles = rolesAndPermissions.Roles,
                Permissions = rolesAndPermissions.Permissions,
                SessionId = matchedRefreshToken.SessionId
            });

            await CreateRefreshTokenAsync(
                matchedUser.Id,
                matchedRefreshToken.SessionId,
                tokenResponse.RefreshToken,
                matchedRefreshToken.Token,
                tokenResponse.RefreshTokenExpiryTime,
                cancellationToken
            );
            
            await transaction.CommitAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<LoginResponseDto>.Ok(new LoginResponseDto
            {
                AccessToken = tokenResponse.AccessToken,
                ExpiryTime = tokenResponse.AccessTokenExpiryTime,
                RefreshToken = tokenResponse.RefreshToken
            });
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            if (!_currentUser.IsAuthenticated || _currentUser.Id == null)
            {
                throw new AppUnauthorizedException();
            }

            var matchedUser = await _userRepository.SingleOrDefaultAsync(
                predicate: u => u.Id == _currentUser.Id,
                cancellationToken: cancellationToken
            );
            if (matchedUser == null)
            {
                throw new AppUnauthorizedException();
            }

            if (_currentUser.SessionId != null)
            {
                var matchedSession = await _sessionRepository.RevokeSessionByUserAsync(
                    _currentUser.SessionId.Value,
                    matchedUser.Id,
                    cancellationToken
                );
                if (matchedSession == null)
                {
                    throw new AppUnauthorizedException();
                }

                await _refreshTokenRepository.RevokeRefreshTokensBySessionAsync(
                    matchedSession.Id,
                    matchedUser.Id,
                    cancellationToken
                );

                await transaction.CommitAsync(cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    private async Task<SignInResult> CheckPasswordSignInAsync(
        User user,
        string password,
        bool lockoutOnFailure,
        CancellationToken cancellationToken
    )
    {
        if (IsLockedOut(user))
        {
            return SignInResult.LockedOut();
        }

        if (VerifyPassword(user, password))
        {
            if (lockoutOnFailure)
            {
                // Başarılı giriş: Kilidi kaldır (kendi transaction'ında)
                await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
                try
                {
                    await UnlockAsync(user, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            return SignInResult.Success();
        }

        if (lockoutOnFailure)
        {
            // Başarısız giriş: AccessFailedCount'u artır (kendi transaction'ında)
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                await AccessFailedAsync(user, cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            if (IsLockedOut(user))
            {
                return SignInResult.LockedOut();
            }
        }

        return SignInResult.Failed();
    }

    private bool IsLockedOut(User user)
    {
        if (!user.LockoutEnabled)
        {
            return false;
        }

        if (user.LockoutEnd == null)
        {
            return false;
        }

        return user.LockoutEnd > DateTimeOffset.UtcNow;
    }

    private bool VerifyPassword(User user, string password)
    {
        var verificationResult = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password
        );

        return verificationResult != PasswordVerificationResult.Failed;
    }

    private async Task UnlockAsync(User user, CancellationToken cancellationToken)
    {
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task AccessFailedAsync(User user, CancellationToken cancellationToken)
    {
        if (!user.LockoutEnabled)
        {
            return;
        }

        user.AccessFailedCount++;

        if (user.AccessFailedCount >= UserConsts.MaxFailedAccessAttempts)
        {
            var lockoutMinutes = UserConsts.DefaultLockoutTimeSpanMinutes.TotalMinutes;
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
    }

    private async Task<Guid> CreateSessionAsync(Guid userId, CancellationToken cancellationToken)
    {
        var revokedSessionIds = await _sessionRepository.GetExcessSessionIdsAsync(
            userId,
            UserConsts.MaxActiveSessionsPerUser,
            cancellationToken
        );
        
        await _sessionRepository.RevokeSessionsByUserAsync(revokedSessionIds, userId, cancellationToken);
        await _refreshTokenRepository.RevokeRefreshTokensBySessionsAsync(revokedSessionIds, userId, cancellationToken);

        var deviceInfo = _httpContextAccessor.HttpContext?.GetDeviceInfo();
        var clientIp = _httpContextAccessor.HttpContext?.GetClientIpAddress() ?? "Unknown";
        var userAgent = _httpContextAccessor.HttpContext?.GetUserAgent() ?? "Unknown";
        var correlationId = _httpContextAccessor.HttpContext?.GetCorrelationId();

        var newUserSession = new Session
        {
            Id = Guid.NewGuid(),
            IsRevoked = false,
            ClientIp = clientIp,
            UserAgent = userAgent,
            DeviceFamily = deviceInfo?.DeviceFamily,
            DeviceModel = deviceInfo?.DeviceModel,
            OsFamily = deviceInfo?.OsFamily,
            OsVersion = deviceInfo?.OsVersion,
            BrowserFamily = deviceInfo?.BrowserFamily,
            BrowserVersion = deviceInfo?.BrowserVersion,
            IsMobile = deviceInfo?.IsMobile ?? false,
            IsDesktop = deviceInfo?.IsDesktop ?? false,
            IsTablet = deviceInfo?.IsTablet ?? false,
            CorrelationId = correlationId,
            UserId = userId,
        };
        await _sessionRepository.AddAsync(newUserSession, cancellationToken);

        return newUserSession.Id;
    }

    private async Task CreateRefreshTokenAsync(
        Guid userId,
        Guid sessionId,
        string refreshToken,
        string? replacedByToken,
        DateTime refreshTokenExpiryTime,
        CancellationToken cancellationToken)
    {
        var newRefreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshToken,
            ExpiryTime = refreshTokenExpiryTime,
            IsUsed = false,
            IsRevoked = false,
            RevokedTime = null,
            ReplacedByToken = replacedByToken,
            UserId = userId,
            SessionId = sessionId,
        };

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
    }
}
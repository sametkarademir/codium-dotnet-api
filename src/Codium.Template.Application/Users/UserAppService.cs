using AutoMapper;
using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Users;

public class UserAppService : IUserAppService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordValidator _passwordValidator;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobExecutor _backgroundJobExecutor;
    private readonly IStringLocalizer<UserAppService> _localizer;

    public UserAppService(IUserRepository userRepository,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork,
        IPasswordValidator passwordValidator,
        IPasswordHasher<User> passwordHasher,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IBackgroundJobExecutor backgroundJobExecutor,
        IStringLocalizer<UserAppService> localizer)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _passwordValidator = passwordValidator;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _backgroundJobExecutor = backgroundJobExecutor;
        _localizer = localizer;
    }

    public async Task<Result<UserResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            include: q => q
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)!,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var mappedUser = new UserResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email,
            EmailConfirmed = matchedUser.EmailConfirmed,
            PhoneNumber = matchedUser.PhoneNumber,
            PhoneNumberConfirmed = matchedUser.PhoneNumberConfirmed,
            TwoFactorEnabled = matchedUser.TwoFactorEnabled,
            LockoutEnd = matchedUser.LockoutEnd,
            LockoutEnabled = matchedUser.LockoutEnabled,
            AccessFailedCount = matchedUser.AccessFailedCount,
            FirstName = matchedUser.FirstName,
            LastName = matchedUser.LastName,
            PasswordChangedTime = matchedUser.PasswordChangedTime,
            IsActive = matchedUser.IsActive,
            Roles = matchedUser.UserRoles.Select(ur => new RoleResponseDto
            {
                Id = ur.Role!.Id,
                Name = ur.Role!.Name
            }).ToList()
        };

        return Result<UserResponseDto>.Ok(mappedUser);
    }

    public async Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUsers = await _userRepository.GetAllAsync(
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? u => u.NormalizedEmail.Contains(request.Search.NormalizeValue())
                : null,
            orderBy: q => q.OrderBy(u => u.NormalizedEmail),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var options = _mapper.Map<List<OptionResponseDto<Guid>>>(matchedUsers);

        return Result<ListResultDto<OptionResponseDto<Guid>>>.Ok(new ListResultDto<OptionResponseDto<Guid>>(options));
    }

    public async Task<Result<PagedResult<UserResponseDto>>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = _userRepository.AsQueryable();

        queryable = queryable.WhereIf(request.IsActive.HasValue, u => u.IsActive == request.IsActive!.Value);
        queryable = queryable.WhereIf(
            !string.IsNullOrWhiteSpace(request.Search),
            u => u.NormalizedEmail.Contains(request.Search!.NormalizeValue())
        );

        queryable = queryable.AsNoTracking();
        queryable = queryable.ApplySort(request.GetSortRequest(nameof(CreationAuditedEntity.CreationTime)));
        var pagedUsers = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);

        var mappedUsers = _mapper.Map<List<UserResponseDto>>(pagedUsers.Data);

        return Result<PagedResult<UserResponseDto>>.Ok(
            new PagedResult<UserResponseDto>(mappedUsers, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PerPage));
    }

    public async Task CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken: cancellationToken);
        if (existingUser)
        {
            throw new AppConflictException(_localizer["UserAppService:CreateAsync:Exists", request.Email]);
        }

        var passwordValidatorResult = _passwordValidator.Validate(request.Password);
        if (!passwordValidatorResult.Succeeded)
        {
            throw new AppValidationException(passwordValidatorResult.Errors);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            NormalizedEmail = request.Email.NormalizeValue(),
            EmailConfirmed = request.EmailConfirmed,
            PhoneNumber = request.PhoneNumber,
            PhoneNumberConfirmed = request.PhoneNumberConfirmed,
            TwoFactorEnabled = request.TwoFactorEnabled,
            LockoutEnd = null,
            AccessFailedCount = 0,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive
        };
        
        newUser.PasswordHash = _passwordHasher.HashPassword(newUser, request.Password);
        newUser.LockoutEnabled = UserConsts.AllowedForNewUsers;
        
        await _userRepository.AddAsync(newUser, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        matchedUser.PhoneNumber = request.PhoneNumber;
        matchedUser.FirstName = request.FirstName;
        matchedUser.LastName = request.LastName;
        matchedUser.IsActive = request.IsActive;
        matchedUser.EmailConfirmed = request.EmailConfirmed;
        matchedUser.PhoneNumberConfirmed = request.PhoneNumberConfirmed;
        matchedUser.TwoFactorEnabled = request.TwoFactorEnabled;

        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _userRepository.DeleteAsync(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AddToRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        
        var existingUserRole = await _userRoleRepository.AnyAsync(
            predicate: ur => 
                ur.UserId == matchedUser.Id && 
                ur.RoleId == matchedRole.Id,
            cancellationToken: cancellationToken
        );
        
        if (existingUserRole)
        {
            throw new AppConflictException(_localizer["UserAppService:AddToRoleAsync:Exists"]);
        }
        
        var newUserRole = new UserRole
        {
            UserId = matchedUser.Id,
            RoleId = matchedRole.Id
        };
        
        await _userRoleRepository.AddAsync(newUserRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromRoleAsync(Guid id, Guid roleId, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );

        var matchedRole = await _roleRepository.GetAsync(
            predicate: r => r.Id == roleId,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var matchedUserRole = await _userRoleRepository.GetAsync(
            predicate: ur =>
                ur.RoleId == matchedRole.Id &&
                ur.UserId == matchedUser.Id,
            cancellationToken: cancellationToken
        );
        
        if (matchedUserRole == null)
        {
            throw new AppConflictException(_localizer["UserAppService:RemoveFromRoleAsync:NotFound"]);
        }
        
        await _userRoleRepository.DeleteAsync(matchedUserRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task SyncRolesAsync(Guid id, SyncUserRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var matchedUser = await _userRepository.GetAsync(
                predicate: u => u.Id == id,
                include: q => q.Include(u => u.UserRoles),
                enableTracking: true,
                cancellationToken: cancellationToken
            );

            var currentRoleIds = matchedUser.UserRoles.Select(ur => ur.RoleId).ToList();
            
            var rolesToAdd = request.RoleIds.Except(currentRoleIds).ToList();
            var rolesToRemove = currentRoleIds.Except(request.RoleIds).ToList();

            if (rolesToAdd.Any())
            {
                var existingRoles = await _roleRepository.GetAllAsync(
                    predicate: r => rolesToAdd.Contains(r.Id),
                    enableTracking: false,
                    cancellationToken: cancellationToken
                );

                if (existingRoles.Count != rolesToAdd.Count)
                {
                    throw new AppEntityNotFoundException(_localizer["UserAppService:SyncRolesAsync:MissingRoles"]);
                }

                var newUserRoles = rolesToAdd.Select(roleId => new UserRole
                {
                    UserId = matchedUser.Id,
                    RoleId = roleId
                }).ToList();

                await _userRoleRepository.AddRangeAsync(newUserRoles, cancellationToken: cancellationToken);
            }

            if (rolesToRemove.Any())
            {
                var userRolesToRemove = matchedUser.UserRoles
                    .Where(ur => rolesToRemove.Contains(ur.RoleId))
                    .ToList();

                await _userRoleRepository.DeleteRangeAsync(userRolesToRemove, cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.EmailConfirmed = !matchedUser.EmailConfirmed;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.PhoneNumberConfirmed = !matchedUser.PhoneNumberConfirmed;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.TwoFactorEnabled = !matchedUser.TwoFactorEnabled;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.IsActive = !matchedUser.IsActive;
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task LockAsync(Guid id, DateTimeOffset? lockoutEnd = null, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = lockoutEnd ?? DateTimeOffset.UtcNow.Add(UserConsts.DefaultLockoutTimeSpanMinutes);
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        _backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
            new InvalidateAllSessionsBackgroundJobArgs
            {
                UserId = matchedUser.Id,
                Reason = "User locked out by admin",
                CorrelationId = _httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid()
            }
        );
    }

    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
        matchedUser.LockoutEnd = null;
        matchedUser.AccessFailedCount = 0;
        
        await _userRepository.UpdateAsync(matchedUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await _userRepository.GetAsync(
            predicate: u => u.Id == id,
            enableTracking: true,
            cancellationToken: cancellationToken
        );
        
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
                Reason = "Password reset by admin",
                CorrelationId = _httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid()
            }
        );
    }
}
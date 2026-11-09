using AutoMapper;
using Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Users;

public class UserAppService : IUserAppService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IMapper _mapper;
    private readonly IBackgroundJobExecutor _backgroundJobExecutor;
    private readonly IStringLocalizer<UserAppService> _localizer;

    public UserAppService(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUnitOfWork unitOfWork,
        ILookupNormalizer lookupNormalizer,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper,
        IBackgroundJobExecutor backgroundJobExecutor,
        IStringLocalizer<UserAppService> localizer)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _lookupNormalizer = lookupNormalizer;
        _httpContextAccessor = httpContextAccessor;
        _mapper = mapper;
        _backgroundJobExecutor = backgroundJobExecutor;
        _localizer = localizer;
    }

    public async Task<Result<UserResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUsers(enableTracking: false)
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (matchedUser == null)
        {
            throw new AppEntityNotFoundException(typeof(User));
        }

        var mappedUser = new UserResponseDto
        {
            Id = matchedUser.Id,
            Email = matchedUser.Email!,
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
                Name = ur.Role!.Name!
            }).ToList()
        };

        return Result<UserResponseDto>.Ok(mappedUser);
    }

    public async Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = GetActiveUsers(enableTracking: false);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = _lookupNormalizer.NormalizeEmail(request.Search)!;
            queryable = queryable.Where(u => u.NormalizedEmail!.Contains(normalizedSearch));
        }

        var matchedUsers = await queryable
            .OrderBy(u => u.NormalizedEmail)
            .ToListAsync(cancellationToken);

        var options = _mapper.Map<List<OptionResponseDto<Guid>>>(matchedUsers);

        return Result<ListResultDto<OptionResponseDto<Guid>>>.Ok(new ListResultDto<OptionResponseDto<Guid>>(options));
    }

    public async Task<Result<PagedResult<UserResponseDto>>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = GetActiveUsers(enableTracking: false);

        queryable = queryable.WhereIf(request.IsActive.HasValue, u => u.IsActive == request.IsActive!.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = _lookupNormalizer.NormalizeEmail(request.Search)!;
            queryable = queryable.Where(u => u.NormalizedEmail!.Contains(normalizedSearch));
        }

        queryable = queryable.ApplySort(request.GetSortRequest(nameof(CreationAuditedEntity.CreationTime)));
        var pagedUsers = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);

        var mappedUsers = _mapper.Map<List<UserResponseDto>>(pagedUsers.Data);

        return Result<PagedResult<UserResponseDto>>.Ok(
            new PagedResult<UserResponseDto>(mappedUsers, pagedUsers.TotalCount, pagedUsers.Page, pagedUsers.PerPage));
    }

    public async Task CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new AppConflictException(_localizer["UserAppService:CreateAsync:Exists", request.Email]);
        }

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            EmailConfirmed = request.EmailConfirmed,
            PhoneNumber = request.PhoneNumber,
            PhoneNumberConfirmed = request.PhoneNumberConfirmed,
            TwoFactorEnabled = request.TwoFactorEnabled,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive,
            ShouldChangePasswordOnNextLogin = true
        };

        var result = await _userManager.CreateAsync(newUser, request.Password);
        result.ThrowIfFailed(_localizer["UserAppService:CreateAsync:Exists", request.Email]);
    }

    public async Task UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        matchedUser.PhoneNumber = request.PhoneNumber;
        matchedUser.FirstName = request.FirstName;
        matchedUser.LastName = request.LastName;
        matchedUser.IsActive = request.IsActive;

        (await _userManager.UpdateAsync(matchedUser)).ThrowIfFailed();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUsers(enableTracking: true)
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (matchedUser == null)
        {
            throw new AppEntityNotFoundException(typeof(User), id);
        }

        matchedUser.IsDeleted = true;
        matchedUser.DeletionTime = DateTime.UtcNow;

        (await _userManager.UpdateAsync(matchedUser)).ThrowIfFailed();
    }

    public async Task SyncRolesAsync(Guid id, SyncUserRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var matchedUser = await GetActiveUserAsync(id, cancellationToken);

            var currentRoles = await _roleManager.Roles
                .Where(r => !r.IsDeleted && r.UserRoles.Any(ur => ur.UserId == matchedUser.Id))
                .ToListAsync(cancellationToken);
            var currentRoleIds = currentRoles.Select(r => r.Id).ToList();

            var roleIdsToAdd = request.RoleIds.Except(currentRoleIds).ToList();
            var roleIdsToRemove = currentRoleIds.Except(request.RoleIds).ToList();

            if (roleIdsToAdd.Any())
            {
                var rolesToAdd = await _roleManager.Roles
                    .Where(r => !r.IsDeleted && roleIdsToAdd.Contains(r.Id))
                    .ToListAsync(cancellationToken);

                if (rolesToAdd.Count != roleIdsToAdd.Count)
                {
                    throw new AppEntityNotFoundException(_localizer["UserAppService:SyncRolesAsync:MissingRoles"]);
                }

                (await _userManager.AddToRolesAsync(matchedUser, rolesToAdd.Select(r => r.Name!))).ThrowIfFailed();
            }

            if (roleIdsToRemove.Any())
            {
                var roleNamesToRemove = currentRoles
                    .Where(r => roleIdsToRemove.Contains(r.Id))
                    .Select(r => r.Name!);

                (await _userManager.RemoveFromRolesAsync(matchedUser, roleNamesToRemove)).ThrowIfFailed();
            }

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        matchedUser.EmailConfirmed = !matchedUser.EmailConfirmed;
        (await _userManager.UpdateAsync(matchedUser)).ThrowIfFailed();
    }

    public async Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        matchedUser.PhoneNumberConfirmed = !matchedUser.PhoneNumberConfirmed;
        (await _userManager.UpdateAsync(matchedUser)).ThrowIfFailed();
    }

    public async Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        (await _userManager.SetTwoFactorEnabledAsync(matchedUser, !matchedUser.TwoFactorEnabled)).ThrowIfFailed();
    }

    public async Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        matchedUser.IsActive = !matchedUser.IsActive;
        (await _userManager.UpdateAsync(matchedUser)).ThrowIfFailed();
    }

    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        // Nothing can be locked when lockout is disabled for the user, so there is nothing to clear.
        if (!matchedUser.LockoutEnabled)
        {
            return;
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            (await _userManager.SetLockoutEndDateAsync(matchedUser, null)).ThrowIfFailed();
            (await _userManager.ResetAccessFailedCountAsync(matchedUser)).ThrowIfFailed();

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedUser = await GetActiveUserAsync(id, cancellationToken);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            matchedUser.PasswordChangedTime = DateTime.UtcNow;
            matchedUser.ShouldChangePasswordOnNextLogin = true;

            (await _userManager.RemovePasswordAsync(matchedUser)).ThrowIfFailed();
            (await _userManager.AddPasswordAsync(matchedUser, request.NewPassword)).ThrowIfFailed();

            await transaction.CommitAsync(cancellationToken);

            _backgroundJobExecutor.Enqueue<InvalidateAllSessionsBackgroundJob, InvalidateAllSessionsBackgroundJobArgs>(
                new InvalidateAllSessionsBackgroundJobArgs
                {
                    UserId = matchedUser.Id,
                    Reason = "Password reset by admin",
                    CorrelationId = _httpContextAccessor.HttpContext?.GetCorrelationId() ?? Guid.NewGuid()
                }
            );
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>Users that are not soft-deleted, read through Identity's UserManager.</summary>
    private IQueryable<User> GetActiveUsers(bool enableTracking)
    {
        var queryable = _userManager.Users.Where(u => !u.IsDeleted);
        return enableTracking ? queryable : queryable.AsNoTracking();
    }

    private async Task<User> GetActiveUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var matchedUser = await GetActiveUsers(enableTracking: true)
            .SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
        if (matchedUser == null)
        {
            throw new AppEntityNotFoundException(typeof(User));
        }

        return matchedUser;
    }
}

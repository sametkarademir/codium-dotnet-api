using AutoMapper;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Roles;

public class RoleAppService : IRoleAppService
{
    private readonly IRolePermissionRepository _rolePermissionRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly RoleManager<Role> _roleManager;
    private readonly ILookupNormalizer _lookupNormalizer;
    private readonly IStringLocalizer<ApplicationResource> _localizer;


    public RoleAppService(
        IRolePermissionRepository rolePermissionRepository,
        IPermissionRepository permissionRepository, 
        IUnitOfWork unitOfWork,
        IMapper mapper,
        RoleManager<Role> roleManager,
        ILookupNormalizer lookupNormalizer,
        IStringLocalizer<ApplicationResource> localizer)
    {
        _rolePermissionRepository = rolePermissionRepository;
        _permissionRepository = permissionRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _roleManager = roleManager;
        _lookupNormalizer = lookupNormalizer;
        _localizer = localizer;
    }

    public async Task<Result<RoleResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedRole = await GetActiveRoles(enableTracking: false)
            .Include(r => r.RolePermissions)
            .ThenInclude(rp => rp.Permission)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (matchedRole == null)
        {
            throw new AppEntityNotFoundException(typeof(Role));
        }

        var mappedRole = new RoleResponseDto
        {
            Id = matchedRole.Id,
            Name = matchedRole.Name!,
            Description = matchedRole.Description,
            Permissions = matchedRole.RolePermissions.Select(rp => new PermissionResponseDto
            {
                Id = rp.Permission!.Id,
                Name = rp.Permission!.Name
            }).ToList()
        };

        return Result<RoleResponseDto>.Ok(mappedRole);
    }

    public async Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = GetActiveRoles(enableTracking: false);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = _lookupNormalizer.NormalizeName(request.Search)!;
            queryable = queryable.Where(r => r.NormalizedName!.Contains(normalizedSearch));
        }

        var matchedRoles = await queryable
            .OrderBy(r => r.NormalizedName)
            .ToListAsync(cancellationToken);

        var options = _mapper.Map<List<OptionResponseDto<Guid>>>(matchedRoles);

        return Result<ListResultDto<OptionResponseDto<Guid>>>.Ok(new ListResultDto<OptionResponseDto<Guid>>(options));
    }

    public async Task<Result<PagedResult<RoleResponseDto>>> GetPageableAndFilterAsync(GetListRolesRequestDto request, CancellationToken cancellationToken = default)
    {
        var queryable = GetActiveRoles(enableTracking: false);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var normalizedSearch = _lookupNormalizer.NormalizeName(request.Search)!;
            queryable = queryable.Where(r => r.NormalizedName!.Contains(normalizedSearch));
        }

        queryable = queryable.ApplySort(request.GetSortRequest(nameof(CreationAuditedEntity.CreationTime)));
        var pagedRoles = await queryable.ToPageableAsync(request.Page, request.PerPage, cancellationToken);

        var mappedRoles = _mapper.Map<List<RoleResponseDto>>(pagedRoles.Data);

        return Result<PagedResult<RoleResponseDto>>.Ok(
            new PagedResult<RoleResponseDto>(mappedRoles, pagedRoles.TotalCount, pagedRoles.Page, pagedRoles.PerPage));
    }

    public async Task CreateAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var newRole = new Role
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await _roleManager.CreateAsync(newRole);
        result.ThrowIfFailed(_localizer["RoleAppService:CreateAsync:Exists", request.Name]);
    }

    public async Task UpdateAsync(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedRole = await GetActiveRoles(enableTracking: true)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (matchedRole == null)
        {
            throw new AppEntityNotFoundException(typeof(Role));
        }

        matchedRole.Name = request.Name;
        matchedRole.Description = request.Description;

        var result = await _roleManager.UpdateAsync(matchedRole);
        result.ThrowIfFailed(_localizer["RoleAppService:UpdateAsync:Exists", request.Name]);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var matchedRole = await GetActiveRoles(enableTracking: true)
            .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
        if (matchedRole == null)
        {
            throw new AppEntityNotFoundException(typeof(Role), id);
        }

        matchedRole.IsDeleted = true;
        matchedRole.DeletionTime = DateTime.UtcNow;

        var result = await _roleManager.UpdateAsync(matchedRole);
        result.ThrowIfFailed(_localizer["RoleAppService:UpdateAsync:Exists", matchedRole.Name!]);
    }

    public async Task SyncPermissionsAsync(Guid id, SyncRolePermissionsRequestDto request, CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var matchedRole = await GetActiveRoles(enableTracking: true)
                .Include(r => r.RolePermissions)
                .SingleOrDefaultAsync(r => r.Id == id, cancellationToken);
            if (matchedRole == null)
            {
                throw new AppEntityNotFoundException(typeof(Role));
            }
            
            var currentPermissionIds = matchedRole.RolePermissions.Select(rp => rp.PermissionId).ToList();
       
            var permissionsToAdd = request.PermissionIds.Except(currentPermissionIds).ToList();
            var permissionsToRemove = currentPermissionIds.Except(request.PermissionIds).ToList();
          
            if (permissionsToAdd.Any())
            {
                var existingPermissions = await _permissionRepository.GetAllAsync(
                    predicate: p => permissionsToAdd.Contains(p.Id),
                    enableTracking: false,
                    cancellationToken: cancellationToken
                );

                if (existingPermissions.Count != permissionsToAdd.Count)
                {
                    throw new AppEntityNotFoundException(_localizer["RoleAppService:SyncPermissionsAsync:MissingPermissions"]);
                }

           
                var newRolePermissions = permissionsToAdd.Select(permissionId => new RolePermission
                {
                    RoleId = matchedRole.Id,
                    PermissionId = permissionId
                }).ToList();

                await _rolePermissionRepository.AddRangeAsync(newRolePermissions, cancellationToken: cancellationToken);
            }

          
            if (permissionsToRemove.Any())
            {
                var rolePermissionsToRemove = matchedRole.RolePermissions
                    .Where(rp => permissionsToRemove.Contains(rp.PermissionId))
                    .ToList();

                await _rolePermissionRepository.DeleteRangeAsync(rolePermissionsToRemove, cancellationToken);
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

    private IQueryable<Role> GetActiveRoles(bool enableTracking)
    {
        var queryable = _roleManager.Roles.Where(r => !r.IsDeleted);
        return enableTracking ? queryable : queryable.AsNoTracking();
    }
}
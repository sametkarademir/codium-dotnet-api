using AutoMapper;
using Codium.Template.Application.Contracts.Common;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Permissions;

public class PermissionAppService(
    IPermissionRepository permissionRepository,
    IMapper mapper)
    : IPermissionAppService
{
    public async Task<Result<PagedResult<PermissionResponseDto>>> GetPageableAndFilterAsync(GetListPermissionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var pagedPermissions = await permissionRepository.GetListSortedAsync(
            page: request.Page,
            perPage: request.PerPage,
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? p => p.NormalizedName.Contains(request.Search.NormalizeValue())
                : null,
            sort: request.GetSortRequest(nameof(CreationAuditedEntity.CreationTime)),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var mappedPermissions = mapper.Map<List<PermissionResponseDto>>(pagedPermissions.Data);

        return Result<PagedResult<PermissionResponseDto>>.Ok(
            new PagedResult<PermissionResponseDto>(mappedPermissions, pagedPermissions.TotalCount, pagedPermissions.Page, pagedPermissions.PerPage));
    }

    public async Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default)
    {
        var matchedPermissions = await permissionRepository.GetAllAsync(
            predicate: !string.IsNullOrWhiteSpace(request.Search)
                ? p => p.NormalizedName.Contains(request.Search.NormalizeValue())
                : null,
            orderBy: q => q.OrderBy(p => p.NormalizedName),
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        var options = mapper.Map<List<OptionResponseDto<Guid>>>(matchedPermissions);

        return Result<ListResultDto<OptionResponseDto<Guid>>>.Ok(new ListResultDto<OptionResponseDto<Guid>>(options));
    }
}

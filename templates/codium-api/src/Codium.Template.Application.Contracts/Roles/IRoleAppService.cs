using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Contracts.Roles;

public interface IRoleAppService
{
    Task<Result<RoleResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<RoleResponseDto>>> GetPageableAndFilterAsync(GetListRolesRequestDto request, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateRoleRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task SyncPermissionsAsync(Guid id, SyncRolePermissionsRequestDto request, CancellationToken cancellationToken = default);
}
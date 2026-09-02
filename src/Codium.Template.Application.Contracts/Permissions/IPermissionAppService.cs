using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Contracts.Permissions;

public interface IPermissionAppService
{
    Task<Result<PagedResult<PermissionResponseDto>>> GetPageableAndFilterAsync(GetListPermissionsRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default);
}
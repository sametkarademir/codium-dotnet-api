using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Contracts.Users;

public interface IUserAppService
{
    Task<Result<UserResponseDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<ListResultDto<OptionResponseDto<Guid>>>> GetAllAsOptionsAsync(GetOptionsRequestDto request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<UserResponseDto>>> GetPageableAndFilterAsync(GetListUsersRequestDto request, CancellationToken cancellationToken = default);
    Task CreateAsync(CreateUserRequestDto request, CancellationToken cancellationToken = default);
    Task UpdateAsync(Guid id, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task SyncRolesAsync(Guid id, SyncUserRolesRequestDto request, CancellationToken cancellationToken = default);

    Task ToggleEmailConfirmationAsync(Guid id, CancellationToken cancellationToken = default);
    Task TogglePhoneNumberConfirmationAsync(Guid id, CancellationToken cancellationToken = default);
    Task ToggleTwoFactorEnabledAsync(Guid id, CancellationToken cancellationToken = default);
    Task ToggleIsActiveAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task LockAsync(Guid id, DateTimeOffset? lockoutEnd = null, CancellationToken cancellationToken = default);
    Task UnlockAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task ResetPasswordAsync(Guid id, ResetPasswordUserRequestDto request, CancellationToken cancellationToken = default);
}
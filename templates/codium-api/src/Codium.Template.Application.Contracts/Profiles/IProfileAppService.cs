using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Contracts.Profiles;

public interface IProfileAppService
{
    Task<Result<ProfileResponseDto>> GetProfileAsync(CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(ChangePasswordUserRequestDto request, CancellationToken cancellationToken = default);
}
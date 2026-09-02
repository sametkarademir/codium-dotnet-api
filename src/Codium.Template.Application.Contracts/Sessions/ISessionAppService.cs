using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Domain.Shared.Result;

namespace Codium.Template.Application.Contracts.Sessions;

public interface ISessionAppService
{
    Task<Result<PagedResult<SessionResponseDto>>> GetPageableAndFilterAsync(GetListSessionsRequestDto request, CancellationToken cancellationToken = default);
    Task InvalidateSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
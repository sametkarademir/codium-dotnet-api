using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
    Task RevokeRefreshTokensBySessionAsync(
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    
    Task RevokeRefreshTokensBySessionsAsync(
        List<Guid> sessionIds,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    
    Task<RefreshToken?> ValidateAndUseRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default
    );
}
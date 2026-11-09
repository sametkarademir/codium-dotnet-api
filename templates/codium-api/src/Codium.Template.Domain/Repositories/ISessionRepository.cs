using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface ISessionRepository : IRepository<Session, Guid>
{
    Task<List<Guid>> GetExcessSessionIdsAsync(
        Guid userId,
        int skipCount = 5,
        CancellationToken cancellationToken = default
    );
    
    Task<Session?> RevokeSessionByUserAsync(
        Guid sessionId,
        Guid userId,
        CancellationToken cancellationToken = default
    );
    
    Task RevokeSessionsByUserAsync(
        List<Guid> sessionIds,
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
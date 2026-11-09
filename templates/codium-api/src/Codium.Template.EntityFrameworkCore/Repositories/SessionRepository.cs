using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Sessions;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class SessionRepository(ApplicationDbContext context)
    : EfRepositoryBase<Session, Guid, ApplicationDbContext>(context), ISessionRepository
{
    public async Task<List<Guid>> GetExcessSessionIdsAsync(
        Guid userId,
        int skipCount = 5,
        CancellationToken cancellationToken = default
    )
    {
        return await AsQueryable()
            .AsNoTracking()
            .Where(s => s.UserId == userId && !s.IsRevoked)
            .OrderByDescending(s => s.CreationTime)
            .Skip(skipCount)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<Session?> RevokeSessionByUserAsync(Guid sessionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var matchedSession = await SingleOrDefaultAsync(
            predicate: s => 
                s.Id == sessionId && 
                s.UserId == userId &&
                !s.IsRevoked,
            cancellationToken: cancellationToken
        );
        
        if (matchedSession == null)
        {
            return null;
        }

        matchedSession.IsRevoked = true;
        matchedSession.RevokedTime = DateTime.UtcNow;

        await UpdateAsync(matchedSession, cancellationToken);
        
        return matchedSession;
    }

    public async Task RevokeSessionsByUserAsync(List<Guid> sessionIds, Guid userId, CancellationToken cancellationToken = default)
    {
        var matchedSessions = await GetAllAsync(
            predicate: s => 
                sessionIds.Contains(s.Id) &&
                s.UserId == userId &&
                !s.IsRevoked,
            cancellationToken: cancellationToken
        );
        
        if (matchedSessions.Count == 0)
        {
            return;
        }
        
        var updatedSessions = matchedSessions.Select(s =>
        {
            s.IsRevoked = true;
            s.RevokedTime = DateTime.UtcNow;
            return s;
        }).ToList();
        
        await UpdateRangeAsync(updatedSessions, cancellationToken);
    }
}
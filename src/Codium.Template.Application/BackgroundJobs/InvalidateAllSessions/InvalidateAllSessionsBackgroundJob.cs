using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Repositories;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.BackgroundJobs.InvalidateAllSessions;

public class InvalidateAllSessionsBackgroundJob(
    ISessionRepository sessionRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IUnitOfWork unitOfWork,
    ILogger<InvalidateAllSessionsBackgroundJob> logger) 
    : IBackgroundJob<InvalidateAllSessionsBackgroundJobArgs>
{
    public async Task Execute(InvalidateAllSessionsBackgroundJobArgs args, IJobCancellationToken cancellationToken)
    {
        var logDetail = new Dictionary<string, object>
        {
            { "Service", nameof(InvalidateAllSessionsBackgroundJob) },
            { "ServiceMethod", nameof(Execute) },
            { "JobId", args.JobId },
            { "ScheduledAt", args.ScheduledAt },
            { "CorrelationId", args.CorrelationId },
            { "UserId", args.UserId },
            { "Reason", args.Reason ?? "Not specified" }
        };
        
        logger
            .WithProperties()
            .AddRange(logDetail)
            .LogInformation("Starting InvalidateAllSessionsBackgroundJob");

        try
        {
            // Step 1: Get all active sessions for the user
            var matchedSessions = await sessionRepository.GetAllAsync(
                predicate: s => 
                    s.UserId == args.UserId &&
                    s.IsRevoked == false,
                cancellationToken: cancellationToken.ShutdownToken
            );

            if (matchedSessions.Count == 0)
            {
                logger
                    .WithProperties()
                    .AddRange(logDetail)
                    .LogInformation("No active sessions found. Job completed.");
                
                return;
            }

            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation($"Found {matchedSessions.Count} active sessions");

            // Step 2: Revoke all sessions
            var updatedSessions = matchedSessions.Select(s =>
            {
                s.IsRevoked = true;
                s.RevokedTime = DateTime.UtcNow;
                
                return s;
            }).ToList();

            await sessionRepository.UpdateRangeAsync(updatedSessions, cancellationToken.ShutdownToken);
            await unitOfWork.SaveChangesAsync(cancellationToken.ShutdownToken);

            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation($"Successfully revoked {updatedSessions.Count} sessions");

            // Step 3: Get all active refresh tokens for the user
            var matchedRefreshTokens = await refreshTokenRepository.GetAllAsync(
                predicate: rt => 
                    rt.UserId == args.UserId &&
                    rt.IsRevoked == false &&
                    rt.IsUsed == false,
                enableTracking: true,
                cancellationToken: cancellationToken.ShutdownToken
            );

            if (matchedRefreshTokens.Count == 0)
            {
                logger
                    .WithProperties()
                    .AddRange(logDetail)
                    .LogInformation("No active refresh tokens found. Job completed.");
                
                return;
            }

            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation($"Found {matchedRefreshTokens.Count} active refresh tokens");

            // Step 4: Revoke all refresh tokens
            var updatedRefreshTokens = matchedRefreshTokens.Select(rt =>
            {
                rt.IsRevoked = true;
                rt.RevokedTime = DateTime.UtcNow;
                return rt;
            }).ToList();

            await refreshTokenRepository.UpdateRangeAsync(updatedRefreshTokens, cancellationToken.ShutdownToken);
            await unitOfWork.SaveChangesAsync(cancellationToken.ShutdownToken);

            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation($"Successfully revoked {updatedRefreshTokens.Count} refresh tokens");

            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogInformation("InvalidateAllSessionsBackgroundJob completed successfully");
        }
        catch (Exception ex)
        {
            logger
                .WithProperties()
                .AddRange(logDetail)
                .LogError("An error occurred while invalidating sessions and refresh tokens.", ex);
            
            throw;
        }
    }
}


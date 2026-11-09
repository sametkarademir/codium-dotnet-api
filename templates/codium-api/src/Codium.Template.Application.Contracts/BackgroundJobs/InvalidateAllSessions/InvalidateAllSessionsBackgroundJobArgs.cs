namespace Codium.Template.Application.Contracts.BackgroundJobs.InvalidateAllSessions;

public class InvalidateAllSessionsBackgroundJobArgs : BackgroundJobArgs
{
    public Guid UserId { get; set; }
    public string? Reason { get; set; }

    public Guid CorrelationId { get; set; }
}

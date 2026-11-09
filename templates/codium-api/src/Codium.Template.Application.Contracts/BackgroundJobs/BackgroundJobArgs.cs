namespace Codium.Template.Application.Contracts.BackgroundJobs;

public abstract class BackgroundJobArgs
{
    public Guid JobId { get; } = Guid.NewGuid();
    public DateTime ScheduledAt { get; } = DateTime.UtcNow;
}

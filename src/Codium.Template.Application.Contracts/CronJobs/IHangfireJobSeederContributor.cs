namespace Codium.Template.Application.Contracts.CronJobs;

public interface IHangfireJobSeederContributor
{
    Task SeedAsync();
}
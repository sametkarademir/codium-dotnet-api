using Codium.Template.Application.Contracts.CronJobs;
using Microsoft.Extensions.Logging;

namespace Codium.Template.Application.CronJobs;

public class HangfireJobSeederContributor(
    IEnumerable<IHangfireJobModule> modules,
    ILogger<HangfireJobSeederContributor> logger)
    : IHangfireJobSeederContributor
{
    public Task SeedAsync()
    {
        logger.LogInformation("Starting Hangfire job seeding...");
        
        logger.LogInformation("Found {Count} job modules to configure", modules.Count());

        foreach (var module in modules)
        {
            try
            {
                var moduleName = module.GetType().Name;
                logger.LogInformation("Configuring module: {ModuleName}", moduleName);
                
                module.ConfigureJobs();
                
                logger.LogInformation("Module {ModuleName} configured successfully", moduleName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error configuring module {ModuleName}", module.GetType().Name);
                throw;
            }
        }

        logger.LogInformation("Hangfire job seeding completed successfully");
        
        return Task.CompletedTask;
    }
}
using Codium.Template.Application.Contracts.CronJobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Codium.Template.Application;

public class ApplicationSeedInitializer(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        
        var hangfireSeeder = scope.ServiceProvider.GetRequiredService<IHangfireJobSeederContributor>();
        await hangfireSeeder.SeedAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}
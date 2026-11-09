using Codium.Template.Domain;
using Codium.Template.EntityFrameworkCore.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Codium.Template.EntityFrameworkCore;

public class DbMigrationInitializer(
    ApplicationDbContext context,
    DevelopmentDataSeederContributor seederContributor,
    ILogger<DbMigrationInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Migrating database migrations...");

        var canConnect = await context.Database.CanConnectAsync(cancellationToken);
        if (!canConnect)
        {
            logger.LogInformation("Database does not exist, applying migrations...");
            await context.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Database migrations applied successfully.");
        }
        else
        {
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Pending migrations found, applying...");
                await context.Database.MigrateAsync(cancellationToken);
                logger.LogInformation("Database migrations applied successfully.");
            }
        }

        logger.LogInformation("Database migrations succeeded.");

        await seederContributor.SeedAsync();
    }
}
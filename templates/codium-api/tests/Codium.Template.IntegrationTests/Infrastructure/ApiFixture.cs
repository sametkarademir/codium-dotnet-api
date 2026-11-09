using Codium.Template.EntityFrameworkCore.Contexts;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Codium.Template.IntegrationTests.Infrastructure;

/// <summary>
/// Starts a throwaway PostgreSQL container, creates the schema with EnsureCreated (the template ships no
/// migrations) and hosts the API against it. Shared by all test classes through <see cref="ApiCollection"/>.
/// </summary>
public class ApiFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("integration_tests")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    private WebApplicationFactory<Program>? _factory;

    public HttpClient CreateClient() => _factory!.CreateClient();

    public IServiceProvider Services => _factory!.Services;

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        var connectionString = _postgres.GetConnectionString();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        await using (var context = new ApplicationDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();
        }

        // Environment variables are read by the host builder before any service registration.
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", connectionString);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("Hangfire__CronJobsEnabled", "false");
        Environment.SetEnvironmentVariable("Serilog__Console__MinimumLevel", "Warning");
        Environment.SetEnvironmentVariable("RateLimiting__GlobalPolicy__PermitLimit", "1000000");
        Environment.SetEnvironmentVariable("RateLimiting__ApiPolicy__PermitLimit", "1000000");
        Environment.SetEnvironmentVariable("RateLimiting__AuthPolicy__PermitLimit", "1000000");

        _factory = new WebApplicationFactory<Program>();
        _ = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }

        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(Name)]
public class ApiCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "Api";
}

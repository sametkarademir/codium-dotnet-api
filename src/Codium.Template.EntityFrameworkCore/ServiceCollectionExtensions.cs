using Codium.Template.Domain;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Extensions;
using Codium.Template.EntityFrameworkCore.Repositories;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Codium.Template.EntityFrameworkCore;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkCoreService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<NpgsqlDataSource>(opt =>
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(configuration.GetConnectionString("Default"));
            dataSourceBuilder.EnableDynamicJson();
            var dataSource = dataSourceBuilder.Build();
            
            return dataSource;
        });

        services.AddDbContext<ApplicationDbContext>((serviceProvider, opt) =>
        {
            var dataSource = serviceProvider.GetRequiredService<NpgsqlDataSource>();
            opt.UseNpgsql(dataSource, builder =>
            {
                builder.CommandTimeout(30);
            });
            opt.UseEntityMetadataTracking();
            opt.UseAuditLog();
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IEntityPropertyChangeRepository, EntityPropertyChangeRepository>();
        services.AddScoped<IHttpRequestLogRepository, HttpRequestLogRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IRolePermissionRepository, RolePermissionRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRoleRepository, UserRoleRepository>();

        services.AddScoped<DevelopmentDataSeederContributor>();
        services.AddScoped<DbMigrationInitializer>();

        return services;
    }
}
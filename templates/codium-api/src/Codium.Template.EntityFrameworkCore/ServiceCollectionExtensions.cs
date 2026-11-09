using Codium.Template.Domain;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Extensions;
using Codium.Template.EntityFrameworkCore.Repositories;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.AspNetCore.Identity;
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

        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequiredLength = UserConsts.PasswordRequiredLength;
                options.Password.RequiredUniqueChars = UserConsts.PasswordRequiredUniqueChars;
                options.Password.RequireDigit = UserConsts.PasswordRequireDigit;
                options.Password.RequireLowercase = UserConsts.PasswordRequireLowercase;
                options.Password.RequireUppercase = UserConsts.PasswordRequireUppercase;
                options.Password.RequireNonAlphanumeric = UserConsts.PasswordRequireNonAlphanumeric;

                options.Lockout.AllowedForNewUsers = UserConsts.AllowedForNewUsers;
                options.Lockout.MaxFailedAccessAttempts = UserConsts.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = UserConsts.DefaultLockoutTimeSpanMinutes;

                options.SignIn.RequireConfirmedEmail = UserConsts.RequireConfirmedEmail;
                options.SignIn.RequireConfirmedPhoneNumber = UserConsts.RequireConfirmedPhoneNumber;
            })
            .AddRoles<Role>()
            .AddRoleManager<RoleManager<Role>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

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
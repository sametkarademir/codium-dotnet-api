using System.Globalization;
using System.Reflection;
using System.Text;
using Codium.Template.Application.Auth;
using Codium.Template.Application.AuthTokens;
using Codium.Template.Application.BackgroundJobs;
using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.AuthTokens;
using Codium.Template.Application.Contracts.BackgroundJobs;
using Codium.Template.Application.Contracts.CronJobs;
using Codium.Template.Application.Contracts.Permissions;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Roles;
using Codium.Template.Application.Contracts.Sessions;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Application.CronJobs;
using Codium.Template.Application.Permissions;
using Codium.Template.Application.Profiles;
using Codium.Template.Application.Roles;
using Codium.Template.Application.Sessions;
using Codium.Template.Application.Users;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.HttpRequestLogs;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;

namespace Codium.Template.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<HttpRequestLogOptions>(configuration.GetSection(HttpRequestLogOptions.SectionName));

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        var referencedAssemblies = Assembly.GetExecutingAssembly()
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Where(a =>
                a.GetTypes().Any(t =>
                    !t.IsAbstract &&
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IValidator<>))))
            .Distinct()
            .ToList();

        foreach (var assembly in referencedAssemblies)
        {
            services.AddValidatorsFromAssembly(assembly);
        }
        services.AddFluentValidationAutoValidation();
        services.AddFluentValidationClientsideAdapters();

        services.AddJsonLocalization();
        services.AddIdentityConfiguration(configuration);
        services.AddHangfireServiceRegistration(configuration);

        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IPasswordValidator, PasswordValidator>();
        services.AddScoped<IJwtTokenAppService, JwtTokenAppService>();
        services.AddScoped<IAuthAppService, AuthAppService>();
        services.AddScoped<IPermissionAppService, PermissionAppService>();
        services.AddScoped<IProfileAppService, ProfileAppService>();
        services.AddScoped<IRoleAppService, RoleAppService>();
        services.AddScoped<ISessionAppService, SessionAppService>();
        services.AddScoped<IUserAppService, UserAppService>();
        
        services.AddHostedService<ApplicationSeedInitializer>();

        return services;
    }

    private static void AddJsonLocalization(this IServiceCollection services)
    {
        var resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "Localization");
        services.AddSingleton<IStringLocalizerFactory>(new JsonStringLocalizerFactory(resourcesPath, LocalizationConsts.English));
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new List<CultureInfo>
            {
                new(LocalizationConsts.English),
                new(LocalizationConsts.Turkish)
            };

            options.DefaultRequestCulture = new RequestCulture(LocalizationConsts.English);
            options.SupportedCultures = supportedCultures;
            options.SupportedUICultures = supportedCultures;

            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new AcceptLanguageHeaderRequestCultureProvider(),
                new QueryStringRequestCultureProvider()
            };
        });
    }

    private static void AddIdentityConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var claimsPrincipal = context.Principal;
                        var userId = claimsPrincipal?.GetUserId();
                        var sessionId = claimsPrincipal?.FindFirst(CustomClaimTypes.SessionId)?.Value;

                        if (userId == null || userId == Guid.Empty)
                        {
                            context.Fail("User ID could not be validated.");
                            return;
                        }

                        if (sessionId == null || sessionId == Guid.Empty.ToString())
                        {
                            context.Fail("Session ID could not be validated.");
                            return;
                        }
                        
                        var parsedSessionId = Guid.Parse(sessionId);

                        var serviceProvider = context.HttpContext.RequestServices;
                        var sessionRepository = serviceProvider.GetRequiredService<ISessionRepository>();
                        
                        var matchedUserSession = await sessionRepository.FirstOrDefaultAsync(s =>
                            s.UserId == userId &&
                            s.Id == parsedSessionId &&
                            !s.IsRevoked
                        );
                        
                        if (matchedUserSession == null)
                        {
                            context.Fail("User session is not valid.");
                        }

                        // TODO: Opsional: Validate additional session parameters such as IP address or User-Agent
                        // if (matchedUserSession.ClientIp != context.HttpContext.GetClientIpAddress())
                        // {
                        //     context.Fail("Client IP address does not match.");
                        //     return;
                        // }
                        //
                        // matchedUserSession.LastActivityTime = DateTime.UtcNow;
                        // await userSessionRepository.UpdateAsync(matchedUserSession, true);
                    }
                };
            });
    }

    private static void AddHangfireServiceRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(options =>
        {
            options.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(conf =>
                {
                    conf.UseNpgsqlConnection(configuration.GetConnectionString("Default"));
                });
        });
        services.AddHangfireServer(options =>
        {
            options.Queues = ["critical", "reports", "maintenance", "default"];
            options.WorkerCount = 5;
        });

        services.AddScoped<IBackgroundJobExecutor, HangfireJobExecutor>();
        services.AddScoped<IHangfireJobSeederContributor, HangfireJobSeederContributor>();

        //services.AddScoped<IHangfireJobModule, ExampleCronJob>();

        var jobTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(
                t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    t.GetInterfaces()
                        .Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IBackgroundJob<>)
                        )
            );

        foreach (var jobType in jobTypes)
        {
            services.AddScoped(jobType);
        }
    }

}
namespace Codium.Template.HttpApi.Host.Configuration;

public static class CorsServiceCollectionExtensions
{
    /// <summary>
    /// Configures CORS policy with allowed origins from configuration
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<CorsOptions>(configuration.GetSection(CorsOptions.SectionName));
        
        var corsOptions = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
        
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();

                    if (corsOptions.AllowCredentials)
                    {
                        policy.AllowCredentials();
                    }
                }
                else
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                }
            });
        });

        return services;
    }
    
    private class CorsOptions
    {
        public const string SectionName = "Cors";
    
        public string[] AllowedOrigins { get; set; } = [];
        public bool AllowCredentials { get; set; } = true;
    }
}


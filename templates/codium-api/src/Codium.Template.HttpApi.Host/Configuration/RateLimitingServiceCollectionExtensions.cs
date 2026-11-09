using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Codium.Template.HttpApi.Host.Configuration;

public static class RateLimitingServiceCollectionExtensions
{
    /// <summary>
    /// Configures rate limiting with multiple policies (global, api, auth)
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));
        
        var rateLimitOptions = configuration.GetSection(RateLimitingOptions.SectionName).Get<RateLimitingOptions>() ?? new RateLimitingOptions();
        
        if (!rateLimitOptions.Enabled)
        {
            return services;
        }

        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Global policy - applies to all endpoints without specific policy
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateLimitOptions.GlobalPolicy.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.GlobalPolicy.WindowSeconds),
                        QueueLimit = rateLimitOptions.GlobalPolicy.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            // API endpoints policy - for general API calls
            limiterOptions.AddFixedWindowLimiter("api", options =>
            {
                options.PermitLimit = rateLimitOptions.ApiPolicy.PermitLimit;
                options.Window = TimeSpan.FromSeconds(rateLimitOptions.ApiPolicy.WindowSeconds);
                options.QueueLimit = rateLimitOptions.ApiPolicy.QueueLimit;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Auth endpoints policy - stricter limits for authentication
            limiterOptions.AddFixedWindowLimiter("auth", options =>
            {
                options.PermitLimit = rateLimitOptions.AuthPolicy.PermitLimit;
                options.Window = TimeSpan.FromSeconds(rateLimitOptions.AuthPolicy.WindowSeconds);
                options.QueueLimit = rateLimitOptions.AuthPolicy.QueueLimit;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

            // Custom response for rate limit exceeded
            limiterOptions.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
                }

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "TooManyRequests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = retryAfter.TotalSeconds
                }, cancellationToken: token);
            };
        });

        return services;
    }
    
    private class RateLimitingOptions
    {
        public const string SectionName = "RateLimiting";
    
        public bool Enabled { get; set; } = true;
        public GlobalPolicyOptions GlobalPolicy { get; set; } = new();
        public ApiPolicyOptions ApiPolicy { get; set; } = new();
        public AuthPolicyOptions AuthPolicy { get; set; } = new();
    }

    private class GlobalPolicyOptions
    {
        public int PermitLimit { get; set; } = 100;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 10;
    }

    private class ApiPolicyOptions
    {
        public int PermitLimit { get; set; } = 30;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 5;
    }

    private class AuthPolicyOptions
    {
        public int PermitLimit { get; set; } = 10;
        public int WindowSeconds { get; set; } = 60;
        public int QueueLimit { get; set; } = 2;
    }
}


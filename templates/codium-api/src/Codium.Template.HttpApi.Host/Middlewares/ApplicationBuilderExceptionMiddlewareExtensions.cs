namespace Codium.Template.HttpApi.Host.Middlewares;

public static class ApplicationBuilderMiddlewareExtensions
{
    /// <summary>
    /// Configures all custom middlewares in the correct order
    /// Order is critical for proper functionality:
    /// 1. Exception handling (must be first to catch all errors)
    /// 2. CORS (before authentication)
    /// 3. HTTPS redirection
    /// 4. Routing
    /// 5. Rate limiting (after routing, before authentication)
    /// 6. Authentication & Authorization
    /// 7. Custom middlewares (now user context is available)
    /// 8. Endpoints
    /// </summary>
    public static void UseCustomMiddlewares(this IApplicationBuilder app)
    {
        // 1. Exception handling - Must be first to catch all errors
        app.UseMiddleware<ExceptionHandlerMiddleware>();
        
        // 2. CORS - Before authentication to handle preflight requests
        app.UseCors();
        
        // 3. HTTPS redirection
        app.UseHttpsRedirection();
        
        // 4. Routing - Required for endpoint-based features
        app.UseRouting();
        
        // 5. Rate limiting - After routing to get endpoint info, before auth
        app.UseRateLimiter();
        
        // 6. Request localization
        app.UseRequestLocalization();
        
        // 7. Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();
        
        // 8. Custom middlewares - User context is now available
        app.UseMiddleware<ProgressingStartedMiddleware>();
        app.UseMiddleware<HttpRequestMiddleware>();
    }
}
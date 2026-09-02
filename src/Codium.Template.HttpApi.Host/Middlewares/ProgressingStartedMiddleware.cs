using Codium.Template.Domain.Shared.Extensions;
using Microsoft.Extensions.Caching.Memory;

namespace Codium.Template.HttpApi.Host.Middlewares;

public class ProgressingStartedMiddleware(
    RequestDelegate next, 
    IMemoryCache memoryCache, 
    ILogger<ProgressingStartedMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            var correlationId = context.GetCorrelationId();
            if (correlationId == null)
            {
                context.SetCorrelationId(Guid.NewGuid());
            }
        }
        catch (Exception)
        {
            // ignored
        }

        try
        {
            var sessionId = context.User.GetSessionId();
            if (sessionId != null)
            {
                context.SetSessionId((Guid)sessionId);
            }
        }
        catch (Exception)
        {
            // ignored
        }

        await next(context);
    }
}
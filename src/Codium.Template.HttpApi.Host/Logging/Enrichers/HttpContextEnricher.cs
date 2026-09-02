using Codium.Template.Domain.Shared.Extensions;
using Serilog.Core;
using Serilog.Events;

namespace Codium.Template.HttpApi.Host.Logging.Enrichers;

public class HttpContextEnricher : ILogEventEnricher
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextEnricher(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return;
        }
        
        var correlationId = httpContext.GetCorrelationId();
        if (correlationId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", correlationId));
        }
        
        var sessionId = httpContext.GetSessionId();
        if (sessionId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SessionId", sessionId));
        }
        
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("BaseUrl", httpContext.GetBaseUrl()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", httpContext.GetPath()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", httpContext.GetRequestMethod()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ControllerName", httpContext.GetControllerName()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ActionName", httpContext.GetActionName()));
        
        var queryString = httpContext.GetQueryStringToDictionary();
        if (queryString.Count > 0)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("QueryString", queryString));
        }
        
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserAgent", httpContext.GetUserAgent()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("IpAddress", httpContext.GetClientIpAddress()));

        var userId = httpContext.User.GetUserId();
        if (userId != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", userId));
        }
    }
}
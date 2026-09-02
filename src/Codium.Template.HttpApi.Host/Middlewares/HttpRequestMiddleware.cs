using System.Diagnostics;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.HttpRequestLogs;
using Codium.Template.Domain.Shared.Repositories;
using Microsoft.Extensions.Options;

namespace Codium.Template.HttpApi.Host.Middlewares;

public class HttpRequestMiddleware
{
    private readonly RequestDelegate _next;
    private readonly HttpRequestLogOptions _options;

    public HttpRequestMiddleware(RequestDelegate next, IOptions<HttpRequestLogOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task Invoke(HttpContext context, IHttpRequestLogRepository httpRequestLogRepository, IUnitOfWork unitOfWork)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.GetPath().ToLowerInvariant();
        if (_options.ExcludedPaths.Any(item => path.StartsWith(item, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        if (_options.ExcludedHttpMethods.Contains(context.GetRequestMethod(), StringComparer.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var contentType = context.Request.ContentType?.ToLowerInvariant() ?? string.Empty;
        if (_options.ExcludedContentTypes.Any(ct => contentType.StartsWith(ct, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var executionTime = DateTime.UtcNow;
        string? originalContent = null;
        if (_options.LogRequestBody)
        {
            originalContent = await context.GetRequestBodyAsync(_options.MaxRequestBodyLength);
        }

        await _next(context);
        stopwatch.Stop();

        if (_options.LogOnlySlowRequests && stopwatch.ElapsedMilliseconds < _options.SlowRequestThresholdMs)
        {
            return;
        }

        var deviceInfo = context.GetDeviceInfo();

        var httpRequestLog = new HttpRequestLog
        {
            CreationTime = DateTime.UtcNow,
            CreatorId = context.User.GetUserId(),
            HttpMethod = context.GetRequestMethod(),
            RequestPath = context.GetPath(),
            QueryString = JsonMaskExtensions.MaskSensitiveData(context.GetQueryStringToJson(),
                _options.MaskPattern,
                _options.QueryStringSensitiveProperties.ToArray()),
            RequestBody = JsonMaskExtensions.MaskSensitiveData(originalContent,
                _options.MaskPattern,
                _options.RequestBodySensitiveProperties.ToArray()),
            RequestHeaders = JsonMaskExtensions.MaskSensitiveData(context.GetRequestHeadersToJson(),
                _options.MaskPattern,
                _options.HeaderSensitiveProperties.ToArray()),
            StatusCode = context.Response.StatusCode,
            RequestTime = executionTime,
            ResponseTime = DateTime.UtcNow,
            DurationMs = stopwatch.ElapsedMilliseconds,
            ClientIp = context.GetClientIpAddress(),
            UserAgent = context.GetUserAgent(),
            DeviceFamily = deviceInfo.DeviceFamily,
            DeviceModel = deviceInfo.DeviceModel,
            OsFamily = deviceInfo.OsFamily,
            OsVersion = deviceInfo.OsVersion,
            BrowserFamily = deviceInfo.BrowserFamily,
            BrowserVersion = deviceInfo.BrowserVersion,
            IsMobile = deviceInfo.IsMobile,
            IsTablet = deviceInfo.IsTablet,
            IsDesktop = deviceInfo.IsDesktop,
            ControllerName = context.GetControllerName(),
            ActionName = context.GetActionName(),
            SessionId = context.GetSessionId(),
            CorrelationId = context.GetCorrelationId()
        };

        await httpRequestLogRepository.AddAsync(httpRequestLog);
        await unitOfWork.SaveChangesAsync();
    }
}

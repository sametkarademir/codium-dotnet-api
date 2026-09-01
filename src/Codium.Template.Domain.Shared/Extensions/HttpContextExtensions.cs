using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using UAParser;

namespace Codium.Template.Domain.Shared.Extensions;

public static class HttpContextExtensions
{
    #region Headers

    public static string? GetRequestHeaderValue(this HttpContext context, string key)
    {
        return context.Request.Headers.TryGetValue(key, out var values)
            ? values.FirstOrDefault()
            : null;
    }
    
    public static void SetRequestHeaderValue(this HttpContext context, string key, string value)
    {
        context.Request.Headers.Append(key, value);
    }

    public static Dictionary<string, string> GetRequestHeadersToDictionary(this HttpContext context)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in context.Request.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        return headers;
    }

    public static string GetRequestHeadersToJson(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(context.GetRequestHeadersToDictionary(), jsonSerializerOptions);
    }
    
    public static string? GetResponseHeaderValue(this HttpContext context, string key)
    {
        return context.Response.Headers.TryGetValue(key, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    public static void SetResponseHeaderValue(this HttpContext context, string key, string value)
    {
        context.Response.Headers.Append(key, value);
    }

    public static string GetResponseHeadersToJson(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var headers = new Dictionary<string, string>();
        foreach (var header in context.Response.Headers)
        {
            headers[header.Key] = header.Value.ToString();
        }

        return JsonSerializer.Serialize(headers, jsonSerializerOptions);
    }

    public static Guid? GetCorrelationId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Correlation-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    public static void SetCorrelationId(this HttpContext httpContext, Guid correlationId)
    {
        httpContext.SetRequestHeaderValue("X-Correlation-ID", correlationId.ToString());
        httpContext.SetResponseHeaderValue("X-Correlation-ID", correlationId.ToString());
    }
    
    public static void SetCorrelationId(this HttpResponse response, string? correlationId) 
    {
        if (string.IsNullOrEmpty(correlationId))
        {
            return;
        }

        response.Headers.Append("X-Correlation-ID", correlationId);
    }
    
    public static Guid? GetSessionId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Session-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    public static void SetSessionId(this HttpContext httpContext, Guid sessionId)
    {
        httpContext.SetRequestHeaderValue("X-Session-ID", sessionId.ToString());
    }

    public static Guid? GetSnapshotId(this HttpContext httpContext)
    {
        var value = httpContext.GetRequestHeaderValue("X-Snapshot-ID");
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        Guid.TryParse(value, out var guid);

        return guid;
    }

    public static void SetSnapshotId(this HttpContext httpContext, Guid snapshotId)
    {
        httpContext.SetRequestHeaderValue("X-Snapshot-ID", snapshotId.ToString());
    }

    #endregion

    #region UserAgent

    public static string GetUserAgent(this HttpContext httpContext)
    {
        return httpContext.Request.Headers["User-Agent"].ToString();
    }

    public static DeviceInfo GetDeviceInfo(this HttpContext httpContext)
    {
        var parser = Parser.GetDefault();
        var clientInfo = parser.Parse(httpContext.GetUserAgent());
        var deviceFamily = new DeviceInfo
        {
            DeviceFamily = clientInfo.Device.Family,
            DeviceModel = clientInfo.Device.Model,
            OsFamily = clientInfo.OS.Family,
            OsVersion = string.Join(".", new[]
            {
                clientInfo.OS.Major,
                clientInfo.OS.Minor,
                clientInfo.OS.Patch
            }.Where(v => !string.IsNullOrEmpty(v))),
            BrowserFamily = clientInfo.UA.Family,
            BrowserVersion = string.Join(".", new[]
            {
                clientInfo.UA.Major,
                clientInfo.UA.Minor
            }.Where(v => !string.IsNullOrEmpty(v)))
        };
        var type = GetDeviceType(deviceFamily);
        deviceFamily.IsMobile = type.IsMobile;
        deviceFamily.IsDesktop = type.IsDesktop;
        deviceFamily.IsTablet = type.IsTablet;

        return deviceFamily;
    }

    public static string GetClientIpAddress(this HttpContext context)
    {
        string? ip = null;

        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            ip = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
        }

        if (string.IsNullOrEmpty(ip) && context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            ip = realIp.FirstOrDefault();
        }

        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }

        return ip ?? "unknown";
    }

    private static (bool IsMobile, bool IsDesktop, bool IsTablet) GetDeviceType(DeviceInfo deviceInfo)
    {
        if (IsTablet(deviceInfo))
        {
            return (false, false, true);
        }

        if (IsMobile(deviceInfo))
        {
            return (true, false, false);
        }

        return (false, true, false);
    }

    private static bool IsTablet(DeviceInfo deviceInfo)
    {
        if (!string.IsNullOrEmpty(deviceInfo.DeviceFamily))
        {
            var deviceFamily = deviceInfo.DeviceFamily.ToLower();
            if (deviceFamily.Contains("ipad") ||
                deviceFamily.Contains("tablet") ||
                deviceFamily.Contains("kindle"))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.DeviceModel))
        {
            var deviceModel = deviceInfo.DeviceModel.ToLower();
            if (deviceModel.Contains("ipad") ||
                deviceModel.Contains("tablet") ||
                deviceModel.Contains("kindle"))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.BrowserFamily))
        {
            var browser = deviceInfo.BrowserFamily.ToLower();
            if (browser.Contains("mobile safari") &&
                !string.IsNullOrEmpty(deviceInfo.OsFamily) &&
                deviceInfo.OsFamily.ToLower().Contains("ios"))
            {
                if (!string.IsNullOrEmpty(deviceInfo.DeviceModel) &&
                    deviceInfo.DeviceModel.ToLower().Contains("ipad"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsMobile(DeviceInfo deviceInfo)
    {
        if (!string.IsNullOrEmpty(deviceInfo.DeviceFamily))
        {
            var deviceFamily = deviceInfo.DeviceFamily.ToLower();
            if (deviceFamily.Contains("iphone") ||
                deviceFamily.Contains("android") ||
                deviceFamily.Contains("mobile") ||
                deviceFamily.Contains("phone"))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.OsFamily))
        {
            var osFamily = deviceInfo.OsFamily.ToLower();
            if (osFamily.Contains("android") ||
                osFamily.Contains("windows phone") ||
                osFamily.Contains("blackberry"))
            {
                return true;
            }

            if (osFamily.Contains("ios") &&
                (string.IsNullOrEmpty(deviceInfo.DeviceModel) ||
                 !deviceInfo.DeviceModel.ToLower().Contains("ipad")))
            {
                return true;
            }
        }

        if (!string.IsNullOrEmpty(deviceInfo.BrowserFamily))
        {
            string browser = deviceInfo.BrowserFamily.ToLower();
            if (browser.Contains("mobile") &&
                !browser.Contains("tablet"))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region RequestPath

    public static string GetBaseUrl(this HttpContext context)
    {
        return $"{context.Request.Scheme}://{context.Request.Host}";
    }

    public static string GetPath(this HttpContext context)
    {
        return context.Request.Path.ToString();
    }

    public static string GetRequestMethod(this HttpContext context)
    {
        return context.Request.Method;
    }

    public static string? GetControllerName(this HttpContext context)
    {
        var routeValues = GetRouteValue(context);
        
        return routeValues.TryGetValue("controller", out var value) ? value?.ToString() : "unknown";
    }

    public static string? GetActionName(this HttpContext context)
    {
        var routeValues = GetRouteValue(context);
        
        return routeValues.TryGetValue("action", out var value) ? value?.ToString() : "unknown";
    }

    private static RouteValueDictionary GetRouteValue(HttpContext context)
    {
        var routeData = context.GetRouteData();
        var routeValues = routeData.Values;

        return routeValues;
    }

    #endregion

    #region QueryString

    public static string? GetQueryStringValue(this HttpContext request, string key)
    {
        return request.Request.Query.TryGetValue(key, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    public static Dictionary<string, string> GetQueryStringToDictionary(this HttpContext request)
    {
        var query = new Dictionary<string, string>();
        foreach (var queryItem in request.Request.Query)
        {
            query[queryItem.Key] = queryItem.Value.ToString();
        }

        return query;
    }

    public static string GetQueryStringToJson(this HttpContext request, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        return JsonSerializer.Serialize(request.GetQueryStringToDictionary(), jsonSerializerOptions);
    }

    #endregion

    #region RequestBody

    public static async Task<string> GetRequestBodyAsync(this HttpContext context, int maxLength = 1000, string? trancateMessage = null)
    {
        if (context.Request.Body.CanSeek)
        {
            context.Request.Body.Position = 0;
        }
        else
        {
            context.Request.EnableBuffering();
        }

        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
        var originalContent = await reader.ReadToEndAsync();

        if (originalContent.Length > maxLength)
        {
            originalContent = originalContent.Substring(0, maxLength - trancateMessage?.Length ?? 0 ) + trancateMessage;
        }

        context.Request.Body.Position = 0;

        return originalContent;
    }

    public static async Task<string> GetRequestBodyToJsonAsync(this HttpContext context, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        var body = await context.GetRequestBodyAsync();
        
        return JsonSerializer.Serialize(body, jsonSerializerOptions);
    }

    #endregion

    #region Form Data

    public static async Task<object?> GetFormValueAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.TryGetValue(key, out var values) ? values.FirstOrDefault() : null;
    }

    public static async Task<Dictionary<string, object>> GetFormDataToDictionaryAsync(this HttpContext context)
    {
        var form = await context.Request.ReadFormAsync();
        var formData = new Dictionary<string, object>();

        foreach (var item in form)
        {
            formData[item.Key] = item.Value;
        }

        return formData;
    }

    public static async Task<IFormFile?> GetFormFileAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.Files.GetFile(key);
    }

    public static async Task<List<IFormFile>> GetFormFilesAsync(this HttpContext context, string key)
    {
        var form = await context.Request.ReadFormAsync();
        
        return form.Files.GetFiles(key).ToList();
    }

    #endregion
}

public class DeviceInfo
{
    public string? DeviceFamily { get; set; }
    public string? DeviceModel { get; set; }
    
    public string? OsFamily { get; set; }
    public string? OsVersion { get; set; }
    
    public string? BrowserFamily { get; set; }
    public string? BrowserVersion { get; set; }

    public bool IsMobile { get; set; }
    public bool IsTablet { get; set; }
    public bool IsDesktop { get; set; }
}
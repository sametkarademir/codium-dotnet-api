using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.HttpRequestLogs;

[DisableAuditLog]
public class HttpRequestLog : CreationAuditedEntityWithUser<Guid, User>
{
    public string? HttpMethod { get; set; }
    public string? RequestPath { get; set; }
    public string? QueryString { get; set; }
    public string? RequestBody { get; set; }
    public string? RequestHeaders { get; set; }

    public int? StatusCode { get; set; }

    public DateTime RequestTime { get; set; }
    public DateTime ResponseTime { get; set; }
    public long? DurationMs { get; set; }

    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }

    public string? DeviceFamily { get; set; }
    public string? DeviceModel { get; set; }
    public string? OsFamily { get; set; }
    public string? OsVersion { get; set; }
    public string? BrowserFamily { get; set; }
    public string? BrowserVersion { get; set; }
    public bool IsMobile { get; set; }
    public bool IsTablet { get; set; }
    public bool IsDesktop { get; set; }

    public string? ControllerName { get; set; }
    public string? ActionName { get; set; }

    public Guid? SessionId { get; set; }
    public Guid? CorrelationId { get; set; }
}
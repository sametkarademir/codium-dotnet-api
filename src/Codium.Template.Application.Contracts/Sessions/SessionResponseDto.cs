using Codium.Template.Application.Contracts.BaseEntities;

namespace Codium.Template.Application.Contracts.Sessions;

public class SessionResponseDto : EntityDto<Guid>
{
    public string ClientIp { get; set; } = null!;

    public string? DeviceFamily { get; set; }
    public string? DeviceModel { get; set; }
    public string? OsFamily { get; set; }
    public string? OsVersion { get; set; }
    public string? BrowserFamily { get; set; }
    public string? BrowserVersion { get; set; }

    public bool IsMobile { get; set; }
    public bool IsDesktop { get; set; }
    public bool IsTablet { get; set; }
}
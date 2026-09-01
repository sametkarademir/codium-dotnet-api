using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.Sessions;

public class Session : FullAuditedEntity<Guid>
{
    public bool IsRevoked { get; set; }
    public DateTime? RevokedTime { get; set; }
    
    public required string ClientIp { get; set; }
    public required string UserAgent { get; set; }

    public string? DeviceFamily { get; set; }
    public string? DeviceModel { get; set; }
    public string? OsFamily { get; set; }
    public string? OsVersion { get; set; }
    public string? BrowserFamily { get; set; }
    public string? BrowserVersion { get; set; }

    public bool IsMobile { get; set; }
    public bool IsDesktop { get; set; }
    public bool IsTablet { get; set; }

    public Guid? SnapshotId { get; set; }
    public Guid? CorrelationId { get; set; }
    
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
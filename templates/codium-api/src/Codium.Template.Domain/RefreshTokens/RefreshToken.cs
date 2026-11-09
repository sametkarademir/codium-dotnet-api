using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.RefreshTokens;

public class RefreshToken : FullAuditedEntity<Guid>
{
    public required string Token { get; set; }
    public DateTime? ExpiryTime { get; set; }
    
    public bool IsUsed { get; set; } = false;
    public string? ReplacedByToken { get; set; }
    
    public bool IsRevoked { get; set; } = false;
    public DateTime? RevokedTime { get; set; }
    
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    
    public Guid SessionId { get; set; }
    public virtual Session Session { get; set; } = null!;
}
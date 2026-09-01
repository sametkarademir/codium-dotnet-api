using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.UserRoles;

namespace Codium.Template.Domain.Users;

public class User : FullAuditedEntity<Guid>
{
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }

    public string PasswordHash { get; set; } = null!;
    public bool ShouldChangePasswordOnNextLogin { get; set; }
    public DateTime? PasswordChangedTime { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }
    
    public bool TwoFactorEnabled { get; set; }
    
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public virtual ICollection<Session> Sessions { get; set; } = [];
}
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.UserRoles;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.Domain.Users;

public class User : IdentityUser<Guid>, IEntity<Guid>, IFullAuditedObject
{
    public bool ShouldChangePasswordOnNextLogin { get; set; }
    public DateTime? PasswordChangedTime { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;

    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }

    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public virtual ICollection<Session> Sessions { get; set; } = [];
}
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.UserRoles;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.Domain.Roles;

public class Role : IdentityRole<Guid>, IEntity<Guid>, IFullAuditedObject
{
    public string? Description { get; set; }

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
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
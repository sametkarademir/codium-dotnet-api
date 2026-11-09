using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Users;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.Domain.UserRoles;

/// <summary>
/// Identity's user-role link (composite key <c>UserId</c> + <c>RoleId</c>). It has no surrogate id and no soft-delete;
/// removing a role from a user deletes the row.
/// </summary>
public class UserRole : IdentityUserRole<Guid>, IEntity, ICreationAuditedObject
{
    public virtual User? User { get; set; }
    public virtual Role? Role { get; set; }

    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }
}

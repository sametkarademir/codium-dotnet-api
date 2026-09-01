using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;

namespace Codium.Template.Domain.RolePermissions;

public class RolePermission : FullAuditedEntity<Guid>
{
    public Guid PermissionId { get; set; }
    public virtual Permission? Permission { get; set; }
    
    public Guid RoleId { get; set; }
    public virtual Role? Role { get; set; }
}
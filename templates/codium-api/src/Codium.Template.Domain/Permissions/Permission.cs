using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;

namespace Codium.Template.Domain.Permissions;

public class Permission : FullAuditedEntity<Guid>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
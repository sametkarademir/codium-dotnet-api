using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.UserRoles;

namespace Codium.Template.Domain.Roles;

public class Role : FullAuditedEntity<Guid>
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    
    public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = [];
}
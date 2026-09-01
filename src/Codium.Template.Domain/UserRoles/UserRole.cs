using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.UserRoles;

public class UserRole : FullAuditedEntity<Guid>
{
    public Guid UserId { get; set; }
    public virtual User? User { get; set; }

    public Guid RoleId { get; set; }
    public virtual Role? Role { get; set; }
}
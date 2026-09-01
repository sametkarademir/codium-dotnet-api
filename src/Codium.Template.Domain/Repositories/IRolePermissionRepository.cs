using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IRolePermissionRepository : IRepository<RolePermission, Guid>
{

}
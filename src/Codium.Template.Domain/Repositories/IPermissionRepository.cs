using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IPermissionRepository : IRepository<Permission, Guid>
{
}
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IRoleRepository : IRepository<Role, Guid>
{
}

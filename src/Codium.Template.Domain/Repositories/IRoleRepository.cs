using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IRoleRepository : IRepository<Role, Guid>
{
    Task<bool> ExistsByNameAsync(
        string name,
        Guid? id = null,
        CancellationToken cancellationToken = default
    );
    
    Task<Role?> FindByNameAsync(
        string name,
        CancellationToken cancellationToken = default
    );
}
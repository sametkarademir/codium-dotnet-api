using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.UserRoles;

namespace Codium.Template.Domain.Repositories;

public interface IUserRoleRepository : IRepository<UserRole, Guid>
{
    Task<(List<string> Roles, List<string> Permissions)> GetRolesAndPermissionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    Task<List<string>> GetRolesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
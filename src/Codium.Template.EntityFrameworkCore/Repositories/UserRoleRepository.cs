using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.UserRoles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class UserRoleRepository(ApplicationDbContext context)
    : EfRepositoryBase<UserRole, Guid, ApplicationDbContext>(context), IUserRoleRepository
{
    public async Task<(List<string> Roles, List<string> Permissions)> GetRolesAndPermissionsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var result = await AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => new
            {
                RoleName = ur.Role!.Name,
                Permissions = ur.Role.RolePermissions.Select(rp => rp.Permission!.Name)
            })
            .ToListAsync(cancellationToken);

        return (
            Roles: result.Select(d => d.RoleName).ToList(),
            Permissions: result.SelectMany(d => d.Permissions).Distinct().ToList()
        );
    }

    public async Task<List<string>> GetRolesByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await AsQueryable()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role!.Name)
            .ToListAsync(cancellationToken);
    }
}
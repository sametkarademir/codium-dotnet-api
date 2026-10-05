using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Roles;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class RoleRepository(ApplicationDbContext context)
    : EfRepositoryBase<Role, Guid, ApplicationDbContext>(context), IRoleRepository
{
}

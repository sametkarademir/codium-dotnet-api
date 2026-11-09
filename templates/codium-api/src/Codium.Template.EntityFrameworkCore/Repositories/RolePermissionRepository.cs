using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class RolePermissionRepository(ApplicationDbContext context)
    : EfRepositoryBase<RolePermission, Guid, ApplicationDbContext>(context), IRolePermissionRepository;
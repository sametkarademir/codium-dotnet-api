using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class PermissionRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<Permission, Guid, ApplicationDbContext>(dbContext), IPermissionRepository;
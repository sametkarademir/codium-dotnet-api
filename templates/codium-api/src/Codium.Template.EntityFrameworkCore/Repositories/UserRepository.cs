using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class UserRepository(ApplicationDbContext context)
    : EfRepositoryBase<User, Guid, ApplicationDbContext>(context), IUserRepository
{
}

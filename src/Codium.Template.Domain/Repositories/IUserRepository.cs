using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
}

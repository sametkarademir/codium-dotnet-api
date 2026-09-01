using Codium.Template.Domain.Shared.Repositories;
using Codium.Template.Domain.Users;

namespace Codium.Template.Domain.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<bool> ExistsByEmailAsync(string email, Guid? id = null, CancellationToken cancellationToken = default);
    Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
}
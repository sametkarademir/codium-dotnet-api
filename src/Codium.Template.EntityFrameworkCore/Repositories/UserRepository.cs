using Codium.Template.Domain.Repositories;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class UserRepository(ApplicationDbContext context)
    : EfRepositoryBase<User, Guid, ApplicationDbContext>(context), IUserRepository
{
    public async Task<bool> ExistsByEmailAsync(string email, Guid? id = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.NormalizeValue();
        
        var queryable = AsQueryable();
        queryable = queryable.Where(u => u.NormalizedEmail == normalizedEmail);
        queryable = queryable.WhereIf(id != null, u => u.Id != id);
        
        return await queryable.AnyAsync(cancellationToken);
    }

    public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.NormalizeValue();
        
        var queryable = AsQueryable();
        queryable = queryable.Where(u => u.NormalizedEmail == normalizedEmail);
        
        return await queryable.SingleOrDefaultAsync(cancellationToken);
    }
}
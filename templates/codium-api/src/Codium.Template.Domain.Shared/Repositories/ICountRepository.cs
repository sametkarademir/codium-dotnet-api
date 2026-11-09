using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface ICountRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Checks if any entity matches the predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if any entity matches the predicate; otherwise, false</returns>
    Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the count of entities that match the predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The count of entities that match the predicate</returns>
    Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    );
}
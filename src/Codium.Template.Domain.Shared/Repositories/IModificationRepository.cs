using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IModificationRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Updates an existing entity in the repository
    /// </summary>
    /// <param name="entity">The entity to update</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated entity</returns>
    ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Updates a collection of entities in the repository
    /// </summary>
    /// <param name="entities">The collection of entities to update</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The updated entities</returns>
    ValueTask<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}
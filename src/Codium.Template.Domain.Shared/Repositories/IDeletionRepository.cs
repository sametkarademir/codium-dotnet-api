using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IDeletionRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Deletes an entity from the repository
    /// </summary>
    /// <param name="entity">The entity to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The deleted entity</returns>
    ValueTask<TEntity> DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes a collection of entities from the repository
    /// </summary>
    /// <param name="entities">The collection of entities to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The deleted entities</returns>
    ValueTask<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}

public interface IDeletionRepository<TEntity, in TKey> : IDeletionRepository<TEntity> where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Deletes an entity from the repository
    /// </summary>
    /// <param name="id">The id of the entity to delete</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The deleted entity</returns>
    Task<TEntity> DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default
    );
}
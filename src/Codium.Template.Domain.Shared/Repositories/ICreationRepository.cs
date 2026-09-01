using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

public interface ICreationRepository<TEntity> where TEntity : class, IEntity
{
    /// <summary>
    /// Adds a new entity to the repository
    /// </summary>
    /// <param name="entity">The entity to add</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The added entity</returns>
    Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a collection of entities to the repository
    /// </summary>
    /// <param name="entities">The collection of entities to add</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The added entities</returns>
    Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    );
}
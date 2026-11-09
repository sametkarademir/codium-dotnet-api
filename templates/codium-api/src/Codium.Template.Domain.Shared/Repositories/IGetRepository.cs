using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Microsoft.EntityFrameworkCore.Query;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IGetRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets a single entity by predicate
    /// </summary>
    /// <param name="predicate">The predicate to filter the entity</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The entity that matches the predicate</returns>
    /// <exception cref="AppEntityNotFoundException">Thrown when entity is not found</exception>
    Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the first entity that matches the predicate or null if not found
    /// </summary>
    /// <param name="predicate">The predicate to filter the entity</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The first entity that matches the predicate or null if not found</returns>
    Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the single entity that matches the predicate or null if not found
    /// </summary>
    /// <param name="predicate">The predicate to filter the entity</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The single entity that matches the predicate or null if not found</returns>
    /// <exception cref="InvalidOperationException">Thrown when more than one entity matches the predicate</exception>
    Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
}

public interface IGetRepository<TEntity, in TKey> : IGetRepository<TEntity>
    where TEntity : class, IEntity<TKey>
{
    /// <summary>
    /// Gets a single entity by its key
    /// </summary>
    /// <param name="id">The entity key</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The entity with the specified key</returns>
    /// <exception cref="AppEntityNotFoundException">Thrown when entity is not found</exception>
    Task<TEntity> GetAsync(
        TKey id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
}
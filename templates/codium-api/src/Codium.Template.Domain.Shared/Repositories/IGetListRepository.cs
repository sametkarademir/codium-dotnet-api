using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Querying;
using Microsoft.EntityFrameworkCore.Query;

namespace Codium.Template.Domain.Shared.Repositories;

public interface IGetListRepository<TEntity> : IQueryableRepository<TEntity>
    where TEntity : class, IEntity
{
    /// <summary>
    /// Gets all entities that match the specified criteria
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="orderBy">The ordering function</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of entities that match the criteria</returns>
    Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets all entities that match the specified criteria
    /// </summary>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="sort">The sort requests</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A list of entities that match the criteria</returns>
    Task<List<TEntity>> GetAllSortedAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paged list of entities that match the specified criteria
    /// </summary>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="perPage">The number of items per page</param>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="orderBy">The ordering function</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A pageable response containing the entities and metadata</returns>
    Task<PagedList<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets a paged list of entities with custom sorting
    /// </summary>
    /// <param name="page">The page number (1-based)</param>
    /// <param name="perPage">The number of items per page</param>
    /// <param name="predicate">The predicate to filter entities</param>
    /// <param name="include">The include function for related entities</param>
    /// <param name="sort">The sort requests</param>
    /// <param name="enableTracking">Indicates whether to enable change tracking</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A pageable response containing the entities and metadata</returns>
    Task<PagedList<TEntity>> GetListSortedAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    );
}
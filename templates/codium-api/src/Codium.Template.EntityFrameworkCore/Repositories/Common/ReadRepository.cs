using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.Shared.Querying;
using Codium.Template.Domain.Shared.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Codium.Template.EntityFrameworkCore.Repositories.Common;

public class ReadRepository<TEntity, TKey, TContext>(TContext context) :
    IReadRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{
    public IQueryable<TEntity> AsQueryable()
    {
        return context.Set<TEntity>();
    }

    public async Task<TEntity> GetAsync(
        TKey id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        return await GetAsync(item => Equals(item.Id, id), include, enableTracking, cancellationToken);
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await SingleOrDefaultAsync(predicate, include, enableTracking, cancellationToken);
        if (entity == null)
        {
            throw new AppEntityNotFoundException(typeof(TEntity));
        }

        return entity;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        return await queryable.SingleOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllSortedAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sort != null) queryable = queryable.ApplySort(sort);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<PagedList<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<PagedList<TEntity>> GetListSortedAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sort != null) queryable = queryable.ApplySort(sort);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        queryable = queryable.AsNoTracking();
        return await queryable.AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (predicate != null) queryable = queryable.Where(predicate);
        queryable = queryable.AsNoTracking();
        return await queryable.CountAsync(cancellationToken: cancellationToken);
    }
}

public class ReadRepository<TEntity, TContext>(TContext context) :
    IReadRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public IQueryable<TEntity> AsQueryable()
    {
        return context.Set<TEntity>();
    }

    public async Task<TEntity> GetAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await SingleOrDefaultAsync(predicate, include, enableTracking, cancellationToken);
        if (entity == null)
        {
            throw new AppEntityNotFoundException(typeof(TEntity));
        }

        return entity;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        return await queryable.FirstOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<TEntity?> SingleOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        return await queryable.SingleOrDefaultAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<List<TEntity>> GetAllSortedAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sort != null) queryable = queryable.ApplySort(sort);
        return await queryable.ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<PagedList<TEntity>> GetListAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (orderBy != null) queryable = orderBy(queryable);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<PagedList<TEntity>> GetListSortedAsync(
        int page = 1,
        int perPage = 10,
        Expression<Func<TEntity, bool>>? predicate = null,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        SortRequest? sort = null,
        bool enableTracking = true,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (!enableTracking) queryable = queryable.AsNoTracking();
        if (include != null) queryable = include(queryable);
        if (predicate != null) queryable = queryable.Where(predicate);
        if (sort != null) queryable = queryable.ApplySort(sort);
        return await queryable.ToPageableAsync(page, perPage, cancellationToken: cancellationToken);
    }

    public async Task<bool> AnyAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        queryable = queryable.AsNoTracking();
        return await queryable.AnyAsync(predicate, cancellationToken: cancellationToken);
    }

    public async Task<int> CountAsync(
        Expression<Func<TEntity, bool>>? predicate = null,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = AsQueryable();
        if (predicate != null) queryable = queryable.Where(predicate);
        queryable = queryable.AsNoTracking();
        return await queryable.CountAsync(cancellationToken: cancellationToken);
    }
}
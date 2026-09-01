using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Repositories.Common;

public class WriteRepository<TEntity, TKey, TContext>(TContext context) :
    IWriteRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
    where TContext : DbContext
{
    public async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        context.Update(entity);
        return ValueTask.FromResult(entity);
    }

    public ValueTask<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        context.UpdateRange(entities);
        return ValueTask.FromResult(entities);
    }
    
    public async Task<TEntity> DeleteAsync(
        TKey id,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.Set<TEntity>()
            .SingleOrDefaultAsync(item => Equals(item.Id, id), cancellationToken: cancellationToken);
        if (entity == null)
        {
            throw new AppEntityNotFoundException(typeof(TEntity), id);
        }
        
        if (entity is ISoftDelete softDeletedEntity)
        {
            softDeletedEntity.IsDeleted = true;
            context.Update(entity);
        }
        else
        {
            context.Remove(entity);
        }
       
        return entity;
    }

    public ValueTask<TEntity> DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        if (entity is ISoftDelete softDeletedEntity)
        {
            softDeletedEntity.IsDeleted = true;
            context.Update(entity);
        }
        else
        {
            context.Remove(entity);
        }

        return ValueTask.FromResult(entity);
    }

    public ValueTask<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
        {
            if (entity is ISoftDelete softDeletedEntity)
            {
                softDeletedEntity.IsDeleted = true;
                context.Update(entity);
            }
            else
            {
                context.Remove(entity);
            }
        }

        return ValueTask.FromResult(entities);
    }
}

public class WriteRepository<TEntity, TContext>(TContext context) :
    IWriteRepository<TEntity>
    where TEntity : class, IEntity
    where TContext : DbContext
{
    public async Task<TEntity> AddAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task<ICollection<TEntity>> AddRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        await context.AddRangeAsync(entities, cancellationToken);
        return entities;
    }

    public ValueTask<TEntity> UpdateAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        context.Update(entity);
        return ValueTask.FromResult(entity);
    }

    public ValueTask<ICollection<TEntity>> UpdateRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        context.UpdateRange(entities);
        return ValueTask.FromResult(entities);
    }

    public ValueTask<TEntity> DeleteAsync(
        TEntity entity,
        CancellationToken cancellationToken = default
    )
    {
        if (entity is ISoftDelete softDeletedEntity)
        {
            softDeletedEntity.IsDeleted = true;
            context.Update(entity);
        }
        else
        {
            context.Remove(entity);
        }

        return ValueTask.FromResult(entity);
    }

    public ValueTask<ICollection<TEntity>> DeleteRangeAsync(
        ICollection<TEntity> entities,
        CancellationToken cancellationToken = default
    )
    {
        foreach (var entity in entities)
        {
            if (entity is ISoftDelete softDeletedEntity)
            {
                softDeletedEntity.IsDeleted = true;
                context.Update(entity);
            }
            else
            {
                context.Remove(entity);
            }
        }

        return ValueTask.FromResult(entities);
    }
}
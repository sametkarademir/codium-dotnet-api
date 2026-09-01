using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

/// <summary>
/// Defines read operations for entity repository with typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
public interface IReadRepository<TEntity, in TKey> :
    IGetRepository<TEntity, TKey>,
    IGetListRepository<TEntity>,
    ICountRepository<TEntity>
    where TEntity : class, IEntity<TKey>
{
}

/// <summary>
/// Defines read operations for entity repository without typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IReadRepository<TEntity> :
    IGetRepository<TEntity>,
    IGetListRepository<TEntity>,
    ICountRepository<TEntity>
    where TEntity : class, IEntity
{
}
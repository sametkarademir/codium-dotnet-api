using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Repositories;

/// <summary>
/// Defines write operations for entity repository with typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type</typeparam>
public interface IWriteRepository<TEntity, in TKey> :
    ICreationRepository<TEntity>,
    IModificationRepository<TEntity>,
    IDeletionRepository<TEntity, TKey>
    where TEntity : class, IEntity<TKey>
{
}

/// <summary>
/// Defines write operations for entity repository without typed key
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IWriteRepository<TEntity> :
    ICreationRepository<TEntity>,
    IModificationRepository<TEntity>,
    IDeletionRepository<TEntity>
    where TEntity : class, IEntity
{
}
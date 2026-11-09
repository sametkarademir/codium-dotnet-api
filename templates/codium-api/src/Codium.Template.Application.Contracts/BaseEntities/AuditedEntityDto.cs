using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

namespace Codium.Template.Application.Contracts.BaseEntities;

/// <summary>
/// Base class for audited entity data transfer objects.
/// Contains last modification time and modifier user information.
/// </summary>
[Serializable]
public abstract class AuditedEntityDto : CreationAuditedEntityDto, IAuditedObject
{
    /// <summary>
    /// Gets or sets the last modification time of the entity.
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    public Guid? LastModifierId { get; set; }
}

/// <summary>
/// Base class for audited entity data transfer objects with a specific key type.
/// Contains last modification time and modifier user information.
/// </summary>
/// <typeparam name="TKey">The type of the entity's key.</typeparam>
[Serializable]
public abstract class AuditedEntityDto<TKey> : CreationAuditedEntityDto<TKey>, IAuditedObject
{
    /// <summary>
    /// Gets or sets the last modification time of the entity.
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the user who last modified the entity.
    /// </summary>
    public Guid? LastModifierId { get; set; }
}
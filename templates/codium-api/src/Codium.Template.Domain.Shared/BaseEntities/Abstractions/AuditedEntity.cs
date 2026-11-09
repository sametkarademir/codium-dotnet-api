using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

[Serializable]
public abstract class AuditedEntity : CreationAuditedEntity, IAuditedObject
{

    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }
}

[Serializable]
public abstract class AuditedEntity<TKey> : CreationAuditedEntity<TKey>, IAuditedObject
{
    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }

    protected AuditedEntity()
    {
    }

    protected AuditedEntity(TKey id) : base(id)
    {
    }
}

[Serializable]
public abstract class AuditedEntityWithUser<TKey, TUser> : CreationAuditedEntityWithUser<TKey, TUser>, IAuditedObject<TUser> 
    where TUser : IEntity
{
    [DisableAuditLog]
    public virtual DateTime? LastModificationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? LastModifierId { get; set; }

    public TUser? LastModifier { get; set; }

    protected AuditedEntityWithUser()
    {
    }

    protected AuditedEntityWithUser(TKey id) : base(id)
    {
    }
}
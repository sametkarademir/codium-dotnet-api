using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

[Serializable]
public abstract class CreationAuditedEntity : Entity, ICreationAuditedObject
{
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }
}

[Serializable]
public abstract class CreationAuditedEntity<TKey> : Entity<TKey>, ICreationAuditedObject
{
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }

    protected CreationAuditedEntity()
    {
    }

    protected CreationAuditedEntity(TKey id) : base(id)
    {
    }
}

[Serializable]
public abstract class CreationAuditedEntityWithUser<TKey, TUser> : Entity<TKey>, ICreationAuditedObject<TUser> 
    where TUser : IEntity
{
    [DisableAuditLog]
    public virtual DateTime CreationTime { get; set; }

    [DisableAuditLog]
    public virtual Guid? CreatorId { get; set; }

    public TUser? Creator { get; set; }

    protected CreationAuditedEntityWithUser()
    {
    }

    protected CreationAuditedEntityWithUser(TKey id) : base(id)
    {
    }
}
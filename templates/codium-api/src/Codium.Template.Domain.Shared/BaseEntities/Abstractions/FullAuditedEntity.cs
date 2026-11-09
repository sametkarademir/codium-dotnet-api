using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Abstractions;

[Serializable]
public abstract class FullAuditedEntity : AuditedEntity, IFullAuditedObject
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    [DisableAuditLog]
    public string ConcurrencyStamp { get; set; }

    public FullAuditedEntity()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}

[Serializable]
public abstract class FullAuditedEntity<TKey> : AuditedEntity<TKey>, IFullAuditedObject
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }
    
    [DisableAuditLog]
    public string ConcurrencyStamp { get; set; }

    protected FullAuditedEntity()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    protected FullAuditedEntity(TKey id) : base(id)
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}

[Serializable]
public abstract class FullAuditedEntityWithUser<TKey, TUser> : AuditedEntityWithUser<TKey, TUser>, IFullAuditedObject<TUser> 
    where TUser : IEntity
{
    [DisableAuditLog]
    public virtual bool IsDeleted { get; set; }

    [DisableAuditLog]
    public virtual Guid? DeleterId { get; set; }

    [DisableAuditLog]
    public virtual DateTime? DeletionTime { get; set; }

    public TUser? Deleter { get; set; }
    
    [DisableAuditLog]
    public string ConcurrencyStamp { get; set; }

    protected FullAuditedEntityWithUser()
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }

    protected FullAuditedEntityWithUser(TKey id) : base(id)
    {
        ConcurrencyStamp = Guid.NewGuid().ToString("N");
    }
}
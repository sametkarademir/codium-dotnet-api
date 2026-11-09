using Codium.Template.Domain.Shared.BaseEntities.Interfaces;

namespace Codium.Template.Application.Contracts.BaseEntities;

[Serializable]
public abstract class FullAuditedEntityDto : AuditedEntityDto, IFullAuditedObject
{
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public string ConcurrencyStamp { get; set; }  = null!;
}

[Serializable]
public abstract class FullAuditedEntityDto<TKey> : AuditedEntityDto<TKey>, IFullAuditedObject
{
    public bool IsDeleted { get; set; }
    public Guid? DeleterId { get; set; }
    public DateTime? DeletionTime { get; set; }
    public string ConcurrencyStamp { get; set; }  = null!;
}
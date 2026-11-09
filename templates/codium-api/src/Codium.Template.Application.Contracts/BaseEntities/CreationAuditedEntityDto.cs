using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

namespace Codium.Template.Application.Contracts.BaseEntities;

[Serializable]
public abstract class CreationAuditedEntityDto : EntityDto, ICreationAuditedObject
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
}

[Serializable]
public abstract class CreationAuditedEntityDto<TKey> : EntityDto<TKey>, ICreationAuditedObject
{
    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
}
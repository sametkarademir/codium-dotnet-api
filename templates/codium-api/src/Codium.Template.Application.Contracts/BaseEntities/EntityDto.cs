using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Application.Contracts.BaseEntities;

[Serializable]
public abstract class EntityDto : IEntityDto
{
}

[Serializable]
public abstract class EntityDto<TKey> : EntityDto, IEntityDto<TKey>
{
    public TKey Id { get; set; } = default!;
}
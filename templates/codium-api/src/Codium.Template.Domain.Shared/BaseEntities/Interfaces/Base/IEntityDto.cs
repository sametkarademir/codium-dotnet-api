namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

public interface IEntityDto
{
}

public interface IEntityDto<TKey> : IEntityDto
{
    TKey Id { get; set; }
}
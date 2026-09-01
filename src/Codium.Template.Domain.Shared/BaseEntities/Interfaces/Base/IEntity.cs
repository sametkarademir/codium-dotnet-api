namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

public interface IEntity
{
}

public interface IEntity<out TKey> : IEntity
{
    TKey Id { get; }
}
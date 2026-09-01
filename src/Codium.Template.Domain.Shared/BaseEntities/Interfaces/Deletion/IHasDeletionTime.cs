namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

public interface IHasDeletionTime
{
    DateTime? DeletionTime { get; set; }
}
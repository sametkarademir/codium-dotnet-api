namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

public interface ISoftDelete
{
    bool IsDeleted { get; set; }
}
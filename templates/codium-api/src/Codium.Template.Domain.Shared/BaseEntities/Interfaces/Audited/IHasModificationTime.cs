namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

public interface IHasModificationTime
{
    DateTime? LastModificationTime { get; set; }
}
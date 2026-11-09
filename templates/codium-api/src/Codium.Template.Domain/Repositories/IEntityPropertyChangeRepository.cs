using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IEntityPropertyChangeRepository : IRepository<EntityPropertyChange, Guid>
{
}
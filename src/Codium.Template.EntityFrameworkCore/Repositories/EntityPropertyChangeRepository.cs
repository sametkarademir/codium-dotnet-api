using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class EntityPropertyChangeRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<EntityPropertyChange, Guid, ApplicationDbContext>(dbContext), IEntityPropertyChangeRepository;
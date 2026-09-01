using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

public interface IAuditedObject :
    ICreationAuditedObject,
    IModificationAuditedObject
{
}

public interface IAuditedObject<TUser> : 
    ICreationAuditedObject<TUser>, 
    IModificationAuditedObject<TUser>
    where TUser : IEntity
{
}
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces;

public interface IFullAuditedObject : 
    IAuditedObject,
    IDeletionAuditedObject
{
}

public interface IFullAuditedObject<TUser> : 
    IAuditedObject<TUser>, 
    IDeletionAuditedObject<TUser>
    where TUser : IEntity
{
}
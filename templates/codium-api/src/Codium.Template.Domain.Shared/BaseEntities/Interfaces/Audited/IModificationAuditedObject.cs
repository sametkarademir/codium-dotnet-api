using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

public interface IModificationAuditedObject : 
    IHasModificationTime,
    IMayHaveModifier
{
}

public interface IModificationAuditedObject<TUser> : 
    IModificationAuditedObject,
    IMayHaveModifier<TUser>
    where TUser : IEntity
{
}
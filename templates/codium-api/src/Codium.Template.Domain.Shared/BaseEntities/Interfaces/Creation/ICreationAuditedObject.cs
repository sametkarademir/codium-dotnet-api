using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

public interface ICreationAuditedObject : 
    IHasCreationTime,
    IMayHaveCreator
{
}

public interface ICreationAuditedObject<TUser> : 
    ICreationAuditedObject,
    IMayHaveCreator<TUser>
    where TUser : IEntity
{
}
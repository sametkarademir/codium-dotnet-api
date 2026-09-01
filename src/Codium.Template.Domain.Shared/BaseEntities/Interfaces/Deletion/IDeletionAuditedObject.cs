using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;

public interface IDeletionAuditedObject : 
    IHasDeletionTime,
    IMayHaveDeleter,
    ISoftDelete
{

}

public interface IDeletionAuditedObject<TUser> : 
    IDeletionAuditedObject,
    IMayHaveDeleter<TUser>
    where TUser : IEntity
{

}
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;

public interface IMayHaveCreator
{
    Guid? CreatorId { get; set; }
}

public interface IMayHaveCreator<TUser> where TUser : IEntity
{
    TUser? Creator { get; set; }
}
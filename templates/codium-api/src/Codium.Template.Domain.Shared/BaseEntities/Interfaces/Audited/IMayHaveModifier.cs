using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;

public interface IMayHaveModifier
{
    Guid? LastModifierId { get; set; }
}

public interface IMayHaveModifier<TUser> : IMayHaveModifier
    where TUser : IEntity
{
    TUser? LastModifier { get; set; }
}
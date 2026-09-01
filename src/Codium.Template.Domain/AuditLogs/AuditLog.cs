using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;
using Codium.Template.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.Domain.AuditLogs;

[DisableAuditLog]
public class AuditLog : CreationAuditedEntityWithUser<Guid, User>
{
    public required string EntityId { get; set; }
    public required string EntityName { get; set; }
    public EntityState State { get; set; }

    public Guid? SessionId { get; set; }
    public Guid? CorrelationId { get; set; }

    public virtual ICollection<EntityPropertyChange> EntityPropertyChanges { get; set; } = [];
}
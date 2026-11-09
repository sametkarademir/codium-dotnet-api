using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.Domain.Shared.BaseEntities.Abstractions;

namespace Codium.Template.Domain.EntityPropertyChanges;

[DisableAuditLog]
public class EntityPropertyChange : CreationAuditedEntity<Guid>
{
    public required string PropertyName { get; set; }
    public required string PropertyTypeFullName { get; set; }
    public string? NewValue { get; set; }
    public string? OriginalValue { get; set; }

    public Guid AuditLogId { get; set; }
    public virtual AuditLog? AuditLog { get; set; }
}
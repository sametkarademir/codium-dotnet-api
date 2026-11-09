namespace Codium.Template.Domain.Shared.AuditLogs;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = true)]
public class DisableAuditLogAttribute : Attribute
{
    
}
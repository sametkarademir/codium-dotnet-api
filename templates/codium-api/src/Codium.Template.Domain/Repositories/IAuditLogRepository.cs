using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog, Guid>
{
}
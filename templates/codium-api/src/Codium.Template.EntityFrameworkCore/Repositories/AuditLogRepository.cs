using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class AuditLogRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<AuditLog, Guid, ApplicationDbContext>(dbContext), IAuditLogRepository;
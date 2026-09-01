using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Repositories;
using Codium.Template.EntityFrameworkCore.Contexts;
using Codium.Template.EntityFrameworkCore.Repositories.Common;

namespace Codium.Template.EntityFrameworkCore.Repositories;

public class HttpRequestLogRepository(ApplicationDbContext dbContext)
    : EfRepositoryBase<HttpRequestLog, Guid, ApplicationDbContext>(dbContext), IHttpRequestLogRepository;
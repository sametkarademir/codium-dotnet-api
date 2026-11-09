using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Shared.Repositories;

namespace Codium.Template.Domain.Repositories;

public interface IHttpRequestLogRepository : IRepository<HttpRequestLog, Guid>
{
}
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public class EntityAuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var httpContextAccessor = context.GetService<IHttpContextAccessor>();

        context.SetCreationTimestamps(httpContextAccessor);
        context.SetModificationTimestamps(httpContextAccessor);
        context.SetSoftDelete(httpContextAccessor);
        context.SetTenantId(httpContextAccessor);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
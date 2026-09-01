using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public class AuditLogSaveChangesInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null)
        {
            return result;
        }

        var httpContextAccessor = context.GetService<IHttpContextAccessor>();

        await context.SetAuditLogAsync(httpContextAccessor);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var context = eventData.Context;
        if (context == null)
        {
            return result;
        }

        var httpContextAccessor = context.GetService<IHttpContextAccessor>();

        context.SetAuditLogAsync(httpContextAccessor).GetAwaiter().GetResult();

        return base.SavingChanges(eventData, result);
    }
}
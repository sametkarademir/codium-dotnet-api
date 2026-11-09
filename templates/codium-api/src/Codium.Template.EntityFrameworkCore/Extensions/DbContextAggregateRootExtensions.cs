using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.MultiTenancy;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public static class DbContextAggregateRootExtensions
{
    public static void SetCreationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e is
            {
                Entity: ICreationAuditedObject,
                State: EntityState.Added
            });

        foreach (var entry in entries)
        {
            var entity = (ICreationAuditedObject)entry.Entity;
            entity.CreationTime = DateTime.UtcNow;
            entity.CreatorId = httpContextAccessor.HttpContext?.User.GetUserId();
        }
    }

    public static void SetModificationTimestamps(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker.Entries()
            .Where(e => e is
            {
                Entity: IAuditedObject,
                State: EntityState.Modified
            });

        foreach (var entry in entries)
        {
            if (entry.Entity is IAuditedObject)
            {
                var entity = (IAuditedObject)entry.Entity;
                entity.LastModificationTime = DateTime.UtcNow.ToUniversalTime();
                entity.LastModifierId = httpContextAccessor.HttpContext?.User.GetUserId();
            }
        }
    }

    public static void SetSoftDelete(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e is { Entity: IDeletionAuditedObject, State: EntityState.Modified } &&
                e.CurrentValues["IsDeleted"]!.Equals(true)
            );

        foreach (var entry in entries)
        {
            var entity = (IDeletionAuditedObject)entry.Entity;
            entity.DeletionTime = DateTime.UtcNow.ToUniversalTime();
            entity.DeleterId = httpContextAccessor.HttpContext?.User.GetUserId();
        }
    }
    
    public static void SetTenantId(this DbContext context, IHttpContextAccessor httpContextAccessor)
    {
        var tenantId = httpContextAccessor.HttpContext?.User.GetTenantId();
        if (tenantId == null)
        {
            return;
        }

        var addedEntries = context.ChangeTracker.Entries()
            .Where(e => e is { Entity: IMultiTenant, State: EntityState.Added });

        foreach (var entry in addedEntries)
        {
            var entity = (IMultiTenant)entry.Entity;
            if (entity.TenantId == Guid.Empty)
            {
                entity.TenantId = tenantId.Value;
            }
        }
    }
}
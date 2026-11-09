using System.Linq.Expressions;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Audited;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Creation;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Deletion;
using Codium.Template.Domain.Shared.BaseEntities.Interfaces.MultiTenancy;
using Codium.Template.EntityFrameworkCore.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyMultiTenantQueryFilters(this ModelBuilder builder, ApplicationDbContext dbContext)
    {
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (!typeof(IMultiTenant).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            var parameter = Expression.Parameter(entityType.ClrType, "entity");

            // entity.TenantId
            var tenantIdProperty = Expression.Property(parameter, nameof(IMultiTenant.TenantId));
            // dbContext.CurrentTenantId
            var currentTenantId = Expression.Property(
                Expression.Constant(dbContext),
                nameof(ApplicationDbContext.CurrentTenantId)
            );

            // dbContext.CurrentTenantId == null (bypass when no tenant context)
            var tenantIdIsNull = Expression.Equal(
                currentTenantId,
                Expression.Constant(null, typeof(Guid?))
            );

            // (Guid?)entity.TenantId == dbContext.CurrentTenantId
            var tenantIdEquals = Expression.Equal(
                Expression.Convert(tenantIdProperty, typeof(Guid?)),
                currentTenantId
            );

            // CurrentTenantId == null || entity.TenantId == CurrentTenantId
            var tenantCondition = Expression.OrElse(tenantIdIsNull, tenantIdEquals);

            // Combine with soft-delete filter if applicable
            Expression body;
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var softDeleteFilter = Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false)
                );
                body = Expression.AndAlso(softDeleteFilter, tenantCondition);
            }
            else
            {
                body = tenantCondition;
            }

            var filter = Expression.Lambda(body, parameter);
            builder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    public static void ApplyGlobalEntityConfigurations<T>(this EntityTypeBuilder<T> builder) where T : class
    {
        var entityType = typeof(T);
        var entityInterfaces = entityType.GetInterfaces();

        var isIEntity = entityInterfaces.Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntity<>));
        if (isIEntity)
        {
            var idProperty = entityType.GetProperty("Id");
            if (idProperty != null)
            {
                builder.Property(idProperty.Name)
                    .ValueGeneratedOnAdd();
            }
        }

        if (typeof(ISoftDelete).IsAssignableFrom(entityType) &&
            !typeof(IMultiTenant).IsAssignableFrom(entityType))
        {
            var parameter = Expression.Parameter(entityType, "entity");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(parameter, nameof(ISoftDelete.IsDeleted)),
                    Expression.Constant(false)
                ),
                parameter
            );

            builder.HasQueryFilter(filter);
        }

        if (typeof(ICreationAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(ICreationAuditedObject.CreationTime))
                .IsRequired();

            builder.Property(nameof(ICreationAuditedObject.CreatorId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(ICreationAuditedObject.CreatorId));
            builder.HasIndex(nameof(ICreationAuditedObject.CreationTime));
        }
        
        if (typeof(ICreationAuditedObject<>).IsAssignableFrom(entityType))
        {
            var creationAuditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(ICreationAuditedObject<>)
                );

            if (creationAuditInterface != null)
            {
                var userType = creationAuditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "Creator")
                    .WithMany()
                    .HasForeignKey("CreatorId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            }
        }

        if (typeof(IAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IAuditedObject.LastModificationTime))
                .IsRequired(false);

            builder.Property(nameof(IAuditedObject.LastModifierId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IAuditedObject.LastModifierId));
            builder.HasIndex(nameof(IAuditedObject.LastModificationTime));
        }
        
        if (typeof(IAuditedObject<>).IsAssignableFrom(entityType))
        {
            var auditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IAuditedObject<>)
                );

            if (auditInterface != null)
            {
                var userType = auditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "LastModifier")
                    .WithMany()
                    .HasForeignKey("LastModifierId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            }
        }

        if (typeof(IDeletionAuditedObject).IsAssignableFrom(entityType))
        {
            builder.Property(nameof(IDeletionAuditedObject.DeletionTime))
                .IsRequired(false);

            builder.Property(nameof(IDeletionAuditedObject.DeleterId))
                .HasMaxLength(256)
                .IsRequired(false);

            builder.HasIndex(nameof(IDeletionAuditedObject.DeleterId));
            builder.HasIndex(nameof(IDeletionAuditedObject.DeletionTime));
            builder.HasIndex(nameof(IDeletionAuditedObject.IsDeleted));
        }
        
        if (typeof(IDeletionAuditedObject<>).IsAssignableFrom(entityType))
        {
            var deletionAuditInterface = entityType.GetInterfaces()
                .FirstOrDefault(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(IDeletionAuditedObject<>)
                );

            if (deletionAuditInterface != null)
            {
                var userType = deletionAuditInterface.GetGenericArguments()[0];

                builder
                    .HasOne(userType, "Deleter")
                    .WithMany()
                    .HasForeignKey("DeleterId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            }
        }
    }
}
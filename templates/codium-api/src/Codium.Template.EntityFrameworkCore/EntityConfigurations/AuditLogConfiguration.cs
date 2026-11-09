using Codium.Template.Domain;
using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.Shared.AuditLogs;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "AuditLogs", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.State);
        builder.HasIndex(item => item.EntityId);
        builder.HasIndex(item => item.EntityName);

        builder.Property(item => item.EntityId).HasMaxLength(AuditLogConsts.EntityIdMaxLength).IsRequired();
        builder.Property(item => item.EntityName).HasMaxLength(AuditLogConsts.EntityNameMaxLength).IsRequired();
        builder.Property(item => item.State).IsRequired();
    }
}
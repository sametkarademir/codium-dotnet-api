using Codium.Template.Domain;
using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.Shared.EntityPropertyChanges;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class EntityPropertyChangeConfiguration : IEntityTypeConfiguration<EntityPropertyChange>
{
    public void Configure(EntityTypeBuilder<EntityPropertyChange> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "EntityPropertyChanges", ApplicationConsts.DbSchema);

        builder.Property(item => item.PropertyName).HasMaxLength(EntityPropertyChangeConsts.PropertyNameMaxLength).IsRequired();
        builder.Property(item => item.PropertyTypeFullName).HasMaxLength(EntityPropertyChangeConsts.PropertyTypeFullNameMaxLength).IsRequired();
        builder.Property(item => item.NewValue).IsRequired(false);
        builder.Property(item => item.OriginalValue).IsRequired(false);

        builder.HasOne(item => item.AuditLog)
            .WithMany(item => item.EntityPropertyChanges)
            .HasForeignKey(item => item.AuditLogId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
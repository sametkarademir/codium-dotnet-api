using Codium.Template.Domain;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.Shared.Permissions;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "Permissions", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.NormalizedName)
            .IsUnique()
            .HasFilter($"\"{nameof(Permission.IsDeleted)}\" = FALSE");

        builder.Property(item => item.Name).HasMaxLength(PermissionConsts.NameMaxLength).IsRequired();
        builder.Property(item => item.NormalizedName).HasMaxLength(PermissionConsts.NameMaxLength).IsRequired();
    }
}
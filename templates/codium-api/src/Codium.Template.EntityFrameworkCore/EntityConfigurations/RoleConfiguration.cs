using Codium.Template.Domain;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Shared.Roles;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "Roles", ApplicationConsts.DbSchema);

        // Identity creates a non-filtered unique index on NormalizedName; replace it so soft-deleted roles do not block reuse.
        builder.HasIndex(item => item.NormalizedName)
            .HasDatabaseName("RoleNameIndex")
            .IsUnique()
            .HasFilter($"\"{nameof(Role.IsDeleted)}\" = FALSE");

        builder.Property(item => item.Name).HasMaxLength(RoleConsts.NameMaxLength).IsRequired();
        builder.Property(item => item.NormalizedName).HasMaxLength(RoleConsts.NameMaxLength).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(RoleConsts.DescriptionMaxLength).IsRequired(false);
    }
}

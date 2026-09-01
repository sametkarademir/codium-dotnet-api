using Codium.Template.Domain;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ApplyGlobalEntityConfigurations();
        builder.ToTable(ApplicationConsts.DbTablePrefix + "RolePermissions", ApplicationConsts.DbSchema);
        builder.HasIndex(item => new {item.RoleId, item.PermissionId})
            .IsUnique()
            .HasFilter($"\"{nameof(RolePermission.IsDeleted)}\" = FALSE");
        
        builder.HasOne(item => item.Permission)
            .WithMany(item => item.RolePermissions)
            .HasForeignKey(item => item.PermissionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(item => item.Role)
            .WithMany(item => item.RolePermissions)
            .HasForeignKey(item => item.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Codium.Template.Domain;
using Codium.Template.Domain.UserRoles;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ApplyGlobalEntityConfigurations();
        builder.ToTable(ApplicationConsts.DbTablePrefix + "UserRoles", ApplicationConsts.DbSchema);
        builder.HasKey(item => new { item.UserId, item.RoleId });

        builder.HasOne(item => item.User)
            .WithMany(item => item.UserRoles)
            .HasForeignKey(item => item.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.Role)
            .WithMany(item => item.UserRoles)
            .HasForeignKey(item => item.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

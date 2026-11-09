using Codium.Template.Domain;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Sessions;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "Sessions", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.IsMobile);
        builder.HasIndex(item => item.IsDesktop);
        builder.HasIndex(item => item.IsTablet);
        builder.HasIndex(item => item.ClientIp);
        builder.HasIndex(item => item.CorrelationId);
        builder.HasIndex(item => item.SnapshotId);
        
        builder.Property(item => item.IsRevoked).HasDefaultValue(false).IsRequired();
        builder.Property(item => item.RevokedTime).IsRequired(false);

        builder.Property(item => item.ClientIp).HasMaxLength(SessionConsts.ClientIpMaxLength).IsRequired();
        builder.Property(item => item.UserAgent).HasMaxLength(SessionConsts.UserAgentMaxLength).IsRequired();
        builder.Property(item => item.DeviceFamily).HasMaxLength(SessionConsts.DeviceFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.DeviceModel).HasMaxLength(SessionConsts.DeviceModelMaxLength).IsRequired(false);
        builder.Property(item => item.OsFamily).HasMaxLength(SessionConsts.OsFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.OsVersion).HasMaxLength(SessionConsts.OsVersionMaxLength).IsRequired(false);
        builder.Property(item => item.BrowserFamily).HasMaxLength(SessionConsts.BrowserFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.BrowserVersion).HasMaxLength(SessionConsts.BrowserVersionMaxLength).IsRequired(false);
        builder.Property(item => item.IsMobile).IsRequired();
        builder.Property(item => item.IsDesktop).IsRequired();
        builder.Property(item => item.IsTablet).IsRequired();

        builder.HasOne(item => item.User)
            .WithMany(item => item.Sessions)
            .HasForeignKey(item => item.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();
    }
}
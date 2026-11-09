using Codium.Template.Domain;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.Shared.RefreshTokens;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ApplyGlobalEntityConfigurations();
        builder.ToTable(ApplicationConsts.DbTablePrefix + "RefreshTokens", ApplicationConsts.DbSchema);
        builder.HasIndex(item => new { item.UserId, item.Token })
            .IsUnique()
            .HasFilter($"\"{nameof(Permission.IsDeleted)}\" = FALSE AND \"{nameof(RefreshToken.IsUsed)}\" = FALSE AND \"{nameof(RefreshToken.IsRevoked)}\" = FALSE");
        builder.HasIndex(item => new { item.UserId, item.Token, item.ExpiryTime, item.IsUsed, item.IsRevoked });

        builder.Property(item => item.Token).HasMaxLength(RefreshTokenConsts.TokenMaxLength).IsRequired();
        builder.Property(item => item.ExpiryTime).IsRequired();
        builder.Property(item => item.IsUsed).IsRequired();
        builder.Property(item => item.ReplacedByToken).HasMaxLength(RefreshTokenConsts.TokenMaxLength).IsRequired(false);
        builder.Property(item => item.IsRevoked).IsRequired();
        builder.Property(item => item.RevokedTime).IsRequired(false);

        builder.HasOne(item => item.User)
            .WithMany(item => item.RefreshTokens)
            .HasForeignKey(item => item.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(item => item.Session)
            .WithMany(item => item.RefreshTokens)
            .HasForeignKey(item => item.SessionId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}
using Codium.Template.Domain;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Shared.HttpRequestLogs;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Codium.Template.EntityFrameworkCore.EntityConfigurations;

public class HttpRequestLogConfiguration : IEntityTypeConfiguration<HttpRequestLog>
{
    public void Configure(EntityTypeBuilder<HttpRequestLog> builder)
    {
        builder.ApplyGlobalEntityConfigurations();

        builder.ToTable(ApplicationConsts.DbTablePrefix + "HttpRequestLogs", ApplicationConsts.DbSchema);
        builder.HasIndex(item => item.HttpMethod);
        builder.HasIndex(item => item.RequestPath);
        builder.HasIndex(item => item.ClientIp);
        builder.HasIndex(item => item.DeviceFamily);
        builder.HasIndex(item => item.DeviceModel);
        builder.HasIndex(item => item.OsFamily);
        builder.HasIndex(item => item.OsVersion);
        builder.HasIndex(item => item.BrowserFamily);
        builder.HasIndex(item => item.BrowserVersion);
        builder.HasIndex(item => item.ControllerName);
        builder.HasIndex(item => item.ActionName);
        builder.HasIndex(item => item.CorrelationId);
        builder.HasIndex(item => item.SessionId);

        builder.Property(item => item.HttpMethod).HasMaxLength(HttpRequestLogConsts.HttpMethodMaxLength).IsRequired(false);
        builder.Property(item => item.RequestPath).HasMaxLength(HttpRequestLogConsts.RequestPathMaxLength).IsRequired(false);
        builder.Property(item => item.QueryString).IsRequired(false);
        builder.Property(item => item.RequestBody).IsRequired(false);
        builder.Property(item => item.RequestHeaders).IsRequired(false);

        builder.Property(item => item.StatusCode).IsRequired(false);

        builder.Property(item => item.RequestTime).IsRequired();
        builder.Property(item => item.ResponseTime).IsRequired();
        builder.Property(item => item.DurationMs).IsRequired(false);

        builder.Property(item => item.ClientIp).HasMaxLength(HttpRequestLogConsts.ClientIpMaxLength).IsRequired(false);
        builder.Property(item => item.UserAgent).HasMaxLength(HttpRequestLogConsts.UserAgentMaxLength).IsRequired(false);

        builder.Property(item => item.DeviceFamily).HasMaxLength(HttpRequestLogConsts.DeviceFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.DeviceModel).HasMaxLength(HttpRequestLogConsts.DeviceModelMaxLength).IsRequired(false);
        builder.Property(item => item.OsFamily).HasMaxLength(HttpRequestLogConsts.OsFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.OsVersion).HasMaxLength(HttpRequestLogConsts.OsVersionMaxLength).IsRequired(false);
        builder.Property(item => item.BrowserFamily).HasMaxLength(HttpRequestLogConsts.BrowserFamilyMaxLength).IsRequired(false);
        builder.Property(item => item.BrowserVersion).HasMaxLength(HttpRequestLogConsts.BrowserVersionMaxLength).IsRequired(false);
        builder.Property(item => item.IsMobile).IsRequired();
        builder.Property(item => item.IsTablet).IsRequired();
        builder.Property(item => item.IsDesktop).IsRequired();

        builder.Property(item => item.ControllerName).HasMaxLength(HttpRequestLogConsts.ControllerNameMaxLength).IsRequired(false);
        builder.Property(item => item.ActionName).HasMaxLength(HttpRequestLogConsts.ActionNameMaxLength).IsRequired(false);
    }
}
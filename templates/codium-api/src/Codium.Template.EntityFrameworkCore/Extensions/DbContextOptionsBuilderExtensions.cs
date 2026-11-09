using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Extensions;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseEntityMetadataTracking(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new EntityAuditInterceptor());
        return optionsBuilder;
    }
    
    public static DbContextOptionsBuilder UseAuditLog(this DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(new AuditLogSaveChangesInterceptor());
        return optionsBuilder;
    }
}
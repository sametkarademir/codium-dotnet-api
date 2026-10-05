using System.Reflection;
using Codium.Template.Domain;
using Codium.Template.Domain.AuditLogs;
using Codium.Template.Domain.EntityPropertyChanges;
using Codium.Template.Domain.HttpRequestLogs;
using Codium.Template.Domain.Permissions;
using Codium.Template.Domain.RefreshTokens;
using Codium.Template.Domain.RolePermissions;
using Codium.Template.Domain.Roles;
using Codium.Template.Domain.Sessions;
using Codium.Template.Domain.Shared.Extensions;
using Codium.Template.Domain.UserRoles;
using Codium.Template.Domain.Users;
using Codium.Template.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Codium.Template.EntityFrameworkCore.Contexts;

public class ApplicationDbContext : IdentityDbContext<
    User,
    Role,
    Guid,
    IdentityUserClaim<Guid>,
    UserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public Guid? CurrentTenantId => _httpContextAccessor?.HttpContext?.User.GetTenantId();

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<EntityPropertyChange> EntityPropertyChanges { get; set; }
    public DbSet<HttpRequestLog> HttpRequestLogs { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Session> Sessions { get; set; }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Claims, logins and tokens are created by Identity but unused in this phase; keep them in the template's naming scheme.
        builder.Entity<IdentityUserClaim<Guid>>().ToTable(ApplicationConsts.DbTablePrefix + "UserClaims", ApplicationConsts.DbSchema);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable(ApplicationConsts.DbTablePrefix + "RoleClaims", ApplicationConsts.DbSchema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable(ApplicationConsts.DbTablePrefix + "UserLogins", ApplicationConsts.DbSchema);
        builder.Entity<IdentityUserToken<Guid>>().ToTable(ApplicationConsts.DbTablePrefix + "UserTokens", ApplicationConsts.DbSchema);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.ApplyMultiTenantQueryFilters(this);
    }
}
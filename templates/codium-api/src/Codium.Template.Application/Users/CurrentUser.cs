using System.Security.Claims;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Codium.Template.Application.Users;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    public Guid? Id => httpContextAccessor.HttpContext?.User.GetUserId();
    public string? Email => httpContextAccessor.HttpContext?.User.GetUserEmail();

    public List<string>? Roles => httpContextAccessor.HttpContext?.User.GetRoles();
    public bool HasRole(string role) => httpContextAccessor.HttpContext?.User.HasRole(role) ?? false;
    
    public List<string>? Permissions => httpContextAccessor.HttpContext?.User.GetPermissions();
    public bool HasPermission(string permission) => httpContextAccessor.HttpContext?.User.HasPermission(permission) ?? false;

    public Guid? SessionId => httpContextAccessor.HttpContext?.User.GetSessionId();
    
    public ClaimsPrincipal? User() => httpContextAccessor.HttpContext?.User;
}
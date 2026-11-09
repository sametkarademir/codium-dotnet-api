using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Codium.Template.HttpApi.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class PermissionAuthorizeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissions;

    public PermissionAuthorizeAttribute(params string[] permissions)
    {
        _permissions = permissions ?? [];
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity == null || !context.HttpContext.User.Identity.IsAuthenticated)
        {
            throw new AppUnauthorizedException();
        }

        if (_permissions.Length == 0)
        {
            return;
        }

        var permissions = context.HttpContext.User.GetPermissions();
        var hasPermission = _permissions.Any(
            p => permissions.Contains(p, StringComparer.OrdinalIgnoreCase)
        );

        if (!hasPermission)
        {
            throw new AppForbiddenException();
        }
    }
}
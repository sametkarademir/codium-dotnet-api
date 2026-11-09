using System.Security.Claims;

namespace Codium.Template.Application.Contracts.Users;

public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    Guid? Id { get; }
    string? Email { get; }
    List<string>? Roles { get; }
    bool HasRole(string role);
    List<string>? Permissions { get; }
    bool HasPermission(string permission);
    Guid? SessionId { get; }

    ClaimsPrincipal? User();
}
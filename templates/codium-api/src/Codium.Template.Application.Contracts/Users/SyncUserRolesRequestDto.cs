namespace Codium.Template.Application.Contracts.Users;

public class SyncUserRolesRequestDto
{
    public List<Guid> RoleIds { get; set; } = [];
}


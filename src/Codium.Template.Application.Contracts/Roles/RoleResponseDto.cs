using Codium.Template.Application.Contracts.BaseEntities;
using Codium.Template.Application.Contracts.Permissions;

namespace Codium.Template.Application.Contracts.Roles;

public class RoleResponseDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public List<PermissionResponseDto> Permissions { get; set; } = [];
}
using Codium.Template.Application.Contracts.BaseEntities;

namespace Codium.Template.Application.Contracts.Permissions;

public class PermissionResponseDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
}
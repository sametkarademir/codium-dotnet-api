using Codium.Template.Application.Contracts.BaseEntities;
using Codium.Template.Application.Contracts.Roles;

namespace Codium.Template.Application.Contracts.Users;

public class UserResponseDto : EntityDto<Guid>
{
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }

    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public int AccessFailedCount { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? PasswordChangedTime { get; set; }
    public bool IsActive { get; set; }

    public List<RoleResponseDto> Roles { get; set; } = [];
}
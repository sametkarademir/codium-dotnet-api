using Codium.Template.Application.Contracts.BaseEntities;

namespace Codium.Template.Application.Contracts.Profiles;

public class ProfileResponseDto : EntityDto<Guid>
{
    public string Email { get; set; } = null!;
    public bool EmailConfirmed { get; set; }
    public bool ShouldChangePasswordOnNextLogin { get; set; }
    public DateTime? PasswordChangedTime { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}
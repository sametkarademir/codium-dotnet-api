using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Profiles;

public class ChangePasswordUserRequestDto
{
    public string OldPassword { get; set; } = null!;
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ChangePasswordUserRequestDtoValidator : AbstractValidator<ChangePasswordUserRequestDto>
{
    public ChangePasswordUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.OldPassword)
            .NotEmpty().WithMessage(localizer["ChangePasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength]);

        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ChangePasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength).WithMessage(localizer["ChangePasswordUserRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength]);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ChangePasswordUserRequestDto:PasswordConfirm:MustMatchPassword"]);
    }
}
using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class ResetPasswordUserRequestDto
{
    public string NewPassword { get; set; } = null!;
    public string ConfirmNewPassword { get; set; } = null!;
}

public class ResetPasswordUserRequestDtoValidator : AbstractValidator<ResetPasswordUserRequestDto>
{
    public ResetPasswordUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.NewPassword)
            .NotEmpty().WithMessage(localizer["ResetPasswordUserRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength).WithMessage(localizer["ResetPasswordUserRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength).WithMessage(localizer["ResetPasswordUserRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength])
            .MustSatisfyPasswordPolicy(localizer);
        
        RuleFor(item => item.ConfirmNewPassword)
            .Equal(item => item.NewPassword).WithMessage(localizer["ResetPasswordUserRequestDto:ConfirmPassword:MustMatchPassword"]);
    }
}
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Auth;

public class LoginRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["LoginRequestDto:Email:IsRequired"])
            .EmailAddress().WithMessage(localizer["LoginRequestDto:Email:Invalid"])
            .MaximumLength(UserConsts.EmailMaxLength).WithMessage(localizer["LoginRequestDto:Email:MaxLength", UserConsts.EmailMaxLength]);
        
        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["LoginRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength)
            .WithMessage(localizer["LoginRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength)
            .WithMessage(localizer["LoginRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength]);
    }
}
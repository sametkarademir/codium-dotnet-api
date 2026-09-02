using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class CreateUserRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string ConfirmPassword { get; set; } = null!;

    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateUserRequestDtoValidator : AbstractValidator<CreateUserRequestDto>
{
    public CreateUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Email)
            .NotEmpty().WithMessage(localizer["CreateUserRequestDto:Email:IsRequired"])
            .MaximumLength(UserConsts.EmailMaxLength).WithMessage(localizer["CreateUserRequestDto:Email:MaxLength", UserConsts.EmailMaxLength])
            .EmailAddress().WithMessage(localizer["CreateUserRequestDto:Email:InvalidFormat"]);
        
        RuleFor(item => item.PhoneNumber) 
            .MaximumLength(UserConsts.PhoneNumberMaxLength).WithMessage(localizer["CreateUserRequestDto:PhoneNumber:MaxLength", UserConsts.PhoneNumberMaxLength])
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(localizer["CreateUserRequestDto:PhoneNumber:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.PhoneNumber));

        RuleFor(item => item.Password)
            .NotEmpty().WithMessage(localizer["CreateUserRequestDto:Password:IsRequired"])
            .MinimumLength(UserConsts.PasswordRequiredLength)
            .WithMessage(localizer["CreateUserRequestDto:Password:MinLength", UserConsts.PasswordRequiredLength])
            .MaximumLength(UserConsts.PasswordMaxLength)
            .WithMessage(localizer["CreateUserRequestDto:Password:MaxLength", UserConsts.PasswordMaxLength]);
        
        RuleFor(item => item.ConfirmPassword)
            .Equal(item => item.Password).WithMessage(localizer["CreateUserRequestDto:ConfirmPassword:MustMatchPassword"]);
        
        RuleFor(item => item.FirstName) 
            .MaximumLength(UserConsts.FirstNameMaxLength).WithMessage(localizer["CreateUserRequestDto:FirstName:MaxLength", UserConsts.FirstNameMaxLength])
            .When(item => !string.IsNullOrWhiteSpace(item.FirstName));
        
        RuleFor(item => item.LastName) 
            .MaximumLength(UserConsts.LastNameMaxLength).WithMessage(localizer["CreateUserRequestDto:LastName:MaxLength", UserConsts.LastNameMaxLength])
            .When(item => !string.IsNullOrWhiteSpace(item.LastName));
    }
}
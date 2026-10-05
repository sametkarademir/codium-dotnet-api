
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class UpdateUserRequestDto
{
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateUserRequestDtoValidator : AbstractValidator<UpdateUserRequestDto>
{
    public UpdateUserRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.PhoneNumber) 
            .MaximumLength(UserConsts.PhoneNumberMaxLength).WithMessage(localizer["UpdateUserRequestDto:PhoneNumber:MaxLength", UserConsts.PhoneNumberMaxLength])
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage(localizer["UpdateUserRequestDto:PhoneNumber:InvalidFormat"])
            .When(item => !string.IsNullOrWhiteSpace(item.PhoneNumber));

        RuleFor(item => item.FirstName) 
            .MaximumLength(UserConsts.FirstNameMaxLength).WithMessage(localizer["UpdateUserRequestDto:FirstName:MaxLength", UserConsts.FirstNameMaxLength])
            .When(item => !string.IsNullOrWhiteSpace(item.FirstName));
        
        RuleFor(item => item.LastName) 
            .MaximumLength(UserConsts.LastNameMaxLength).WithMessage(localizer["UpdateUserRequestDto:LastName:MaxLength", UserConsts.LastNameMaxLength])
            .When(item => !string.IsNullOrWhiteSpace(item.LastName));
    }
}
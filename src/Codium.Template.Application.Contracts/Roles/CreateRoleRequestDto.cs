using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Roles;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Roles;

public class CreateRoleRequestDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}

public class CreateRoleRequestDtoValidator : AbstractValidator<CreateRoleRequestDto>
{
    public CreateRoleRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Name)
            .NotEmpty().WithMessage(localizer["CreateRoleRequestDto:Name:NotEmpty"])
            .MaximumLength(RoleConsts.NameMaxLength).WithMessage(localizer["CreateRoleRequestDto:Name:MaxLength", RoleConsts.NameMaxLength]);
        
        RuleFor(item => item.Description)
            .MaximumLength(RoleConsts.DescriptionMaxLength).WithMessage(localizer["CreateRoleRequestDto:Description:MaxLength", RoleConsts.DescriptionMaxLength]);
    }
}
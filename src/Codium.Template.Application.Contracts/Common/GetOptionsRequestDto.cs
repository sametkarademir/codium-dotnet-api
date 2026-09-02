using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Common;

public class GetOptionsRequestDto
{
    public string? Search { get; set; }
}

public class GetOptionsRequestDtoValidator : AbstractValidator<GetOptionsRequestDto>
{
    public GetOptionsRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.Search)
            .MaximumLength(32).WithMessage(localizer["GetList:Search:MaxLength", 32]);
    }
}

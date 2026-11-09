using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Sessions;

public class GetListSessionsRequestDto : GetListRequestDto
{
    public bool? IsMobile { get; set; }
    public bool? IsDesktop { get; set; }
    public bool? IsTablet { get; set; }
}

public class GetListSessionsRequestDtoValidator : AbstractValidator<GetListSessionsRequestDto>
{
    public GetListSessionsRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        Include(new GetListRequestDtoValidator(localizer));
    }
}
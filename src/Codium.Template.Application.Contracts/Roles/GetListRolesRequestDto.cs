using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Roles;

public class GetListRolesRequestDto : GetListRequestDto
{

}

public class GetListRolesRequestDtoValidator : AbstractValidator<GetListRolesRequestDto>
{
    public GetListRolesRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        Include(new GetListRequestDtoValidator(localizer));
    }
}
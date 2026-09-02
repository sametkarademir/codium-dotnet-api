using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Permissions;

public class GetListPermissionsRequestDto : GetListRequestDto
{

}

public class GetListPermissionsRequestDtoValidator : AbstractValidator<GetListPermissionsRequestDto>
{
    public GetListPermissionsRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        Include(new GetListRequestDtoValidator(localizer));
    }
}

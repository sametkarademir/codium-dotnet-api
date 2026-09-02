using Codium.Template.Application.Contracts.Common;
using Codium.Template.Domain.Shared.Localization;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Users;

public class GetListUsersRequestDto : GetListRequestDto
{
    public bool? IsActive { get; set; }
}

public class GetListUsersRequestDtoValidator : AbstractValidator<GetListUsersRequestDto>
{
    public GetListUsersRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        Include(new GetListRequestDtoValidator(localizer));
    }
}
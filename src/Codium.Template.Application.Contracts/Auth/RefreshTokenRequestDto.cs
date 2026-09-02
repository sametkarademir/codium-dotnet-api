using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.RefreshTokens;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Auth;

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = null!;
}

public class RefreshTokenRequestDtoValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestDtoValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        RuleFor(item => item.RefreshToken)
            .NotEmpty().WithMessage(localizer["RefreshTokenRequestDto:RefreshToken:NotEmpty"])
            .MaximumLength(RefreshTokenConsts.TokenMaxLength).WithMessage(localizer["RefreshTokenRequestDto:RefreshToken:MaxLength", RefreshTokenConsts.TokenMaxLength]);
    }
}
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Contracts.Extensions;

public static class PasswordRuleBuilderExtensions
{
    /// <summary>
    /// Adds the character rules of the password policy (digit, lowercase, uppercase, special character, unique characters)
    /// with localized messages. Length rules stay on each DTO validator. The checks mirror ASP.NET Core Identity's
    /// PasswordValidator (ASCII letters and digits) so a password accepted here is accepted by Identity as well;
    /// Identity remains the authority when the password is saved.
    /// </summary>
    public static IRuleBuilderOptions<T, string> MustSatisfyPasswordPolicy<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        IStringLocalizer<ApplicationResource> localizer)
    {
        return ruleBuilder
            .Must(password => string.IsNullOrEmpty(password) || password.Any(IsDigit))
            .When(_ => UserConsts.PasswordRequireDigit, ApplyConditionTo.CurrentValidator)
            .WithMessage(localizer["PasswordValidator:Password:RequireDigit"])
            .Must(password => string.IsNullOrEmpty(password) || password.Any(IsLower))
            .When(_ => UserConsts.PasswordRequireLowercase, ApplyConditionTo.CurrentValidator)
            .WithMessage(localizer["PasswordValidator:Password:RequireLowercase"])
            .Must(password => string.IsNullOrEmpty(password) || password.Any(IsUpper))
            .When(_ => UserConsts.PasswordRequireUppercase, ApplyConditionTo.CurrentValidator)
            .WithMessage(localizer["PasswordValidator:Password:RequireUppercase"])
            .Must(password => string.IsNullOrEmpty(password) || password.Any(c => !IsLetterOrDigit(c)))
            .When(_ => UserConsts.PasswordRequireNonAlphanumeric, ApplyConditionTo.CurrentValidator)
            .WithMessage(localizer["PasswordValidator:Password:RequireNonAlphanumeric"])
            .Must(password => string.IsNullOrEmpty(password) || password.Distinct().Count() >= UserConsts.PasswordRequiredUniqueChars)
            .When(_ => UserConsts.PasswordRequiredUniqueChars > 0, ApplyConditionTo.CurrentValidator)
            .WithMessage(localizer["PasswordValidator:Password:RequireUniqueChars", UserConsts.PasswordRequiredUniqueChars]);
    }

    private static bool IsDigit(char c) => c is >= '0' and <= '9';
    private static bool IsLower(char c) => c is >= 'a' and <= 'z';
    private static bool IsUpper(char c) => c is >= 'A' and <= 'Z';
    private static bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);
}

using Codium.Template.Application.Contracts.Common.Results;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Exceptions;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.Domain.Shared.Users;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Application.Users;

public class PasswordValidator : IPasswordValidator
{
    private readonly IStringLocalizer<ApplicationResource> _localizer;

    public PasswordValidator(IStringLocalizer<ApplicationResource> localizer)
    {
        _localizer = localizer;
    }

    public ValidationResult Validate(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        
        var errors = new List<string>();
        
        // Length validation
        if (password.Length < UserConsts.PasswordRequiredLength)
        {
            errors.Add(_localizer["PasswordValidator:Password:MinLength", UserConsts.PasswordRequiredLength]);
        }
        
        if (password.Length > UserConsts.PasswordMaxLength)
        {
            
            errors.Add(_localizer["PasswordValidator:Password:MaxLength", UserConsts.PasswordMaxLength]);
        }
        
        // Digit validation
        if (UserConsts.PasswordRequireDigit && !password.Any(char.IsDigit))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireDigit"]);
        }
        
        // Lowercase validation
        if (UserConsts.PasswordRequireLowercase && !password.Any(char.IsLower))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireLowercase"]);
        }
        
        // Uppercase validation
        if (UserConsts.PasswordRequireUppercase && !password.Any(char.IsUpper))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireUppercase"]);
        }
        
        // Non-alphanumeric validation
        if (UserConsts.PasswordRequireNonAlphanumeric && !password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireNonAlphanumeric"]);
        }
        
        // Check for unique characters
        if (UserConsts.PasswordRequiredUniqueChars > 0 && password.Distinct().Count() < UserConsts.PasswordRequiredUniqueChars)
        {
            errors.Add(_localizer["PasswordValidator:Password:RequireUniqueChars", UserConsts.PasswordRequiredUniqueChars]);
        }

        return errors.Count != 0
            ? ValidationResult.Failed(
                new List<ValidationExceptionModel> { 
                    new()
                    {
                        Property = "Password",
                        Errors = errors
                    }
                }
            )
            : ValidationResult.Success;
    }
}
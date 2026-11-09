using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.Domain.Users;
using Codium.Template.UnitTests.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.UnitTests.Contracts.Extensions;

public class PasswordPolicyTests
{
    private sealed class PasswordModel
    {
        public string Password { get; set; } = null!;
    }

    private static InlineValidator<PasswordModel> CreateFluentValidator(KeyLocalizer localizer)
    {
        var validator = new InlineValidator<PasswordModel>();
        validator.RuleFor(item => item.Password).MustSatisfyPasswordPolicy(localizer);
        return validator;
    }

    private static IdentityOptions IdentityOptionsFromConsts() => new()
    {
        Password =
        {
            RequiredLength = UserConsts.PasswordRequiredLength,
            RequiredUniqueChars = UserConsts.PasswordRequiredUniqueChars,
            RequireDigit = UserConsts.PasswordRequireDigit,
            RequireLowercase = UserConsts.PasswordRequireLowercase,
            RequireUppercase = UserConsts.PasswordRequireUppercase,
            RequireNonAlphanumeric = UserConsts.PasswordRequireNonAlphanumeric
        }
    };

    // All candidates are at least RequiredLength long: the extension only owns the character rules.
    public static TheoryData<string> Candidates => new()
    {
        "Abcdef1!",       // satisfies everything
        "abcdef1!",       // no uppercase
        "ABCDEF1!",       // no lowercase
        "Abcdefg!",       // no digit
        "Abcdefg1",       // no special character
        "alllowercase",   // only lowercase
        "12345678",       // only digits
        "!@#$%^&*",       // only symbols
        "ğüşiöçı1",       // non-ASCII letters count as special characters for Identity (ASCII-only letter checks)
        "ğüşöçı1A!",      // lowercase letters are all non-ASCII: no lowercase for Identity
        "ĞÜŞÖÇİ1a!",      // uppercase letters are all non-ASCII: no uppercase for Identity
        "Abcdefg1Ä",      // non-ASCII letter satisfies the special-character rule
        "Pp123456*",      // seeded admin password
        "Aaaaaaa1!",      // repeated characters
        "        ",       // spaces only: space is a special character, nothing else
    };

    [Theory]
    [MemberData(nameof(Candidates))]
    public async Task FluentPolicy_AgreesWithIdentityPasswordValidator(string password)
    {
        var localizer = new KeyLocalizer();
        var fluentResult = await CreateFluentValidator(localizer).ValidateAsync(new PasswordModel { Password = password });

        var userManager = IdentityMocks.UserManager(IdentityOptionsFromConsts());
        var identityResult = await new PasswordValidator<User>().ValidateAsync(userManager, new User(), password);

        Assert.Equal(identityResult.Succeeded, fluentResult.IsValid);
    }

    [Fact]
    public async Task ValidPassword_ProducesNoErrors()
    {
        var result = await CreateFluentValidator(new KeyLocalizer()).ValidateAsync(new PasswordModel { Password = "Abcdef1!" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task AllLowercasePassword_ReportsDigitUppercaseAndSpecialCharacter()
    {
        var result = await CreateFluentValidator(new KeyLocalizer()).ValidateAsync(new PasswordModel { Password = "alllowercase" });

        Assert.Equal(
            [
                "PasswordValidator:Password:RequireDigit",
                "PasswordValidator:Password:RequireUppercase",
                "PasswordValidator:Password:RequireNonAlphanumeric"
            ],
            result.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public async Task EmptyPassword_IsLeftToTheRequiredRule()
    {
        var result = await CreateFluentValidator(new KeyLocalizer()).ValidateAsync(new PasswordModel { Password = "" });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task NullPassword_DoesNotThrow()
    {
        var result = await CreateFluentValidator(new KeyLocalizer()).ValidateAsync(new PasswordModel { Password = null! });

        Assert.True(result.IsValid);
    }
}

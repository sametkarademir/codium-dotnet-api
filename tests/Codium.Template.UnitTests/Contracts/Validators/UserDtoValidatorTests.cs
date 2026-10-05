using Codium.Template.Application.Contracts.Auth;
using Codium.Template.Application.Contracts.Profiles;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Users;
using Codium.Template.UnitTests.Infrastructure;
using FluentValidation.TestHelper;

namespace Codium.Template.UnitTests.Contracts.Validators;

public class UserDtoValidatorTests
{
    private const string StrongPassword = "Abcdef1!";

    private static CreateUserRequestDto ValidCreateRequest() => new()
    {
        Email = "user@example.com",
        Password = StrongPassword,
        ConfirmPassword = StrongPassword
    };

    // ---- CreateUserRequestDto

    [Fact]
    public void Create_ValidRequest_HasNoErrors()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(ValidCreateRequest()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Create_WeakPassword_ReportsEveryMissingCharacterClassOnPassword()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.Password = request.ConfirmPassword = "alllowercase";

        var result = validator.TestValidate(request);

        var messages = result.ShouldHaveValidationErrorFor(item => item.Password).Select(e => e.ErrorMessage).ToList();
        Assert.Equal(3, messages.Count);
        Assert.Contains("PasswordValidator:Password:RequireDigit", messages);
        Assert.Contains("PasswordValidator:Password:RequireUppercase", messages);
        Assert.Contains("PasswordValidator:Password:RequireNonAlphanumeric", messages);
    }

    [Fact]
    public void Create_ShortPassword_ReportsMinLengthAndCharacterRules()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.Password = request.ConfirmPassword = "short";

        var result = validator.TestValidate(request);

        var messages = result.ShouldHaveValidationErrorFor(item => item.Password).Select(e => e.ErrorMessage).ToList();
        Assert.Contains("CreateUserRequestDto:Password:MinLength", messages);
        Assert.Contains("PasswordValidator:Password:RequireDigit", messages);
    }

    [Fact]
    public void Create_TooLongPassword_ReportsMaxLength()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.Password = request.ConfirmPassword = "Aa1!" + new string('x', UserConsts.PasswordMaxLength);

        var result = validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(item => item.Password)
            .WithErrorMessage("CreateUserRequestDto:Password:MaxLength");
    }

    [Fact]
    public void Create_PasswordConfirmationMismatch_IsReportedOnConfirmPassword()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.ConfirmPassword = "Different1!";

        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(item => item.ConfirmPassword)
            .WithErrorMessage("CreateUserRequestDto:ConfirmPassword:MustMatchPassword");
    }

    [Theory]
    [InlineData("", "CreateUserRequestDto:Email:IsRequired")]
    [InlineData("not-an-email", "CreateUserRequestDto:Email:InvalidFormat")]
    public void Create_InvalidEmail_IsReported(string email, string expectedKey)
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.Email = email;

        validator.TestValidate(request)
            .ShouldHaveValidationErrorFor(item => item.Email)
            .WithErrorMessage(expectedKey);
    }

    [Theory]
    [InlineData("+905551112233", true)]
    [InlineData("905551112233", true)]
    [InlineData("0555111", false)]
    [InlineData("+90 555 111 22 33", false)]
    public void Create_PhoneNumber_MustMatchTheInternationalFormat(string phone, bool valid)
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.PhoneNumber = phone;

        var result = validator.TestValidate(request);

        if (valid) result.ShouldNotHaveValidationErrorFor(item => item.PhoneNumber);
        else result.ShouldHaveValidationErrorFor(item => item.PhoneNumber);
    }

    [Fact]
    public void Create_EmptyPhoneNumber_IsAllowed()
    {
        var validator = new CreateUserRequestDtoValidator(new KeyLocalizer());
        var request = ValidCreateRequest();
        request.PhoneNumber = "";

        validator.TestValidate(request).ShouldNotHaveValidationErrorFor(item => item.PhoneNumber);
    }

    // ---- ResetPasswordUserRequestDto

    [Fact]
    public void Reset_WeakPassword_IsReportedOnNewPassword()
    {
        var validator = new ResetPasswordUserRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new ResetPasswordUserRequestDto
        {
            NewPassword = "alllowercase",
            ConfirmNewPassword = "alllowercase"
        });

        Assert.Equal(3, result.ShouldHaveValidationErrorFor(item => item.NewPassword).Count());
    }

    [Fact]
    public void Reset_ValidRequest_HasNoErrors()
    {
        var validator = new ResetPasswordUserRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new ResetPasswordUserRequestDto { NewPassword = StrongPassword, ConfirmNewPassword = StrongPassword })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Reset_ConfirmationMismatch_IsReported()
    {
        var validator = new ResetPasswordUserRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new ResetPasswordUserRequestDto { NewPassword = StrongPassword, ConfirmNewPassword = "Other1234!" })
            .ShouldHaveValidationErrorFor(item => item.ConfirmNewPassword);
    }

    // ---- ChangePasswordUserRequestDto

    [Fact]
    public void Change_WeakNewPassword_IsReported_ButOldPasswordOnlyNeedsValidLength()
    {
        var validator = new ChangePasswordUserRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new ChangePasswordUserRequestDto
        {
            OldPassword = "alllowercase",
            NewPassword = "alllowercase",
            ConfirmNewPassword = "alllowercase"
        });

        result.ShouldHaveValidationErrorFor(item => item.NewPassword);
        result.ShouldNotHaveValidationErrorFor(item => item.OldPassword);
    }

    [Fact]
    public void Change_ValidRequest_HasNoErrors()
    {
        var validator = new ChangePasswordUserRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new ChangePasswordUserRequestDto
        {
            OldPassword = "OldPass1!",
            NewPassword = StrongPassword,
            ConfirmNewPassword = StrongPassword
        }).ShouldNotHaveAnyValidationErrors();
    }

    // ---- UpdateUserRequestDto

    [Fact]
    public void Update_NamesAndPhone_AreLengthChecked()
    {
        var validator = new UpdateUserRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new UpdateUserRequestDto
        {
            FirstName = new string('a', UserConsts.FirstNameMaxLength + 1),
            LastName = new string('b', UserConsts.LastNameMaxLength + 1),
            PhoneNumber = "+" + new string('1', UserConsts.PhoneNumberMaxLength + 1)
        });

        result.ShouldHaveValidationErrorFor(item => item.FirstName);
        result.ShouldHaveValidationErrorFor(item => item.LastName);
        result.ShouldHaveValidationErrorFor(item => item.PhoneNumber);
    }

    // ---- LoginRequestDto (length only; the character policy is not applied to existing passwords)

    [Fact]
    public void Login_ValidRequest_HasNoErrors()
    {
        var validator = new LoginRequestDtoValidator(new KeyLocalizer());

        validator.TestValidate(new LoginRequestDto { Email = "user@example.com", Password = "alllowercase" })
            .ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Login_InvalidEmailAndShortPassword_AreReported()
    {
        var validator = new LoginRequestDtoValidator(new KeyLocalizer());

        var result = validator.TestValidate(new LoginRequestDto { Email = "bad", Password = "x" });

        result.ShouldHaveValidationErrorFor(item => item.Email);
        result.ShouldHaveValidationErrorFor(item => item.Password);
    }
}

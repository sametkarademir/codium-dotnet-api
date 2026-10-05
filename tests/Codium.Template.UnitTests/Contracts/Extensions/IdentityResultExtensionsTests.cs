using Codium.Template.Application.Contracts.Extensions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.UnitTests.Contracts.Extensions;

public class IdentityResultExtensionsTests
{
    private static readonly IdentityErrorDescriber Describer = new();

    [Fact]
    public void ThrowIfFailed_OnSuccess_DoesNotThrow()
    {
        IdentityResult.Success.ThrowIfFailed("duplicate");
        IdentityResult.Success.ThrowIfFailed();
    }

    public static TheoryData<IdentityError> DuplicateErrors => new()
    {
        Describer.DuplicateRoleName("Admin"),
        Describer.DuplicateUserName("a@b.com"),
        Describer.DuplicateEmail("a@b.com"),
    };

    [Theory]
    [MemberData(nameof(DuplicateErrors))]
    public void ThrowIfFailed_DuplicateErrorWithMessage_ThrowsConflict(IdentityError error)
    {
        var exception = Assert.Throws<AppConflictException>(() =>
            IdentityResult.Failed(error).ThrowIfFailed("already exists"));

        Assert.Equal(409, exception.StatusCode);
        Assert.Equal("already exists", exception.Message);
    }

    [Fact]
    public void ThrowIfFailed_DuplicateErrorWithoutMessage_ThrowsValidation()
    {
        var error = Describer.DuplicateRoleName("Admin");

        var exception = Assert.Throws<AppValidationException>(() => IdentityResult.Failed(error).ThrowIfFailed());

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal(error.Description, exception.Message);
    }

    [Fact]
    public void ThrowIfFailed_OtherErrors_ThrowValidationWithAllDescriptions()
    {
        var result = IdentityResult.Failed(
            new IdentityError { Code = "A", Description = "first" },
            new IdentityError { Code = "B", Description = "second" });

        var exception = Assert.Throws<AppValidationException>(() => result.ThrowIfFailed("already exists"));

        Assert.Equal("first; second", exception.Message);
    }
}

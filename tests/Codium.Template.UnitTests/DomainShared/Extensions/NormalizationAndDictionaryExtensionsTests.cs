using Codium.Template.Domain.Shared.Extensions;

namespace Codium.Template.UnitTests.DomainShared.Extensions;

public class NormalizationAndDictionaryExtensionsTests
{
    [Theory]
    [InlineData("User.View", "USER.VIEW")]
    [InlineData("  keep spaces ", "  KEEP SPACES ")]
    [InlineData("Çağlar Şöför", "CAGLAR SOFOR")]
    [InlineData("ÀÉÎõü", "AEIOU")]
    public void NormalizeValue_RemovesDiacriticsAndUppercases(string input, string expected)
    {
        Assert.Equal(expected, input.NormalizeValue());
    }

    [Fact]
    public void NormalizeValue_DotlessI_IsNotUppercased()
    {
        // Characterization of current behavior: ToUpperInvariant leaves the Turkish dotless "ı" as is, so "ı" and "I" normalize
        // differently. Permission names are ASCII, which is why this has no effect today; documented here so a change is deliberate.
        Assert.Equal("ı", "ı".NormalizeValue());
        Assert.Equal("I", "I".NormalizeValue());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void NormalizeValue_EmptyOrNull_IsReturnedAsIs(string? input)
    {
        Assert.Equal(input, input!.NormalizeValue());
    }

    [Fact]
    public void CamelizeKeys_LowercasesTheFirstLetterOfEveryKey_AndKeepsValues()
    {
        var source = new Dictionary<string, object?>
        {
            ["UserName"] = "ada",
            ["Age"] = 36,
            ["nickName"] = null
        };

        var result = source.CamelizeKeys();

        Assert.Equal(["userName", "age", "nickName"], result.Keys);
        Assert.Equal("ada", result["userName"]);
        Assert.Equal(36, result["age"]);
        Assert.Null(result["nickName"]);
    }
}

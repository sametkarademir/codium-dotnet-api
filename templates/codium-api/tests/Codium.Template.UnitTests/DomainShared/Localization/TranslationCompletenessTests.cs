using System.Reflection;
using System.Text.Json;
using Codium.Template.Application.Contracts.Users;
using Codium.Template.Domain.Shared.Localization;
using Codium.Template.UnitTests.Infrastructure;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Codium.Template.UnitTests.DomainShared.Localization;

/// <summary>
/// The JSON localizer silently falls back to the key when a translation is missing, so a missing key only shows up
/// as a raw key in an API response. These tests make that visible.
/// </summary>
public class TranslationCompletenessTests
{
    public static TheoryData<string> Cultures => new() { "en", "tr" };

    private static Dictionary<string, string> LoadTranslations(string culture)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Resources", "Localization", $"{culture}.json");
        return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
    }

    private static List<string> KeysRequestedByValidators()
    {
        var localizer = new KeyLocalizer();
        var validatorTypes = typeof(CreateUserRequestDtoValidator).Assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsClass: true } && IsValidator(t))
            .Where(t => t.GetConstructors().Any(c =>
                c.GetParameters() is [{ } p] && p.ParameterType == typeof(IStringLocalizer<ApplicationResource>)))
            .ToList();

        foreach (var type in validatorTypes)
        {
            Activator.CreateInstance(type, localizer);
        }

        return localizer.RequestedKeys.Distinct().ToList();
    }

    private static bool IsValidator(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(AbstractValidator<>))
            {
                return true;
            }
        }

        return false;
    }

    [Fact]
    public void ValidatorDiscovery_FindsTheValidatorsOfTheContractsAssembly()
    {
        // Guards the reflection above: if it silently found nothing, the completeness tests would pass vacuously.
        Assert.True(KeysRequestedByValidators().Count > 30);
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void EveryKeyUsedByAValidator_IsTranslated(string culture)
    {
        var translations = LoadTranslations(culture);

        var missing = KeysRequestedByValidators().Where(key => !translations.ContainsKey(key)).ToList();

        Assert.True(missing.Count == 0, $"Missing in {culture}.json: {string.Join(", ", missing)}");
    }

    [Fact]
    public void EnglishAndTurkishFiles_HaveTheSameKeys()
    {
        var english = LoadTranslations("en").Keys.ToHashSet();
        var turkish = LoadTranslations("tr").Keys.ToHashSet();

        Assert.Empty(english.Except(turkish));
        Assert.Empty(turkish.Except(english));
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Translations_AreNotEmpty(string culture)
    {
        var empty = LoadTranslations(culture).Where(kvp => string.IsNullOrWhiteSpace(kvp.Value)).Select(kvp => kvp.Key).ToList();

        Assert.True(empty.Count == 0, $"Empty translations in {culture}.json: {string.Join(", ", empty)}");
    }

    [Theory]
    [MemberData(nameof(Cultures))]
    public void FormatPlaceholders_MatchBetweenCultures(string culture)
    {
        var english = LoadTranslations("en");
        var other = LoadTranslations(culture);

        static string[] Placeholders(string text) =>
            System.Text.RegularExpressions.Regex.Matches(text, @"\{\d+\}").Select(m => m.Value).Distinct().Order().ToArray();

        var mismatched = english
            .Where(kvp => other.TryGetValue(kvp.Key, out var translated) && !Placeholders(kvp.Value).SequenceEqual(Placeholders(translated)))
            .Select(kvp => kvp.Key)
            .ToList();

        Assert.True(mismatched.Count == 0, $"Placeholder mismatch in {culture}.json: {string.Join(", ", mismatched)}");
    }
}

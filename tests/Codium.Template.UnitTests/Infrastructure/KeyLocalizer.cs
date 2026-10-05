using Codium.Template.Domain.Shared.Localization;
using Microsoft.Extensions.Localization;

namespace Codium.Template.UnitTests.Infrastructure;

/// <summary>
/// Localizer for tests: returns the requested key as the message and remembers every key that was asked for,
/// so validator messages can be asserted without translation files and used keys can be checked against them.
/// </summary>
public sealed class KeyLocalizer : IStringLocalizer<ApplicationResource>
{
    private readonly List<string> _requestedKeys = [];

    public IReadOnlyList<string> RequestedKeys => _requestedKeys;

    public LocalizedString this[string name] => Resolve(name);

    public LocalizedString this[string name, params object[] arguments] => Resolve(name);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => [];

    private LocalizedString Resolve(string name)
    {
        _requestedKeys.Add(name);
        return new LocalizedString(name, name);
    }
}

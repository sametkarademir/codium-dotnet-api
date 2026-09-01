using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace Codium.Template.Domain.Shared.Localization;

public class JsonStringLocalizer : IStringLocalizer
{
    private readonly ConcurrentDictionary<string, Dictionary<string, string>> _localization;
    private readonly string _resourcesPath;
    private readonly string _defaultCulture;

    public JsonStringLocalizer(string resourcesPath, string defaultCulture)
    {
        _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));
        _defaultCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));
        _localization = new ConcurrentDictionary<string, Dictionary<string, string>>();
    }

    public LocalizedString this[string name]
    {
        get
        {
            var value = GetString(name);
            return new LocalizedString(name, value ?? name, value == null);
        }
    }

    public LocalizedString this[string name, params object[] arguments]
    {
        get
        {
            var format = GetString(name);
            var value = string.Format(format ?? name, arguments);
            return new LocalizedString(name, value, format == null);
        }
    }

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var resources = GetLocalizationResources(culture);
        
        return resources.Select(r => new LocalizedString(r.Key, r.Value, false));
    }

    private string? GetString(string name)
    {
        var culture = CultureInfo.CurrentUICulture.Name;
        var resources = GetLocalizationResources(culture);

        return resources.GetValueOrDefault(name, name);
    }

    private Dictionary<string, string> GetLocalizationResources(string culture)
    {
        return _localization.GetOrAdd(culture, _ =>
        {
            var filePath = Path.Combine(_resourcesPath, $"{culture}.json");
            if (!File.Exists(filePath))
            {
                var languageCode = culture.Split('-')[0];
                filePath = Path.Combine(_resourcesPath, $"{languageCode}.json");
                
                if (!File.Exists(filePath))
                {
                    filePath = Path.Combine(_resourcesPath, $"{_defaultCulture}.json");
                }
            }

            if (!File.Exists(filePath))
            {
                return new Dictionary<string, string>();
            }

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
        });
    }
}
using Microsoft.Extensions.Localization;

namespace Codium.Template.Domain.Shared.Localization;

public class JsonStringLocalizerFactory : IStringLocalizerFactory
{
    private readonly string _resourcesPath;
    private readonly string _defaultCulture;

    public JsonStringLocalizerFactory(string resourcesPath, string defaultCulture)
    {
        _resourcesPath = resourcesPath ?? throw new ArgumentNullException(nameof(resourcesPath));
        _defaultCulture = defaultCulture ?? throw new ArgumentNullException(nameof(defaultCulture));
    }

    public IStringLocalizer Create(Type resourceSource)
    {
        return new JsonStringLocalizer(_resourcesPath, _defaultCulture);
    }

    public IStringLocalizer Create(string baseName, string location)
    {
        return new JsonStringLocalizer(_resourcesPath, _defaultCulture);
    }
}
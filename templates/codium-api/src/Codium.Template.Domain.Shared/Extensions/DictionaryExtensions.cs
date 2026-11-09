using System.Text.Json;

namespace Codium.Template.Domain.Shared.Extensions;

public static class DictionaryExtensions
{
    public static Dictionary<string, object?> CamelizeKeys(this Dictionary<string, object?> source)
    {
        return source.ToDictionary(
            kvp => JsonNamingPolicy.CamelCase.ConvertName(kvp.Key),
            kvp => kvp.Value
        );
    }
}

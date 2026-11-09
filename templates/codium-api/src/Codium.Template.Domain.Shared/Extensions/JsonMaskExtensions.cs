using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace Codium.Template.Domain.Shared.Extensions;

public static class JsonMaskExtensions
{
    public static string? MaskSensitiveData(
        string? data, 
        string maskPattern = "***MASKED***",
        string[]? sensitivePropertyNames = null,
        JsonSerializerOptions? jsonSerializerOptions = null)
    {
        sensitivePropertyNames ??= ["Password", "Token", "Secret", "ApiKey", "RecoveryKey", "Key", "Credential", "Ssn", "Credit", "Card"];
        if (string.IsNullOrEmpty(data))
        {
            return data;
        }

        try
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            var reader = new Utf8JsonReader(dataBytes);

            if (JsonSerializer.Deserialize<JsonElement>(ref reader, jsonSerializerOptions) is var jsonElement)
            {
                return MaskJsonProperties(jsonElement, sensitivePropertyNames, maskPattern);
            }
        }
        catch
        {
            foreach (var prop in sensitivePropertyNames)
            {
                // Mask property values
                var propertyPattern = $@"(""{prop}""\s*:\s*"")(.*?)("")";
                data = Regex.Replace(data, propertyPattern, $"$1{maskPattern}$3", RegexOptions.IgnoreCase);

                // Mask connection string like values
                var connectionStringPattern = $@"({prop}=)([^;]+)";
                data = Regex.Replace(data, connectionStringPattern, $"$1{maskPattern}", RegexOptions.IgnoreCase);
            }
        }

        return data;
    }

    private static string MaskJsonProperties(
        JsonElement element,
        string[] sensitivePropertyNames,
        string maskPattern = "***MASKED***")
    {
        var sensitiveProps = new HashSet<string>(sensitivePropertyNames, StringComparer.OrdinalIgnoreCase);

        using var ms = new MemoryStream();
        using var writer = new Utf8JsonWriter(ms, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            Indented = false
        });

        MaskJsonElement(element, writer, sensitiveProps, maskPattern);

        writer.Flush();
        return Encoding.UTF8.GetString(ms.ToArray());
    }

    private static void MaskJsonElement(
        JsonElement element,
        Utf8JsonWriter writer,
        HashSet<string> sensitiveProps,
        string maskPattern)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                writer.WriteStartObject();
                foreach (var property in element.EnumerateObject())
                {
                    writer.WritePropertyName(property.Name);
                    if (sensitiveProps.Contains(property.Name))
                    {
                        writer.WriteStringValue(maskPattern);
                    }
                    else if (property.Value.ValueKind == JsonValueKind.String)
                    {
                        var value = property.Value.GetString();
                        if (value != null)
                        {
                            // Check if the value is a connection string like structure
                            var maskedValue = value;
                            foreach (var prop in sensitiveProps)
                            {
                                var pattern = $@"({prop}=)([^;]+)";
                                maskedValue = Regex.Replace(maskedValue, pattern, $"$1{maskPattern}", RegexOptions.IgnoreCase);
                            }
                            writer.WriteStringValue(maskedValue);
                        }
                        else
                        {
                            writer.WriteStringValue(value);
                        }
                    }
                    else
                    {
                        MaskJsonElement(property.Value, writer, sensitiveProps, maskPattern);
                    }
                }
                writer.WriteEndObject();
                break;

            case JsonValueKind.Array:
                writer.WriteStartArray();
                foreach (var item in element.EnumerateArray())
                {
                    MaskJsonElement(item, writer, sensitiveProps, maskPattern);
                }
                writer.WriteEndArray();
                break;

            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (stringValue != null)
                {
                    // Check if the value is a connection string like structure
                    var maskedValue = stringValue;
                    foreach (var prop in sensitiveProps)
                    {
                        var pattern = $@"({prop}=)([^;]+)";
                        maskedValue = Regex.Replace(maskedValue, pattern, $"$1{maskPattern}", RegexOptions.IgnoreCase);
                    }
                    writer.WriteStringValue(maskedValue);
                }
                else
                {
                    writer.WriteStringValue(stringValue);
                }
                break;

            case JsonValueKind.Number:
                writer.WriteNumberValue(element.GetDecimal());
                break;

            case JsonValueKind.True:
                writer.WriteBooleanValue(true);
                break;

            case JsonValueKind.False:
                writer.WriteBooleanValue(false);
                break;

            case JsonValueKind.Null:
                writer.WriteNullValue();
                break;
            
            default:
                writer.WriteNullValue();
                break;
        }
    }
}
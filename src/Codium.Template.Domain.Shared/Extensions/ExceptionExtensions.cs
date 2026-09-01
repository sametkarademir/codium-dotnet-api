using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Codium.Template.Domain.Shared.Extensions;

public static class ExceptionExtensions
{
    public static string GenerateFingerprint(this Exception exception)
    {
        using var sha = SHA256.Create();

        var exceptionType = exception.GetType().FullName ?? "UnknownType";
        var message = exception.Message;
        var stackTrace = exception.StackTrace?.Substring(0, Math.Min(500, (exception.StackTrace?.Length ?? 0))) ?? string.Empty;

        var input = $"{exceptionType}|{message}|{stackTrace}";
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    public static Dictionary<string, object> ConvertExceptionDataToDictionary(this Exception exception)
    {
        var data = new Dictionary<string, object>(exception.Data.Count);
        if (exception.Data.Count <= 0)
        {
            return data;
        }

        foreach (var keyObject in exception.Data.Keys)
        {
            var key = keyObject.ToString();
            if (string.IsNullOrWhiteSpace(key))
            {
                continue;
            }

            var value = exception.Data[keyObject];
            if (value != null)
            {
                data.Add(key, value);
            }
        }

        return data;
    }

    public static string? ConvertExceptionDataToJson(this Exception exception, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        try
        {
            var data = ConvertExceptionDataToDictionary(exception);

            return JsonSerializer.Serialize(data, jsonSerializerOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static List<Dictionary<string, string>> ConvertInnerExceptionsToList(this Exception exception)
    {
        var innerExceptionsList = new List<Dictionary<string, string>>();
        var innerException = exception.InnerException;
        var depth = 0;

        while (innerException != null)
        {
            innerExceptionsList.Add(new Dictionary<string, string>
            {
                { "Type", innerException.GetType().Name },
                { "Message", innerException.Message },
                { "StackTrace", innerException.StackTrace ?? "No stack trace available" },
                { "Depth", depth.ToString() }
            });
            innerException = innerException.InnerException;
            depth++;
        }

        return innerExceptionsList;
    }

    public static string? ConvertInnerExceptionsToJson(this Exception exception, JsonSerializerOptions? jsonSerializerOptions = null)
    {
        try
        {
            var innerExceptionsList = ConvertInnerExceptionsToList(exception);

            return JsonSerializer.Serialize(innerExceptionsList, jsonSerializerOptions);
        }
        catch (Exception)
        {
            return null;
        }
    }
}
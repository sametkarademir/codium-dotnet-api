using System.Text.Encodings.Web;
using System.Text.Json;
using Codium.Template.Domain.Shared.Exceptions.Abstractions;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Codium.Template.HttpApi.Host.Logging.Sinks;

public class CustomConsoleWriteToJsonSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var logData = new Dictionary<string, object?>
        {
            ["Timestamp"] = logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"),
            ["Level"] = logEvent.Level.ToString(),
            ["Message"] = logEvent.RenderMessage()
        };

        foreach (var property in logEvent.Properties)
        {
            var value = property.Value;
            if (value is ScalarValue scalarValue)
            {
                logData[property.Key] = scalarValue.Value;
            }
            else if (value is SequenceValue sequenceValue)
            {
                logData[property.Key] = sequenceValue.Elements
                    .Select(e => e is ScalarValue sv ? sv.Value : e.ToString())
                    .ToArray();
            }
            else if (value is StructureValue structureValue)
            {
                var structDict = new Dictionary<string, object?>();
                foreach (var prop in structureValue.Properties)
                {
                    structDict[prop.Name] = prop.Value is ScalarValue sv ? sv.Value : prop.Value.ToString();
                }
                logData[property.Key] = structDict;
            }
            else
            {
                logData[property.Key] = value.ToString();
            }
        }

        if (logEvent.Exception != null)
        {
            if (logEvent.Exception is AppException baseException)
            {
                logData["ErrorCode"] = baseException.ErrorCode;
                logData["StatusCode"] = baseException.StatusCode;
                logData["Details"] = baseException.Details;

                if (!string.IsNullOrWhiteSpace(baseException.CorrelationId))
                {
                    logData["CorrelationId"] = baseException.CorrelationId;
                }
            }

            logData["Data"] = logEvent.Exception.Data.Count > 0
                ? logEvent.Exception.Data
                    .Cast<System.Collections.DictionaryEntry>()
                    .ToDictionary(de => de.Key.ToString() ?? "null", de => de.Value)
                : null;
            logData["ExceptionType"] = logEvent.Exception.GetType().FullName;
            logData["ExceptionMessage"] = logEvent.Exception.Message;
            logData["StackTrace"] = logEvent.Exception.StackTrace;
            logData["InnerException"] = logEvent.Exception.InnerException?.ToString();
        }

        var json = JsonSerializer.Serialize(logData, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        Console.WriteLine(json);
    }
}

public static class CustomConsoleWriteToJsoSinkExtensions
{
    public static LoggerConfiguration CustomConsoleWriteToJson(this LoggerSinkConfiguration loggerConfiguration)
    {
        var sink = new CustomConsoleWriteToJsonSink();

        return loggerConfiguration.Sink(sink);
    }
}
using Microsoft.Extensions.Logging;

namespace Codium.Template.Domain.Shared.Extensions;

public static class LoggerExtensions
{
    public static LoggerWithPropertiesBuilder WithProperties(this ILogger logger)
    {
        return new LoggerWithPropertiesBuilder(logger);
    }
}

public class LoggerWithPropertiesBuilder
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, object> _properties = new();

    public LoggerWithPropertiesBuilder(ILogger logger)
    {
        _logger = logger;
    }

    public LoggerWithPropertiesBuilder Add(string key, object value)
    {
        _properties[key] = value;
        return this;
    }

    public LoggerWithPropertiesBuilder AddRange(Dictionary<string, object> properties)
    {
        foreach (var prop in properties)
        {
            _properties[prop.Key] = prop.Value;
        }
        return this;
    }

    public void LogInformation(string message)
    {
        using (_logger.BeginScope(_properties))
        {
            _logger.LogInformation(message);
        }
    }

    public void LogWarning(string message)
    {
        using (_logger.BeginScope(_properties))
        {
            _logger.LogWarning(message);
        }
    }

    public void LogError(string message, Exception? exception = null)
    {
        using (_logger.BeginScope(_properties))
        {
            if (exception != null)
                _logger.LogError(exception, message);
            else
                _logger.LogError(message);
        }
    }

    public void Log(LogLevel logLevel, string message, Exception? exception = null)
    {
        using (_logger.BeginScope(_properties))
        {
            if (exception != null)
                _logger.Log(logLevel, exception, message);
            else
                _logger.Log(logLevel, message);
        }
    }
}
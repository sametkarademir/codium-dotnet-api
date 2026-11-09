using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Codium.Template.HttpApi.Host.Logging.Models;

public class SerilogOptions
{
    public const string SectionName = "Serilog";
    public bool Enabled { get; set; } = true;
    public ConsoleOptions Console { get; set; } = new();
    public FileOptions File { get; set; } = new();
}

public class ConsoleOptions
{
    public bool Enabled { get; set; } = true;
    public bool IsCustom { get; set; } = false;
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}";
    public ConsoleTheme Theme { get; set; } = AnsiConsoleTheme.Code;
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Verbose;
}

public class FileOptions
{
    public bool Enabled { get; set; } = false;
    public string PathToTxt { get; set; } = "logs/txt-.txt";
    public RollingInterval RollingInterval { get; set; } = RollingInterval.Day;
    public string OutputTemplate { get; set; } = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{Properties:j}{NewLine}{Exception}";
}
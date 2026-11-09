namespace Codium.Template.Application.Contracts.Healths;

public class HealthResponseDto
{
    public string StartupTime { get; set; } = null!;
    public string CurrentTime { get; set; } = null!;
    public long UptimeMs { get; set; }
    public long UptimePreciseMs { get; set; }
    public string Status { get; set; } = "UP";
}
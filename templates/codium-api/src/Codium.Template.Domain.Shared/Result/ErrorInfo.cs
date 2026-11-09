namespace Codium.Template.Domain.Shared.Result;

public class ErrorInfo
{
    public int StatusCode { get; set; } = 500;
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
}
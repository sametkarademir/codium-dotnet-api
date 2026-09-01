namespace Codium.Template.Domain.Shared.Exceptions.Abstractions;

public class AppProblemDetails
{
    public AppProblemDetails(string? message, int? statusCode, string? errorCode, object? details, string? correlationId)
    {
        Message = message;
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Details = details;
        CorrelationId = correlationId;
    }

    public string? Message { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public object? Details { get; set; }
    public string? CorrelationId { get; set; }
}
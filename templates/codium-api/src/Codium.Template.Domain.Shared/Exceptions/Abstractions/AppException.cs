namespace Codium.Template.Domain.Shared.Exceptions.Abstractions;

public abstract class AppException :
    Exception,
    IHasCorrelationId,
    IHasErrorCode,
    IHasErrorDetails,
    IHasStatusCode
{
    public virtual string? CorrelationId { get; protected set; }
    public virtual string ErrorCode { get; protected set; } = "APP:ERROR";
    public virtual object? Details { get; protected set; }
    public virtual int StatusCode { get; protected set; } = 500;

    protected AppException() : base("An error occurred in the application.")
    {
    }

    protected AppException(string? message) : base(message)
    {
    }

    protected AppException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public AppException WithData(string name, object value)
    {
        Data[name] = value;
        return this;
    }
    
    public AppException AppendData(IDictionary<string, object> data)
    {
        if (data.Count == 0)
        {
            return this;
        }

        foreach (var kvp in data)
        {
            Data[kvp.Key] = kvp.Value;
        }

        return this;
    }

    public AppException WithCode(string errorCode)
    {
        ErrorCode = errorCode;
        return this;
    }

    public AppException WithDetails(object details)
    {
        Details = details;
        return this;
    }

    public AppException WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public AppException WithCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
        return this;
    }
}
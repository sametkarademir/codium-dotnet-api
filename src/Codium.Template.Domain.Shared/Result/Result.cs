using Codium.Template.Domain.Shared.BaseEntities.Interfaces.Base;

namespace Codium.Template.Domain.Shared.Result;


public class Result<T> where T: IEntityDto
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public ErrorInfo? Error { get; set; }

    public static Result<T> Ok(T data)
    {
        return new Result<T>
        {
            Success = true,
            Data = data,
            Error = null
        };
    }

    public static Result<T> Fail(int status, string message, string? code = null, object? details = null, string? correlationId = null)
    {
        return new Result<T>
        {
            Success = false,
            Data = default,
            Error = new ErrorInfo
            {
                StatusCode = status,
                Message = message,
                ErrorCode = code,
                Details = details,
                CorrelationId = correlationId,
            }
        };
    }
}

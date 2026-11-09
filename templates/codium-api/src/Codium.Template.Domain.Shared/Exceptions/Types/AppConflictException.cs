using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppConflictException : AppException
{
    public override int StatusCode { get; protected set; } = 409;
    public override string ErrorCode { get; protected set; } = "APP:CONFLICT";

    public AppConflictException()
    {
    }

    public AppConflictException(string message) : base(message)
    {
    }

    public AppConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }
}



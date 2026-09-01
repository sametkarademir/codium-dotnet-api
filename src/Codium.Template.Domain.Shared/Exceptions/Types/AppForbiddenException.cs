using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppForbiddenException : AppException
{
    public override int StatusCode { get; protected set; } = 403;
    public override string ErrorCode { get; protected set; } = "APP:FORBIDDEN";

    public AppForbiddenException()
    {
    }

    public AppForbiddenException(string message) : base(message)
    {
    }

    public AppForbiddenException(string message, Exception innerException) : base(message, innerException)
    {
    }
}



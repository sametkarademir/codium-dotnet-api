using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppUnauthorizedException : AppException
{
    public override int StatusCode { get; protected set; } = 401;
    public override string ErrorCode { get; protected set; } = "APP:UNAUTHORIZED";

    public AppUnauthorizedException()
    {
    }

    public AppUnauthorizedException(string message) : base(message)
    {
    }

    public AppUnauthorizedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}



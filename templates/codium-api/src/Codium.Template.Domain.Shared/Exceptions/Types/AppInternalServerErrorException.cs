using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppInternalServerErrorException : AppException
{
    public override int StatusCode { get; protected set; } = 500;
    public override string ErrorCode { get; protected set; } = "APP:INTERNAL_SERVER_ERROR";

    public AppInternalServerErrorException()
    {
    }

    public AppInternalServerErrorException(string message) : base(message)
    {
    }

    public AppInternalServerErrorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}



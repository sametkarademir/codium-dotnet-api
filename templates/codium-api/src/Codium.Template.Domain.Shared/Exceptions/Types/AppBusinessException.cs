using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppBusinessException : AppException
{
    public override int StatusCode { get; protected set; } = 422;
    public override string ErrorCode { get; protected set; } = "APP:BUSINESS";

    public AppBusinessException()
    {
    }

    public AppBusinessException(string message) : base(message)
    {
    }

    public AppBusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}



using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Exceptions.Types;

public class AppValidationException : AppException
{
    public override int StatusCode { get; protected set; } = 400;
    public override string ErrorCode { get; protected set; } = "APP:VALIDATION";

    public AppValidationException()
    {
    }

    public AppValidationException(string message) : base(message)
    {
    }

    public AppValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public AppValidationException(IEnumerable<ValidationExceptionModel> errors) : base(BuildErrorMessage(errors))
    {
        WithDetails(errors);
    }

    private static string BuildErrorMessage(IEnumerable<ValidationExceptionModel> errors)
    {
        var arr = errors.Select(item =>
            $"{Environment.NewLine} -- {item.Property}: {string.Join(Environment.NewLine, values: item.Errors ?? Array.Empty<string>())}"
        );

        return string.Join(string.Empty, arr);
    }
}



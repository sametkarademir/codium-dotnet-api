using System.Text.Json;
using Codium.Template.Domain.Shared.Exceptions.Abstractions;

namespace Codium.Template.Domain.Shared.Extensions;

public static class ProblemDetailsExtensions
{
    public static string ToJson<TProblemDetail>(this TProblemDetail details)
        where TProblemDetail : AppProblemDetails
    {
        return JsonSerializer.Serialize(details);
    }
}
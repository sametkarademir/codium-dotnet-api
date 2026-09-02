using Codium.Template.Domain.Shared.Exceptions;

namespace Codium.Template.Application.Contracts.Common.Results;

public class ValidationResult
{
    public bool Succeeded { get; init; }
    public List<ValidationExceptionModel> Errors { get; init; }
    
    private ValidationResult(bool succeeded, List<ValidationExceptionModel> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }
    
    public static ValidationResult Success => new(true, []);
    public static ValidationResult Failed(IEnumerable<ValidationExceptionModel> errors) => new(false, errors.ToList());
}
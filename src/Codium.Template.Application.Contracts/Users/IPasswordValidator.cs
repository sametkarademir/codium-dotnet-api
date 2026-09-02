using Codium.Template.Application.Contracts.Common.Results;

namespace Codium.Template.Application.Contracts.Users;

public interface IPasswordValidator
{
    ValidationResult Validate(string password);
}
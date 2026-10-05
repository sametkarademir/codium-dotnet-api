using Codium.Template.Domain.Shared.Exceptions.Types;
using Microsoft.AspNetCore.Identity;

namespace Codium.Template.Application.Contracts.Extensions;

public static class IdentityResultExtensions
{
    /// <summary>
    /// Turns a failed <see cref="IdentityResult"/> into an application exception: a duplicate user/role name becomes a conflict
    /// (when a message is supplied), everything else a validation error carrying Identity's descriptions.
    /// </summary>
    public static void ThrowIfFailed(this IdentityResult result, string? duplicateMessage = null)
    {
        if (result.Succeeded)
        {
            return;
        }

        if (duplicateMessage != null && result.Errors.Any(e =>
                e.Code is nameof(IdentityErrorDescriber.DuplicateRoleName)
                    or nameof(IdentityErrorDescriber.DuplicateUserName)
                    or nameof(IdentityErrorDescriber.DuplicateEmail)))
        {
            throw new AppConflictException(duplicateMessage);
        }

        throw new AppValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}

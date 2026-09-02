using Codium.Template.Domain.Shared.Exceptions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Codium.Template.HttpApi.Attributes;

public class ValidationActionFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .Select(kvp => new ValidationExceptionModel
                {
                    Property = kvp.Key,
                    Errors = kvp.Value?.Errors.Select(e => e.ErrorMessage).ToList()
                })
                .ToList();

            throw new AppValidationException(errors);
        }

        base.OnActionExecuting(context);
    }
}
using Codium.Template.Domain.Shared.Exceptions.Types;

namespace Codium.Template.Domain.Shared.Exceptions.Handlers;

public abstract class ExceptionHandler
{
    public abstract Task HandleException(AppBusinessException exception);
    public abstract Task HandleException(AppConflictException exception);
    public abstract Task HandleException(AppEntityNotFoundException exception);
    public abstract Task HandleException(AppForbiddenException exception);
    public abstract Task HandleException(AppInternalServerErrorException exception);
    public abstract Task HandleException(AppUnauthorizedException exception);
    public abstract Task HandleException(AppValidationException exception);
    public abstract Task HandleException(Exception exception, string? correlationId = null);
}
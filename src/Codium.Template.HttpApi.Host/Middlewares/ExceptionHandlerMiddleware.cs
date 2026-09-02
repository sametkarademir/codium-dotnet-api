using System.Net.Mime;
using Codium.Template.Domain.Shared.Exceptions.Handlers;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;

namespace Codium.Template.HttpApi.Host.Middlewares;

public class  ExceptionHandlerMiddleware(RequestDelegate next)
{
    private readonly HttpExceptionHandler _httpExceptionHandler = new();

    public async Task Invoke(HttpContext context, ILogger<ExceptionHandlerMiddleware> logger)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context.Response, exception, context);
            logger.LogError(exception, exception.Message);
        }
    }

    protected virtual Task HandleExceptionAsync(HttpResponse response, dynamic exception, HttpContext context)
    {
        response.ContentType = MediaTypeNames.Application.Json;
        _httpExceptionHandler.Response = response;

        return exception switch
        {
            AppBusinessException e => _httpExceptionHandler.HandleException(e),
            AppConflictException e => _httpExceptionHandler.HandleException(e),
            AppEntityNotFoundException e => _httpExceptionHandler.HandleException(e),
            AppForbiddenException e => _httpExceptionHandler.HandleException(e),
            AppInternalServerErrorException e => _httpExceptionHandler.HandleException(e),
            AppUnauthorizedException e => _httpExceptionHandler.HandleException(e),
            AppValidationException e => _httpExceptionHandler.HandleException(e),
            _ => _httpExceptionHandler.HandleException(exception, context.GetCorrelationId().ToString())
        };
    }
}
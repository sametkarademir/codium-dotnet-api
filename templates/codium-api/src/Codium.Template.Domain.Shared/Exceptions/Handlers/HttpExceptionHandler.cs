using Codium.Template.Domain.Shared.Exceptions.Abstractions;
using Codium.Template.Domain.Shared.Exceptions.Types;
using Codium.Template.Domain.Shared.Extensions;
using Microsoft.AspNetCore.Http;

namespace Codium.Template.Domain.Shared.Exceptions.Handlers;

public class HttpExceptionHandler : ExceptionHandler
{
    public HttpResponse Response
    {
        get => _response ?? throw new NullReferenceException(nameof(_response));
        set => _response = value;
    }
    private HttpResponse? _response;

    public override Task HandleException(AppBusinessException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppConflictException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppEntityNotFoundException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppForbiddenException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppInternalServerErrorException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppUnauthorizedException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(AppValidationException exception)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.Details,
                exception.CorrelationId)
            .ToJson();
        
        Response.SetCorrelationId(exception.CorrelationId);
        Response.StatusCode = exception.StatusCode;
        return Response.WriteAsync(problemDetails);
    }

    public override Task HandleException(Exception exception, string? correlationId = null)
    {
        var problemDetails = new AppProblemDetails(
                exception.Message,
                500,
                "APP:INTERNAL_SERVER_ERROR",
                null,
                correlationId)
            .ToJson();
        
        Response.SetCorrelationId(correlationId);
        Response.StatusCode = 500;
        return Response.WriteAsync(problemDetails);
    }
}
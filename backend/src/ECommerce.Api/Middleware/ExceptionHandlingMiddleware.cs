using System.Net;
using System.Text.Json;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Domain.Exceptions;
using FluentValidationException = FluentValidation.ValidationException;

namespace ECommerce.Api.Middleware;

/// <summary>Translates domain/application exceptions into consistent ProblemDetails-shaped JSON responses.</summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, errors) = exception switch
        {
            AppValidationException appValidationEx => (HttpStatusCode.BadRequest, "Validation failed", (object?)appValidationEx.Errors),
            FluentValidationException fluentValidationEx =>
                (HttpStatusCode.BadRequest, "Validation failed", (object?)AppValidationException.ToErrorDictionary(fluentValidationEx.Errors)),
            NotFoundException => (HttpStatusCode.NotFound, exception.Message, null),
            UnauthorizedException => (HttpStatusCode.Unauthorized, exception.Message, null),
            ForbiddenAccessException => (HttpStatusCode.Forbidden, exception.Message, null),
            DomainException => (HttpStatusCode.BadRequest, exception.Message, null),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.", null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);
        else
            _logger.LogWarning("{ExceptionType}: {Message}", exception.GetType().Name, exception.Message);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            status = (int)statusCode,
            title,
            errors,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }
}

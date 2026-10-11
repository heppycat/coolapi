using System.Net;
using System.Text.Json;
using Application.Exceptions;
using FluentValidation;

namespace Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            _logger.LogInformation("mew mew Exception middle ware is wooooshing!");
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        var statusCode = exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            ConflictException => (int)HttpStatusCode.Conflict,
            AuthenticationFailedException => (int)HttpStatusCode.Unauthorized,
            _ => (int)HttpStatusCode.InternalServerError,
        };

        response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(
            new
            {
                Title = exception.GetType().Name,
                Status = statusCode,
                Detail = exception.Message,
                // In production, hide stack trace unless you want to show it.
                // StackTrace = exception.StackTrace
            }
        );

        return response.WriteAsync(result);
    }
}

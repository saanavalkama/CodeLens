using System.Text.Json;
using CodeLens.Domain.Exceptions;

namespace CodeLens.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware>_logger;

    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware>logger
    )
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
        catch(Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (statusCode, message) = ex switch
        {
            NotFoundException e =>(404, e.Message),
            ForbiddenException e => (403, e.Message),
            ConflictException e => (409, e.Message),
            UnauthorizedException e => (401, e.Message),
            BadRequestException e  => (400, e.Message),
            _ => (500, "An unexpected error occured")
        };

        if(statusCode == 500) _logger.LogError(ex, "Unhandled exception");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var response = new {error = message};
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));

    }
}
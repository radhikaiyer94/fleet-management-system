using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace FleetManagementApi.Exceptions;

/// <summary>
/// Central exception handler. Maps thrown exceptions to HTTP status and consistent JSON.
/// Register with AddExceptionHandler<ApiExceptionHandler>() and UseExceptionHandler().
/// </summary>
public class ApiExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;

    public ApiExceptionHandler(IHostEnvironment env)
    {
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            NotFoundException => (HttpStatusCode.NotFound, exception.Message),
            BadRequestException => (HttpStatusCode.BadRequest, exception.Message),
            ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
            ConflictException => (HttpStatusCode.Conflict, exception.Message),
            _ => (_env.IsDevelopment()
                ? HttpStatusCode.InternalServerError
                : HttpStatusCode.InternalServerError,
                _env.IsDevelopment() ? exception.Message : "An error occurred. Please try again later.")
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsJsonAsync(new { message }, cancellationToken);
        return true;
    }
}

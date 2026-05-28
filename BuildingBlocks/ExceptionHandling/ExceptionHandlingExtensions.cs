using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Company.WebApi.Template.BuildingBlocks.ExceptionHandling;

public static class ExceptionHandlingExtensions
{
    public static IServiceCollection AddExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, title) = exception switch
        {
            ArgumentNullException or ArgumentException  => (StatusCodes.Status400BadRequest,          "Bad Request"),
            UnauthorizedAccessException                 => (StatusCodes.Status401Unauthorized,         "Unauthorized"),
            KeyNotFoundException                        => (StatusCodes.Status404NotFound,             "Not Found"),
            InvalidOperationException                   => (StatusCodes.Status422UnprocessableEntity,  "Unprocessable Entity"),
            NotImplementedException                     => (StatusCodes.Status501NotImplemented,       "Not Implemented"),
            _                                           => (StatusCodes.Status500InternalServerError,  "Internal Server Error")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}"
        };

        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}

using Serilog.Context;

namespace Company.WebApi.Template.BuildingBlocks.Correlation;

public interface ICorrelationIdAccessor
{
    string CorrelationId { get; set; }
}

internal sealed class CorrelationIdAccessor : ICorrelationIdAccessor
{
    public string CorrelationId { get; set; } = string.Empty;
}

public static class CorrelationExtensions
{
    public static IServiceCollection AddCorrelationId(this IServiceCollection services)
    {
        services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        return services;
    }

    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            var accessor = context.RequestServices.GetRequiredService<ICorrelationIdAccessor>();
            accessor.CorrelationId = correlationId;

            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Correlation-ID"] = correlationId;
                return Task.CompletedTask;
            });

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                await next(context);
            }
        });

        return app;
    }
}

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Formatting.Compact;

namespace Company.WebApi.Template.BuildingBlocks.Observability;

public static class ObservabilityExtensions
{
    public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            if (context.Configuration.GetSection("Serilog").Exists())
            {
                configuration.ReadFrom.Configuration(context.Configuration);
            }
            else
            {
                configuration
                    .WriteTo.Console(new CompactJsonFormatter())
                    .WriteTo.File(
                        new CompactJsonFormatter(),
                        path: "logs/app-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 7);
            }

            configuration
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();
        });

        return builder;
    }

    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection("Observability");
        var serviceName    = section["ServiceName"]    ?? "UnnamedService";
        var serviceVersion = section["ServiceVersion"] ?? "1.0.0";
        var otlpEndpoint   = section["OtlpEndpoint"];

        var resource = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion);

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                    .AddSource(serviceName);

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    tracing.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                else
                    tracing.AddConsoleExporter();
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resource)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter(serviceName);

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                    metrics.AddOtlpExporter(opts => opts.Endpoint = new Uri(otlpEndpoint));
                else
                    metrics.AddConsoleExporter();
            });

        return services;
    }
}

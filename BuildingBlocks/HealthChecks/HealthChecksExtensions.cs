using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Company.WebApi.Template.BuildingBlocks.HealthChecks;

public static class HealthChecksExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"]);

//#if (addSqlServerHealthCheck)
        healthChecks.AddSqlServer(
            configuration.GetConnectionString("SqlServer")!,
            name: "sqlserver",
            tags: ["db"]);
//#endif
//#if (addPostgresHealthCheck)
        healthChecks.AddNpgSql(
            configuration.GetConnectionString("Postgres")!,
            name: "postgres",
            tags: ["db"]);
//#endif
//#if (addRedisHealthCheck)
        healthChecks.AddRedis(
            configuration.GetConnectionString("Redis")!,
            name: "redis",
            tags: ["cache"]);
//#endif

        return services;
    }

    public static IEndpointRouteBuilder MapApplicationHealthChecks(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live"),
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        return endpoints;
    }
}

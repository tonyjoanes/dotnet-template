# Company Web API Template

An opinionated `dotnet new` template for ASP.NET Core Web API projects.

## What's included

| Building block | Where |
|---|---|
| Structured logging (Serilog + CLEF) | `BuildingBlocks/Observability` |
| Distributed tracing + metrics (OpenTelemetry) | `BuildingBlocks/Observability` |
| Health checks (`/health`, `/health/live`) | `BuildingBlocks/HealthChecks` |
| Global exception handling (RFC 7807 ProblemDetails) | `BuildingBlocks/ExceptionHandling` |
| Correlation ID propagation (`X-Correlation-ID`) | `BuildingBlocks/Correlation` |
| CORS (dev-permissive / prod-strict via config) | `BuildingBlocks/Cors` |
| Vertical slice auto-discovery (`IFeatureModule`) | `BuildingBlocks/Features` |
| Functional result types (LanguageExt `Option<T>`, `Fin<T>`) | `BuildingBlocks/Functional` |

`Program.cs` is intentionally minimal — every concern is an extension method call.

## Install

```bash
dotnet new install .
```

## Create a project

```bash
# defaults (.NET 9, with Customer sample)
dotnet new company-webapi -n MyApp.Api

# with SQL Server health check
dotnet new company-webapi -n MyApp.Api --add-sqlserver-health-check

# with Postgres health check
dotnet new company-webapi -n MyApp.Api --add-postgres-health-check

# with Redis health check
dotnet new company-webapi -n MyApp.Api --add-redis-health-check

# target .NET 8
dotnet new company-webapi -n MyApp.Api --framework net8.0

# strip out the sample feature
dotnet new company-webapi -n MyApp.Api --include-customer-sample false
```

## Adding a new feature (vertical slice)

1. Create `Features/MyThing/`
2. Add your models, service, and a module class:

```csharp
public sealed class MyThingModule : IFeatureModule
{
    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IMyThingService, MyThingService>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/things", GetAllAsync).WithTags("Things");
        return endpoints;
    }
}
```

That's it — the module is discovered automatically. No changes to `Program.cs`.

## Removing a feature

Delete the feature folder. Done.

## Functional layer

Services return `Fin<T>` (success/failure) or `Option<T>` (present/absent) instead of throwing exceptions.

```csharp
// service
public Task<Fin<Order>> PlaceOrderAsync(PlaceOrderRequest request) { ... }

// endpoint — no try/catch, no null checks
var result = await service.PlaceOrderAsync(request);
return result.Map(OrderResponse.From).ToCreatedResult(r => $"/api/orders/{r.Id}");
```

Error codes map to HTTP status automatically:
- `Error.New(400, "...")` → 400 Bad Request
- `Error.New(404, "...")` → 404 Not Found
- `Error.New(409, "...")` → 409 Conflict
- `Error.New(422, "...")` → 422 Unprocessable Entity
- anything else → 500 Problem

## Probe after running

```bash
curl http://localhost:5000/health
curl http://localhost:5000/health/live
curl -X POST http://localhost:5000/api/customers \
  -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@example.com"}'
```

## Ship traces to Jaeger (local)

```yaml
# docker-compose.yml
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"   # UI
      - "4317:4317"     # OTLP gRPC
```

Set `Observability:OtlpEndpoint` to `http://localhost:4317` in `appsettings.Development.json`.

## Uninstall

```bash
dotnet new uninstall .
```

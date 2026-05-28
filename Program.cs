using Company.WebApi.Template.BuildingBlocks.Correlation;
using Company.WebApi.Template.BuildingBlocks.Cors;
using Company.WebApi.Template.BuildingBlocks.ExceptionHandling;
using Company.WebApi.Template.BuildingBlocks.Features;
using Company.WebApi.Template.BuildingBlocks.HealthChecks;
using Company.WebApi.Template.BuildingBlocks.Observability;

var builder = WebApplication.CreateBuilder(args);

builder.AddSerilogLogging();
builder.Services.AddObservability(builder.Configuration);
builder.Services.AddApplicationHealthChecks(builder.Configuration);
builder.Services.AddApplicationCors(builder.Configuration);
builder.Services.AddExceptionHandling();
builder.Services.AddCorrelationId();
builder.Services.AddOpenApi();
builder.Services.AddFeatureModules(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCorrelationId();
app.UseApplicationCors();
app.UseExceptionHandling();
app.MapApplicationHealthChecks();
app.MapFeatureModules();

app.Run();

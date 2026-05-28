namespace Company.WebApi.Template.BuildingBlocks.Cors;

public static class CorsExtensions
{
    private const string PolicyName = "AppCorsPolicy";

    public static IServiceCollection AddApplicationCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                if (allowedOrigins.Length > 0)
                    policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
                else
                    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });

        return services;
    }

    public static IApplicationBuilder UseApplicationCors(this IApplicationBuilder app)
    {
        app.UseCors(PolicyName);
        return app;
    }
}

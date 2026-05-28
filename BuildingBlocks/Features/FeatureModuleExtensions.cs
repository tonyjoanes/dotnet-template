using System.Reflection;

namespace Company.WebApi.Template.BuildingBlocks.Features;

public static class FeatureModuleExtensions
{
    public static IServiceCollection AddFeatureModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var modules = Assembly.GetEntryAssembly()!
            .GetTypes()
            .Where(t => typeof(IFeatureModule).IsAssignableFrom(t)
                     && !t.IsInterface
                     && !t.IsAbstract)
            .Select(t => (IFeatureModule)Activator.CreateInstance(t)!)
            .ToList();

        foreach (var module in modules)
            module.AddServices(services, configuration);

        services.AddSingleton<IEnumerable<IFeatureModule>>(modules);
        return services;
    }

    public static WebApplication MapFeatureModules(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IEnumerable<IFeatureModule>>();
        foreach (var module in modules)
            module.MapEndpoints(app);
        return app;
    }
}

namespace Company.WebApi.Template.BuildingBlocks.Features;

/// <summary>
/// Vertical slice contract. Implement this interface in each feature folder.
/// Modules are discovered automatically via assembly scanning — no changes to
/// Program.cs are needed when adding or removing a feature.
/// </summary>
public interface IFeatureModule
{
    IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}

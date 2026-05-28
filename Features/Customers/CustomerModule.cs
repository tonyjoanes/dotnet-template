using Company.WebApi.Template.BuildingBlocks.Features;
using Company.WebApi.Template.BuildingBlocks.Functional;

namespace Company.WebApi.Template.Features.Customers;

/// <summary>
/// Vertical slice entry point for the Customers feature.
/// Drop this folder to remove the feature — no other files need changing.
/// </summary>
public sealed class CustomerModule : IFeatureModule
{
    public IServiceCollection AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICustomerService, CustomerService>();
        return services;
    }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints
            .MapGroup("/api/customers")
            .WithTags("Customers")
            .WithOpenApi();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetCustomerById")
            .WithSummary("Get a customer by ID.")
            .Produces<CustomerResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateAsync)
            .WithName("CreateCustomer")
            .WithSummary("Create a new customer.")
            .Produces<CustomerResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return endpoints;
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        return (await service.GetByIdAsync(CustomerId.From(id), cancellationToken))
            .Map(CustomerResponse.From)
            .ToHttpResult();
    }

    private static async Task<IResult> CreateAsync(
        CreateCustomerRequest request,
        ICustomerService service,
        CancellationToken cancellationToken)
    {
        return (await service.CreateAsync(request, cancellationToken))
            .Map(CustomerResponse.From)
            .ToCreatedResult(r => $"/api/customers/{r.Id}");
    }
}

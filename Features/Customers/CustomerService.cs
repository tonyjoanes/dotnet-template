using System.Collections.Concurrent;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace Company.WebApi.Template.Features.Customers;

public interface ICustomerService
{
    Task<Option<Customer>> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default);
    Task<Fin<Customer>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default);
}

public sealed class CustomerService(ILogger<CustomerService> logger) : ICustomerService
{
    // Replace with a real repository in production code.
    private readonly ConcurrentDictionary<CustomerId, Customer> _store = new();

    public Task<Option<Customer>> GetByIdAsync(CustomerId id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var customer);
        return Task.FromResult(Optional(customer));
    }

    public Task<Fin<Customer>> CreateAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default) =>
        Task.FromResult(
            Validate(request)
                .Bind(CheckNoDuplicate)
                .Map(Build));

    private Fin<CreateCustomerRequest> CheckNoDuplicate(CreateCustomerRequest request)
    {
        if (_store.Values.Any(c => string.Equals(c.Email, request.Email, StringComparison.OrdinalIgnoreCase)))
            return FinFail<CreateCustomerRequest>(
                Error.New(StatusCodes.Status409Conflict,
                    $"A customer with email '{request.Email}' already exists."));

        return FinSucc(request);
    }

    private Customer Build(CreateCustomerRequest request)
    {
        var customer = new Customer(
            Id:        CustomerId.New(),
            Name:      request.Name.Trim(),
            Email:     request.Email.Trim().ToLowerInvariant(),
            CreatedAt: DateTimeOffset.UtcNow);

        _store[customer.Id] = customer;
        logger.LogInformation("Customer created {CustomerId}", customer.Id.Value);
        return customer;
    }

    private static Fin<CreateCustomerRequest> Validate(CreateCustomerRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Name))
            errors.Add("Name is required.");
        else if (request.Name.Trim().Length > 100)
            errors.Add("Name must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(request.Email))
            errors.Add("Email is required.");
        else if (!IsValidEmail(request.Email))
            errors.Add("A valid email address is required.");

        return errors.Count == 0
            ? FinSucc(request)
            : FinFail<CreateCustomerRequest>(
                Error.New(StatusCodes.Status400BadRequest, string.Join(" ", errors)));
    }

    private static bool IsValidEmail(string email) =>
        email.Contains('@') && email.LastIndexOf('.') > email.IndexOf('@');
}

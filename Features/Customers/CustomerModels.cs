namespace Company.WebApi.Template.Features.Customers;

public sealed record CustomerId(Guid Value)
{
    public static CustomerId New()         => new(Guid.NewGuid());
    public static CustomerId From(Guid id) => new(id);
}

public sealed record Customer(
    CustomerId      Id,
    string          Name,
    string          Email,
    DateTimeOffset  CreatedAt);

public sealed record CreateCustomerRequest(string Name, string Email);

public sealed record CustomerResponse(Guid Id, string Name, string Email, DateTimeOffset CreatedAt)
{
    public static CustomerResponse From(Customer c) => new(c.Id.Value, c.Name, c.Email, c.CreatedAt);
}

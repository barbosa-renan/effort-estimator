# Examples — Core Layer

## Domain Entity (POCO — no ORM attributes)

Entities live in `*.Core/Domain/Entities/`. They are plain C# classes with private
setters and named mutation methods. EF Core's private parameterless constructor is
the only concession to infrastructure — keep it `private`.

```csharp
// YourApp.Core/Domain/Entities/Order.cs
namespace YourApp.Core.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }
    public string CustomerId { get; private set; } = string.Empty;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Order() { } // EF Core only — never call directly

    public static Order Create(string customerId, decimal totalAmount)
    {
        if (string.IsNullOrWhiteSpace(customerId))
            throw new DomainException("CustomerId cannot be empty.");
        if (totalAmount <= 0)
            throw new DomainException("TotalAmount must be positive.");

        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            TotalAmount = totalAmount,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Named mutation — domain intent is explicit
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed.");
        Status = OrderStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Confirmed)
            throw new DomainException("Confirmed orders cannot be cancelled.");
        Status = OrderStatus.Cancelled;
    }
}

public enum OrderStatus { Pending, Confirmed, Cancelled }
```

---

## Domain Exception

```csharp
// YourApp.Core/Domain/Exceptions/DomainException.cs
namespace YourApp.Core.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
```

---

## Value Object (optional — for domain primitives)

```csharp
// YourApp.Core/Domain/ValueObjects/Money.cs
namespace YourApp.Core.Domain.ValueObjects;

public record Money(decimal Amount, string Currency)
{
    public static Money Of(decimal amount, string currency)
    {
        if (amount < 0) throw new DomainException("Amount cannot be negative.");
        if (string.IsNullOrWhiteSpace(currency)) throw new DomainException("Currency is required.");
        return new Money(amount, currency.ToUpperInvariant());
    }

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new DomainException("Cannot add amounts of different currencies.");
        return new Money(Amount + other.Amount, Currency);
    }
}
```

---

## Port Interfaces

Ports live in `*.Core/Application/Interfaces/` and express **what Core needs** from
the outside world — with no knowledge of how it is implemented.

### Repository Port (data access)

```csharp
// YourApp.Core/Application/Interfaces/Repositories/IOrderRepository.cs
namespace YourApp.Core.Application.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(string customerId, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
```

### Service Port (external concern)

```csharp
// YourApp.Core/Application/Interfaces/Services/IEmailNotifier.cs
namespace YourApp.Core.Application.Interfaces.Services;

public interface IEmailNotifier
{
    Task SendOrderConfirmationAsync(string to, Guid orderId, CancellationToken ct = default);
}

// YourApp.Core/Application/Interfaces/Services/IPasswordHasher.cs
public interface IPasswordHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}
```

---

## Application Service (use-case orchestration)

Application Services live in `*.Core/Application/Services/`. They coordinate domain
entities and port interfaces — **no infrastructure code**, no HTTP types, no EF types.

```csharp
// YourApp.Core/Application/Services/OrderService.cs
namespace YourApp.Core.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _orders;
    private readonly IEmailNotifier _notifier;

    public OrderService(IOrderRepository orders, IEmailNotifier notifier)
    {
        _orders = orders;
        _notifier = notifier;
    }

    public async Task<Order> PlaceOrderAsync(
        string customerId, decimal totalAmount, CancellationToken ct = default)
    {
        var order = Order.Create(customerId, totalAmount);
        await _orders.AddAsync(order, ct);
        return order;
    }

    public async Task ConfirmOrderAsync(Guid orderId, CancellationToken ct = default)
    {
        var order = await _orders.GetByIdAsync(orderId, ct)
            ?? throw new DomainException($"Order '{orderId}' not found.");

        order.Confirm();

        await _orders.UpdateAsync(order, ct);
        await _notifier.SendOrderConfirmationAsync(order.CustomerId, order.Id, ct);
    }

    public async Task<IReadOnlyList<Order>> GetCustomerOrdersAsync(
        string customerId, CancellationToken ct = default)
        => await _orders.GetByCustomerIdAsync(customerId, ct);
}
```

---

## Core `.csproj` — Zero External Dependencies

```xml
<!-- YourApp.Core/YourApp.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <!-- No PackageReference. No ProjectReference. -->
</Project>
```

### Verify after any change

```bash
# Must return empty — any output here is a violation
dotnet list src/YourApp.Core/YourApp.Core.csproj reference
dotnet list src/YourApp.Core/YourApp.Core.csproj package
```

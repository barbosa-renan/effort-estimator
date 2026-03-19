---
name: hexagonal-csharp-dotnet-rules
description: >
  Mandatory coding rules for C# .NET Hexagonal Architecture projects.
  Reference alongside SKILL.md. Apply these rules to every code generation,
  review, or refactoring task in a hexagonal .NET solution.
---

# Coding Rules — C# .NET Hexagonal Architecture

These rules are **non-negotiable** constraints that enforce architectural purity,
testability, and long-term maintainability. When generating or reviewing code,
verify compliance with every applicable rule.

---

## RULE 01 — Domain Entities Are Plain C# Classes (POCOs)

- **NEVER** add EF Core attributes (`[Key]`, `[Column]`, `[Table]`, `[Required]`,
  `[ForeignKey]`, etc.) to entities in `*.Core`.
- **NEVER** reference `Microsoft.EntityFrameworkCore` in any Core project file.
- Use **Fluent API** in `IEntityTypeConfiguration<T>` classes located in
  `*.Infrastructure/Data/Configurations/`.

```csharp
// ✅ CORRECT
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
}

// ❌ WRONG
[Table("products")]
public class Product
{
    [Key]
    public Guid Id { get; set; }

    [Column("product_name")]
    public string Name { get; set; } = string.Empty;
}
```

---

## RULE 02 — Core Has Zero Infrastructure Dependencies

- The `*.Core` project's `.csproj` file must contain **no `<PackageReference>` or
  `<ProjectReference>`** pointing to EF Core, BCrypt, JWT, HTTP clients, cloud SDKs,
  or any third-party library.
- Allowed Core references: `Microsoft.Extensions.DependencyInjection.Abstractions`,
  `System.*`, language features, and pure domain libraries (e.g., FluentResults, if
  fully POCO-compatible).

```xml
<!-- ✅ CORRECT: YourApp.Core.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  <!-- No PackageReference or ProjectReference to anything external -->
</Project>

<!-- ❌ WRONG: adding EF Core to Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.*" />
```

---

## RULE 03 — Dependency Direction Is Inward Only

Dependencies must flow:

```
API → Infrastructure → Core
```

- **NEVER** let `*.Core` reference `*.Infrastructure` or `*.API`.
- **NEVER** let `*.Infrastructure` reference `*.API`.
- Use interfaces (ports) in Core for all outbound dependencies.

---

## RULE 04 — All Ports Are Interfaces, All Adapters Are in Infrastructure

- Every external concern (database, email, file system, HTTP client, cryptography,
  messaging) **must** be represented by an interface in
  `*.Core/Application/Interfaces/`.
- The concrete implementation lives exclusively in `*.Infrastructure/`.

| External Concern | Port (Core) | Adapter (Infrastructure) |
|------------------|-------------|--------------------------|
| Database access | `IOrderRepository` | `OrderRepository : IOrderRepository` |
| Email sending | `IEmailNotifier` | `SmtpEmailNotifier : IEmailNotifier` |
| Password hashing | `IPasswordHasher` | `BCryptPasswordHasher : IPasswordHasher` |
| JWT generation | `ITokenProvider` | `JwtTokenProvider : ITokenProvider` |
| File storage | `IFileStorage` | `AzureBlobStorage : IFileStorage` |

---

## RULE 05 — Application Services Are the Only Entry Point to Core Logic

- Controllers (or Minimal API handlers) **must not** directly call repositories,
  DbContext, or any infrastructure class.
- The only constructor injection allowed in a controller from `*.Core` is an
  **Application Service** class or, for simple read-only queries, a read-only port
  interface.

```csharp
// ✅ CORRECT
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrdersController(OrderService orderService) => _orderService = orderService;
}

// ❌ WRONG
public class OrdersController : ControllerBase
{
    private readonly AppDbContext _db; // Infrastructure leaking into API
    public OrdersController(AppDbContext db) => _db = db;
}
```

---

## RULE 06 — No Business Logic in Controllers

- Controllers are responsible **only** for:
  - Receiving and validating HTTP input (via model binding + FluentValidation or
    DataAnnotations).
  - Delegating to an Application Service.
  - Mapping the result to an HTTP response.
- **NEVER** place `if` statements, domain validations, or entity manipulation in
  controllers.

```csharp
// ✅ CORRECT
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
{
    var order = await _orderService.PlaceOrderAsync(req.CustomerId, req.TotalAmount, ct);
    return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapToResponse(order));
}

// ❌ WRONG — business logic in controller
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateOrderRequest req)
{
    if (req.TotalAmount <= 0) return BadRequest("Invalid amount"); // domain rule leaking here
    var order = new Order { CustomerId = req.CustomerId };         // entity construction here
    _db.Orders.Add(order);                                         // infrastructure here
    await _db.SaveChangesAsync();
    return Ok(order);
}
```

---

## RULE 07 — EF Core Fluent API Is Mandatory for All Mappings

- Create one `IEntityTypeConfiguration<T>` class per entity, placed in
  `*.Infrastructure/Data/Configurations/`.
- Apply all configurations via `modelBuilder.ApplyConfigurationsFromAssembly(...)`.
- **NEVER** configure entity mappings inline inside `OnModelCreating` using lambdas
  (except calling `ApplyConfigurationsFromAssembly`).

```csharp
// ✅ CORRECT
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

// ❌ WRONG
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Order>().ToTable("orders").HasKey(o => o.Id); // inline — grows unmanageable
}
```

---

## RULE 08 — DI Registration Lives Exclusively in Infrastructure and API

- Infrastructure registers its own adapters via a static extension method
  (`AddInfrastructure`).
- Core Application Services are registered in `Program.cs` (API layer).
- **NEVER** call `services.AddScoped<>()` inside a Core class or Application Service.

```csharp
// ✅ CORRECT — Infrastructure/DependencyInjection.cs
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<AppDbContext>(o => o.UseSqlServer(config.GetConnectionString("Default")));
    services.AddScoped<IOrderRepository, OrderRepository>();
    services.AddScoped<IEmailNotifier, SmtpEmailNotifier>();
    return services;
}

// ✅ CORRECT — Program.cs (API)
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<OrderService>();
```

---

## RULE 09 — Always Use CancellationToken in Async Methods

- Every `async` method signature in repositories, services, and controllers **must**
  accept a `CancellationToken ct = default` parameter.
- Pass `ct` through every async call chain.

```csharp
// ✅ CORRECT
public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    => await _db.Orders.FindAsync([id], ct);

// ❌ WRONG
public async Task<Order?> GetByIdAsync(Guid id)
    => await _db.Orders.FindAsync(id); // not cancellable
```

---

## RULE 10 — Domain Entities Use Private Setters and Factory Methods

- Expose state mutation **only** through explicitly named domain methods.
- Constructors may be `private` or `protected` to force the use of factory methods.
- EF Core can use the private parameterless constructor — keep it `private`.

```csharp
// ✅ CORRECT
public class Order
{
    public Guid Id { get; private set; }
    public OrderStatus Status { get; private set; }

    private Order() { } // EF Core only

    public static Order Create(string customerId, decimal amount) { … }
    public void Confirm() { … }   // named mutation
    public void Cancel() { … }    // named mutation
}

// ❌ WRONG
public class Order
{
    public Guid Id { get; set; }         // public setter — anyone can mutate
    public OrderStatus Status { get; set; }
}
```

---

## RULE 11 — Request and Response Models Are API-Layer Only

- `*Request` and `*Response` / `*Dto` types used for HTTP serialization belong to
  `*.API/Models/`.
- **NEVER** return domain entities directly from controllers.
- **NEVER** use API request models as parameters to Core Application Services.
- Application Services work with primitive types or internal Core DTOs.

```csharp
// ✅ CORRECT — controller maps between API models and Core
var order = await _orderService.PlaceOrderAsync(request.CustomerId, request.TotalAmount, ct);
return Ok(new OrderResponse(order.Id, order.Status.ToString(), order.TotalAmount));

// ❌ WRONG — entity exposed directly
return Ok(order); // serializes private fields, couples API to domain model
```

---

## RULE 12 — Tests Must Not Depend on Infrastructure

- `*.Core.Tests` projects must reference only `*.Core`, `Moq` (or NSubstitute),
  and xUnit / NUnit.
- **NEVER** reference `Microsoft.EntityFrameworkCore` or any adapter in Core tests.
- Use in-memory fake implementations or mocks for all port interfaces.

```csharp
// ✅ CORRECT — pure unit test
var repo = new Mock<IOrderRepository>();
var sut = new OrderService(repo.Object, new Mock<IEmailNotifier>().Object);
var order = await sut.PlaceOrderAsync("cust-1", 50m);
Assert.Equal(OrderStatus.Pending, order.Status);

// ❌ WRONG — infrastructure in unit test
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase("test").Build();      // EF leak into Core tests
```

---

## RULE 13 — Avoid Static Classes for External Concerns

- **NEVER** call static methods that wrap external libraries (e.g., `BCrypt.HashPassword(…)`,
  `File.ReadAllText(…)`, `DateTime.Now`) directly inside Core.
- Wrap them behind interfaces so they can be mocked and swapped.

```csharp
// ✅ CORRECT
public interface IPasswordHasher
{
    string Hash(string plainText);
    bool Verify(string plainText, string hash);
}

// ❌ WRONG — static call inside Core
public class UserService
{
    public void Register(string password)
        => BCrypt.HashPassword(password); // untestable, infra in Core
}
```

---

## RULE 14 — HttpClient Is Always Injected via IHttpClientFactory

- **NEVER** instantiate `new HttpClient()` directly.
- Register typed or named clients in `AddInfrastructure` and inject them into
  adapters.

```csharp
// ✅ CORRECT
services.AddHttpClient<PaymentGatewayAdapter>(client =>
    client.BaseAddress = new Uri(config["PaymentGateway:BaseUrl"]!));

// ❌ WRONG
public class PaymentGatewayAdapter
{
    private readonly HttpClient _client = new(); // resource leak + untestable
}
```

---

## RULE 15 — Migrations Target Infrastructure, Startup Is API

```bash
# ✅ CORRECT
dotnet ef migrations add <Name> \
  --project src/YourApp.Infrastructure \
  --startup-project src/YourApp.API

# ❌ WRONG — running migrations from Core or skipping --startup-project
dotnet ef migrations add <Name> --project src/YourApp.Core
```

---

## Summary Checklist (apply before every PR / code review)

- [ ] Core `.csproj` has no infrastructure `<PackageReference>` or `<ProjectReference>`
- [ ] All entity properties use `private set` or are readonly
- [ ] No EF/ORM attributes (`[Key]`, `[Column]`, etc.) on Core entities
- [ ] Each external concern has a port interface in Core and an adapter in Infrastructure
- [ ] Controllers only call Application Services — no repositories or DbContext directly
- [ ] No business logic (domain rules, validations) in Controllers or Infrastructure
- [ ] All async methods accept and propagate `CancellationToken`
- [ ] Request/Response models stay in `*.API/Models/` — entities never exposed directly
- [ ] DI registrations only in `DependencyInjection.cs` and `Program.cs`
- [ ] Core tests use only mocks or in-memory fakes — no EF Core reference
- [ ] Migrations use `--project Infrastructure --startup-project API`

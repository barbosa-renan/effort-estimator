# Examples — API Layer

## Composition Root (`Program.cs`)

The API project is the only place that knows about all layers simultaneously.
It wires everything together and starts the application.

```csharp
// YourApp.API/Program.cs
using YourApp.Core.Application.Services;
using YourApp.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 1. Infrastructure registers all adapter → port bindings
builder.Services.AddInfrastructure(builder.Configuration);

// 2. Core Application Services are registered here
builder.Services.AddScoped<OrderService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

## Controller (Primary Adapter)

Controllers handle HTTP only: binding input, delegating to an Application Service,
and mapping the result to a response. No business logic, no domain rules, no
repository calls.

```csharp
// YourApp.API/Controllers/OrdersController.cs
using Microsoft.AspNetCore.Mvc;
using YourApp.API.Models.Requests;
using YourApp.API.Models.Responses;
using YourApp.Core.Application.Services;
using YourApp.Core.Domain.Exceptions;

namespace YourApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService) => _orderService = orderService;

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder(
        [FromBody] PlaceOrderRequest request,
        CancellationToken ct)
    {
        var order = await _orderService.PlaceOrderAsync(
            request.CustomerId, request.TotalAmount, ct);

        return CreatedAtAction(
            nameof(GetOrder),
            new { id = order.Id },
            new OrderResponse(order.Id, order.Status.ToString(), order.TotalAmount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        var orders = await _orderService.GetCustomerOrdersAsync(id.ToString(), ct);
        var order = orders.FirstOrDefault();

        return order is null
            ? NotFound()
            : Ok(new OrderResponse(order.Id, order.Status.ToString(), order.TotalAmount));
    }

    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmOrder(Guid id, CancellationToken ct)
    {
        await _orderService.ConfirmOrderAsync(id, ct);
        return NoContent();
    }
}
```

---

## Global Exception Handling (Middleware)

Translate domain exceptions to HTTP responses without polluting controllers with
try/catch blocks.

```csharp
// YourApp.API/Middleware/GlobalExceptionHandler.cs
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YourApp.Core.Domain.Exceptions;

namespace YourApp.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            DomainException => (StatusCodes.Status400BadRequest, "Domain rule violation"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        await ctx.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = ex.Message
        }, ct);

        return true;
    }
}
```

Register in `Program.cs`:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
// …
app.UseExceptionHandler();
```

---

## Request / Response Models (API layer only)

These types never cross into Core. Application Services receive primitives or Core
DTOs, not request models.

```csharp
// YourApp.API/Models/Requests/PlaceOrderRequest.cs
namespace YourApp.API.Models.Requests;

public record PlaceOrderRequest(string CustomerId, decimal TotalAmount);

// YourApp.API/Models/Responses/OrderResponse.cs
namespace YourApp.API.Models.Responses;

public record OrderResponse(Guid Id, string Status, decimal TotalAmount);
```

---

## Inter-Module Port (Cross-Domain Communication)

When Module A needs data from Module B, define a port in Module A's Core and implement
it as an adapter in Module B's Infrastructure. Neither Core touches the other directly.

```
ModuleA.Core                          ModuleB
  IInventoryServicePort  ←──────────── InventoryServiceAdapter
  (outbound port)                        implements IInventoryServicePort
                                         delegates to InventoryService (ModuleB.Core)
```

```csharp
// ModuleA.Core/Application/Interfaces/Services/IInventoryServicePort.cs
public interface IInventoryServicePort
{
    Task<bool> IsAvailableAsync(Guid productId, int quantity, CancellationToken ct = default);
}

public record InventoryStatus(Guid ProductId, bool IsAvailable, int Stock);
```

```csharp
// ModuleB.Infrastructure/Adapters/InventoryServiceAdapter.cs
public class InventoryServiceAdapter : IInventoryServicePort
{
    private readonly InventoryService _inventoryService;

    public InventoryServiceAdapter(InventoryService inventoryService)
        => _inventoryService = inventoryService;

    public async Task<bool> IsAvailableAsync(Guid productId, int quantity, CancellationToken ct = default)
    {
        var stock = await _inventoryService.GetStockAsync(productId, ct);
        return stock >= quantity;
    }
}
```

Register in ModuleB's `DependencyInjection.cs`:

```csharp
services.AddScoped<IInventoryServicePort, InventoryServiceAdapter>();
```

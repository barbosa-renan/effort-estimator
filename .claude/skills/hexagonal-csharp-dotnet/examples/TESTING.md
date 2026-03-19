# Examples — Testing

## Unit Tests — Core Service with Mocked Ports

Core tests reference **only** `*.Core` and a mocking library (Moq or NSubstitute).
No EF Core, no DbContext, no HTTP client — those are infrastructure details.

```csharp
// tests/YourApp.Core.Tests/OrderServiceTests.cs
using Moq;
using YourApp.Core.Application.Interfaces.Repositories;
using YourApp.Core.Application.Interfaces.Services;
using YourApp.Core.Application.Services;
using YourApp.Core.Domain.Entities;
using YourApp.Core.Domain.Exceptions;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IEmailNotifier> _notifierMock = new();
    private readonly OrderService _sut;

    public OrderServiceTests()
        => _sut = new OrderService(_repoMock.Object, _notifierMock.Object);

    [Fact]
    public async Task PlaceOrder_ValidInput_ReturnsOrderWithPendingStatus()
    {
        var order = await _sut.PlaceOrderAsync("customer-1", 99.99m);

        Assert.Equal("customer-1", order.CustomerId);
        Assert.Equal(OrderStatus.Pending, order.Status);
        _repoMock.Verify(r => r.AddAsync(order, default), Times.Once);
    }

    [Fact]
    public async Task PlaceOrder_NegativeAmount_ThrowsDomainException()
    {
        await Assert.ThrowsAsync<DomainException>(
            () => _sut.PlaceOrderAsync("customer-1", -1m));
    }

    [Fact]
    public async Task ConfirmOrder_NotFound_ThrowsDomainException()
    {
        _repoMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), default))
                 .ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<DomainException>(
            () => _sut.ConfirmOrderAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ConfirmOrder_ValidOrder_SendsEmailNotification()
    {
        var order = Order.Create("customer-1", 50m);
        _repoMock.Setup(r => r.GetByIdAsync(order.Id, default))
                 .ReturnsAsync(order);

        await _sut.ConfirmOrderAsync(order.Id);

        _notifierMock.Verify(
            n => n.SendOrderConfirmationAsync(order.CustomerId, order.Id, default),
            Times.Once);
    }
}
```

---

## Unit Tests — Domain Entity

Test domain rules directly — no mocks needed.

```csharp
// tests/YourApp.Core.Tests/OrderTests.cs
public class OrderTests
{
    [Fact]
    public void Create_ValidInput_ReturnsPendingOrder()
    {
        var order = Order.Create("cust-1", 100m);

        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal("cust-1", order.CustomerId);
        Assert.Equal(100m, order.TotalAmount);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_InvalidCustomerId_ThrowsDomainException(string? customerId)
    {
        Assert.Throws<DomainException>(() => Order.Create(customerId!, 100m));
    }

    [Fact]
    public void Confirm_PendingOrder_ChangesStatusToConfirmed()
    {
        var order = Order.Create("cust-1", 100m);
        order.Confirm();
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public void Confirm_AlreadyConfirmed_ThrowsDomainException()
    {
        var order = Order.Create("cust-1", 100m);
        order.Confirm();

        Assert.Throws<DomainException>(() => order.Confirm());
    }
}
```

---

## In-Memory Fake Repository (alternative to Moq)

Fakes are useful when you want stateful behavior across multiple calls without
verbose mock setups.

```csharp
// tests/YourApp.Core.Tests/Fakes/InMemoryOrderRepository.cs
using YourApp.Core.Application.Interfaces.Repositories;
using YourApp.Core.Domain.Entities;

public class InMemoryOrderRepository : IOrderRepository
{
    private readonly List<Order> _store = [];

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => Task.FromResult(_store.FirstOrDefault(o => o.Id == id));

    public Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        string customerId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<Order>>(
               _store.Where(o => o.CustomerId == customerId).ToList());

    public Task AddAsync(Order order, CancellationToken ct = default)
    {
        _store.Add(order);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order order, CancellationToken ct = default)
        => Task.CompletedTask; // list holds the reference — already updated
}
```

Using the fake:

```csharp
public class OrderServiceWithFakeTests
{
    private readonly InMemoryOrderRepository _repo = new();
    private readonly Mock<IEmailNotifier> _notifierMock = new();
    private readonly OrderService _sut;

    public OrderServiceWithFakeTests()
        => _sut = new OrderService(_repo, _notifierMock.Object);

    [Fact]
    public async Task PlaceAndConfirm_FullFlow_OrderIsConfirmed()
    {
        var order = await _sut.PlaceOrderAsync("cust-1", 200m);
        await _sut.ConfirmOrderAsync(order.Id);

        var saved = await _repo.GetByIdAsync(order.Id);
        Assert.Equal(OrderStatus.Confirmed, saved!.Status);
    }
}
```

---

## Core Tests `.csproj` — No Infrastructure Reference

```xml
<!-- tests/YourApp.Core.Tests/YourApp.Core.Tests.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="Moq" Version="4.*" />
  </ItemGroup>

  <ItemGroup>
    <!-- Only Core — never Infrastructure or API -->
    <ProjectReference Include="..\..\src\YourApp.Core\YourApp.Core.csproj" />
  </ItemGroup>
</Project>
```

---

## Integration Tests (Infrastructure + real DB)

Integration tests live in a separate project and may reference Infrastructure.
Use `WebApplicationFactory<Program>` for end-to-end API tests.

```csharp
// tests/YourApp.Integration.Tests/OrdersApiTests.cs
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;

public class OrdersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersApiTests(WebApplicationFactory<Program> factory)
        => _client = factory.CreateClient();

    [Fact]
    public async Task PlaceOrder_ReturnsCreated()
    {
        var response = await _client.PostAsJsonAsync("/api/orders", new
        {
            CustomerId = "cust-integration-1",
            TotalAmount = 150.00m
        });

        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
    }
}
```

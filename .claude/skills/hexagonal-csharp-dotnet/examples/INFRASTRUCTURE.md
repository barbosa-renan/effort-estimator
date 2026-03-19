# Examples — Infrastructure Layer

## EF Core DbContext

```csharp
// YourApp.Infrastructure/Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using YourApp.Core.Domain.Entities;

namespace YourApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        // Discovers all IEntityTypeConfiguration<T> in this assembly automatically
        => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
```

---

## Fluent API Configuration (one file per entity)

Never configure mappings inline inside `OnModelCreating`. One `IEntityTypeConfiguration<T>`
class per entity, placed in `Infrastructure/Data/Configurations/`.

```csharp
// YourApp.Infrastructure/Data/Configurations/OrderConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YourApp.Core.Domain.Entities;

namespace YourApp.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(o => o.TotalAmount)
               .HasColumnType("decimal(18,2)")
               .IsRequired();

        builder.Property(o => o.Status)
               .HasConversion<string>()   // stores enum as string
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(o => o.CreatedAt)
               .IsRequired();
    }
}
```

---

## Repository Implementation (Secondary Adapter)

```csharp
// YourApp.Infrastructure/Repositories/OrderRepository.cs
using Microsoft.EntityFrameworkCore;
using YourApp.Core.Application.Interfaces.Repositories;
using YourApp.Core.Domain.Entities;
using YourApp.Infrastructure.Data;

namespace YourApp.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.Orders.FindAsync([id], ct);

    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        string customerId, CancellationToken ct = default)
        => await _db.Orders
                    .Where(o => o.CustomerId == customerId)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(ct);
    }
}
```

---

## External Service Adapters

### Email Adapter

```csharp
// YourApp.Infrastructure/ExternalServices/SmtpEmailNotifier.cs
using YourApp.Core.Application.Interfaces.Services;

namespace YourApp.Infrastructure.ExternalServices;

public class SmtpEmailNotifier : IEmailNotifier
{
    private readonly HttpClient _client;

    public SmtpEmailNotifier(HttpClient client) => _client = client;

    public async Task SendOrderConfirmationAsync(
        string to, Guid orderId, CancellationToken ct = default)
    {
        // Send via SMTP / SendGrid / SES — implementation detail hidden from Core
        await Task.CompletedTask;
    }
}
```

### Password Hasher Adapter

```csharp
// YourApp.Infrastructure/ExternalServices/BCryptPasswordHasher.cs
using BCrypt.Net;
using YourApp.Core.Application.Interfaces.Services;

namespace YourApp.Infrastructure.ExternalServices;

public class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainText) => BCrypt.HashPassword(plainText);
    public bool Verify(string plainText, string hash) => BCrypt.Verify(plainText, hash);
}
```

---

## DI Registration Extension Method

All adapter-to-port bindings are declared here. This is the only file in Infrastructure
that calls `services.AddScoped<>` / `AddTransient<>` / `AddSingleton<>`.

```csharp
// YourApp.Infrastructure/DependencyInjection.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YourApp.Core.Application.Interfaces.Repositories;
using YourApp.Core.Application.Interfaces.Services;
using YourApp.Infrastructure.Data;
using YourApp.Infrastructure.ExternalServices;
using YourApp.Infrastructure.Repositories;

namespace YourApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Repository ports → adapters
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Service ports → adapters
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddHttpClient<SmtpEmailNotifier>();
        services.AddScoped<IEmailNotifier, SmtpEmailNotifier>();

        return services;
    }
}
```

---

## Migrations

DbContext lives in Infrastructure; the startup project is always API.

```bash
# Add migration
dotnet ef migrations add <MigrationName> \
  --project src/YourApp.Infrastructure \
  --startup-project src/YourApp.API \
  --output-dir Data/Migrations

# Apply to database
dotnet ef database update \
  --project src/YourApp.Infrastructure \
  --startup-project src/YourApp.API
```

---
name: swagger-dotnet-10
description: >
  Use this skill whenever the user wants to document a .NET 10 API with Swagger/OpenAPI.
  Triggers include: 'swagger', 'openapi', 'document API', 'api docs', 'Swashbuckle',
  'NSwag', '.NET API documentation', 'xml comments', 'swagger UI', or requests to add
  documentation to ASP.NET Core controllers, endpoints, DTOs, or minimal APIs.
  Also use when reviewing, generating, or fixing OpenAPI specs, response schemas,
  security definitions, or versioning in .NET projects.
license: MIT
---

# Swagger / OpenAPI Documentation — .NET 10

## Overview

.NET 10 uses **ASP.NET Core** with built-in OpenAPI support via `Microsoft.AspNetCore.OpenApi`
(the recommended path) or third-party libraries such as **Swashbuckle** and **NSwag**.
This skill covers setup, annotation conventions, best practices, and common pitfalls.

---

## Quick Reference

| Goal | Recommended Approach |
|------|----------------------|
| Quick setup (.NET 10 native) | `Microsoft.AspNetCore.OpenApi` + scalar/swagger-ui |
| Full-featured UI + codegen | Swashbuckle.AspNetCore |
| Client codegen priority | NSwag |
| API Versioning | `Asp.Versioning.Http` + per-version OpenAPI docs |
| Auth documentation | `AddSecurityDefinition` / `SecurityRequirement` |
| XML Comments | Enable in `.csproj` + `IncludeXmlComments()` |

---

## 1. Package Installation

### Option A — .NET 10 Native (Microsoft.AspNetCore.OpenApi)

```bash
dotnet add package Microsoft.AspNetCore.OpenApi
dotnet add package Scalar.AspNetCore          # Modern Swagger UI alternative
```

### Option B — Swashbuckle (most widely used)

```bash
dotnet add package Swashbuckle.AspNetCore
dotnet add package Swashbuckle.AspNetCore.Annotations   # [SwaggerOperation] etc.
dotnet add package Swashbuckle.AspNetCore.Filters        # Response examples
```

### Option C — NSwag

```bash
dotnet add package NSwag.AspNetCore
```

---

## 2. Program.cs Setup

### .NET 10 Native + Scalar

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new()
        {
            Title   = "My API",
            Version = "v1",
            Description = "Full description of the API",
            Contact = new() { Name = "Team Name", Email = "team@company.com" }
        };
        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();           // /openapi/v1.json
    app.MapScalarApiReference();// /scalar/v1  (interactive UI)
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

### Swashbuckle Setup

```csharp
// Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "My API",
        Version     = "v1",
        Description = "API description",
        Contact     = new OpenApiContact { Name = "Team", Email = "team@company.com" },
        License     = new OpenApiLicense { Name = "MIT" }
    });

    // XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    // Annotations support
    options.EnableAnnotations();

    // JWT Security definition
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "JWT token. Example: Bearer {token}"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                    { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ...

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty; // Swagger at root
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}
```

---

## 3. Enabling XML Comments in .csproj

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>  <!-- Suppress missing XML doc warnings -->
</PropertyGroup>
```

---

## 4. Controller Annotation Patterns

### Full Controller Example

```csharp
/// <summary>
/// Operations related to customer management.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
[Tags("Customers")]
public class CustomersController : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of customers.
    /// </summary>
    /// <param name="page">Page number (starts at 1).</param>
    /// <param name="pageSize">Number of records per page (max 100).</param>
    /// <returns>A paginated list of customers.</returns>
    /// <response code="200">Returns the customer list successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="401">Unauthorized — missing or invalid token.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CustomerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        // ...
    }

    /// <summary>
    /// Retrieves a customer by their unique identifier.
    /// </summary>
    /// <param name="id">The customer's GUID.</param>
    /// <returns>Customer details.</returns>
    /// <response code="200">Customer found.</response>
    /// <response code="404">Customer not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id) { /* ... */ }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="request">Customer creation payload.</param>
    /// <returns>The newly created customer.</returns>
    /// <response code="201">Customer created successfully.</response>
    /// <response code="400">Validation error in the request body.</response>
    /// <response code="409">A customer with this email already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request) { /* ... */ }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequest request) { /* ... */ }

    /// <summary>
    /// Removes a customer from the system.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id) { /* ... */ }
}
```

---

## 5. DTO Annotation Patterns

```csharp
/// <summary>
/// Payload for creating a new customer.
/// </summary>
public record CreateCustomerRequest
{
    /// <summary>Full name of the customer.</summary>
    /// <example>João da Silva</example>
    [Required]
    [StringLength(150, MinimumLength = 3)]
    public string Name { get; init; } = default!;

    /// <summary>Unique email address used for login.</summary>
    /// <example>joao.silva@email.com</example>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;

    /// <summary>Customer's date of birth (ISO 8601).</summary>
    /// <example>1990-05-20</example>
    [Required]
    public DateOnly BirthDate { get; init; }
}

/// <summary>
/// Customer response contract.
/// </summary>
public record CustomerResponse
{
    /// <summary>Unique identifier.</summary>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; init; }

    /// <summary>Full customer name.</summary>
    /// <example>João da Silva</example>
    public string Name { get; init; } = default!;

    /// <summary>Email address.</summary>
    /// <example>joao.silva@email.com</example>
    public string Email { get; init; } = default!;

    /// <summary>Timestamp of creation in UTC.</summary>
    /// <example>2025-03-20T10:00:00Z</example>
    public DateTimeOffset CreatedAt { get; init; }
}
```

---

## 6. Minimal API Documentation (.NET 10)

```csharp
app.MapGet("/api/products/{id:guid}", async (Guid id, IProductService svc) =>
{
    var product = await svc.GetByIdAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);
})
.WithName("GetProductById")
.WithSummary("Get a product by ID")
.WithDescription("Returns the product matching the provided GUID.")
.WithTags("Products")
.Produces<ProductResponse>(StatusCodes.Status200OK)
.Produces<ProblemDetails>(StatusCodes.Status404NotFound)
.RequireAuthorization();
```

---

## 7. API Versioning + Per-Version Swagger Docs

```bash
dotnet add package Asp.Versioning.Http
dotnet add package Asp.Versioning.ApiExplorer
```

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// Register one SwaggerDoc per discovered version
builder.Services.AddSwaggerGen(options =>
{
    var provider = builder.Services
        .BuildServiceProvider()
        .GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var desc in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(desc.GroupName, new OpenApiInfo
        {
            Title   = $"My API {desc.ApiVersion}",
            Version = desc.GroupName,
            Description = desc.IsDeprecated ? "⚠️ This version is deprecated." : string.Empty
        });
    }
    // ... xml comments, security, etc.
});
```

---

## 8. Response Examples with Swashbuckle Filters

```csharp
// Install: Swashbuckle.AspNetCore.Filters
builder.Services.AddSwaggerGen(options =>
{
    options.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

// Define an example
public class CreateCustomerRequestExample : IExamplesProvider<CreateCustomerRequest>
{
    public CreateCustomerRequest GetExamples() => new()
    {
        Name      = "João da Silva",
        Email     = "joao@email.com",
        BirthDate = new DateOnly(1990, 5, 20)
    };
}

// Apply to endpoint
[SwaggerRequestExample(typeof(CreateCustomerRequest), typeof(CreateCustomerRequestExample))]
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request) { /* ... */ }
```

---

## 9. ProblemDetails Standard (RFC 9457)

Always configure `ProblemDetails` for consistent error responses:

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = Activity.Current?.Id
            ?? ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["instance"] =
            $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

// In controllers, always return Problem() instead of raw error strings
return NotFound(new ProblemDetails
{
    Title    = "Customer not found",
    Detail   = $"No customer with ID {id} was found.",
    Status   = StatusCodes.Status404NotFound
});
```

---

## 10. Health Check Documentation

```csharp
app.MapHealthChecks("/health/live",  new() { Predicate = _ => false })
   .WithTags("Health")
   .WithSummary("Liveness probe — checks if the process is alive.");

app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") })
   .WithTags("Health")
   .WithSummary("Readiness probe — checks if dependencies (DB, cache) are ready.");
```

---

## 11. Checklist Before Merging

- [ ] All public controllers and actions have XML `<summary>` comments
- [ ] All DTOs have `<summary>` and `<example>` on every property
- [ ] Every action has `[ProducesResponseType]` for all possible HTTP status codes
- [ ] Security definition matches actual auth scheme (Bearer JWT / OAuth2 / ApiKey)
- [ ] API version is reflected in OpenAPI `Info.Version`
- [ ] `GenerateDocumentationFile` is enabled in `.csproj`
- [ ] Swagger UI is **only** exposed in Development environment
- [ ] `ProblemDetails` is used for all error responses
- [ ] Deprecated endpoints are annotated with `[Obsolete]` and `[ApiExplorerSettings]`

---

## 12. Common Pitfalls

| Problem | Fix |
|---------|-----|
| XML comments not appearing | Enable `<GenerateDocumentationFile>true</GenerateDocumentationFile>` in `.csproj` |
| Swagger exposed in production | Wrap `UseSwagger()` inside `if (app.Environment.IsDevelopment())` |
| Duplicate schema names | Use `options.CustomSchemaIds(t => t.FullName)` |
| Nullable types not shown | Use `options.UseAllOfToExtendReferenceSchemas()` |
| Enum shown as int | Add `options.UseInlineDefinitionsForEnums()` or `JsonStringEnumConverter` |
| Missing route params in spec | Ensure route template matches `[HttpGet("{id}")]` exactly |
| Version not substituted in URL | Set `SubstituteApiVersionInUrl = true` in `AddApiExplorer` |

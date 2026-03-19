---
name: hexagonal-csharp-dotnet
description: >
  Hexagonal Architecture (Ports & Adapters) for C# .NET APIs. Use when:
  creating new Web API projects, structuring Core/Infrastructure/API layers,
  defining ports (interfaces) and adapters (implementations), placing entities,
  repositories, or application services, configuring EF Core with Fluent API,
  registering dependencies via DI, or ensuring domain purity. Business-agnostic
  — applies to any domain.
---

# Hexagonal Architecture for C# .NET APIs

## Core Principle

The **Domain (Core)** must have **ZERO dependencies** on infrastructure, external
libraries, or frameworks. All dependencies point inward — from outer layers toward
the domain.

```
┌──────────────────────────────────────────────────┐
│               API  (Primary Adapters)            │
│  Controllers · Endpoints · Middleware            │
│  DTOs · Request/Response Models                  │
│  DI Composition Root (Program.cs)                │
└────────────────────┬─────────────────────────────┘
                     │  references
┌────────────────────▼─────────────────────────────┐
│           INFRASTRUCTURE  (Secondary Adapters)   │
│  EF Core DbContext + Fluent API Configurations   │
│  Repository & External Service Implementations   │
│  DependencyInjection.cs (extension method)       │
└────────────────────┬─────────────────────────────┘
                     │  implements interfaces from
┌────────────────────▼─────────────────────────────┐
│                  CORE  (The Hexagon)             │
│  Domain Entities (plain C# — no ORM attributes)  │
│  Port Interfaces  (IRepository, IService…)       │
│  Application Services (use-case orchestration)   │
│  Domain Exceptions · Value Objects               │
└──────────────────────────────────────────────────┘
```

**Dependency direction — memorize this:**
```
API → Infrastructure → Core      (Core → nothing)
```

---

## Project Structure

```
src/
├── YourApp.Core/                      # Zero external NuGet packages
│   ├── Domain/
│   │   ├── Entities/                  # POCOs — no EF/ORM attributes
│   │   ├── ValueObjects/              # Immutable domain primitives (optional)
│   │   └── Exceptions/
│   └── Application/
│       ├── Interfaces/
│       │   ├── Repositories/          # Outbound ports — data access
│       │   └── Services/              # Outbound ports — external concerns
│       └── Services/                  # Use-case orchestration
│
├── YourApp.Infrastructure/            # Implements Core interfaces
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   └── Configurations/            # IEntityTypeConfiguration<T> per entity
│   ├── Repositories/
│   ├── ExternalServices/
│   └── DependencyInjection.cs
│
├── YourApp.API/                       # Composition Root
│   ├── Program.cs
│   ├── Controllers/                   # Or Endpoints/ for Minimal API
│   └── Models/
│       ├── Requests/
│       └── Responses/
│
└── tests/
    ├── YourApp.Core.Tests/            # Pure unit tests — no infrastructure
    └── YourApp.Integration.Tests/
```

---

## Quick Reference

| Component | Project | Namespace Pattern | Depends On |
|-----------|---------|-------------------|------------|
| Domain Entity | `*.Core` | `…Core.Domain.Entities` | Nothing |
| Domain Exception | `*.Core` | `…Core.Domain.Exceptions` | Nothing |
| Value Object | `*.Core` | `…Core.Domain.ValueObjects` | Nothing |
| Port — Repository | `*.Core` | `…Core.Application.Interfaces.Repositories` | Core entities |
| Port — Service | `*.Core` | `…Core.Application.Interfaces.Services` | Core entities |
| Application Service | `*.Core` | `…Core.Application.Services` | Core interfaces only |
| EF Configuration | `*.Infrastructure` | `…Infrastructure.Data.Configurations` | Core entities + EF Core |
| Repository Impl | `*.Infrastructure` | `…Infrastructure.Repositories` | Core interface + EF Core |
| External Adapter | `*.Infrastructure` | `…Infrastructure.ExternalServices` | Core interface + 3rd-party lib |
| DI Registration | `*.Infrastructure` | `…Infrastructure.DependencyInjection` | All Infrastructure |
| Controller | `*.API` | `…API.Controllers` | Core Application Services |
| Request / Response | `*.API` | `…API.Models.Requests · Responses` | Nothing |
| Composition Root | `*.API` | `Program.cs` | Infrastructure + Core |

---

## When to Create a New Port

Create an interface in Core whenever a dependency:

1. Uses an **external library** (BCrypt, JWT, SendGrid, Twilio…)
2. Touches **database, filesystem, network, or clock**
3. **Varies by environment** (stub in dev, real in prod)
4. **Needs mocking** in unit tests
5. Represents an **I/O boundary** the domain cares about logically, not technically

---

## Detailed Examples (by layer)

- **[CORE](./examples/CORE.md)** — Entities, ports, application services, domain exceptions
- **[INFRASTRUCTURE](./examples/INFRASTRUCTURE.md)** — EF Core, Fluent API, repositories, external adapters, DI registration, migrations
- **[API](./examples/API.md)** — Controllers, composition root, request/response models, inter-module ports
- **[TESTING](./examples/TESTING.md)** — Unit tests with mocks, in-memory fakes, integration test structure

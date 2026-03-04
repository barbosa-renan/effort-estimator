# Project Structure — .NET Console Application

## Recommended Structure

```
MyApp/
├── MyApp.sln                           ← solution file, references all projects
└── src/
    └── MyApp/
        ├── MyApp.csproj                ← project file, dependencies and build configuration
        ├── Program.cs                  ← entry point and composition root
        ├── Models/                     ← data shapes, records and entities
        ├── Services/                   ← business logic and use case orchestration
        └── Utils/                      ← stateless helpers and extension methods
```

---

## Folder Responsibilities

### Models/

Contains data shapes used across the application — input models, output
records, enums, and any type whose sole purpose is to carry data.

| What belongs here | What does not belong here |
|---|---|
| Input and output `record` types | Classes with business logic |
| Enums replacing string discriminators | Services or calculators |
| Lightweight value objects | Anything that reads from or writes to I/O |

```
Models/
├── TaskInput.cs
├── EstimationResult.cs
├── ConfidenceRange.cs
└── Enums/
    ├── ComplexityLevel.cs
    └── RiskLevel.cs
```

### Services/

Contains business logic and use case orchestration. Classes here receive
models, apply rules, and return results. They do not read from console or
write to output directly.

| What belongs here | What does not belong here |
|---|---|
| Classes that implement business rules | Raw `Console.WriteLine` calls |
| Use case orchestration | JSON deserialization |
| Interfaces for the above | Data shape definitions |

```
Services/
├── IEstimationService.cs
└── EstimationService.cs
```

### Utils/

Contains stateless helpers and extension methods with no dependencies.
A class in Utils should never instantiate a service or hold state.

| What belongs here | What does not belong here |
|---|---|
| Extension methods on primitives or collections | Classes with constructor dependencies |
| Pure math or string utilities | Business logic |
| Stateless formatting helpers | Anything that changes based on configuration |

```
Utils/
├── MathExtensions.cs
└── StringExtensions.cs
```

### Program.cs

Single responsibility: **compose dependencies and start the application.**
Contains no business logic, no output formatting, no JSON parsing.

```csharp
// Ideal Program.cs
var service = new EstimationService();

var json   = args.Contains("--stdin") ? Console.In.ReadToEnd() : DefaultJson;
var input  = JsonSerializer.Deserialize<TaskInput>(json)!;
var result = service.Estimate(input);

Console.WriteLine(result);
```

---

## Dependency Rules

```
Program.cs  ->  Services  ->  Models
                   |
                 Utils
```

- **Models** has no dependencies on Services or Utils
- **Utils** has no dependencies on Models or Services
- **Services** depends on Models and may use Utils
- **Program.cs** depends on everything and is the only place that does I/O

---

## File and Type Naming Conventions

### One file per type

Every class, interface, record, and enum lives in its own file.
The filename must exactly match the type name.

```
// BAD - multiple types in one file
EstimationService.cs  (contains: TaskInput, EstimationResult,
                                 IEstimationService, EstimationService, Program)

// GOOD - one type per file
Models/TaskInput.cs
Models/EstimationResult.cs
Models/Enums/RiskLevel.cs
Services/IEstimationService.cs
Services/EstimationService.cs
Program.cs
```

### Naming patterns by type

| Type | Pattern | Example |
|---|---|---|
| Service interface | `I` + noun + `Service` | `IEstimationService` |
| Service implementation | noun + `Service` | `EstimationService` |
| Input model | noun + `Input` | `TaskInput` |
| Output model | noun + `Result` | `EstimationResult` |
| Enum | singular noun | `RiskLevel`, `OrderStatus` |
| Utility class | noun + `Extensions` or `Utils` | `MathExtensions` |

---

## Recommended .csproj for .NET 10

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  </PropertyGroup>
</Project>
```

`<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`
are mandatory on new projects — they eliminate entire categories of runtime bugs
at compile time.

---

## Static Classes — When to Use and When Not To

| Scenario | Recommendation |
|---|---|
| Pure utility with no state and no dependencies | `static class` is fine — place in `Utils/` |
| Class with lookup tables or configuration data | Instance class in `Services/` |
| Class that implements a business rule | Instance class in `Services/` |

```csharp
// Acceptable - pure, stateless, no dependencies -> Utils/
public static class MathExtensions
{
    public static double RoundToDecimalPlaces(this double value, int places)
        => Math.Round(value, places);
}

// BAD - hides state, prevents testing -> should not be static
public static class EstimationService
{
    private static readonly Dictionary<string, double> Weights = new() { ... };
    public static EstimationResult Estimate(TaskInput input) { ... }
}

// GOOD - injectable instance -> Services/
public class EstimationService : IEstimationService
{
    private readonly EstimationWeights _weights;
    public EstimationService(EstimationWeights weights) => _weights = weights;
    public EstimationResult Estimate(TaskInput input) { ... }
}
```
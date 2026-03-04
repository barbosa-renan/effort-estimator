# SOLID Principles — .NET C#

## S — Single Responsibility Principle

Each class should have only one reason to change.

### Common violation pattern

```csharp
// BAD — one class handles data, logic, configuration, and output
public static class OrderProcessor
{
    private static readonly Dictionary<string, double> TaxRates = new() { ... };

    public static void Process(string json)
    {
        // deserializes input
        // applies business rules
        // calculates taxes using embedded table
        // prints result to console
    }
}
```

### Correct separation

Each class has one reason to change:

```csharp
// Changes only when tax rates change
public class TaxConfiguration
{
    public IReadOnlyDictionary<Region, double> Rates { get; } = ...;
}

// Changes only when tax calculation rules change
public class TaxCalculationService : ITaxCalculationService
{
    private readonly TaxConfiguration _config;
    public TaxCalculationService(TaxConfiguration config) => _config = config;
    public decimal Calculate(Order order) { ... }
}

// Changes only when output format changes
public class ConsoleOrderPresenter : IOrderPresenter
{
    public void Present(OrderResult result) { ... }
}

// Changes only when application composition changes
// Program.cs — wires everything together
```

---

## O — Open/Closed Principle

Classes should be open for extension and closed for modification.

### Violation: adding a feature requires modifying existing code

```csharp
// BAD — to support CSV export, you must edit this class
public class ReportGenerator
{
    public void Generate(Report report, string format)
    {
        if (format == "pdf") { ... }
        if (format == "excel") { ... }
        // adding "csv" means editing this method
    }
}
```

### Correct: new behavior through new classes

```csharp
public interface IReportExporter
{
    void Export(Report report);
}

public class PdfReportExporter  : IReportExporter { ... }
public class ExcelReportExporter : IReportExporter { ... }
public class CsvReportExporter  : IReportExporter { ... }  // new format, no existing code touched

public class ReportGenerator
{
    private readonly IReportExporter _exporter;
    public ReportGenerator(IReportExporter exporter) => _exporter = exporter;
    public void Generate(Report report) => _exporter.Export(report);
}
```

---

## L — Liskov Substitution Principle

Subtypes must be substitutable for their base types without altering
program behavior.

### Violation: override that breaks the contract

```csharp
// BAD — substituting Square for Rectangle changes behavior
public class Rectangle
{
    public virtual double Width  { get; set; }
    public virtual double Height { get; set; }
    public double Area => Width * Height;
}

public class Square : Rectangle
{
    public override double Width  { set { base.Width = base.Height = value; } }
    public override double Height { set { base.Width = base.Height = value; } }
}

// Code that works with Rectangle breaks with Square
void Resize(Rectangle r) { r.Width = 5; r.Height = 3; /* expects area = 15 */ }
```

### How to avoid

Prefer composition over inheritance. If a subtype cannot honor the full
contract of the base type, it should not extend it. Use interfaces to define
contracts only for what the implementing type can fully support.

---

## I — Interface Segregation Principle

No class should be forced to depend on methods it does not use.

### Violation: fat interface

```csharp
// BAD — forces every implementor to handle reading, validation, AND presentation
public interface IOrderService
{
    Order ReadFromJson(string json);
    void  Validate(Order order);
    OrderResult Process(Order order);
    void  PrintSummary(OrderResult result);
    void  ExportToCsv(OrderResult result);
}
```

### Correct: focused interfaces

```csharp
public interface IOrderReader    { Order Read(string json); }
public interface IOrderValidator { void Validate(Order order); }
public interface IOrderProcessor { OrderResult Process(Order order); }
public interface IOrderPresenter { void Present(OrderResult result); }
public interface IOrderExporter  { void Export(OrderResult result, string path); }
```

Each interface has one reason to change. Implementors only depend on what
they actually use.

---

## D — Dependency Inversion Principle

High-level modules should not depend on low-level modules.
Both should depend on abstractions.

### Violation: high-level class instantiates its own dependencies

```csharp
// BAD — OrderProcessor is coupled to concrete implementations
public class OrderProcessor
{
    public void Process(string json)
    {
        var validator = new OrderValidator();        // direct instantiation
        var calculator = new PriceCalculator();     // direct instantiation
        Console.WriteLine("Result: " + result);    // direct I/O coupling
    }
}
```

### Correct: dependencies injected via constructor

```csharp
// GOOD — OrderProcessor depends on abstractions, not implementations
public class OrderProcessor
{
    private readonly IOrderValidator _validator;
    private readonly IPriceCalculator _calculator;
    private readonly IOrderPresenter _presenter;

    public OrderProcessor(
        IOrderValidator validator,
        IPriceCalculator calculator,
        IOrderPresenter presenter)
    {
        _validator  = validator;
        _calculator = calculator;
        _presenter  = presenter;
    }

    public void Process(Order order)
    {
        _validator.Validate(order);
        var result = _calculator.Calculate(order);
        _presenter.Present(result);
    }
}

// Program.cs — the only place that knows about concrete types
var processor = new OrderProcessor(
    new OrderValidator(),
    new PriceCalculator(),
    new ConsoleOrderPresenter()
);
```

Benefit: `OrderProcessor` can be unit tested by injecting test doubles
without any dependency on console, database, or file system.

---

## DRY — Don't Repeat Yourself

### Violation: defensive fallback repeated per case

```csharp
// BAD — same fallback pattern duplicated for every lookup
var statusKey = input.Status?.ToLowerInvariant() ?? "pending";
if (!StatusMultipliers.TryGetValue(statusKey, out var statusMult))
    statusMult = StatusMultipliers["pending"];

var regionKey = input.Region?.ToLowerInvariant() ?? "default";
if (!RegionRates.TryGetValue(regionKey, out var regionRate))
    regionRate = RegionRates["default"];
```

### Correct: extract reusable helper

```csharp
private static TValue GetOrDefault<TKey, TValue>(
    IReadOnlyDictionary<TKey, TValue> dictionary,
    TKey key,
    TKey defaultKey) where TKey : notnull
    => dictionary.TryGetValue(key, out var value) ? value : dictionary[defaultKey];

// usage
var statusMult = GetOrDefault(StatusMultipliers, input.Status, OrderStatus.Pending);
var regionRate = GetOrDefault(RegionRates, input.Region, Region.Default);
```

When string discriminators are replaced by enums, this fallback becomes
unnecessary — enum values are always valid at compile time.

---

## KISS — Keep It Simple, Stupid

### Violation: static class with hidden state

```csharp
// BAD — static class hides dependencies, prevents testing, couples callers
public static class PriceCalculator
{
    private static readonly Dictionary<string, double> TaxRates = new() { ... };
    public static decimal Calculate(Order order) { ... }
}
```

### Correct: instance class with explicit dependencies

```csharp
// GOOD — dependencies are visible, injectable, and testable
public class PriceCalculator : IPriceCalculator
{
    private readonly TaxConfiguration _taxConfig;

    public PriceCalculator(TaxConfiguration taxConfig)
        => _taxConfig = taxConfig;

    public decimal Calculate(Order order) { ... }
}
```

Use `static` only for pure utility methods with no state and no dependencies:

```csharp
// Acceptable static usage — pure function, no state, no dependencies
public static class MathUtils
{
    public static double RoundToDecimalPlaces(double value, int places)
        => Math.Round(value, places);
}
```
# Naming Conventions — .NET C#

## Core Principle

A good name eliminates the need for a comment. If you need a comment to
explain what a variable does, the name is wrong.

---

## Local Variables

### Intent-revealing names

Names should describe what the variable holds, not its type or a shortened
version of its role.

```csharp
// BAD — opaque abbreviations
double sd    = (p - o) / 6.0;
double cv    = sd / est;
int    cnt   = items?.Count ?? 0;
var    k     = input.Category?.ToLowerInvariant() ?? "default";
double mult  = 1 + cnt * (base - 1);

// GOOD — clear intent
double standardDeviation      = (pessimisticHours - optimisticHours) / 6.0;
double coefficientOfVariation = standardDeviation / estimatedHours;
int    itemCount              = items?.Count ?? 0;
var    categoryKey            = input.Category?.ToLowerInvariant() ?? "default";
double categoryMultiplier     = 1 + itemCount * (baseMultiplier - 1);
```

### Loop variables

Single-letter loop variables are acceptable only in trivial index loops.
Use descriptive names when the variable has semantic meaning:

```csharp
// acceptable — pure index iteration
for (int i = 0; i < thresholds.Length; i++) { ... }

// BAD — semantic variable with no-name
foreach (var x in orders) { Process(x); }

// GOOD
foreach (var order in orders) { Process(order); }
```

### Boolean variables

Boolean names should answer a yes/no question:

```csharp
// BAD
bool flag  = args.Length > 0;
bool check = result is null;

// GOOD
bool hasArguments   = args.Length > 0;
bool isResultEmpty  = result is null;
bool canProceed     = user.IsActive && user.HasPermission;
```

---

## Constants and Static Fields

Use `PascalCase` for named constants. Never leave numeric literals inline
when they carry domain meaning:

```csharp
// BAD — what do 4, 6, 1.2 and 0.6 mean?
double result  = (a + 4 * b + c) / 6;
value         *= other * 1.2;
adjusted      *= 1 + (penalty - 1) * 0.6;

// GOOD — each constant declares its purpose
private const double BetaDistributionCenterWeight = 4.0;
private const double BetaDistributionDivisor      = 6.0;
private const double PessimisticAmplifier         = 1.2;
private const double MostLikelyPenaltyRatio       = 0.6;

double result  = (optimistic + BetaDistributionCenterWeight * mostLikely + pessimistic) / BetaDistributionDivisor;
value         *= other * PessimisticAmplifier;
adjusted      *= 1 + (penalty - 1) * MostLikelyPenaltyRatio;
```

---

## Method Parameters

Parameters must be as descriptive as local variables.
Single-character parameters are only acceptable in short lambdas:

```csharp
// BAD
private void Save(User u, bool f) { ... }
private double Calculate(double a, double b, double c) { ... }

// GOOD
private void Save(User user, bool overwriteExisting) { ... }
private double Calculate(double optimistic, double mostLikely, double pessimistic) { ... }

// Acceptable in lambdas
var names = users.Select(u => u.Name);
var total = values.Aggregate(0.0, (acc, x) => acc + x);
```

---

## Methods

### Single responsibility and size

A method should do one thing. If it needs numbered comments internally,
each section should be its own method:

```csharp
// BAD — numbered comments indicate hidden method boundaries
public decimal CalculateOrderTotal(Order order)
{
    // 1. Apply discounts
    ...
    // 2. Calculate taxes
    ...
    // 3. Add shipping
    ...
}

// GOOD — each step is a named private method
public decimal CalculateOrderTotal(Order order)
{
    var discountedAmount = ApplyDiscounts(order);
    var taxAmount        = CalculateTaxes(discountedAmount, order.Region);
    var shippingCost     = CalculateShipping(order.Items, order.Destination);
    return discountedAmount + taxAmount + shippingCost;
}
```

### Method naming

Use verbs in infinitive form. The name should describe what the method does,
not how it does it:

```csharp
// BAD — vague or describes implementation
private double Calc(double value)
private int Fib(double hours)
private string DoFormat(PertResult r)

// GOOD — describes intent
private double RoundToOneDecimal(double value)
private int ConvertHoursToStoryPoints(double estimatedHours)
private string FormatResultSummary(EstimationResult result)
```

---

## Classes and Records

Prefer `record` for immutable data transfer objects. Use `class` when the
type has mutable state or meaningful behavior:

```csharp
// BAD — mutable class used purely to carry immutable data
public class DateRange
{
    public DateTime Start { get; init; }
    public DateTime End   { get; init; }
}

// GOOD — record expresses immutability and value equality natively
public record DateRange(DateTime Start, DateTime End);

// Also valid with JSON deserialization
public record EstimationResult(
    double OptimisticHours,
    double MostLikelyHours,
    double PessimisticHours,
    double PertHours,
    int    StoryPoints
);
```

---

## Strings as Discriminators

Strings used as keys or conditional flags are a code smell. Replace with enums:

```csharp
// BAD — fragile, case-sensitive, no compile-time safety
public string Status { get; set; } = "pending";

if (order.Status == "approved") { ... }

var multipliers = new Dictionary<string, double>
{
    ["low"]    = 1.1,
    ["medium"] = 1.3,
    ["high"]   = 1.6,
};

// GOOD — compile-time safe, IDE-supported, refactor-friendly
public enum RiskLevel { Low, Medium, High }
public enum OrderStatus { Pending, Approved, Rejected }

public RiskLevel Risk { get; set; } = RiskLevel.Low;

if (order.Status == OrderStatus.Approved) { ... }

var multipliers = new Dictionary<RiskLevel, double>
{
    [RiskLevel.Low]    = 1.1,
    [RiskLevel.Medium] = 1.3,
    [RiskLevel.High]   = 1.6,
};
```

For JSON deserialization, add the converter on the property:

```csharp
[JsonConverter(typeof(JsonStringEnumConverter))]
public RiskLevel Risk { get; set; } = RiskLevel.Low;
```

---

## Comments

### Remove: comments that repeat the code

```csharp
// BAD — the comment adds nothing
// increment counter
counter++;

// BAD — the method name already says this
// get base hours from technical complexity
var baseHours = GetBaseHours(complexity);

// BAD — numbered section comments are disguised method boundaries
// 1. Validate input
// 2. Calculate result
// 3. Format output
```

### Keep: comments that explain why

```csharp
// GOOD — explains a non-obvious design decision
// Weight of 4 derives from the Beta distribution, which models
// asymmetric bounded processes like task delivery duration.
double pertHours = (optimistic + 4 * mostLikely + pessimistic) / 6.0;

// GOOD — explains a counterintuitive value
// Unknown knowledge scores higher risk than Beginner:
// a team of unknown skill carries maximum epistemic uncertainty.
[KnowledgeLevel.Unknown] = new Multiplier(1.2, 1.5, 2.8),
```
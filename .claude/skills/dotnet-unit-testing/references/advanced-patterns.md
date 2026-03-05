# Advanced Test Patterns — xUnit + FluentAssertions

## Theory — Parameterized Tests

Use `[Theory]` when the same behavior must hold across multiple input values.
Never use a loop inside a `[Fact]` for this purpose.

### InlineData — for simple scalar values

```csharp
[Theory]
[InlineData("trivial",      1)]
[InlineData("simple",       2)]
[InlineData("moderate",     5)]
[InlineData("complex",      13)]
[InlineData("very_complex", 34)]
public void Estimate_EachComplexityLevel_MapsToExpectedStoryPoints(
    string complexity,
    int    expectedStoryPoints)
{
    // Arrange
    var input = BuildInput(technicalComplexity: complexity);

    // Act
    var result = service.Estimate(input);

    // Assert
    result.StoryPoints.Should().Be(expectedStoryPoints);
}
```

### MemberData — for complex objects or many parameters

Use when `[InlineData]` would be unreadable due to many parameters or complex types.

```csharp
public static IEnumerable<object[]> FullEstimationScenarios =>
[
    // complexity,      knowledge,       intCount, intComplexity, depCount, depReliability, expectedHours
    ["trivial",        "expert",         0,        "low",         0,        "medium",        1.0 ],
    ["complex",        "intermediate",   2,        "high",        1,        "medium",        58.4],
    ["very_complex",   "beginner",       0,        "low",         0,        "medium",        97.2],
];

[Theory]
[MemberData(nameof(FullEstimationScenarios))]
public void Estimate_VariousScenarios_ReturnsExpectedPertHours(
    string complexity, string knowledge,
    int intCount, string intComplexity,
    int depCount, string depReliability,
    double expectedHours)
{
    // Arrange
    var input = BuildInput(complexity, knowledge, intCount, intComplexity, depCount, depReliability);

    // Act
    var result = service.Estimate(input);

    // Assert
    result.PertHours.Should().Be(expectedHours);
}
```

### When to use InlineData vs MemberData

| Condition | Use |
|---|---|
| 1–3 simple scalar parameters | `[InlineData]` |
| 4+ parameters or complex objects | `[MemberData]` |
| Inputs require computation to build | `[MemberData]` with a method |
| Test data should be readable inline | `[InlineData]` |

---

## Mocking with NSubstitute

Use NSubstitute when the class under test depends on an interface that
performs I/O, external calls, or has behavior you need to control.

### Setup

```csharp
public class OrderServiceTests
{
    private readonly IOrderRepository _repository;
    private readonly IOrderService    _sut;

    public OrderServiceTests()
    {
        _repository = Substitute.For<IOrderRepository>();
        _sut        = new OrderService(_repository);
    }
}
```

### Configuring return values

```csharp
[Fact]
public void GetOrder_ExistingId_ReturnsOrder()
{
    // Arrange
    var expectedOrder = new Order { Id = 1, Status = OrderStatus.Pending };
    _repository.GetById(1).Returns(expectedOrder);

    // Act
    var result = _sut.GetOrder(1);

    // Assert
    result.Should().BeEquivalentTo(expectedOrder);
}
```

### Verifying calls

```csharp
[Fact]
public void CancelOrder_ValidOrder_CallsRepositoryUpdate()
{
    // Arrange
    var order = new Order { Id = 1, Status = OrderStatus.Pending };
    _repository.GetById(1).Returns(order);

    // Act
    _sut.CancelOrder(1);

    // Assert
    _repository.Received(1).Update(Arg.Is<Order>(o => o.Status == OrderStatus.Cancelled));
}
```

### Throwing exceptions from mocks

```csharp
[Fact]
public void GetOrder_RepositoryThrows_PropagatesException()
{
    // Arrange
    _repository.GetById(Arg.Any<int>()).Throws<DatabaseException>();

    // Act
    var act = () => _sut.GetOrder(99);

    // Assert
    act.Should().Throw<DatabaseException>();
}
```

---

## Exception Testing

Always use the `act` variable pattern with FluentAssertions — never use
`Assert.Throws`:

```csharp
// BAD
Assert.Throws<ArgumentNullException>(() => service.Estimate(null));

// GOOD
[Fact]
public void Estimate_NullInput_ThrowsArgumentNullException()
{
    // Arrange
    TaskInput? input = null;

    // Act
    var act = () => service.Estimate(input!);

    // Assert
    act.Should().Throw<ArgumentNullException>()
       .WithParameterName("input");
}
```

For async methods:

```csharp
[Fact]
public async Task ProcessAsync_InvalidOrder_ThrowsValidationException()
{
    // Arrange
    var invalidOrder = new Order { Id = 0 };

    // Act
    var act = async () => await service.ProcessAsync(invalidOrder);

    // Assert
    await act.Should().ThrowAsync<ValidationException>()
             .WithMessage("*Id*");
}
```

---

## IClassFixture — Shared Setup Across Tests

Use when setup is expensive and safe to share (e.g., building a read-only
configuration object, starting a test server). Do not use for mutable state.

```csharp
public class EstimationWeightsFixture
{
    public EstimationWeights Weights { get; } = new EstimationWeights();
}

public class EstimationServiceTests : IClassFixture<EstimationWeightsFixture>
{
    private readonly IEstimationService _sut;

    public EstimationServiceTests(EstimationWeightsFixture fixture)
    {
        _sut = new EstimationService(fixture.Weights);
    }
}
```

Rules:
- Fixture state must be **read-only after construction**
- Never share mutable state between tests — it creates test interdependence
- Prefer constructor setup over `IClassFixture` for cheap objects

---

## Test Categories with Traits

Use `[Trait]` to categorize tests and enable selective test runs:

```csharp
[Fact]
[Trait("Category", "UnitTest")]
public void Estimate_TrivialTask_ReturnsOneStoryPoint() { ... }

[Fact]
[Trait("Category", "Integration")]
public void Api_PostTask_ReturnsEstimationResult() { ... }
```

Run a specific category in CLI:

```bash
dotnet test --filter "Category=UnitTest"
dotnet test --filter "Category=Integration"
```

---

## BeEquivalentTo — Deep Object Comparison

Use for comparing complex output objects without asserting each property
individually:

```csharp
[Fact]
public void Estimate_TrivialWithExpert_ReturnsExpectedResult()
{
    // Arrange
    var input = BuildInput(technicalComplexity: "trivial", teamKnowledge: "expert");
    var expected = new PertResult
    {
        PertHours         = 1.0,
        StoryPoints       = 1,
        RiskLevel         = "Low",
        StandardDeviation = 0.3,
    };

    // Act
    var result = service.Estimate(input);

    // Assert
    result.Should().BeEquivalentTo(expected, options =>
        options.Including(r => r.PertHours)
               .Including(r => r.StoryPoints)
               .Including(r => r.RiskLevel)
               .Including(r => r.StandardDeviation));
}
```

Use `options.Including(...)` to scope the comparison to relevant properties —
avoid comparing every field when some (like timestamps) are volatile.

---

## Shared Builders Across Test Classes

When multiple test classes need the same builder, extract it to a dedicated
static class:

```
tests/
└── ProjectName.Tests/
    ├── TestBuilders/
    │   ├── TaskInputBuilder.cs
    │   └── OrderBuilder.cs
    ├── Services/
    │   └── EstimationServiceTests.cs
    └── Utils/
        └── MathExtensionsTests.cs
```

```csharp
// TestBuilders/TaskInputBuilder.cs
namespace ProjectName.Tests.TestBuilders;

public static class TaskInputBuilder
{
    public static TaskInput Build(
        string technicalComplexity   = "moderate",
        string teamKnowledge         = "intermediate",
        int    integrationCount      = 0,
        string integrationComplexity = "low",
        int    dependencyCount       = 0,
        string dependencyReliability = "medium")
        => new()
        {
            TechnicalComplexity  = technicalComplexity,
            TeamKnowledge        = teamKnowledge,
            ExternalIntegrations = new() { Count = integrationCount, Complexity = integrationComplexity },
            ExternalDependencies = new() { Count = dependencyCount, TeamReliability = dependencyReliability },
        };
}
```
---
name: dotnet-unit-testing
description: >
  Write, review, and standardize unit tests for .NET C# projects using xUnit,
  FluentAssertions, and best practices. Use this skill whenever the user wants
  to: write new unit tests, review existing tests for quality, fix poorly named
  test methods, apply the AAA pattern, create test builders or helpers, use
  Theory with InlineData or MemberData, organize test classes, set up a test
  project, or improve test coverage for a service or method. Also trigger when
  the user mentions "unit test", "xunit", "FluentAssertions", "AAA", "Arrange
  Act Assert", "test coverage", "test naming", "[Fact]", "[Theory]", or asks
  to test any specific class or method in a .NET project.
---

# .NET Unit Testing Standards — xUnit + FluentAssertions

## Quick Reference

| Topic | Reference |
|---|---|
| Naming, AAA, structure rules | `references/conventions.md` |
| Theory, builders, fixtures, advanced patterns | `references/advanced-patterns.md` |

Read the relevant reference before writing or reviewing any test.

---

## Process

### Writing new tests

1. Read the class under test — understand its public contract, edge cases, and expected failures
2. Map test scenarios into categories (happy path, edge cases, fallbacks, invariants)
3. Create one `[Fact]` per scenario — never merge two behaviors into one test
4. Follow the naming pattern: `MethodName_Scenario_ExpectedResult`
5. Apply AAA structure with one blank line between each section
6. Assert with FluentAssertions — never use `Assert.Equal` directly
7. Group related tests with comment separators

### Reviewing existing tests

Scan for these violations and fix them:

| Violation | What to look for |
|---|---|
| **Naming** | Vague names like `Test1`, `TestSuccess`, `ShouldWork` |
| **Multiple behaviors** | Single `[Fact]` asserting unrelated things |
| **Missing AAA** | No blank lines separating Arrange / Act / Assert |
| **Weak assertions** | `Assert.True(x != null)` instead of `x.Should().NotBeNull()` |
| **Magic values** | Unexplained literals in Arrange with no comment or constant |
| **Logic in tests** | `if`, `foreach`, or `switch` inside a `[Fact]` body |
| **Test interdependence** | Tests that rely on execution order or shared mutable state |

---

## Naming Patterns

Two patterns are accepted. Choose based on how obvious the class name is in context.
Always start the `ExpectedResult` segment with **`Should`**.

**Pattern 1** — use when the class name adds meaningful context:
```
SubjectUnderTest_MethodOrBehavior_ShouldExpectedResult

Order_AddItem_ShouldIncrementUnitsIfItemAlreadyExists
Inventory_RemoveItem_ShouldSendEmailWhenBelowTenUnits
PertEngine_Estimate_ShouldReturnLowRiskForTrivialTask
```

**Pattern 2** — use when the class is obvious from the test file name:
```
MethodOrBehavior_StateOrCondition_ShouldExpectedResult

AddItem_ItemAlreadyInCart_ShouldIncrementItemUnits
Estimate_TrivialComplexityWithExpertTeam_ShouldReturnLowRiskAndOneStoryPoint
Estimate_UnknownComplexityValue_ShouldFallBackToModerate
```

Be consistent — never mix both patterns within the same test class.
For full rules, decision guide, and anti-patterns → `references/conventions.md`

---

## AAA Pattern

Every `[Fact]` must have exactly three sections separated by blank lines.
Never omit the blank lines — they are part of the standard.

```csharp
[Fact]
public void Estimate_ModerateComplexityWithIntermediateTeam_ShouldReturnBaselineEstimate()
{
    // Arrange
    var input = BuildInput(technicalComplexity: "moderate", teamKnowledge: "intermediate");

    // Act
    var result = service.Estimate(input);

    // Assert
    result.PertHours.Should().Be(9.0);
    result.StoryPoints.Should().Be(5);
    result.RiskLevel.Should().Be("Low");
}
```

Rules:
- **Arrange** — build inputs and configure dependencies only. No assertions here
- **Act** — one single method call. Never call the system under test twice
- **Assert** — assert only the behavior described in the test name. Avoid asserting unrelated properties

---

## FluentAssertions — Required Patterns

Never use raw xUnit assertions. Always use FluentAssertions:

```csharp
// BAD
Assert.Equal(9.0, result.PertHours);
Assert.True(result.PertHours > 0);
Assert.NotNull(result);
Assert.Contains(result.StoryPoints, validFibonacci);

// GOOD
result.PertHours.Should().Be(9.0);
result.PertHours.Should().BePositive();
result.Should().NotBeNull();
validFibonacci.Should().Contain(result.StoryPoints);
```

Use `because:` parameter to document non-obvious assertions:

```csharp
result.Optimistic.Should().BeLessThan(result.Pessimistic,
    because: "optimistic scenario always resolves faster than pessimistic");

result.PertHours.Should().BePositive(
    because: $"complexity={input.TechnicalComplexity} should always yield work");
```

---

## Test Class Structure

```csharp
// File: tests/ProjectName.Tests/Services/ServiceNameTests.cs
namespace ProjectName.Tests.Services;

public class ServiceNameTests
{
    // ── Setup ─────────────────────────────────────────────────────────────────

    private readonly IServiceName _sut;  // sut = System Under Test

    public ServiceNameTests()
    {
        _sut = new ServiceName();
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private static InputType BuildInput(...) => new() { ... };

    // ── [Category / Method Group] ─────────────────────────────────────────────

    [Fact]
    public void MethodName_Scenario_ExpectedResult() { ... }
}
```

Rules:
- One test class per production class
- File path mirrors the production code path: `Services/EstimationService.cs` → `Services/EstimationServiceTests.cs`
- Name the instance under test `_sut` for clarity
- Group tests by method or behavior using comment separators
- Keep helpers private and static when they have no side effects

---

## Project Setup

### Folder structure

```
tests/
└── ProjectName.Tests/
    ├── ProjectName.Tests.csproj
    ├── Services/
    │   └── EstimationServiceTests.cs
    └── Utils/
        └── MathExtensionsTests.cs
```

### .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
    <PackageReference Include="FluentAssertions" Version="6.*" />
    <PackageReference Include="NSubstitute" Version="5.*" />  <!-- for mocking -->
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ProjectName\ProjectName.csproj" />
  </ItemGroup>
</Project>
```

---

## Final Checklist

Before considering a test file done:

- [ ] Every test method follows `MethodName_Scenario_ExpectedResult`
- [ ] Every `[Fact]` tests exactly one behavior
- [ ] Every test has Arrange / Act / Assert separated by blank lines
- [ ] No raw `Assert.*` — only FluentAssertions
- [ ] No logic (`if`, `foreach`, `switch`) inside `[Fact]` bodies
- [ ] Parameterized cases use `[Theory]` with `[InlineData]` or `[MemberData]`
- [ ] No test depends on another test's execution or shared mutable state
- [ ] Builder helpers exist for complex input construction
- [ ] One test class per production class, mirroring the folder structure

For Theory, builders, fixtures, and mocking patterns → `references/advanced-patterns.md`
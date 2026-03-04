# Unit Test Conventions — Naming, AAA, and Structure

## Naming Patterns

The name of a test is its specification. When it fails in CI, the name alone
must tell the developer what broke and why — without opening the file.

Two patterns are accepted, chosen based on how obvious the class name is in
context.

---

### Pattern 1 — SubjectUnderTest_MethodOrBehavior_ExpectedResult

Use when the **class name adds meaningful context** — when the same method
name exists in multiple classes, or when the subject is not obvious from the
test file alone.

```
Order_AddItem_ShouldIncrementUnitsIfItemAlreadyExists
│      │        │
│      │        └─ ExpectedResult: observable outcome, starts with Should
│      └─ MethodOrBehavior: the method or action being exercised
└─ SubjectUnderTest: the class or domain object
```

Examples:
```csharp
public void Order_AddItem_ShouldIncrementUnitsIfItemAlreadyExists() { }
public void Inventory_RemoveItem_ShouldSendEmailWhenBelowTenUnits() { }
public void PertEngine_Estimate_ShouldReturnLowRiskForTrivialTask() { }
public void PertEngine_Estimate_ShouldFallBackToModerateForUnknownComplexity() { }
```

---

### Pattern 2 — MethodOrBehavior_StateOrCondition_ExpectedResult

Use when the **class name is obvious** from the test file name and adding it
would be redundant. This is the shorter, more common form for focused test
classes.

```
AddItem_ItemAlreadyInCart_ShouldIncrementItemUnits
│        │                  │
│        │                  └─ ExpectedResult: observable outcome, starts with Should
│        └─ StateOrCondition: the input state or precondition
└─ MethodOrBehavior: the method or action being exercised
```

Examples:
```csharp
public void AddItem_ItemAlreadyInCart_ShouldIncrementItemUnits() { }
public void RemoveFromInventory_StockBelowTenUnits_ShouldSendWarningEmail() { }
public void Estimate_TrivialComplexityWithExpertTeam_ShouldReturnLowRiskAndOneStoryPoint() { }
public void Estimate_UnknownComplexityValue_ShouldFallBackToModerate() { }
public void Estimate_WithExternalDependencies_ShouldNotAffectOptimisticHours() { }
```

---

### Choosing between Pattern 1 and Pattern 2

| Situation | Use |
|---|---|
| Method name exists in multiple classes | Pattern 1 |
| Test file is named after the class (`OrderTests.cs`) | Pattern 2 |
| Disambiguation helps when reading CI output | Pattern 1 |
| Class name is already obvious from file context | Pattern 2 |

Both patterns are valid. **Be consistent within a test class** — never mix
the two patterns in the same file.

---

### ExpectedResult naming guidelines

Always start `ExpectedResult` with **`Should`**. Describe the **observable
behavior**, not the implementation detail:

```csharp
// BAD — describes implementation, not behavior
public void Estimate_BeginnerTeam_MultipliesPBy2Point5() { }
public void Estimate_BeginnerTeam_SetsRiskLevelToAlto() { }

// GOOD — describes observable contract
public void Estimate_BeginnerTeam_ShouldProduceHigherPessimisticThanIntermediateTeam() { }
public void PertEngine_Estimate_ShouldReturnHighRiskForBeginnerTeamOnComplexTask() { }
```

### Prefix conventions for ExpectedResult

| Prefix | Use when |
|---|---|
| `ShouldReturn` | Method returns a value matching a specific condition |
| `ShouldThrow` | Method is expected to throw an exception |
| `ShouldNotAffect` / `ShouldNotChange` | A factor should have no effect on a property |
| `ShouldFallBackTo` | Invalid input triggers a default/fallback behavior |
| `ShouldProduceHigher` / `ShouldProduceLower` | Relative comparison between two outcomes |
| `ShouldSend` / `ShouldCall` / `ShouldTrigger` | Verifying a side effect or interaction |
| `ShouldAlways` | Invariant that holds across all valid inputs |

---

### Anti-patterns to avoid

```csharp
// BAD — no information about what is being tested or expected
public void Test1() { }
public void TestSuccess() { }
public void ShouldWork() { }
public void Estimate_HappyPath() { }
public void Estimate_WhenCalled_ReturnsResult() { }

// BAD — describes what the code does, not what it should do
public void Estimate_BeginnerTeam_PessimisticIs44() { }

// GOOD
public void Estimate_BeginnerTeam_ShouldProduceHigherPessimisticHoursThanIntermediateTeam() { }
public void PertEngine_Estimate_ShouldReturnBaselineEstimateForModerateTaskWithIntermediateTeam() { }
```

---

## AAA Pattern — Detailed Rules

### Structure

```csharp
[Fact]
public void MethodOrBehavior_StateOrCondition_ShouldExpectedResult()
{
    // Arrange
    /* build inputs, configure mocks, set up preconditions */

    // Act
    /* one single call to the system under test */

    // Assert
    /* verify the observable outcome */
}
```

The `// Arrange`, `// Act`, `// Assert` comments are **mandatory** in every
`[Fact]`. The blank lines between sections are also mandatory.

### Arrange — rules

Only build inputs and configure dependencies. Never call the system under test here.

```csharp
// BAD — Act inside Arrange
var result = service.Estimate(BuildInput());  // this is Act, not Arrange
var expected = result.PertHours * 2;

// GOOD
var input    = BuildInput(technicalComplexity: "complex");
var expected = 58.4;
```

### Act — rules

One call only. If you need to call the system under test twice, you have two tests.

```csharp
// BAD — two unrelated Act calls
var result1 = service.Estimate(inputA);
var result2 = service.Estimate(inputB);
result1.PertHours.Should().BeLessThan(result2.PertHours);

// GOOD — two calls are acceptable only for relative comparison
[Fact]
public void Estimate_ExpertTeam_ShouldProduceLowerEstimateThanIntermediateTeam()
{
    // Arrange
    var expertInput       = BuildInput(teamKnowledge: "expert");
    var intermediateInput = BuildInput(teamKnowledge: "intermediate");

    // Act
    var expertResult       = service.Estimate(expertInput);
    var intermediateResult = service.Estimate(intermediateInput);

    // Assert
    expertResult.PertHours.Should().BeLessThan(intermediateResult.PertHours);
}
```

The two-call case is acceptable **only** when both calls exist to compare one
result against the other — not to assert independent behaviors.

### Assert — rules

Assert only what the test name promises. Extra assertions hide the real intent
and make failures harder to diagnose.

```csharp
// BAD — name says "ShouldReturnLowRisk" but asserts hours and story points too
public void Estimate_TrivialTask_ShouldReturnLowRisk()
{
    var result = service.Estimate(BuildInput("trivial", "expert"));

    result.RiskLevel.Should().Be("Low");
    result.PertHours.Should().Be(1.0);          // not in the name
    result.StoryPoints.Should().Be(1);          // not in the name
    result.StandardDeviation.Should().Be(0.3); // not in the name
}

// GOOD — focused test
public void Estimate_TrivialComplexityWithExpertTeam_ShouldReturnLowRisk()
{
    var result = service.Estimate(BuildInput("trivial", "expert"));

    result.RiskLevel.Should().Be("Low");
}
```

Exception: when all properties are part of the same observable contract and
the test name covers them all:

```csharp
// Acceptable — name explicitly covers all three values
public void Estimate_TrivialComplexityWithExpertTeam_ShouldReturnExpectedOptimisticMostLikelyAndPessimistic()
{
    var result = service.Estimate(BuildInput("trivial", "expert"));

    result.Optimistic.Should().Be(0.4);
    result.MostLikely.Should().Be(0.9);
    result.Pessimistic.Should().Be(2.0);
}
```

---

## No Logic in Tests

Tests must be deterministic and linear. Any control flow hides scenarios
and makes failures ambiguous.

```csharp
// BAD — loop hides which input failed
[Fact]
public void Estimate_AllComplexityLevels_ShouldReturnPositiveHours()
{
    var levels = new[] { "trivial", "simple", "moderate", "complex", "very_complex" };
    foreach (var level in levels)
    {
        var result = service.Estimate(BuildInput(level));
        result.PertHours.Should().BePositive();  // which level failed?
    }
}

// GOOD — use Theory to keep cases visible and individually reportable
[Theory]
[InlineData("trivial")]
[InlineData("simple")]
[InlineData("moderate")]
[InlineData("complex")]
[InlineData("very_complex")]
public void Estimate_AnyComplexityLevel_ShouldReturnPositiveHours(string complexity)
{
    // Arrange
    var input = BuildInput(technicalComplexity: complexity);

    // Act
    var result = service.Estimate(input);

    // Assert
    result.PertHours.Should().BePositive();
}
```

---

## Builder Pattern for Inputs

When the input object has many fields, a private builder method prevents
repetitive setup and makes the relevant parameter stand out.

```csharp
// BAD — every test sets up all fields, burying what actually matters
[Fact]
public void Estimate_BeginnerTeam_ShouldProduceHighPessimistic()
{
    var input = new TaskInput
    {
        TechnicalComplexity  = "moderate",
        TeamKnowledge        = "beginner",  // ← this is what matters
        ExternalIntegrations = new() { Count = 0, Complexity = "low" },
        ExternalDependencies = new() { Count = 0, TeamReliability = "medium" },
    };
}

// GOOD — builder with defaults, test only overrides what it cares about
private static TaskInput BuildInput(
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

[Fact]
public void Estimate_BeginnerTeam_ShouldProduceHighPessimistic()
{
    // Arrange
    var input = BuildInput(teamKnowledge: "beginner");  // ← intent is clear

    // Act
    var result = service.Estimate(input);

    // Assert
    result.Pessimistic.Should().BeGreaterThan(50.0);
}
```

Builder rules:
- Always provide sensible defaults for every parameter
- Use named parameters at call sites — never rely on positional order
- Keep builders `private static` inside the test class unless shared across multiple test classes
- If shared, move to a `TestBuilders/` folder as a `static class`
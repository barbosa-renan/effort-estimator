# .NET Testing Conventions — xUnit + FluentAssertions

Always-active conventions for any test code in this project.
For step-by-step test writing/reviewing procedures, use the `.claude/dotnet-unit-testing/SKILL.md` skill.

---

## Naming

Two accepted patterns. Choose one and **be consistent within the same test class** — never mix.

**Pattern 1** — when the class name adds meaningful context:
```
SubjectUnderTest_MethodOrBehavior_ShouldExpectedResult
PertEngine_Estimate_ShouldReturnLowRiskForTrivialTask
```

**Pattern 2** — when the class is obvious from the test file name:
```
MethodOrBehavior_StateOrCondition_ShouldExpectedResult
Estimate_TrivialComplexityWithExpertTeam_ShouldReturnLowRisk
```

`ExpectedResult` must always start with `Should` and describe observable behavior, not implementation details.

---

## AAA Pattern

Every `[Fact]` must have three sections with mandatory comments and blank lines between them:

```csharp
[Fact]
public void MethodOrBehavior_StateOrCondition_ShouldExpectedResult()
{
    // Arrange
    /* inputs only — never call the SUT here */

    // Act
    /* one single call to the system under test */

    // Assert
    /* only what the test name promises */
}
```

- **Arrange**: build inputs only. Never call the SUT here.
- **Act**: one single call. If you need two calls, you have two tests.
- **Assert**: assert only the behavior described in the test name.

---

## Assertions

Never use raw xUnit assertions. Always use FluentAssertions:

```csharp
// BAD
Assert.Equal(9.0, result.PertHours);
Assert.NotNull(result);

// GOOD
result.PertHours.Should().Be(9.0);
result.Should().NotBeNull();
```

Use `because:` to document non-obvious assertions.

---

## Rules

- One `[Fact]` per behavior — never merge unrelated assertions into one test
- No logic (`if`, `foreach`, `switch`) inside `[Fact]` bodies — use `[Theory]` instead
- `[InlineData]` for 1–3 simple scalar parameters; `[MemberData]` for 4+ or complex objects
- Tests must not depend on execution order or shared mutable state
- Name the system under test `_sut`
- One test class per production class, mirroring the folder structure
- Use a private `BuildInput(...)` helper with sensible defaults for complex input construction

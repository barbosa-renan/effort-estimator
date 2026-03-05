---
name: dotnet-unit-testing
description: "Write, review, and standardize unit tests for .NET C# projects using xUnit and FluentAssertions. Trigger when: writing new tests, reviewing existing tests, fixing test naming, applying AAA pattern, creating builders or helpers, using Theory/InlineData/MemberData, organizing test classes, setting up a test project, or when the user mentions unit test, xunit, FluentAssertions, AAA, test coverage, [Fact], [Theory], or asks to test a specific class or method."
---

# .NET Unit Testing — Procedures

Naming conventions, AAA pattern, and assertion rules are defined in `.claude/rules/dotnet-testing.md`.
Advanced patterns (Theory, NSubstitute, IClassFixture, builders) are in `references/advanced-patterns.md`.

---

## Process

### Writing new tests

1. Read the class under test — understand its public contract, edge cases, and expected failures
2. Map test scenarios into categories (happy path, edge cases, fallbacks, invariants)
3. Create one `[Fact]` per scenario — never merge two behaviors into one test
4. Follow the naming pattern from `.claude/rules/dotnet-testing.md`
5. Apply AAA structure with one blank line between each section
6. Assert with FluentAssertions — never use `Assert.*` directly
7. Group related tests with comment separators

### Reviewing existing tests

Scan for these violations and fix them:

| Violation | What to look for |
|---|---|
| **Naming** | Vague names like `Test1`, `TestSuccess`, `ShouldWork` |
| **Multiple behaviors** | Single `[Fact]` asserting unrelated things |
| **Missing AAA** | No blank lines or missing `// Arrange / Act / Assert` comments |
| **Weak assertions** | `Assert.True(x != null)` instead of `x.Should().NotBeNull()` |
| **Magic values** | Unexplained literals in Arrange with no comment or constant |
| **Logic in tests** | `if`, `foreach`, or `switch` inside a `[Fact]` body |
| **Test interdependence** | Tests that rely on execution order or shared mutable state |

---

## Test Class Structure

```csharp
// File: tests/ProjectName.Tests/Services/ServiceNameTests.cs
namespace ProjectName.Tests.Services;

public class ServiceNameTests
{
    // ── Setup ─────────────────────────────────────────────────────────────────

    private readonly IServiceName _sut;

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
- File path mirrors the production code path
- Name the instance under test `_sut`
- Group tests by method or behavior using comment separators
- Keep helpers `private static` when they have no side effects

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
    <PackageReference Include="NSubstitute" Version="5.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\src\ProjectName\ProjectName.csproj" />
  </ItemGroup>
</Project>
```

---

## Final Checklist

- [ ] Every test method follows `MethodName_Scenario_ExpectedResult`
- [ ] Every `[Fact]` tests exactly one behavior
- [ ] Every test has `// Arrange / Act / Assert` comments separated by blank lines
- [ ] No raw `Assert.*` — only FluentAssertions
- [ ] No logic (`if`, `foreach`, `switch`) inside `[Fact]` bodies
- [ ] Parameterized cases use `[Theory]` with `[InlineData]` or `[MemberData]`
- [ ] No test depends on another test's execution or shared mutable state
- [ ] Builder helpers exist for complex input construction
- [ ] One test class per production class, mirroring the folder structure

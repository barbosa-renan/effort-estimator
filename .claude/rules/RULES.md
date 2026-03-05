# Project Rules

Non-negotiable rules for the **EffortEstimator** codebase.
These apply to every action, regardless of task size or scope.

---

## Code

- Never use strings as discriminators — always use enums
- Never leave numeric literals inline — always extract to a named constant
- Prefer `record` over `class` for immutable data models
- Never use single-letter or abbreviated variable names outside of short lambdas
- Never use `static class` with state — use injectable instances
- Use explicit types when the right-hand side does not make the type obvious

---

## Tests

- Every new public method requires at least one unit test
- Test names must follow `MethodOrBehavior_StateOrCondition_ShouldExpectedResult`
  or `SubjectUnderTest_MethodOrBehavior_ShouldExpectedResult`
- Never use `Assert.*` directly — always use FluentAssertions
- Every `[Fact]` must have Arrange / Act / Assert sections separated by blank lines
- No logic (`if`, `foreach`, `switch`) inside a `[Fact]` body — use `[Theory]` instead

---

## Process

- Never modify more than one layer per task (Models, Services, Utils, or Program.cs)
- Always run `dotnet test` before marking any task complete
- If a change touches more than 3 files unexpectedly, stop and re-plan

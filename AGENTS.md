# AGENTS.md

Agent behavior guidelines for the **EffortEstimator** project.
This file is the single source of truth for how any agent should operate in this codebase.

---

## Core Principles

These principles apply to every action, regardless of task size.

**Simplicity first.** Every change should be as small and focused as possible.
Touch only what is necessary. The best solution is often the simplest one that
fully solves the problem.

**No workarounds.** Find the root cause. Temporary fixes and shortcuts are not
acceptable. Hold yourself to senior engineer standards before presenting work.

**Minimal blast radius.** Changes should not introduce side effects outside
their scope. If a fix requires touching unrelated code, stop and re-evaluate
the approach.

---

## Rules

These rules apply to every action in this codebase, without exception.
They are non-negotiable and require no justification to enforce.

### Code

- Never use strings as discriminators — always use enums
- Never leave numeric literals inline — always extract to a named constant
- Prefer `record` over `class` for immutable data models
- Never use single-letter or abbreviated variable names outside of short lambdas
- Never use `static class` with state — use injectable instances
- Use explicit types when the right-hand side does not make the type obvious

### Tests

- Every new public method requires at least one unit test
- Test names must follow `MethodOrBehavior_StateOrCondition_ShouldExpectedResult`
  or `SubjectUnderTest_MethodOrBehavior_ShouldExpectedResult`
- Never use `Assert.*` directly — always use FluentAssertions
- Every `[Fact]` must have Arrange / Act / Assert sections separated by blank lines
- No logic (`if`, `foreach`, `switch`) inside a `[Fact]` body — use `[Theory]` instead

### Process

- Never modify more than one layer per task (Models, Services, Utils, or Program.cs)
- Always run `dotnet test` before marking any task complete
- If a change touches more than 3 files unexpectedly, stop and re-plan

---

## Planning

Enter plan mode for any task that involves 3 or more steps, touches multiple
files, or requires an architectural decision.

Before writing code, write a short spec:
- What is the problem or goal?
- What files will be affected?
- What is the intended approach?
- Are there simpler alternatives?

If the task goes sideways mid-execution — unexpected failures, cascading
changes, or growing complexity — stop immediately and re-plan. Do not push
through ambiguity. A bad plan executed quickly causes more damage than a
short pause to reconsider.

Use plan mode for verification steps too, not just for building. Planning
how you will prove the work is done is as important as planning how to do it.

---

## Execution

### Subagents

Use subagents to keep the main context window focused. Delegate:
- Research and exploration of unfamiliar APIs or patterns
- Parallel analysis of multiple files or options
- Isolated tasks that have a clear input and expected output

One task per subagent. Focused execution produces better results than
broad, multi-purpose subagents.

### Elegance check

For any non-trivial change, pause before submitting and ask:
*"Is there a more elegant way to do this?"*

If a solution feels hacky or brittle, it probably is. In that case:
*"Knowing everything I know now, what is the clean solution?"*

Then implement that. Skip this check for simple, obvious fixes — do not
over-engineer a one-line change.

---

## Verification

Never consider a task complete without proving it works.

Before marking anything done:
1. Run the full test suite: `dotnet test`
2. Confirm no existing tests regressed
3. Confirm the specific behavior you changed is covered by a test
4. Ask yourself: *"Would a senior engineer approve this as-is?"*

If the answer to step 4 is no, keep working.

---

## Bug Fixing

When given a bug report, fix it autonomously:
1. Reproduce the bug with a failing test
2. Identify the root cause — not the symptom
3. Implement the fix
4. Run the full test suite to confirm the fix and detect regressions
5. Present the fix with a clear explanation of the root cause

Do not ask for step-by-step guidance. Do not ask which file to look at.
Investigate, fix, verify, report.

---

## Commands

```bash
# Build
dotnet build

# Run with embedded example
dotnet run --project src/EffortEstimator

# Run tests
dotnet test

# Run tests with output
dotnet test --logger "console;verbosity=detailed"
```
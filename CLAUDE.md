# CLAUDE.md

## Agent Behavior

All agent behavior guidelines — planning, verification, bug fixing, elegance
standards, and core principles — are defined in `AGENTS.md`.

Read `AGENTS.md` before taking any action in this codebase.

---

## Project

**EffortEstimator** — PERT-based software effort estimation console application built with .NET 10.

Accepts a task description as JSON input and returns estimated hours, Story Points (Fibonacci),
standard deviation, confidence interval, and risk level based on the PERT formula.

---

## Skills

Before creating, refactoring, or reviewing any C# file, read the relevant skill:

| Skill | When to use |
|---|---|
| `.claude/skills/dotnet-standards/SKILL.md` | Any C# code — naming, SOLID, project structure |
| `.claude/skills/dotnet-unit-testing/SKILL.md` | Writing or reviewing unit tests |

---

## Project Structure

```
EffortEstimator/
├── .claude/
│   ├── dotnet-standards/
│   │   ├── references/
│   │   │   ├── naming-conventions.md
│   │   │   ├── project-structure.md
│   │   │   └── solid-principles.md
│   │   └── SKILL.md
│   └── dotnet-unit-testing/
│   │   ├── references/
│   │   │   ├── advanced-patterns.md
│   │   │   └── convetions.md
│   │   └── SKILL.md
├── **.gitignore**
├── AGENTS.md
├── CLAUDE.md
├── docs/
│   ├── ALGORITHM.md
│   └── DECISIONS.md
├── EffortEstimator.sln
├── project_structure.md
├── **README.md**
├── src/
│   ├── EffortEstimator.cs
│   ├── EffortEstimator.csproj
│   ├── Models/
│   │   └── Enums/
│   ├── Program.cs
│   ├── Services/
│   ├── skills/
│   └── src.sln
└── tests/
│   └── EffortEstimator.Tests/
│   │   ├── EffortEstimator.Tests.csproj
│   │   ├── PertEngineTests.cs
```

## Key Decisions

The algorithm design decisions, multiplier rationale, and literature references
are documented in `docs/DECISIONS.md` and `docs/ALGORITHM.md`.

## Running the Project

```bash
# Run with embedded example
dotnet run --project src/EffortEstimator

# Run with custom JSON input
dotnet run --project src/EffortEstimator -- --stdin < task.json
```
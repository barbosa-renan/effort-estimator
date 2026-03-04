# CLAUDE.md

## Project

**EffortEstimator** — PERT-based software effort estimation console application built with .NET 10.

Accepts a task description as JSON input and returns estimated hours, Story Points (Fibonacci),
standard deviation, confidence interval, and risk level based on the PERT formula.

## Standards

Before refactoring, creating, or reviewing any C# file in this project, read and follow:

- `.claude/skills/dotnet-standards/SKILL.md`

This skill covers:
- Naming conventions (variables, parameters, constants, methods, enums)
- Clean Code principles (DRY, KISS, single responsibility)
- SOLID principles with C# examples
- Project folder structure and dependency rules

Detailed reference files are in `.claude/skills/dotnet-standards/references/`:
- `naming-conventions.md` — before/after naming examples
- `solid-principles.md` — SOLID with practical C# code
- `project-structure.md` — folder structure, naming patterns, .csproj config

## Project Structure

```
EffortEstimator/
├── EffortEstimator.sln
└── src/
    └── EffortEstimator/
        ├── EffortEstimator.csproj
        ├── Program.cs              ← composition and I/O only
        ├── Models/                 ← records, enums, value objects
        │   └── Enums/
        ├── Services/               ← business logic and use cases
        └── Utils/                  ← stateless helpers and extensions
```

### Dependency rules

```
Program.cs  ->  Services  ->  Models
                   |
                 Utils
```

- **Models** has no dependencies on Services or Utils
- **Utils** has no dependencies on Models or Services
- **Services** depends on Models and may use Utils
- **Program.cs** is the only place that does I/O and composes dependencies

## Key Decisions

The algorithm design decisions, multiplier rationale, and literature references
are documented in `DECISIONS.md`.

## Running the Project

```bash
# Run with embedded example
dotnet run --project src/EffortEstimator

# Run with custom JSON input
dotnet run --project src/EffortEstimator -- --stdin < task.json
```
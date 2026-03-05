# CLAUDE.md

## Agent Behavior

All agent behavior guidelines — planning, verification, bug fixing, elegance
standards, and core principles — are defined in `AGENTS.md`.

Project rules (code style, test conventions, process constraints) are in `.claude/rules/`.
Read all three rule files before taking any action in this codebase:

| Rule file | Scope |
|---|---|
| `.claude/rules/RULES.md` | Non-negotiable code, test, and process rules |
| `.claude/rules/dotnet-standards.md` | Naming, type design, project structure conventions |
| `.claude/rules/dotnet-testing.md` | AAA pattern, FluentAssertions, test naming conventions |

---

## Project

**EffortEstimator** — PERT-based software effort estimation console application built with .NET 10.

Accepts a task description as JSON input and returns estimated hours, Story Points (Fibonacci),
standard deviation, confidence interval, and risk level based on the PERT formula.

---

## Skills

Invoke the relevant skill for active refactoring or test writing/reviewing procedures:

| Skill | When to invoke |
|---|---|
| `.claude/skills/dotnet-standards/SKILL.md` | Refactoring C# code — scan, diagnose, and fix violations |
| `.claude/skills/dotnet-unit-testing/SKILL.md` | Writing new tests or reviewing existing test files |
| `.claude/skills/azure-devops-mcp/SKILL.md` | Reading work items and implementing tasks from Azure DevOps |

---

## Project Structure

```
EffortEstimator/
├── .claude/
│   ├── skills/
│   │   ├── dotnet-standards/
│   │   │   ├── references/
│   │   │   │   ├── naming-conventions.md
│   │   │   │   ├── project-structure.md
│   │   │   │   └── solid-principles.md
│   │   │   └── SKILL.md
│   │   ├── dotnet-unit-testing/
│   │   │   ├── references/
│   │   │   │   ├── advanced-patterns.md
│   │   │   │   └── convetions.md
│   │   │   └── SKILL.md
│   │   └── azure-devops-mcp/
│   │       ├── references/
│   │       │   ├── setup.md
│   │       │   └── mcp-tools.md
│   │       └── SKILL.md
│   └── rules/
│   │   ├── RULES.md
│   │   ├── dotnet-standards.md
│   │   └── dotnet-testing.md
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
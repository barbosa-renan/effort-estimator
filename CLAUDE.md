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

**EffortEstimator** — PERT-based software effort estimation API built with .NET 10, structured as a monorepo.

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
│   ├── rules/
│   │   ├── RULES.md
│   │   ├── dotnet-standards.md
│   │   └── dotnet-testing.md
│   └── skills/
│       ├── dotnet-standards/SKILL.md
│       ├── dotnet-unit-testing/SKILL.md
│       └── azure-devops-mcp/SKILL.md
├── .gitignore
├── AGENTS.md
├── CLAUDE.md
├── README.md
├── docs/
│   ├── ALGORITHM.md
│   └── DECISIONS.md
└── apps/
    ├── api/                          # Backend — .NET 10 Hexagonal
    │   ├── EffortEstimator.sln
    │   ├── src/
    │   │   ├── EffortEstimator.API/
    │   │   ├── EffortEstimator.Core/
    │   │   └── EffortEstimator.Infrastructure/
    │   └── tests/
    │       └── EffortEstimator.Core.Tests/
    └── web/                          # Frontend Angular (placeholder)
        └── README.md
```

## Key Decisions

The algorithm design decisions, multiplier rationale, and literature references
are documented in `docs/DECISIONS.md` and `docs/ALGORITHM.md`.

## Running the Project

```bash
# Build
dotnet build apps/api/EffortEstimator.sln

# Run the API
dotnet run --project apps/api/src/EffortEstimator.API

# Run tests
dotnet test apps/api/EffortEstimator.sln
```
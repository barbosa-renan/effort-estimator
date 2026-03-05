# .NET Standards — Conventions

Always-active conventions for any C# code in this project.
For step-by-step refactoring/review procedures, use the `.claude/dotnet-standards/SKILL.md` skill.

---

## Naming

| Context | Pattern |
|---|---|
| Local variable | `camelCase`, intent-revealing, never abbreviated |
| Method parameter | `camelCase`, never abbreviated |
| Private field | `_camelCase` |
| Constant / `static readonly` | `PascalCase` |
| Property | `PascalCase` |
| Method | `PascalCase`, verb in infinitive form |
| Class / Record / Enum | `PascalCase`, noun |
| Interface | `IPascalCase` |

- Single-letter names acceptable only in trivial index loops or short lambdas
- Boolean names must answer a yes/no question (`hasArguments`, not `flag`)
- Method names describe intent, not implementation (`ConvertHoursToStoryPoints`, not `Calc`)

---

## Type Design

- Prefer `record` over `class` for immutable data models
- Never use strings as discriminators — always use enums
- Never leave numeric literals inline — always extract to a named constant
- Never use `static class` with state — use injectable instance classes
- Use explicit types when the right-hand side does not make the type obvious
- One file per type; filename must exactly match the type name

---

## Project Structure

```
Models/    — data shapes (records, enums). No business logic, no I/O.
Services/  — business logic and orchestration. No console I/O.
Utils/     — stateless helpers and extension methods. No constructor dependencies.
Program.cs — composition root and I/O only.
```

Dependency flow:

```
Program.cs  →  Services  →  Models
                  |
                Utils
```

- **Models**: no dependencies on Services or Utils
- **Utils**: no dependencies on Models or Services
- **Services**: depends on Models, may use Utils
- **Program.cs**: the only place that does I/O

---

## Comments

- Remove comments that repeat what the code already says
- Keep comments that explain *why* — design decisions, non-obvious domain choices
- Numbered section comments (`// 1. Validate`, `// 2. Calculate`) signal hidden method boundaries — extract them

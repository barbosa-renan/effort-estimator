---
name: dotnet-standards
description: "Apply Clean Code, SOLID, DRY, and KISS to any .NET C# project. Trigger when: refactoring C# code, fixing naming, extracting constants or enums, splitting oversized methods, reorganizing project structure, applying SRP/DI, converting static classes to injectable services, or when the user mentions clean code, SOLID, refactor, naming, project structure, code smell."
---

# .NET Standards — Refactoring Procedure

Conventions and naming rules are defined in `.claude/rules/dotnet-standards.md`.
Detailed before/after examples are in the `references/` folder.

Apply both phases in sequence. Present the diagnosis of each phase before applying corrections.

---

## Phase 1 — Code Quality

### Diagnosis

Scan the code and classify each violation:

| Category | What to look for |
|---|---|
| **Names** | Single-letter or abbreviated variables, abbreviated parameters |
| **Magic numbers** | Inline numeric literals without a named constant |
| **Strings as enums** | Strings used as discriminators in dictionaries or conditionals |
| **Large methods** | Methods over ~20 lines or with numbered internal comments |
| **Redundant comments** | Comments that only repeat what the code already says |
| **Records vs classes** | Immutable data classes with no behavior |
| **Stateful static classes** | Static classes with static dictionaries or lists |

### Correction Order

1. **Enums** — create before renaming, since renaming depends on them
2. **Constants** — extract magic numbers to `const` or `static readonly`
3. **Names** — rename variables, parameters, and methods with clear intent
4. **Method extraction** — break large methods into named private methods
5. **Records** — convert immutable data classes to `record`
6. **Comments** — remove redundant ones, keep those that explain "why"

For naming patterns and before/after examples → `references/naming-conventions.md`
For SOLID, DRY, KISS examples → `references/solid-principles.md`

---

## Phase 2 — Project Structure

### Diagnosis

For each existing type, identify its correct folder:

| Type | Correct folder |
|---|---|
| String discriminators / type-safe flags | `Models/Enums/` |
| Input and output records | `Models/` |
| Business logic and use cases | `Services/` |
| Stateless helpers and extension methods | `Utils/` |
| Dependency composition and I/O | `Program.cs` |

For the full structure reference → `references/project-structure.md`

---

## Final Checklist

### Code
- [ ] No single or two-character variables outside lambdas
- [ ] No magic numbers without a named constant
- [ ] No strings used as discriminators where an enum applies
- [ ] Every method does one single thing
- [ ] No comments that only describe what the code already says
- [ ] Records used for immutable data models
- [ ] No stateful `static class`

### Project
- [ ] One file per type
- [ ] Models has no dependencies on Services or Utils
- [ ] Services depends only on Models and Utils
- [ ] Program.cs contains only composition and I/O
- [ ] `.csproj` with `<Nullable>enable</Nullable>` and `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`

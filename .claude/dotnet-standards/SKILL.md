---
name: dotnet-standards
description: >
  Apply Clean Code, SOLID, DRY, and KISS principles to any .NET C# project,
  covering both code quality refactoring and project structure reorganization.
  Use this skill whenever the user wants to: refactor C# code for better
  readability, fix poorly named variables or parameters, replace magic numbers
  with named constants, replace string discriminators with enums, split
  oversized methods, remove redundant comments, separate classes into proper
  files, apply SRP to Models/Services/Utils folders, introduce interfaces for
  dependency inversion, convert static classes to injectable services, or
  reorganize a .NET project folder structure. Also trigger when the user
  mentions "clean code", "SOLID", "refactor", "separate classes", "naming",
  "project structure", "dependency injection", "code smell", or when multiple
  classes live in a single file.
---

# .NET Standards — Clean Code + Project Structure

This skill covers two levels of improvement that typically go hand in hand.
Both phases are project-agnostic and apply to any .NET C# codebase.

| Level | Scope | Reference |
|---|---|---|
| **Code** | Naming, constants, enums, methods, comments | `references/naming-conventions.md` |
| **Project** | Folder separation, interfaces, files | `references/project-structure.md` |
| **Principles** | SOLID, DRY, KISS with practical examples | `references/solid-principles.md` |

Read the relevant reference file before proposing any changes.

---

## Normalization Process

When receiving a C# project or file to normalize, run both phases in sequence.
Present the diagnosis of each phase to the user before applying corrections.

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

### Quick Naming Rules

```
Local variable:     camelCase, intent-revealing name
Parameter:          camelCase, never abbreviated
Private field:      _camelCase with underscore prefix
Constant:           PascalCase (const or static readonly)
Property:           PascalCase
Method:             PascalCase, verb in infinitive form
Class/Record/Enum:  PascalCase, noun
Interface:          IPascalCase
```

For detailed before/after examples -> `references/naming-conventions.md`

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

### Standard Folder Structure

```
MyApp/
├── MyApp.sln
└── src/
    └── MyApp/
        ├── MyApp.csproj
        ├── Program.cs
        ├── Models/
        │   └── Enums/
        ├── Services/
        └── Utils/
```

For the full structure with naming conventions -> `references/project-structure.md`

### Dependency Rules

```
Program.cs  ->  Services  ->  Models
                   |
                 Utils
```

- Models has no dependencies on Services or Utils
- Utils has no dependencies on Models or Services
- Services depends on Models and may use Utils
- Program.cs is the only place that does I/O

For SOLID principles and code examples -> `references/solid-principles.md`

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
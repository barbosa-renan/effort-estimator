---
name: azure-devops-mcp
description: >
  Connect to Azure DevOps via MCP to read work items and implement tasks
  directly from the backlog. Use this skill whenever the user wants to: read
  a work item or task from Azure DevOps, implement what is described in a
  ticket, fetch acceptance criteria from a user story, list tasks in a sprint,
  update the status of a work item, or link a commit or PR to a work item.
  Also trigger when the user mentions "ADO", "Azure DevOps", "work item",
  "task", "user story", "PBI", "sprint", or "backlog".
---

# Azure DevOps MCP — Setup and Usage

This skill requires the Azure DevOps MCP server to be installed and registered
in Claude Code before any tool calls can be made.

## Prerequisites

Read `references/setup.md` for full installation and authentication steps
before attempting to use any MCP tool.

Quick checklist:
- [ ] `@azure-devops/mcp` installed globally via npm
- [ ] `AZURE_DEVOPS_ORG_URL` environment variable set
- [ ] `AZURE_DEVOPS_PAT` personal access token set
- [ ] MCP server registered in `.claude/settings.json`

---

## Workflow: Read a Task and Implement It

Follow these steps in order. Do not skip the planning step.

### Step 1 — Fetch the work item

```
mcp_azure_devops_get_work_item({ id: <WORK_ITEM_ID> })
```

Extract from the response:
- **Title** — what is being built
- **Description** — full context and requirements
- **Acceptance Criteria** — the definition of done
- **Type** — Task, User Story, Bug, etc.

If any of these fields are missing or ambiguous, do not proceed to
implementation. Ask the user to clarify or update the work item in ADO.

### Step 2 — Plan before coding

Enter plan mode. Write a short spec based on the work item:

```
Work Item: #<ID> — <Title>

Goal: <one sentence summary>

Files to change:
- <file 1> — <reason>
- <file 2> — <reason>

Approach:
<describe the implementation strategy>

Acceptance criteria to satisfy:
- [ ] <criterion 1>
- [ ] <criterion 2>
```

Do not write a single line of code before this spec is complete.

### Step 3 — Implement

Follow the project standards defined in:
- `.claude/skills/dotnet-standards/SKILL.md` — naming, SOLID, structure
- `.claude/skills/dotnet-unit-testing/SKILL.md` — tests for any new public method

### Step 4 — Verify

Run the test suite before considering the work item done:

```bash
dotnet test
```

Check each acceptance criterion against the implementation. Every criterion
must be satisfied and verifiable.

### Step 5 — Update the work item

After successful implementation, update the work item state:

```
mcp_azure_devops_update_work_item({
  id: <WORK_ITEM_ID>,
  state: "Resolved",
  comment: "Implemented in <branch/commit>. All acceptance criteria satisfied."
})
```

---

## Available MCP Tools

| Tool | Purpose |
|---|---|
| `mcp_azure_devops_get_work_item` | Fetch a work item by ID |
| `mcp_azure_devops_list_work_items` | List work items by query or sprint |
| `mcp_azure_devops_update_work_item` | Update state, assignee, or add a comment |
| `mcp_azure_devops_get_work_item_comments` | Read discussion thread on a work item |
| `mcp_azure_devops_list_iterations` | List sprints for a project |

For full tool signatures and parameters → `references/mcp-tools.md`

---

## Rules for This Skill

- Never implement without reading the full work item first
- Never mark a work item Resolved without a passing test suite
- If the acceptance criteria are missing, ask before implementing
- Never update ADO state to Resolved if `dotnet test` has failures

# Azure DevOps MCP — Tool Reference

## mcp_azure_devops_get_work_item

Fetches a single work item by ID.

```
mcp_azure_devops_get_work_item({
  id: 1234
})
```

**Response fields to extract:**

| Field | Path in response | Notes |
|---|---|---|
| Title | `fields["System.Title"]` | |
| Description | `fields["System.Description"]` | May contain HTML |
| Acceptance Criteria | `fields["Microsoft.VSTS.Common.AcceptanceCriteria"]` | May be empty |
| State | `fields["System.State"]` | New, Active, Resolved, Closed |
| Type | `fields["System.WorkItemType"]` | Task, User Story, Bug, Epic |
| Assigned To | `fields["System.AssignedTo"]` | |
| Iteration | `fields["System.IterationPath"]` | Sprint name |

---

## mcp_azure_devops_list_work_items

Fetches work items using a WIQL query or by iteration.

```
# By sprint
mcp_azure_devops_list_work_items({
  project: "MyProject",
  query: "SELECT [System.Id], [System.Title], [System.State] FROM WorkItems WHERE [System.IterationPath] = 'MyProject\\Sprint 5' AND [System.AssignedTo] = @Me"
})

# By state
mcp_azure_devops_list_work_items({
  project: "MyProject",
  query: "SELECT [System.Id], [System.Title] FROM WorkItems WHERE [System.State] = 'Active' AND [System.WorkItemType] = 'Task'"
})
```

---

## mcp_azure_devops_update_work_item

Updates fields or adds a comment to a work item.

```
# Update state
mcp_azure_devops_update_work_item({
  id: 1234,
  fields: {
    "System.State": "Resolved"
  }
})

# Add a comment
mcp_azure_devops_update_work_item({
  id: 1234,
  comment: "Implemented in branch feature/1234-payment-gateway. All acceptance criteria satisfied."
})

# Update state and add comment in one call
mcp_azure_devops_update_work_item({
  id: 1234,
  fields: {
    "System.State": "Resolved"
  },
  comment: "Fixed. Tests passing. PR #42 linked."
})
```

---

## mcp_azure_devops_get_work_item_comments

Reads the full discussion thread on a work item. Useful for understanding
context, decisions, or clarifications added after the original description.

```
mcp_azure_devops_get_work_item_comments({
  id: 1234
})
```

---

## mcp_azure_devops_list_iterations

Lists all sprints (iterations) for a project.

```
mcp_azure_devops_list_iterations({
  project: "MyProject"
})
```

Use this to find the exact `IterationPath` string needed in WIQL queries.

---

## Common WIQL Patterns

```sql
-- All active tasks assigned to me in current sprint
SELECT [System.Id], [System.Title], [System.State]
FROM WorkItems
WHERE [System.WorkItemType] = 'Task'
  AND [System.State] = 'Active'
  AND [System.AssignedTo] = @Me
  AND [System.IterationPath] = @CurrentIteration

-- All work items in a specific sprint
SELECT [System.Id], [System.Title], [System.WorkItemType], [System.State]
FROM WorkItems
WHERE [System.IterationPath] = 'MyProject\Sprint 5'
ORDER BY [System.WorkItemType], [System.State]

-- Bugs not yet resolved
SELECT [System.Id], [System.Title], [System.Priority]
FROM WorkItems
WHERE [System.WorkItemType] = 'Bug'
  AND [System.State] <> 'Resolved'
  AND [System.State] <> 'Closed'
ORDER BY [Microsoft.VSTS.Common.Priority]
```

# Azure DevOps MCP — Setup and Authentication

## 1. Install the MCP Server

```bash
npm install -g @azure-devops/mcp
```

Verify the installation:

```bash
azure-devops-mcp --version
```

---

## 2. Create a Personal Access Token (PAT)

1. Go to `https://dev.azure.com/<your-org>`
2. Click your profile picture → **Personal Access Tokens**
3. Click **New Token**
4. Set the following scopes:
   - **Work Items** → Read & Write
   - **Code** → Read (if you need to link commits)
5. Copy the token — it will not be shown again

---

## 3. Set Environment Variables

### macOS / Linux

Add to your shell profile (`~/.zshrc` or `~/.bashrc`):

```bash
export AZURE_DEVOPS_ORG_URL="https://dev.azure.com/your-organization"
export AZURE_DEVOPS_PAT="your-personal-access-token"
```

Reload the profile:

```bash
source ~/.zshrc
```

### Windows (PowerShell)

```powershell
[System.Environment]::SetEnvironmentVariable("AZURE_DEVOPS_ORG_URL", "https://dev.azure.com/your-organization", "User")
[System.Environment]::SetEnvironmentVariable("AZURE_DEVOPS_PAT", "your-personal-access-token", "User")
```

---

## 4. Register the MCP Server in Claude Code

Add the server to `.claude/settings.json` at the project root:

```json
{
  "mcpServers": {
    "azure-devops": {
      "command": "azure-devops-mcp",
      "env": {
        "AZURE_DEVOPS_ORG_URL": "${AZURE_DEVOPS_ORG_URL}",
        "AZURE_DEVOPS_PAT": "${AZURE_DEVOPS_PAT}"
      }
    }
  }
}
```

The `${VAR}` syntax reads from your environment — the token is never
hardcoded in the settings file.

---

## 5. Verify the Connection

Start a Claude Code session and run:

```
mcp_azure_devops_list_iterations({ project: "your-project-name" })
```

If you see a list of sprints, the connection is working.

If you see an authentication error:
- Confirm the PAT has not expired
- Confirm the `AZURE_DEVOPS_ORG_URL` includes the org name (not just `dev.azure.com`)
- Confirm the PAT has Work Items scope

---

## Security Notes

- Never commit the PAT to source control
- Never hardcode the PAT in `.claude/settings.json` — always use `${ENV_VAR}`
- Rotate the PAT every 90 days or according to your organization's policy
- Use the minimum required scopes — do not grant full access

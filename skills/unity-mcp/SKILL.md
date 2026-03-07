---
name: unity-mcp
description: Verify Unity C# compilation via MCP after code changes. Domain reload and compile error checking through Unity Editor bridge. Use when checking if scripts compile in Unity. Triggers on "compile", "コンパイル", "domain reload", "Unity MCP", "ビルドチェック", "compile check", "コンパイルエラー".
model: sonnet
allowed-tools: mcp__unity-mcp__*
---

# Unity MCP Compile Check

Verify C# script compilation in Unity Editor via MCP tools after code changes.

## Prerequisites

- Unity 6+ with `com.unity.ai.assistant` (2.0+) installed
- MCP bridge running (Edit > Project Settings > AI > Unity MCP → green status)
- MCP client connection approved in Unity

## Compile Check Procedure

After writing or editing C# scripts:

### Step 1: Trigger Domain Reload

Execute `AssetDatabase.Refresh()` via the MCP command execution tool (e.g., `Unity_RunCommand`):

```
AssetDatabase.Refresh();
```

This triggers Unity's domain reload — the editor reimports changed scripts and recompiles assemblies.

**Wait for the reload to complete** before proceeding. Call the console log tool after a few seconds — if the reload is still in progress, logs may not yet reflect the final compilation state. Retry until logs stabilize.

### Step 2: Check Compile Errors

Call `Unity_GetConsoleLogs` with error filter:

- `logTypes`: `"error"` (also accepts `"info"`, `"warning"`, comma-separated e.g. `"info,warning,error"`)

### Step 3: Interpret Results

- **No errors** → Compilation succeeded. Proceed with next task.
- **Errors found** → Fix the reported issues and repeat from Step 1.

Report errors with file path and line number so the user can locate them.

## Notes

- **Tool names vary by package version and MCP server configuration.** The names used above (`Unity_RunCommand`, `Unity_GetConsoleLogs`, `Unity_EditorWindow_CaptureScreenshot`) are representative. Check the actual tools exposed by your MCP server if calls fail.
- MCP server name may vary by setup. If `unity-mcp` tools are unavailable, check the MCP server name in your configuration.
- The official package exposes tools with `Unity_` prefix (e.g., `Unity_ManageScene`, `Unity_ManageGameObject`). Tool availability depends on package version.

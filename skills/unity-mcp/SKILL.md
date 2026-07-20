---
name: unity-mcp
description: Verify Unity C# compilation via MCP after code changes — domain reload and compile-error checking through the Unity Editor bridge. Use when checking if scripts compile in Unity.
when_to_use: Triggers on "compile", "コンパイル", "domain reload", "Unity MCP", "ビルドチェック", "compile check", "コンパイルエラー".
model: sonnet
allowed-tools: mcp__unity-mcp__* TaskCreate TaskUpdate TaskList
---

# Unity MCP Compile Check

Verify C# script compilation in Unity Editor via MCP tools after code changes.

## Phase Tracking

**At workflow start, create tasks for each phase:**

```
TaskCreate: "Phase 1: Trigger domain reload | MUST run AssetDatabase.Refresh() via MCP"
TaskCreate: "Phase 2: Check compile errors  | MUST read console via MCP (retry until logs stabilize)"
TaskCreate: "Phase 3: Report / fix loop     | errors -> fix -> repeat from Phase 1"
```

Update status as you progress: `in_progress` when starting, `completed` when done. On errors, reset Phases 1-2 to `pending` and loop until the console is clean.

## Prerequisites

- Unity 6 (6000.0)+ with `com.unity.ai.assistant` installed (the official Unity MCP server ships inside this package; there is no separate `com.unity.mcp` package)
- MCP bridge running (Edit > Project Settings > AI > Unity MCP Server → "Unity Bridge: Running" green status; it activates automatically on editor startup). MCP clients connect through a platform-specific relay binary in `~/.unity/relay/` launched with the `--mcp` flag over stdio
- MCP client connection approved in Unity (tools can be enabled/disabled per-tool in the same settings page)

## Compile Check Procedure

After writing or editing C# scripts:

### Step 1: Trigger Domain Reload

Trigger a script reimport/recompile via the MCP script-management tool (official name: `Unity_ManageScript`; older/community servers may expose a command-execution tool instead — run `AssetDatabase.Refresh();` there):

```
AssetDatabase.Refresh();
```

This triggers Unity's domain reload — the editor reimports changed scripts and recompiles assemblies.

**Wait for the reload to complete** before proceeding. Call the console tool after a few seconds — if the reload is still in progress, logs may not yet reflect the final compilation state. Retry until logs stabilize.

### Step 2: Check Compile Errors

Call the console-reading tool (official name: `Unity_ReadConsole`; documented params: `Action: Get`, `Types: Error`) filtered to errors. Community servers may name it `Unity_GetConsoleLogs` with a `logTypes` parameter (`"error"`, or comma-separated `"info,warning,error"`).

### Step 3: Interpret Results

- **No errors** → Compilation succeeded. Proceed with next task.
- **Errors found** → Fix the reported issues and repeat from Step 1.

Report errors with file path and line number so the user can locate them.

## Notes

- **Official built-in tools** (com.unity.ai.assistant 2.15 docs): `Unity_ManageScene`, `Unity_ManageGameObject`, `Unity_ReadConsole`, `Unity_ManageScript` (its validation level is configurable in the server settings: Basic/Standard/Comprehensive/Strict). Custom tools: `[McpTool]` attribute on a static method (schema auto-generated), `IUnityMcpTool<T>` for stateful class-based tools, or `McpToolRegistry.RegisterTool<T>()` at runtime.
- **Tool names vary by package version and MCP server configuration.** If a call fails, list the actual tools exposed by the connected server and use the closest match.
- MCP server name may vary by setup. If `unity-mcp` tools are unavailable, check the MCP server name in your configuration.
- Don't confuse this with "Assistant MCP Extensions" (the settings page where Unity's Assistant *consumes* external MCP servers — the opposite direction).

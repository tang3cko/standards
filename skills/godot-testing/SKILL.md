---
name: godot-testing
description: Write and review tests for Godot 4.x + .NET projects following Tang3cko standards — xUnit, Humble Object, link-referenced pure C# logic, no Godot runtime required. Use when writing tests or reviewing test code in a Godot project.
when_to_use: Triggers on "Godot test", "xUnit", "Tests.csproj", "dotnet test", "Humble Object", "Godotテスト", "テスト", "link reference", "リンク参照".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Testing

Help users write, review, and improve tests for Godot 4.x + .NET projects following Tang3cko testing standards.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references     | testing.md"
TaskCreate: "Phase 2: Decide test scope   | pure C# only — per the scope table"
TaskCreate: "Phase 3: Write/review tests  | AAA structure, link references in Tests.csproj"
TaskCreate: "Phase 4: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Strategy questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `references/testing.md` | Always |
| 2 | Justify scope per the "Choosing What to Test" table | New tests |
| 4 | Self-Check every written/edited test | Tests were written or edited |

## Core Rules (always apply)

1. **Pure C# only** — test Godot-independent logic (calculation, data transformation, validation). Node lifecycle, scenes, UI, and input are manually verified, not unit-tested
2. **Humble Object** — logic lives in pure C# classes separated from Godot nodes; the node stays a thin shell
3. **Peer Tests project** — `Tests/Tests.csproj` sits beside the main project; the main `.csproj` excludes it via `<Compile Remove="Tests/**" />`
4. **Link references, not project references** — `Tests.csproj` pulls in only the needed pure-logic files with `<Compile Include="../Scripts/..." Link="..." />`; files depending on Godot nodes are never linked. `internal` dependencies of linked files must be linked too
5. **AAA structure** — every test reads as Arrange, Act, Assert; one behavior per test; file name = `{Feature}Test.cs`, method name = `Method_Condition_ExpectedResult`
6. **Run with `dotnet test`** — no Godot editor or runtime involved

## Workflow

Identify the task type and follow the matching path.

### Writing new tests

1. Load `references/testing.md` (structure, csproj pattern, Humble Object example)
2. Confirm the target is pure C#; if it is embedded in a Node, extract it first (Humble Object)
3. Add the target file (and its `internal` dependencies) as link references in `Tests.csproj`
4. Write tests following AAA; run `dotnet test`
5. Run the Self-Check below before presenting

### Reviewing test code

1. Check against the Core Rules above
2. Verify no linked file depends on Godot nodes, and no `internal` dependency is missing from `Tests.csproj`
3. Categorize issues by priority (must fix, should fix, nice to have)

### A testing-strategy question

1. Load `references/testing.md`
2. Answer using the "Choosing What to Test" table; recommend Humble Object extraction when logic is trapped in a Node

## Self-Check (before presenting generated/edited tests)

Verify every file you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant code.

- [ ] Every tested class is pure C# (no Godot node dependency)
- [ ] `Tests.csproj` links the target files and all their `internal` dependencies
- [ ] Test files are named `{Feature}Test.cs`; namespace is `{Project}.Tests`
- [ ] Every test follows AAA and asserts one behavior
- [ ] `dotnet test` passes (or the user was told it was not run and why)

## Reference Files

| File | Use When |
|------|----------|
| references/testing.md | Project structure, Tests.csproj pattern, Humble Object example, scope table, internal-linking |

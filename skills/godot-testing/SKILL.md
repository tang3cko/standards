---
name: godot-testing
description: Write and review tests for Godot 4.x + .NET projects following Tang3cko standards — xUnit link-referenced pure C# unit tests, GdUnit4Net SceneRunner E2E tests, and Tools/ numeric regression gates. Use when writing tests or reviewing test code in a Godot project.
when_to_use: Triggers on "Godot test", "xUnit", "Tests.csproj", "dotnet test", "Humble Object", "E2E", "GdUnit", "GdUnit4Net", "SceneRunner", "Tests.E2E", "regression gate", "Godotテスト", "テスト", "link reference", "リンク参照", "E2Eテスト", "回帰ゲート".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Testing

Help users write, review, and improve tests for Godot 4.x + .NET projects following Tang3cko testing standards. Tests span three layers — xUnit link-referenced pure C#, GdUnit4Net E2E, and Tools/ numeric regression gates — each catching what the layer below structurally cannot.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references     | testing.md (+ e2e-harness.md / visual-verification.md if that layer applies)"
TaskCreate: "Phase 2: Decide test layer   | xUnit pure C# / GdUnit4Net E2E / numeric gate — per the scope table"
TaskCreate: "Phase 3: Write/review tests  | AAA structure for xUnit; SceneRunner flow for E2E; gate script for numeric"
TaskCreate: "Phase 4: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Strategy questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `references/testing.md` | Always |
| 1 | Read `references/e2e-harness.md` | E2E test involved |
| 1 | Read `references/visual-verification.md` | Visual/pose/animation change involved |
| 2 | Justify layer per the "Choosing What to Test" table | New tests |
| 4 | Self-Check every written/edited test | Tests were written or edited |

## Core Rules (always apply)

Tests are three layers, fastest/narrowest to slowest/broadest. Each layer exists to catch what the layer(s) below it structurally cannot see. Node lifecycle, scenes, UI, and input are covered by manual verification + E2E (SceneRunner) + numeric/visual gates — never by Layer 1 unit tests.

### Layer 1 — xUnit, link-referenced pure C# (`Tests/`)

1. **Pure C# only** — test Godot-independent logic (calculation, data transformation, validation). Node lifecycle, scenes, UI, and input are out of scope for this layer
2. **Humble Object** — logic lives in pure C# classes separated from Godot nodes; the node stays a thin shell
3. **Peer Tests project** — `Tests/Tests.csproj` sits beside the main project; the main `.csproj` excludes it via `<Compile Remove="Tests/**" />`
4. **Link references, not project references** — `Tests.csproj` pulls in only the needed pure-logic files with `<Compile Include="../Scripts/..." Link="..." />`; files depending on Godot nodes are never linked. `internal` dependencies of linked files must be linked too
5. **AAA structure** — every test reads as Arrange, Act, Assert; one behavior per test; file name = `{Feature}Test.cs`, method name = `Method_Condition_ExpectedResult`
6. **Run with `dotnet test`** — no Godot editor or runtime involved

### Layer 2 — GdUnit4Net E2E, SceneRunner (`Tests.E2E/`)

7. **Drive the real scene, not a mock** — `Tests.E2E.csproj` project-references the full game assembly and loads the actual `.tscn` through `ISceneRunner`; it exists to catch wiring bugs (an event that never fires, a node the signal handler never finds) that a pure-logic Layer 1 test cannot see because it never touches the scene graph
8. **Engine errors must fail the test** — a `GD.PushError` during a run is a real bug even when no assertion catches it; wire the class up so it is never silently swallowed (see `references/e2e-harness.md`)
9. **Sandbox all persistence** — save/profile I/O is redirected to a throwaway directory via an env var; the test refuses to run if it is unset, never touches the developer's real save data (see `references/e2e-harness.md`)
10. **Ship-bug regressions get their story in a doc comment** — a regression test born from a shipped bug documents the original bug, why Layer 1 missed it (wiring vs. pure logic), and asserts the precondition that triggers it was actually hit (see `references/e2e-harness.md`)

### Layer 3 — numeric/visual regression gates (`Tools/`)

11. **Gate numerically first, render visually second** — GDScript/C# tools under `Tools/` assert measurable properties (angles, ratios, distances) as a cheap regression check before any human or automated visual pass; add a gate for every failure pattern that has actually slipped through once (see `references/visual-verification.md`)
12. **"Differs from rest" is not "is correct"** — a non-zero numeric delta only proves something changed, not that it changed correctly; pair magnitude checks with orientation/sign checks

### Choosing the layer

Prefer the narrowest layer that can catch the bug: Layer 1 for anything expressible as pure C#, Layer 2 when the bug lives in scene wiring or cross-node flow, Layer 3 (plus manual/visual confirmation) when it is about pose, rendering, or in-game appearance.

## Workflow

Identify the task type and follow the matching path.

### Writing new Layer 1 (xUnit) tests

1. Load `references/testing.md` (structure, csproj pattern, Humble Object example)
2. Confirm the target is pure C#; if it is embedded in a Node, extract it first (Humble Object)
3. Add the target file (and its `internal` dependencies) as link references in `Tests.csproj`
4. Write tests following AAA; run `dotnet test`
5. Run the Self-Check below before presenting

### Writing new Layer 2 (E2E) tests

1. Load `references/e2e-harness.md` (`.runsettings`, engine-error-as-failure wiring, save sandboxing, regression doc-comment convention)
2. Confirm the target genuinely needs a real scene (wiring, cross-node flow, persistence) — if it is expressible as pure C#, it belongs in Layer 1 instead
3. Drive the scene through `ISceneRunner`; assert on the real node tree, never on internals reached via reflection shortcuts
4. If this is a shipped-bug regression, write the doc comment per `references/e2e-harness.md` (bug, why Layer 1 missed it, precondition assertion)
5. Run the Self-Check below before presenting

### Adding/updating a Layer 3 numeric or visual gate

1. Load `references/visual-verification.md` (three-stage gate progression, "differs ≠ correct" principle, sub-agent verification rule)
2. Identify which stage the new check belongs to: numeric gate (magnitude + orientation), headless PNG render, or in-game screenshot
3. For PNG-capture launch flags, see the godot-headless skill rather than duplicating them here
4. Add the check as a targeted response to an actual failure pattern that slipped through, not speculatively

### Reviewing test code

1. Check against the Core Rules above for whichever layer(s) the changed files belong to
2. Layer 1: verify no linked file depends on Godot nodes, and no `internal` dependency is missing from `Tests.csproj`
3. Layer 2: verify engine errors are not silently swallowed, persistence is sandboxed, and regression tests carry the doc-comment story
4. Layer 3: verify a new gate pairs magnitude with orientation, and that a visual claim is backed by an actual rendered image, not just a numeric pass
5. Categorize issues by priority (must fix, should fix, nice to have)

### A testing-strategy question

1. Load `references/testing.md`, plus `references/e2e-harness.md` and/or `references/visual-verification.md` if the question touches those layers
2. Answer using the "Choosing the layer" guidance above and the "Choosing What to Test" table in testing.md; recommend Humble Object extraction when logic is trapped in a Node

## Self-Check (before presenting generated/edited tests)

Verify every file you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant code.

**Layer 1 (xUnit):**
- [ ] Every tested class is pure C# (no Godot node dependency)
- [ ] `Tests.csproj` links the target files and all their `internal` dependencies
- [ ] Test files are named `{Feature}Test.cs`; namespace is `{Project}.Tests`
- [ ] Every test follows AAA and asserts one behavior
- [ ] `dotnet test` passes (or the user was told it was not run and why)

**Layer 2 (E2E), when applicable:**
- [ ] The test drives a real scene via `ISceneRunner`, not a reflection shortcut
- [ ] Engine errors during the run are not silently swallowed
- [ ] Any persistence touched is sandboxed to a throwaway path, never the developer's real save data
- [ ] A shipped-bug regression carries a doc comment covering the original bug, why Layer 1 missed it, and an assertion that the precondition was actually hit

**Layer 3 (numeric/visual gates), when applicable:**
- [ ] A new gate checks orientation/sign, not just magnitude
- [ ] Any "it looks right now" claim is backed by an actual rendered image the reviewer opened, not just a numeric pass

**All paths:**
- [ ] The references named by the chosen workflow path were actually Read this session, and the `## Checklist` at the end of every loaded reference was run against the result — SKILL.md is only the index; the acceptance criteria (csproj linking details, `.runsettings` load-bearing values, gate-stage evidence rules) live in those checklists, so skipping them means presenting unreviewed work

## Reference Files

| File | Load When |
|------|-----------|
| references/testing.md | Before writing Layer 1 tests or answering scope questions — the only file with the full `Tests.csproj` XML (link-reference syntax, package versions), the Choosing What to Test scope table, and the `internal`-dependency linking rule with the `InternalsVisibleTo` trade-off |
| references/e2e-harness.md | Before writing or reviewing any GdUnit4Net E2E test — the only file with the `.runsettings` XML and why each value is load-bearing (`MaxCpuCount=1`, `--path ..`, `CompileProcessTimeout`), the engine-error-as-failure wiring + self-check test, the sandbox delete-safety code, and the 3-part regression doc-comment convention |
| references/visual-verification.md | Before adding a Layer 3 gate or judging pose/animation/rendering changes — the only file with the three-stage gate progression, the per-stage catches/cannot-catch table, the headless-render capture rules (front+side, multiple timestamps), and the sub-agent evidence rules |

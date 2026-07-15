---
name: godot-headless
description: Run Godot 4.x + .NET (C#) tests and CLI verification without the Editor open — godot-mono batchmode asset import, --build-solutions compile checks, dotnet test (xUnit link-reference + GdUnit4Net E2E), and offscreen PNG render harnesses for visual verification. Use when setting up or debugging CLI/CI-driven Godot test, import, or render execution.
when_to_use: Triggers on "godot headless", "godot-mono", "batchmode", "--headless", "--import", "--build-solutions", "GODOT_BIN", "GdUnit4Net", "run-tests.sh", "rendering-driver", "offscreen render", "PNG render", "スクリーンショット検証", "ヘッドレス", "ビルド検証".
model: sonnet
allowed-tools: Read Glob Grep Bash Write Edit TaskCreate TaskUpdate TaskList
---

# Godot Headless Operations

Set up and debug Editor-non-resident (CLI/CI) Godot 4.x + .NET execution, following Tang3cko standards proven across three internal Godot projects. Generalized for any godot-mono project — not a specific project's build.

This skill covers **CLI execution mechanics**: what to invoke, in what order, and the traps that make output silently wrong. It does not cover how to *write* tests (see `godot-testing`) or how to structure `Scripts/Core` (see `godot-core`/`godot-architecture`).

**This skill ships working scripts — invoke them, don't reimplement their logic.** `scripts/run-tests.sh` already handles the unit/e2e/all layer selection and exit-code aggregation that every project reinvents. The only things you write per-project are the pieces that are inherently project-specific: the visual-verification harness's scene path and controller lookup (`assets/VerifyRender.cs.template`).

## Phase Tracking

**At workflow start, create tasks for each phase:**

```
TaskCreate: "Phase 1: Identify task   | import/compile check, dotnet test, or offscreen PNG render"
TaskCreate: "Phase 2: Prepare inputs  | project path + (render only) harness scene/entry point in place"
TaskCreate: "Phase 3: Run command     | scripts/run-tests.sh or the direct godot-mono invocation via Bash"
TaskCreate: "Phase 4: Verify result   | read command output + Self-Check"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Pure troubleshooting questions (pitfall lookup) skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 2 | For a PNG render task: confirm a harness node exists under `Tools/`; copy from `assets/VerifyRender.cs.template` and fill in TODOs (scene path, controller class, injected-input hook) if missing | Render tasks |
| 3 | `dotnet test` runs go through `scripts/run-tests.sh <unit\|e2e\|all>` — do not hand-roll parallel `dotnet test` invocations | dotnet test tasks |
| 3 | A real screenshot render never passes `--headless` — see Core Rule 2 | Render tasks |
| 4 | Read the command's own summary/exit path; don't declare success from Bash exit status alone when the tool prints failure detail (dotnet test) or produces a file to inspect (PNG render) | Always |

## Core Rules (always apply)

1. **CLI is `godot-mono` — never `godot` or the `.app` bundle directly.** The mono/.NET build is a separate binary from vanilla Godot; using the wrong one either fails outright or silently runs without C# support. Key invocations:
   - `godot-mono --headless --path . --import` — asset (re)import, no window, no C#
   - `godot-mono --headless --path . --build-solutions --quit` — compiles the C# solution and exits; the cheapest way to catch a compile error without opening the Editor
   - `dotnet test Tests/Tests.csproj` — link-referenced pure C# unit tests, no Godot runtime at all
   - `dotnet test Tests.E2E/Tests.E2E.csproj --settings Tests.E2E/.runsettings` — GdUnit4Net SceneRunner E2E tests; this spawns `godot-mono` itself per `GODOT_BIN` in the `.runsettings` (see `references/commands.md`)

2. **`--headless` forces the dummy rendering driver — it cannot produce a real screenshot.** On Godot 4.6.x, `--headless` selects the "headless" display driver, which only ever backs the "dummy" rendering driver regardless of any `--rendering-driver` flag also passed. `GetViewport().GetTexture().GetImage()` then either throws (null texture) or silently returns black — no error tells you the render never happened. For an actual pixel-backed screenshot, **omit `--headless`** and instead pass `--rendering-driver opengl3 --audio-driver Dummy` — this opens a window that is effectively offscreen in a CI/dev environment but gets a real GL context. See `references/rendering.md` for the full harness pattern (fast-forward, input injection, `--out=`).

3. **Verification is layered, not single-shot.** For anything touching pose, layout, or visual behavior, use the three-gate progression: numeric/behavior checks → offscreen PNG render (Core Rule 2's invocation) → in-game screenshot from the real running scene. Each gate catches what the one before it structurally cannot — "the value changed" never proves "the value is now correct" (a limb can be numerically non-rest and still be twisted into an impossible pose). Don't skip straight to numeric-only sign-off on a visual change, and don't accept a sub-agent's "looks right" report without an attached rendered image.

4. **The project holds one lock — a second `godot-mono` process against the same project directory will not start while another (Editor or CLI) holds it.** `--import`, `--build-solutions`, `dotnet test` (which spawns its own `godot-mono` for E2E), and a render harness invocation are all mutually exclusive against the same project path. Sequence them; never run two concurrently against the same project. GdUnit4Net's own E2E layer already serializes itself for exactly this reason (`.runsettings` sets `MaxCpuCount=1` — see `references/pitfalls.md`).

5. **Never put a lowercase `scripts/` runner next to `Scripts/` source.** macOS's default case-insensitive filesystem merges the two, so this skill's `run-tests.sh` equivalent belongs in `Tools/` (or another name distinct from `Scripts/`) in every consuming project — see `godot-core`'s `references/gotchas.md` for the general case-insensitive-filesystem gotcha this follows from.

## Workflow

Identify the task type and follow the matching path.

### Verifying assets import / C# compiles cleanly (no test run)

1. Run: `godot-mono --headless --path <project> --import`
2. Run: `godot-mono --headless --path <project> --build-solutions --quit`
3. A nonzero exit or `SCRIPT ERROR`/`Cannot open file` output in either step is the failure signal — read the printed error, don't just check the exit code (import warnings about unrelated missing files are common and not necessarily fatal; match the error to the file you actually changed)

### Running dotnet test (xUnit unit + GdUnit4Net E2E)

1. Run: `bash ${CLAUDE_PLUGIN_ROOT}/skills/godot-headless/scripts/run-tests.sh <project_path> [unit|e2e|all]`
2. Read the printed per-layer summary and overall PASS/FAIL
3. If the E2E layer fails to start (no `GODOT_BIN`, compile timeout, project lock), load `references/pitfalls.md`
4. Run the Self-Check below

### Rendering an offscreen PNG for visual verification

1. Confirm the target project has a render harness node. If not, copy `assets/VerifyRender.cs.template` into `<project>/Tools/<Domain>/VerifyRender.cs` (+ a matching minimal `.tscn` with this node as root) and resolve every `TODO` (scene path to load, entry-point node lookup, any input-injection hook the scene needs)
2. Run (note: no `--headless`, per Core Rule 2):
   `godot-mono --path <project> --rendering-driver opengl3 --audio-driver Dummy res://Tools/<Domain>/VerifyRender.tscn -- --out=<abs_path>.png [harness-specific args]`
3. Load `references/rendering.md` if the harness needs frame-count tuning, input injection, or a `--fast-forward-ms`-style time skip
4. Open/inspect the produced PNG — a script exiting 0 does not confirm the image shows what was intended; this is gate (b) of Core Rule 3, not a replacement for gate (c) (in-game screenshot)
5. Run the Self-Check below

### Debugging a headless run that fails or hangs

1. Load `references/pitfalls.md` and match the symptom (project lock, GdUnit4Net compile timeout, black/null screenshot, orphaned `godot-mono` process)
2. Apply the fix; re-run the same command
3. If the failure only reproduces with the Editor open/closed unexpectedly, re-check Core Rule 4

## Self-Check (before presenting results)

Verify every run you report on, and any project-specific file you wrote or changed. If any check fails, fix it first — do not present a result that violates these.

- [ ] Used `godot-mono`, never `godot` or a `.app` bundle, in every invocation shown
- [ ] `dotnet test` runs went through `scripts/run-tests.sh`, not a hand-rolled parallel invocation
- [ ] Any screenshot/render command omits `--headless` and instead passes `--rendering-driver opengl3 --audio-driver Dummy`
- [ ] Reported the tool's own summary (Total/Passed/Failed for tests; the actual PNG contents for renders), not just "exit code was 0"
- [ ] A visual-verification claim is backed by an inspected rendered image, not a numeric-only pass
- [ ] Any customized `VerifyRender.cs` has no leftover `TODO` markers
- [ ] No two `godot-mono`/`dotnet test` invocations were run concurrently against the same project path

## Reference Files

| File | Use When |
|------|----------|
| references/commands.md | Full CLI command reference: import, build-solutions, both dotnet test layers, `.runsettings` fields (`GODOT_BIN`, `MaxCpuCount`, `CompileProcessTimeout`) and why each exists |
| references/rendering.md | The `--headless`-forces-dummy-driver trap in full, the PNG harness anatomy (load scene → advance N frames → inject input → `save_png`), designing verification args (`--out`, `--frames`, `--fast-forward-ms`) |
| references/pitfalls.md | Project lock (second process refuses to start), GdUnit4Net compile timeout, black/null screenshot from a `--headless` render, orphaned `godot-mono` processes, `Tools/` vs `scripts/` case-folding |

## Bundled Scripts

| File | Purpose |
|------|---------|
| scripts/run-tests.sh | Run both dotnet test layers end-to-end: layer selector (`unit`/`e2e`/`all`), exit-code aggregation |
| assets/VerifyRender.cs.template | Offscreen PNG render harness node to copy into the target project and customize |

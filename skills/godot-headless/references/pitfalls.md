# Headless Operation Pitfalls

Common failure modes when running godot-mono from the CLI, and how to diagnose them.

---

## Summary

| Symptom | Cause | Fix |
|---------|-------|-----|
| A second `godot-mono` invocation against the same project refuses to start or hangs | Editor (or another `godot-mono` process — including one spawned by `dotnet test`'s GdUnit4Net adapter) already holds the project lock | Close/wait for the other process; never run two invocations concurrently against the same project path |
| First `dotnet test Tests.E2E/...` run appears to hang, then eventually fails with a timeout | GdUnit4Net's spawned `godot-mono` is doing a full solution recompile on a cold `.godot` cache before any test executes | Raise `CompileProcessTimeout` in `.runsettings` (120000ms+) rather than treating this as a real hang; subsequent runs are fast once the cache is warm |
| E2E tests intermittently fail or corrupt each other's state when run with any parallelism | GdUnit4 runtime tests share one Godot process/`.godot` cache; concurrent test sessions race on it | `<MaxCpuCount>1</MaxCpuCount>` in `.runsettings` — this is a correctness requirement, not a performance knob |
| A render harness exits 0, prints "saved screenshot", but the PNG is solid black or the process throws on `GetImage()` | `--headless` was combined with `--rendering-driver` — the dummy display driver wins regardless | Drop `--headless`; use `--rendering-driver opengl3 --audio-driver Dummy` alone (see `rendering.md`) |
| A fresh clone/worktree fails to build with a "type not found" on a file that's clearly in the repo | Asset/script import cache is stale or an `.import`-adjacent file wasn't regenerated | Run `godot-mono --headless --path . --import` before the failing command |
| `dotnet test`/`run-tests.sh` "passes" but a stray `godot-mono` process lingers afterward, later blocking a new run with a project-lock error | A crashed or force-quit prior run left an orphaned process holding the lock | `ps aux \| grep -i "[g]odot-mono.*--path"`, kill the stray process, re-run |
| A script in a `.gdignore`d directory still executes via `-s` or `load()` — or an ignore file has no effect at all | `.gdignore` only gates the editor's filesystem scan (and, via that, pck export), not `ResourceLoader`; a misnamed `.godotignore` is silently ignored | Don't rely on `.gdignore` to block execution; name the file exactly `.gdignore` (see Pitfall 5) |
| A fresh clone cannot run headless `--export-*` — the preset simply isn't there | `export_presets.cfg` excluded by a stock Godot `.gitignore` template | Track the file if it carries no secrets (see Pitfall 6) |

---

## Pitfall 1: Project Lock (Editor or Another Process Already Holds It)

### Problem

Running any `godot-mono` invocation (`--import`, `--build-solutions`, a render harness, or the
`godot-mono` that GdUnit4Net's adapter spawns for E2E) against a project that the Editor, or
another `godot-mono` process, already has open. The second process fails fast (lock error) or
hangs waiting for a lock that will not release while the first process is still running.

### Diagnosis

```bash
ps aux | grep -i "[g]odot-mono.*--path"
```

If this shows a process against the same project path, that's the lock holder.

### Solution

Close the Editor for this project (or wait for the other CLI process to finish), then re-run.
Never sequence a render harness and an E2E `dotnet test` run to overlap against the same project —
run them one after another. This is `SKILL.md` Core Rule 4.

---

## Pitfall 2: GdUnit4Net Compile Timeout on Cold Cache

### Problem

`dotnet test Tests.E2E/Tests.E2E.csproj --settings Tests.E2E/.runsettings` appears to hang (no
output for minutes) and eventually times out, on the *first* run after a fresh clone, a
`.godot/` cache wipe, or a large change.

### Why it's not a bug

The spawned `godot-mono` process recompiles the entire game project's C# solution inside Godot
before running any `[RequireGodotRuntime]` test. On a cold cache this can genuinely take over a
minute on a non-trivial project.

### Solution

Set a generous `<CompileProcessTimeout>` in `Tests.E2E/.runsettings` (120000ms observed as a
reasonable floor; raise further for larger projects) rather than treating a cold-cache first run
as a real failure. Subsequent runs against a warm cache are fast.

---

## Pitfall 3: Black or Null Screenshot from a `--headless` Render

### Problem

A render harness (`assets/VerifyRender.cs.template` or similar) runs to completion, prints a
"saved screenshot" success message, and either throws inside `GetImage()` (null texture, caught
and logged rather than crashing) or writes a PNG that is entirely black.

### Diagnosis

Check the invocation for `--headless` alongside `--rendering-driver`. Any combination of the two
reproduces this on Godot 4.6.x — `--headless` always wins and forces the dummy rendering driver.

### Solution

Remove `--headless`; keep `--rendering-driver opengl3 --audio-driver Dummy`. See `rendering.md`
for the full explanation and the harness pattern that assumes this invocation shape from the
start.

---

## Pitfall 4: `Tools/` vs `scripts/` Case-Folding (macOS)

### Problem

Placing this skill's `run-tests.sh` pattern (or any lowercase `scripts/` directory) at the
project root when a `Scripts/` (C# source) directory already exists there. macOS's default
case-insensitive filesystem treats `scripts/` and `Scripts/` as the same directory, silently
merging the two — files land in unexpected places, and the merge is invisible in `git status` on
the same machine (Git itself is case-sensitive; the corruption is filesystem-level, not
version-control-level) until a Linux CI runner or a case-sensitive volume exposes it.

### Solution

Use `Tools/` (or any name that doesn't collide with an existing PascalCase directory) for CLI
runner scripts, per `SKILL.md` Core Rule 5. This is the same class of case-insensitive-filesystem
issue as the `.gitignore` pitfall documented in `godot-core`'s `references/gotchas.md` — read that
entry for the general mechanism if this is a new failure mode to the reader.

---

## Pitfall 5: `.gdignore` Gates the Editor Scan Only — Not Execution

### Problem

Expecting `.gdignore` to make a directory invisible to all of Godot — e.g. assuming a script under
a `.gdignore`d `tools/` directory cannot be executed, or that ignoring a directory changes CLI
script-run behavior.

### What `.gdignore` actually does

Verified against the engine source (Godot 4.6.3, 2026-07): `.gdignore` is honored only by the
editor's `EditorFileSystem` scan (`_should_skip_directory` in
`editor/file_system/editor_file_system.cpp`). It gates editor scanning/indexing (`.uid`
generation) and, via that, what `--export-*` includes in the pck. It does NOT affect
`-s script.gd` execution or a direct `res://` `load()` of a script — that path goes straight to
`ResourceLoader`, which never consults `.gdignore`.

Also: the file must be named `.gdignore` exactly — `.godotignore` is silently ignored, with no
warning; the directory simply keeps getting scanned.

### Solution

Use `.gdignore` for what it is (keep the editor from scanning/importing a directory, keep it out
of exports) and never as an execution or load barrier. If an ignore file appears to do nothing,
check its name first.

---

## Pitfall 6: `export_presets.cfg` Excluded by Stock `.gitignore` Templates

### Problem

A fresh clone cannot run headless `--export-*` — no presets exist — and any `exclude_filter` or
preset fixes made locally never reach other machines or CI.

### Why the templates exclude it

The popular Godot `.gitignore` templates exclude `export_presets.cfg` because Android keystore
paths/passwords can be stored in it (godot-proposals#1156).

### Solution

If a project's presets carry no secrets (signing handled outside the presets), track the file.
Official env-var keystore support (godotengine/godot PR #35930: `GODOT_ANDROID_KEYSTORE_PATH`,
`GODOT_ANDROID_KEYSTORE_USER`, `GODOT_ANDROID_KEYSTORE_PASSWORD`) exists precisely so the file can
be tracked safely.

---

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-lock boundary
- [commands.md](commands.md) - Full CLI invocation reference
- [rendering.md](rendering.md) - Offscreen PNG rendering, harness anatomy

## Checklist

- [ ] Confirmed no Editor or stray `godot-mono` process held the project lock (`ps aux | grep -i "[g]odot-mono.*--path"`) before declaring a start failure a bug
- [ ] A first-run E2E timeout was treated as a cold-cache compile (raise `CompileProcessTimeout`), not as a real hang
- [ ] Any black/empty PNG was diagnosed against `--headless` in the invocation before touching harness code
- [ ] Nothing relies on `.gdignore` to block `-s`/`load()` execution, and every ignore file is named exactly `.gdignore`
- [ ] The fresh-clone headless story holds: `export_presets.cfg` is tracked, or its exclusion is a deliberate secrets decision
- [ ] Project-local runner scripts live under `Tools/` (or another non-colliding name), never a lowercase `scripts/` beside `Scripts/`

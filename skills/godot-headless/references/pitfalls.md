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

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-lock boundary
- [commands.md](commands.md) - Full CLI invocation reference
- [rendering.md](rendering.md) - Offscreen PNG rendering, harness anatomy

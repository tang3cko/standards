# CLI Command Reference

Full invocation reference for CLI/CI-driven godot-mono execution. See `SKILL.md` Core Rule 1 for
the summary table — this file has the reasoning and flags behind each command.

## Asset import

```
godot-mono --headless --path <project> --import
```

Forces a full asset (re)import without opening a window. Run this before `--build-solutions` or
`dotnet test` if `.import` files are stale (fresh clone, new asset added, `.godot/` cache wiped) —
a stale import cache can make an otherwise-correct scene fail to load with a resource-not-found
error that looks like a code bug.

## C# compile check

```
godot-mono --headless --path <project> --build-solutions --quit
```

Builds the project's C# solution and exits — no scene is loaded, no test runs. This is the
cheapest headless signal that a change compiles at all, useful as a first gate before a full
`dotnet test` run (which recompiles anyway, but takes longer and mixes compile errors with test
failures in its output). `--quit` matters: without it the process can sit waiting on other startup
work after the build finishes.

## Unit tests (xUnit, link-referenced pure C#)

```
dotnet test Tests/Tests.csproj
```

No Godot runtime involved at all — this is a pure .NET test run against link-referenced files from
`Scripts/Core` (or equivalent). See `godot-testing` for how this project layout is built. Fastest
feedback loop; run this first when iterating.

## E2E tests (GdUnit4Net, SceneRunner)

```
dotnet test Tests.E2E/Tests.E2E.csproj --settings Tests.E2E/.runsettings
```

GdUnit4Net's test adapter spawns its own `godot-mono` process per test session to host
`[RequireGodotRuntime]` tests via `SceneRunner`. The spawned process's behavior is controlled
entirely through `Tests.E2E/.runsettings`, not through flags on this `dotnet test` invocation:

| `.runsettings` field | Purpose |
|---|---|
| `<GODOT_BIN>` (env var) | Absolute path to the `godot-mono` executable the adapter should launch. Without this the adapter may fail to find godot-mono at all, or pick up a different version than intended |
| `<Parameters>--headless --audio-driver Dummy --path ..</Parameters>` | Flags passed to the spawned `godot-mono`. `--path ..` matters when the adapter's working directory is the `Tests.E2E/` folder itself (no `project.godot` there) — the trailing `--path ..` wins over an implicit `.` and targets the real project root one level up |
| `<MaxCpuCount>1</MaxCpuCount>` | GdUnit4 runtime tests share one Godot process/`.godot` cache; running them in parallel races on that shared state. Always 1, not a performance knob to raise |
| `<CompileProcessTimeout>` | The first E2E run recompiles the entire game project inside Godot before any test executes — a generous timeout (120000ms+) avoids a false "hang" report on a cold cache. Bump this for larger projects rather than treating a first-run timeout as a real failure |
| `<TestSessionTimeout>` | Overall ceiling for the whole `dotnet test` invocation; separate from the per-compile timeout above |
| Any project-specific env var (e.g. a save-path sandbox override) | E2E runs should never depend on or pollute real user-facing state (save files, prefs). If the project under test reads such paths from an env var, override it here so E2E runs are deterministic |

Note that E2E's own `--headless --audio-driver Dummy` is legitimate here — these tests assert on
scene/logic state (spawn counts, signal firing, HUD bindings), not on rendered pixels. The
`--headless`-forces-dummy-driver trap in `SKILL.md` Core Rule 2 only bites when the goal is an
actual screenshot; SceneRunner tests never call `GetViewport().GetTexture()`.

## Offscreen PNG render (visual verification)

```
godot-mono --path <project> --rendering-driver opengl3 --audio-driver Dummy \
  res://Tools/<Domain>/VerifyRender.tscn -- --out=/abs/path/out.png [harness-specific args]
```

See `rendering.md` for the full trap explanation and harness anatomy — the short version is:
no `--headless` here, ever, or the render is silently black/null.

## References

- [../SKILL.md](../SKILL.md) - Core Rules and workflow paths
- [rendering.md](rendering.md) - The dummy-driver trap and PNG harness anatomy
- [pitfalls.md](pitfalls.md) - Project lock, compile timeout, black-screenshot diagnosis

## Checklist

- [ ] `--import` was run first if the `.godot/` cache could be stale (fresh clone, new asset, cache wipe) — stale-cache failures masquerade as code bugs
- [ ] Every `--build-solutions` invocation includes `--quit` — without it the process can sit after the build finishes
- [ ] The E2E run passed `--settings Tests.E2E/.runsettings`, and its `GODOT_BIN` points at the intended `godot-mono` binary
- [ ] `MaxCpuCount` is still 1 in `.runsettings` — it is a correctness requirement, not a performance knob
- [ ] No command whose goal is an actual screenshot carries `--headless` (E2E's `--headless` is fine — SceneRunner tests never read pixels)

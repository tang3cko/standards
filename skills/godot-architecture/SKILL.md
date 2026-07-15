---
name: godot-architecture
description: Design Godot 4.x + .NET simulation architecture following Tang3cko standards — Godot-free Core layer, SimEvent pull feeds, external notify APIs, dt substep loops. Use when layering a simulation/game-logic system, deciding what belongs in Scripts/Core, or reviewing Godot simulation architecture.
when_to_use: Triggers on "Godot architecture", "SimEvent", "Godot-free", "シミュレーション設計", "dt", "サブステップ", "Scripts/Core", "座標レス", "リンク参照テスト", "Tick".
model: opus
effort: max
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Architecture

Help users design, implement, and review the layering of a Godot 4.x + .NET simulation — where the game-logic core lives, how it feeds the Godot layer, and how it survives large dt without silently under-executing.

This skill covers whole-simulation layering (Core vs Godot layer, event feed, tick loop). For individual Resource/EventChannel/interaction patterns inside the Godot layer, see `godot-core`. For the test project that proves the Core layer is decoupled, see `godot-testing`.

## Phase Tracking

**At workflow start (designing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load foundations    | godot-free-core.md"
TaskCreate: "Phase 2: Judge core shape    | coordinateless vs spatial decision"
TaskCreate: "Phase 3: Wire the event feed | sim-events.md + dt-substep.md"
TaskCreate: "Phase 4: Write/review code   | follow Core Rules"
TaskCreate: "Phase 5: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Design-question-only paths skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `references/godot-free-core.md` | Always |
| 2 | Apply the coordinateless-vs-spatial decision criterion | Designing |
| 3 | Read `references/sim-events.md` + `references/dt-substep.md` | Designing or reviewing the tick/event loop |
| 5 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

1. **Godot-free core, not coordinate-free core** — `Scripts/Core/` has zero Godot dependency (no `using Godot;`, no node/scene/Resource types), but that does not mean it must avoid coordinates. If patrol routes, line-of-sight, or pathing *are* the game logic, the core owns a deterministic 2D/3D spatial simulation (positions, bounds, vision, waypoints). If coordinates are purely a presentation detail (movement/nav delegated to the Godot layer), keep the core coordinateless. Judge by whether spatial reasoning is core game logic or rendering — never assume "no Godot" implies "no coordinates."
2. **External notify API for anything the Godot layer owns** — when movement/navigation lives in the Godot layer, the core never self-detects completion by interpolating state. It exposes narrow `NotifyX(id)` methods (e.g. `NotifyArrivedAtSeat`, `NotifyExited`) that the Godot layer calls once its own animation/nav finishes. The core drives state transitions from these calls, not from polling geometry it doesn't own.
3. **SimEvent pull-type feed** — the core never pushes into Godot objects or holds Godot references. It appends to an internal event list and exposes `DrainEvents()`; the Godot layer drains once per frame/tick and fans events out to animation, sound, and HUD. This is a batched, poll-driven feed — distinct from ReactiveSO's `EventChannel` (Resource-based Signal push, see `godot-core`), which is for Godot-side node-to-node decoupling, not Core→Godot delivery. See `references/sim-events.md` for the selection criteria.
4. **dt substep, never one-Tick-per-frame** — clamp per-call dt to a fixed step (e.g. 50ms) and loop `Tick(step)` in the Godot layer's `_Process`. Never assume the frame's raw `delta` maps to exactly one `Tick` call; speed multipliers and frame hitches otherwise cause silent under-execution. See `references/dt-substep.md`.
5. **Testability is the acceptance criterion** — this whole layering earns its keep only if `Scripts/Core/**/*.cs` link-references cleanly into `Tests/Tests.csproj` and `dotnet test` runs without the Godot runtime (see `godot-testing`). If a design can't be exercised that way, it isn't actually Godot-free.

## Workflow

Identify the task type and follow the matching path.

### Designing a new simulation

1. Load `references/godot-free-core.md` and judge coordinateless vs spatial for this game's core logic
2. Load `references/sim-events.md` and design the `SimEvent` enum + `DrainEvents()` shape, deciding what stays Godot-side `EventChannel` instead
3. Load `references/dt-substep.md` and place the clamp/loop in the Godot-layer driver, never inside Core
4. Generate code following the Core Rules above
5. Run the Self-Check below before presenting

### Reviewing simulation architecture

1. Load `references/godot-free-core.md`; confirm `Scripts/Core/` has no `using Godot;` and no node/scene references
2. Check the coordinateless-vs-spatial call was made deliberately (not accidentally coupled to presentation, not accidentally missing needed spatial logic)
3. Check the Tick loop for implicit "processes at most N items" assumptions that would under-execute under a large or multiplied dt (`references/dt-substep.md`)
4. Note issues with priority (must fix, should fix, nice to have)

### A layering question

1. Load `references/godot-free-core.md` for the decision criterion
2. Load `references/sim-events.md` and/or `references/dt-substep.md` as the question requires
3. Explain with the coordinateless (management sim) vs spatial (stealth guidance) contrast where useful

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] `Scripts/Core/` has zero Godot dependency — no `using Godot;`, no node/scene/Resource types anywhere in the linked files
- [ ] The coordinateless-vs-spatial choice was made deliberately, matching whether spatial reasoning is game logic or presentation
- [ ] Anything the Godot layer owns (movement, nav, animation) reaches the core only through a narrow `NotifyX(id)`-style API, never by the core polling geometry
- [ ] The core exposes state via `DrainEvents()` (pull), not by pushing into Godot objects or holding Godot references
- [ ] The Godot-layer driver clamps dt and loops `Tick(step)`; no code path assumes one `Tick` per frame
- [ ] `Scripts/Core/**/*.cs` link-references into `Tests/Tests.csproj` and `dotnet test` passes without the Godot runtime

## Reference Files

| File | Use When |
|------|----------|
| references/godot-free-core.md | Core layer definition, coordinateless-vs-spatial decision, external notify API, layer layout |
| references/sim-events.md | `SimEvent` pull-feed design, `DrainEvents()` pattern, SimEvent vs EventChannel selection |
| references/dt-substep.md | dt clamp+loop pattern, the "1 Tick = 1 item" under-execution pitfall |

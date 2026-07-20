# Godot-Free Core

The core layering decision for any Godot 4.x + .NET simulation: what lives in `Scripts/Core/` (pure C#, testable without the Godot runtime) versus what lives in the Godot layer (nodes, scenes, rendering, input).

## The principle, precisely

`Scripts/Core/` must never touch Godot: no `using Godot;`, no node/scene types, no `Resource`, no `[Export]`/`[Signal]`. That is the whole contract.

It is easy to over-read this as "the core must not hold coordinates." That reading is wrong. The contract is **"never touches Godot,"** not **"never touches coordinates."** Whether the core holds spatial state is a separate question, answered by what the game logic actually is — not by a blanket rule.

## Deciding: coordinateless vs spatial core

Ask: **is spatial reasoning itself the game logic, or is it a presentation detail of otherwise abstract logic?**

| | Coordinateless core | Spatial core |
|---|---|---|
| Example | A tavern/shop simulation: order queues, cooking, seating, pricing | A stealth-guidance game: patrol routes, line-of-sight, pathing |
| What the core owns | Customer state machine, queues, timers, pricing — never an `(x, y)` | Positions, wall bounds, vision cones, waypoint nav — a deterministic 2D/3D simulation |
| What the Godot layer owns | All movement/navigation; the core learns "arrived" only via an external notify call | Rendering the core's spatial state in 3D, feeding input in |
| Why | Movement is purely how arrival *looks*; the simulation doesn't care about the path taken | Whether a guard can *see* a target, or a visitor can *reach* an exit, **is** the mechanic being simulated |

If you delegate spatial reasoning to the Godot layer when it's actually gameplay-critical, you can't unit-test the mechanic (line-of-sight, pathing correctness) without spinning up the engine — the point of a Godot-free core is lost. If you duplicate coordinates into the core when they're pure presentation, you've coupled the core to camera/animation concerns for no benefit and every asset-replacement pass now risks touching `Scripts/Core/`.

Judge per-system, not per-project: a game can have a coordinateless economy core and a spatial combat core side by side.

## External notify API

When the Godot layer owns movement/navigation (coordinateless core case), the core must not self-detect completion by interpolating a position and checking a threshold — it doesn't have a position to interpolate. Instead it exposes narrow completion callbacks that the Godot layer calls once its own animation/nav finishes:

```csharp
// Scripts/Core/Simulation/LiveSimulation.cs — Godot-independent. Movement/geometry belongs
// entirely to the Godot layer; this class never holds coordinates. Instead of interpolating
// toward a target and self-detecting arrival, it exposes NotifyX for the shell to call once
// the corresponding travel animation finishes.
public void NotifyArrivedAtSeat(int customerId)  // WalkingToSeat -> Ordered
public void NotifyExited(int customerId)          // Leaving -> Left
```

The Godot layer calls these at the moment its own nav/animation completes:

```csharp
// Godot layer (e.g. Scripts/Tavern/TavernController.cs)
private void OnCustomerReachedSeat(int customerId)
{
    _sim.NotifyArrivedAtSeat(customerId);
}
```

Rules for this API shape:

- Name it for the event, not the mechanism (`NotifyArrivedAtSeat`, not `SetPosition`)
- It takes an ID and whatever data the transition needs — never a Godot type
- It's the *only* way the Godot layer pushes information back into the core (state reads flow the other way, via `DrainEvents()` — see `sim-events.md`)

A spatial core has no need for this pattern for movement itself (it computes arrival internally), but still uses the same shape for anything genuinely owned by the Godot layer — e.g. an input event like "player clicked this waypoint."

## Layer layout

| Layer | Location | Role |
|-------|----------|------|
| Core simulation | `Scripts/Core/` | Pure C#: state machine / spatial simulation, `Tick(dt)`, `DrainEvents()`, `NotifyX(...)` |
| Godot layer | `Scripts/<GameArea>/` (e.g. `Scripts/Tavern/`, `Scripts/Watch/`) | Nodes: rendering, nav, input, animation, sound, HUD. Drains events every frame, calls `NotifyX` on completion, drives the dt-substep loop |
| Tests | `Tests/Tests.csproj` | Link-references `Scripts/Core/**/*.cs`; `dotnet test` runs with zero Godot runtime (see `godot-testing`) |

## Payoff

A coordinateless (or correctly-scoped spatial) core stays untouched through unrelated churn — asset replacement, animation-pack swaps, camera rework — because none of that is core logic. A project that ported a 2D prototype to full 3D across multiple asset/animation-pack replacements recorded zero commits to `Scripts/Core/` across the entire process; every "no `Scripts/Core` change" note in that project's asset-migration plan is a direct consequence of this layering, not luck.

The discipline was also verified at full-portability scale in a production Godot project, 2026-07: a complete game core — board/simulation logic, session progression, scoring, and persistence models, roughly half the codebase — was re-ported 1:1 to a different engine with zero changes to any core file; the engine-side shell was the only rewrite. The receiving side of such a port must hold the same line — see the unity-architecture skill's "Core/Shell Boundary" (its `references/dependency-management.md`): the pure C# core must not depend on the ScriptableObject framework either.

## Checklist

When writing or reviewing a Core layer against this reference:

- [ ] No `using Godot;`, no node/scene/`Resource` types, no `[Export]`/`[Signal]` anywhere under `Scripts/Core/`
- [ ] The coordinateless-vs-spatial call was made per-system by asking "is spatial reasoning the mechanic, or how the mechanic looks?" — not by a blanket project-wide rule
- [ ] Every notify method is named for the event (`NotifyArrivedAtSeat`), takes an ID plus plain transition data, and never a Godot type
- [ ] Information flows one way each: Godot layer → core only via `NotifyX(...)`, core → Godot layer only via `DrainEvents()`
- [ ] `Scripts/Core/**/*.cs` link-references into `Tests/Tests.csproj` and `dotnet test` passes with no Godot runtime

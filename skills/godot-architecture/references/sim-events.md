# SimEvent Pull-Type Feed

How the Core layer reports what happened, without ever holding a Godot reference.

## The pattern

The core never pushes into Godot objects — it can't, it has no Godot types available. Instead it accumulates a list of plain-data events during `Tick()` and exposes a drain method that clears and returns them:

```csharp
namespace TinyCounter;

public enum SimEventKind
{
    CustomerSpawned,
    SeatAssigned,
    OrderPlaced,
    CookStarted,
    ServeCompleted,
    CustomerLeftAngry,
    CustomerLeft,
    QuestCompleted,
    ClosingStarted,
    DayEnded,
}

// Emitted by the core simulation and drained by the Godot layer each frame.
public sealed class SimEvent
{
    public SimEventKind Kind { get; init; }
    public int CustomerId { get; init; }
    public string ItemKey { get; init; }
    // ... one payload per Kind that needs it; not every field is meaningful for every Kind
}
```

```csharp
// Core: internal accumulation, public drain
private readonly List<SimEvent> _events = new();

public IReadOnlyList<SimEvent> DrainEvents()
{
    var drained = _events.ToArray();
    _events.Clear();
    return drained;
}
```

```csharp
// Godot layer: drain once per frame, fan out to whatever cares
public override void _Process(double delta)
{
    // ... run the dt-substep Tick loop (see dt-substep.md) ...
    foreach (var evt in _sim.DrainEvents())
    {
        switch (evt.Kind)
        {
            case SimEventKind.ServeCompleted:
                _soundDirector.PlayServe(evt.ItemKey);
                _hud.ShowServed(evt.CustomerId);
                break;
            // ...
        }
    }
}
```

A single `SimEvent` can fan out to animation, sound, and HUD simultaneously — the core doesn't know or care how many consumers there are, which is the point.

## SimEvent vs EventChannel — which to use

Both are decoupling mechanisms, but they solve different problems and live in different layers. Don't reach for one where the other fits.

| | `SimEvent` (this pattern) | `EventChannel` (Resource, see `godot-core`) |
|---|---|---|
| Direction | Core → Godot layer, one specific consumer (the driver node) that fans out manually | Godot node → Godot node(s), any number of subscribers via `Signal` |
| Delivery model | Pull: batched, drained once per frame/Tick | Push: `Raise()` fires immediately, subscribers react synchronously |
| Producer type | Plain C# class with zero Godot dependency | `Resource` (`.tres` asset), inherently Godot-typed |
| Ordering | Naturally batches everything since the last drain — fine for "N things happened this frame" | One event per `Raise()` call — fine for a single discrete moment |
| Use when | The producer is the Core layer and must stay Godot-free | Both producer and consumer already live in the Godot layer |

In short: if the emitting code is inside `Scripts/Core/`, it must be `SimEvent` — an `EventChannel` is a `Resource` and would violate the Godot-free constraint outright. If both sides are already Godot nodes, prefer `EventChannel` (or a direct node `Signal` per `godot-core`'s dependency priority ladder) over inventing a second drain-based feed inside the Godot layer.

## Design notes

- Keep `SimEvent` a single class with an enum `Kind` plus a superset of optional payload fields, not a class hierarchy — it stays trivially serializable and the Godot layer can `switch` on `Kind` without casting.
- `DrainEvents()` must clear on drain — a forgotten clear silently re-delivers stale events every subsequent frame.
- Because `SimEvent` and `DrainEvents()` are pure C#, they're exactly what a link-referenced xUnit test asserts on directly (`sim.Tick(...); Assert.Contains(events, e => e.Kind == SimEventKind.ServeCompleted)`) — see `godot-testing`.

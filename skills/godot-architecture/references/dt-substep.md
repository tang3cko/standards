# dt Substep

Why the Godot layer, never the core, owns the frame-to-Tick mapping, and the bug class this prevents.

## The pattern

Clamp the per-call dt to a fixed maximum step and loop, calling `Tick(step)` repeatedly until the frame's full `delta` is consumed:

```csharp
// Godot layer (e.g. Scripts/Tavern/TavernController.cs)
private const double MaxSimStepMs = 50.0;

public override void _Process(double delta)
{
    double remaining = delta * 1000.0; // seconds -> ms
    while (remaining > 0)
    {
        double step = Math.Min(remaining, MaxSimStepMs);
        _sim.Tick(step);
        remaining -= step;
    }
}
```

This loop lives entirely in the Godot layer. `Scripts/Core/`'s `Tick(dtMs)` itself needs no change — it just needs to be safe to call multiple times per frame with a bounded `dtMs`.

## The bug class this prevents

A `Tick(dt)` implementation that does at most one "big" thing per call — e.g. spawns at most one customer, advances at most one cook job — works fine as long as callers pass small, frame-sized dt values. The constraint is invisible until something passes a *large* dt in one call: a speed-multiplier UI (2x/4x), a frame hitch, a fast-forward/skip feature. When that happens, the excess dt is silently discarded rather than reprocessed, and the simulation quietly under-executes — e.g. customer spawns go missing at 4x speed with no error, no exception, just a wrong outcome.

This is exactly the kind of bug that "looks tested" but isn't: unit tests written against small dt values never exercise the multi-item-per-call path, so the bug ships and only surfaces once a speed-multiplier feature is bolted on later.

## Review checklist for this pattern

When designing or reviewing a `Tick(dtMs)` method:

- [ ] Does `Tick` ever return early after handling "one" spawn/job/transition, leaving remaining dt unprocessed?
- [ ] Is the clamp-and-loop applied in the Godot-layer driver (`_Process`), not inside `Tick` itself? (`Tick` should be dt-size-agnostic; the driver is what guarantees dt never exceeds the safe step)
- [ ] Does at least one test call `Tick` with a dt several multiples of the normal per-frame value and assert nothing was silently dropped compared to the equivalent number of small-step calls?
- [ ] Is the clamp value (`MaxSimStepMs` or equivalent) the same value the original design assumed for "one unit of simulated time" — not an arbitrary smaller/larger number chosen for the loop?

Because `Tick` is pure C#, this whole checklist is directly testable without the Godot runtime — see `godot-testing` for the link-reference setup that makes `dotnet test` the fast feedback loop for it.

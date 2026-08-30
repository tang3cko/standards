# Placing a System

Load this when placing any new system.

## The procedure

Answer in this order. Stop at the first answer that constrains the choice.

1. **Who is allowed to decide this?** Server-only → GameMode. Everyone must agree → GameState. One player owns it → PlayerState. See `replication.md`.
2. **How long must it live?** Level → `UWorldSubsystem`. Across travel → `UGameInstanceSubsystem`. One local player → `ULocalPlayerSubsystem`. Attached to something in the world → Component.
3. **Does it need to exist in the world?** Spawned, placed, or replicated → Actor. Otherwise not.
4. **Does it need a transform?** No → `UActorComponent`. Yes → `USceneComponent`. Yes and geometry → `UPrimitiveComponent`.
5. **Does it need reflection at all?** No → a plain struct or class, which is the cheapest thing to test.

"It was already in scope" is not an answer to any of these. That is the defect this procedure exists to prevent.

What each class *is* — Actor versus Component versus Subsystem, their transforms, their tick defaults — is stated once in `unreal-core/references/class-design.md`. This file is about which to reach for, and why the answer is so often the Component.

## Why the answer is so often the Component

An Actor is for something that exists in the world; its defining jobs are being placeable and replicating. Everything else is behaviour, and behaviour belongs in a Component — even behaviour you are confident will never be reused, for three reasons that have nothing to do with reuse:

- A Component is constructible and testable with far less ceremony than an Actor. That decides whether the logic is ever tested at all.
- A Component is the unit that Game Features attach at runtime. Behaviour already in a Component can later become modular; behaviour on the Actor cannot. See `modularity.md`.
- An Actor that accumulates unrelated responsibilities is this framework's default failure mode, and nothing warns you. The Component boundary is the only thing that makes the accumulation visible.

## Subsystem versus manager Actor versus singleton

Take the subsystem. Its lifetime is defined by the engine rather than by whoever calls first, it needs no spawning, and it survives nothing it should not.

The Programming Subsystems page (`dev.epicgames.com/documentation/en-us/unreal-engine/programming-subsystems-in-unreal-engine`) gives two reasons, both of which apply constantly: "Subsystems help you avoid adding more API on an already busy class" and "Subsystems help you avoid overriding engine classes."

A manager Actor is justified only when the thing genuinely must be in the world — placed by a designer, replicated, or visible. Otherwise it is a singleton with extra steps and a spawn order problem.

A singleton is not on the ladder. See `subsystems.md`.

## Tick is a decision, not a formality

The defaults and tick groups are in `class-design.md`. What belongs here is the choice: **prefer a delegate broadcast, a timer, or a replicated property change.** Polling is the fallback.

Reach for `TickInterval` before writing your own accumulator, and remember that logic split across an Actor and its Component runs in different tick groups by default — that is a design consequence, not a detail.

## Where the logic goes inside the placement

Whatever the placement, the decision-making belongs in something a test can construct: a plain struct with static functions, or a Component. The Actor or Subsystem translates decisions into engine calls.

When conditions start appearing in the translating layer, a decision has leaked down. See `core-shell.md`.

## Checklist

- [ ] Placement was derived from authority and lifetime, in that order, not from what was in scope
- [ ] Behaviour lives in a Component wherever it plausibly could
- [ ] No manager Actor exists for something that never needed to be in the world
- [ ] No singleton
- [ ] Tick is enabled only where needed, with `bCanEverTick` set in the constructor and the tick group chosen against physics needs
- [ ] Components are constructed in constructors rather than registered during play, unless there is a reason
- [ ] The decision-making layer is constructible by a test; the engine-facing layer holds no branching worth asserting on

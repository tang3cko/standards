# Architecture Overview

Load this at the start of any design or placement choice.

## The shape

Unreal's gameplay framework already is an architecture. It decides where authority lives, what replicates, what ticks and in what order, and what survives a level change. Designing against it means fitting into those decisions; designing around it means reimplementing them worse.

```
GameInstance          survives travel, not replicated, one per machine
  └ World
      ├ GameMode      server only — the rules
      ├ GameState     replicated to all — what everyone agrees on
      ├ PlayerState   replicated to all — what belongs to one player
      ├ PlayerController   server + owning client — intent
      │   └ Pawn / Character   the body, replicated, prediction included
      └ WorldSubsystems   services scoped to this level
```

Two axes decide placement: **authority** (who may decide) and **lifetime** (how long it lives). Take them in that order. See `placement.md`.

## What you get without writing it

Knowing this list is most of the value of the framework, and most of what gets needlessly rebuilt.

- **Character movement, prediction and correction.** `APawn` replicates by default; `UCharacterMovementComponent` carries prediction. Movement and animation need no project code.
- **Relevance and bandwidth budgeting.** `AActor::GetNetPriority` (`Runtime/Engine/Private/ActorReplication.cpp`) weights by distance and facing, and the priority-sorted list is truncated at the connection's budget. Distance changes update rate with no code from you.
- **Property replication with change callbacks** — `ReplicatedUsing`, rather than diffing snapshots.
- **Seamless travel** between maps, with a transition world.
- **Subsystems** with engine-defined lifetimes, instead of hand-rolled singletons and bootstrappers.
- **Reflection, serialization and GC** for anything that opts in with `UPROPERTY`.

Snapshot chunking, bandwidth budgeting and traffic measurement all have engine equivalents. Reach for the engine's version first and measure before replacing it.

## What you do not get

- **No engine-wide event bus.** Nothing by that name exists in an installed engine; the `EventChannel` types that are there belong to Sequencer. Notification is a multicast delegate the owner declares. See `communication.md`, including what is and is not checkable about the Lyra plugin usually cited as the counter-example.
- **No per-player pause.** Pause is global and replicated. See `replication.md`.
- **No enforced module boundary.** Engine-freeness is a convention you maintain, not something UBT checks. See `core-shell.md`.
- **No roster notification.** `AGameStateBase::PlayerArray` is not replicated (absent from `AGameStateBase::GetLifetimeReplicatedProps`) and announces nothing.

These are constraints, not gaps waiting to be filled. Design around them rather than going looking for the feature again.

## Two failure modes to watch for

**Everything on the Actor.** The Actor is already in scope, so the next feature goes there, and nothing objects until the class cannot be tested without a map. Push behaviour into Components and decisions into plain types.

**A structure imported from elsewhere.** An asset-mediated event channel, a reference graph wired in the level, a namespace around reflected types — each either has no Unreal form or is forbidden outright. Build from the framework's own pieces: delegates, subsystems, components, replication.

## Modularity, when it comes up

Start with one runtime module and split for a concrete reason. Horizontal feature isolation — a feature that ships and loads as a unit — is what Game Feature plugins are for, and that is the axis Lyra actually uses. See `modularity.md`.

## Checklist

- [ ] The design fits the framework's authority and lifetime decisions rather than reimplementing them
- [ ] Nothing on the "what you get free" list was rebuilt without a measurement justifying it
- [ ] Nothing on the "what you do not get" list is assumed to exist
- [ ] Behaviour is in Components and decisions in plain types, not accumulated on Actors
- [ ] No structure was imported from another engine without checking it has an Unreal form

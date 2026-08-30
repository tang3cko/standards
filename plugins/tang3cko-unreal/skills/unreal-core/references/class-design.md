# Class Design and Placement

Load this when choosing a class type or deciding where gameplay logic belongs.

Two questions decide placement, in this order. **Where does authority live?** — that fixes which framework class owns the logic. **What lifetime does this need?** — that fixes whether it is an Actor, a Component or a Subsystem. Convenience decides nothing.

## Authority map

| Class | Exists on | Lives for |
|---|---|---|
| `AGameModeBase` | **Server only.** Never instanced on a client | One level |
| `AGameStateBase` | Server and every client, fully replicated | One level |
| `APlayerState` | Server and every client, replicated to all | One player's session |
| `APlayerController` | The server, and the owning client. **Not** on other clients | Persists while the Pawn is transient |
| `APawn` / `ACharacter` | Wherever it is relevant, replicated | Possessed and unpossessed freely |
| `UGameInstance` | Server and each client **independently**. Not replicated | Engine launch to shutdown |

Rules of the game go in the GameMode, because only the server may decide them. State every machine must agree on goes in the GameState. Per-player state goes in the PlayerState — not in the Pawn, which can die, and not in the PlayerController, which does not exist on other clients.

Anything that must survive level travel goes in the GameInstance or a GameInstance subsystem.

`AGameStateBase::PlayerArray` (`Engine/Source/Runtime/Engine/Classes/GameFramework/GameStateBase.h`) carries no `Replicated` specifier of its own and no engine notification announces a change to it; each machine rebuilds it as `APlayerState` actors arrive. A roster UI needs its own notification.

## Subsystems

Five lifetimes, not the four the official Programming Subsystems page lists — see `official-doc-errata.md` #1 for the discrepancy against `Subsystem.h`'s own header comment.

| Base class | One instance per | Torn down when |
|---|---|---|
| `UEngineSubsystem` | Engine | Engine shuts down |
| `UEditorSubsystem` | Editor | Editor closes |
| `UGameInstanceSubsystem` | GameInstance | Game exits — **survives level travel** |
| `UWorldSubsystem` | World | The level is torn down |
| `ULocalPlayerSubsystem` | Local player | The player is removed |

Prefer a subsystem to a manager Actor or a singleton. They exist precisely to avoid bolting more API onto an already busy class and to avoid subclassing engine classes, and unlike a singleton their lifetime is defined by the engine rather than by whoever calls first.

- `Initialize(FSubsystemCollectionBase&)` and `Deinitialize()` (`USubsystem`, `Engine/Source/Runtime/Engine/Public/Subsystems/Subsystem.h`) are the entry points. Call `FSubsystemCollectionBase::InitializeDependency` **only** from `Initialize`, and only to order construction.
- `USubsystem::ShouldCreateSubsystem(UObject* Outer)` gates creation — the natural place for "server only". It is called on the CDO before any instance exists. **If it can return false, every call site must null-check the result.**
- `UWorldSubsystem::GetWorld()` is declared `override final`. By default world subsystems support Game, Editor and PIE worlds, but not preview worlds; override `UWorldSubsystem::DoesSupportWorldType` to change that.
- `UTickableWorldSubsystem` requires subclasses to forward `Initialize`/`Deinitialize` to `Super`, or ticking never enables — `UTickableWorldSubsystem::IsAllowedToTick` ensures with "Check for missing Super call", and `UTickableWorldSubsystem::BeginDestroy` with "Check for missing Super::Deinitialize call".
- Separately, `UWorldSubsystem::PostInitialize` and `OnWorldBeginPlay` ensure against being called twice. That is a different guard from the one above, and does not concern ticking.
- A subsystem in a module nobody loads is never created. If instances are missing, check that the module is loaded before blaming the subsystem.

## Actors and Components

An Actor is for something that exists in the world: it can be spawned, placed, and replicated. Replication is one of its two defining jobs, and `AActor::bReplicates` (`Engine/Source/Runtime/Engine/Classes/GameFramework/Actor.h`) defaults to **false** — set in the `AActor` constructor in `Actor.cpp`.

A `UActorComponent` is for reusable behaviour with no geometry — movement, inventory, attributes. It has no transform. A `USceneComponent` adds one. A `UPrimitiveComponent` adds geometry.

Push behaviour down into Components. The reasons to prefer them, and how to decide, are in `unreal-architecture/references/placement.md`; what matters here is the mechanics.

Registering a Component during play costs something. Prefer constructing them in the constructor.

The Actor Lifecycle comment block at the top of `Runtime/Engine/Classes/GameFramework/Actor.h` — not the one-line comment above the `BeginPlay` declaration further down — states that it "normally happens right after PostInitializeComponents but can be delayed for networked or child actors". Do not assume ordering across actors.

## Tick

Tick is off by default and turning it on is a decision, not a formality.

- `PrimaryActorTick.bCanEverTick` and `PrimaryComponentTick.bCanEverTick` both default to `false`. `SetActorTickEnabled(true)` does nothing unless `bCanEverTick` was set in the constructor — it is a silent no-op, not an error.
- Actors default to `TG_PrePhysics`; Components default to `TG_DuringPhysics`. Logic split across both runs in different groups and sees different physics state.
- `TG_PrePhysics` sees physics data one frame old. `TG_DuringPhysics` cannot know whether it is seeing this frame or the last. `TG_PostPhysics` sees this frame's results.
- `TickInterval` throttles without disabling. Use it before writing your own accumulator.

Prefer an event, a timer, or a replicated property change over polling in Tick.

## `GetWorld()` returns null more often than you think

`UObject::GetWorld()` walks the Outer chain and returns null when that chain does not reach a World — so a plain `UObject` created with no meaningful Outer has no world. `AActor::GetWorld()` (`Engine/Source/Runtime/Engine/Private/Actor.cpp`) returns null in four cases: the object is a class default object, its Outer is gone, its Outer is unreachable, or it has no Level.

For a subsystem or a Slate-side object, take the world from the object that owns you rather than asking a global — a `UObject` far from the world context (an editor utility, a Slate widget's backing object) is exactly where this null shows up in practice.

## USTRUCT

Use a USTRUCT for data with no identity and no lifetime. It is faster to create than an equivalent UObject, supports `UPROPERTY`, and is reflected. It cannot have `UFUNCTION`s and is **not managed by the garbage collector** — a UObject pointer inside a USTRUCT still needs `UPROPERTY` to be seen.

The struct itself is not considered for replication; its `UPROPERTY` members are.

When interactions get complicated, that is the signal to become a UObject or an Actor.

## Interfaces

The U/I pair mechanics — what the `UINTERFACE` macro requires, what the `I`-prefixed class may and may not contain, and the Blueprint-exposure rule — live in `reflection-macros.md`. The placement question this file answers: an interface is how you let unrelated classes share a contract without forcing them into the same branch of the Class Selection Ladder, so reach for one before adding a soft `Cast<T>`-and-check chain against a growing list of concrete classes.

## Checklist

- [ ] Server-only rules live in the GameMode; shared state in the GameState; per-player state in the PlayerState
- [ ] Nothing assumes a PlayerController exists for a remote player, or that the GameInstance is replicated
- [ ] Anything crossing level travel lives in the GameInstance or a GameInstance subsystem
- [ ] A subsystem was preferred over a manager Actor or a singleton, with the lifetime chosen deliberately from the five
- [ ] Every `ShouldCreateSubsystem` that can return false has null-checked call sites
- [ ] Reusable behaviour is in a Component, not accumulated on the Actor
- [ ] Tick is enabled only where needed, with `bCanEverTick` set in the constructor and the tick group chosen against physics needs
- [ ] `GetWorld()` results are null-checked, and no unwarranted world-context assumption was made
- [ ] USTRUCTs hold only simple data, and any UObject pointer inside one is a `UPROPERTY`
- [ ] Interfaces follow the U/I pair rules in `reflection-macros.md`

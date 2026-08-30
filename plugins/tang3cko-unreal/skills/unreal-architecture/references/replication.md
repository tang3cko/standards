# Replication and Authority

Load this whenever anything crosses a machine boundary.

Start every networked design by answering two questions in writing: **which machine is authoritative** and **what happens when the two disagree**. Most networking defects are a missing answer to the second.

## Where things exist

| Class | Server | Owning client | Other clients |
|---|---|---|---|
| `AGameModeBase` | yes | **no** | **no** |
| `AGameStateBase` | yes | yes | yes |
| `APlayerState` | yes | yes | yes |
| `APlayerController` | yes | yes | **no** |
| `UGameInstance` | yes, independently | yes, independently | yes, independently — **not replicated** |

Rules go in the GameMode because only the server may decide them. Agreed state goes in the GameState. Per-player state goes in the PlayerState — not the Pawn, which dies, and not the PlayerController, which other clients cannot see.

Anything that must survive level travel goes in the GameInstance or a GameInstance subsystem, and is maintained independently on each machine.

## What replicates and what does not

- `AActor::bReplicates` defaults to **false**, set in the constructor initializer (`Runtime/Engine/Private/Actor.cpp`).
- A `UPROPERTY(Replicated)` needs `GetLifetimeReplicatedProps`. `ReplicatedUsing=OnRep_X` adds the callback, which is where UI should hang rather than on a poll.
- **A USTRUCT is not considered for replication; its `UPROPERTY` members are.** The Structs page states this directly, and the same page carries a separate wording problem — see `unreal-core/references/official-doc-errata.md`.
- **Delegates never replicate.** `FMulticastDelegateProperty::NetSerializeItem` and its single-cast counterpart `FDelegateProperty::NetSerializeItem` (`Runtime/CoreUObject/Private/UObject/PropertyMulticastDelegate.cpp` and `PropertyDelegate.cpp`) both refuse and return 1, on security grounds. The official multicast page says otherwise and is wrong — see `unreal-core/references/official-doc-errata.md`.
- **`AGameStateBase::PlayerArray` is not replicated.** `AGameStateBase::GetLifetimeReplicatedProps` (`Runtime/Engine/Private/GameStateBase.cpp`) does not register it, and the engine announces no change to it. Each machine rebuilds it as PlayerState actors arrive. A roster UI needs its own notification.
- A UObject subobject replicates only when registered as a replicated subobject of an Actor.

## RPCs

`Server` runs on the server, `Client` on the owning client, `NetMulticast` everywhere. `Reliable`/`Unreliable` are only valid alongside one of those.

**A plain `AActor` with no `Owner` cannot send a server RPC — the call is silently absorbed.** `AActor::HasNetOwner` (`Runtime/Engine/Private/Actor.cpp`) states this directly in its early-out: "all basic AActors are unable to call RPCs without special AActors as their owners (ie APlayerController)". This costs an afternoon the first time. Route through the PlayerController, or give the Actor an owner.

`WithValidation` adds `_Validate`, returning whether the call may proceed. Every RPC a client can send needs one; it is the only place a malicious or broken client is stopped.

## Pause is global — there is no per-player form

`AWorldSettings::PauserPlayerState` is replicated via `DOREPLIFETIME` in `AWorldSettings::GetLifetimeReplicatedProps` (`Runtime/Engine/Private/WorldSettings.cpp`), and `UWorld::IsPaused` is a local read of that replicated state. A host pausing stops the world for everyone. A client cannot pause at all in a packaged build — **but can in a development build**, which is the kind of difference that surfaces after shipping.

For an online game, do not build a feature on pause. If one player needs the game to stop responding to them, suppress *their input* and leave the world running — a CommonUI input config does this — and record it as a decision rather than a workaround.

## What the engine gives you for free

Do not rebuild these:

- `APawn` replicates by default, and `UCharacterMovementComponent` carries prediction and correction. Character movement and animation need no project code.
- **Relevance and bandwidth budgeting**, which is two mechanisms in two files. `AActor::GetNetPriority` (`Runtime/Engine/Private/ActorReplication.cpp`) weights by distance and facing; `UNetDriver::ServerReplicateActors` (`Runtime/Engine/Private/NetDriver.cpp`) is what sorts by that priority and stops when the connection's budget runs out. Together they mean distance changes update rate for players as well as AI, with no code from you.
- A server-driven pawn reads as smooth without any custom prediction; the measured drift is smaller than the character being smoothed.

## Travel

The travel that creates the server must be **hard**; every travel after it should be **seamless**. The `"listen"` URL option is read in several places, including `UWorld::AttemptDeriveFromURL`, but the engine's `Listen()` call that actually opens the port is made only from `UEngine::LoadMap`, which the seamless path never reaches. Hard travel disconnects clients by design.

Seamless travel needs `UGameMapsSettings::TransitionMap` (`Runtime/EngineSettings/Classes/GameMapsSettings.h`) configured, or `FSeamlessTravelHandler::StartTravel` (`Runtime/Engine/Private/World.cpp`) substitutes a bare world when the configured map resolves empty — the black frame between maps.

A widget is **not** destroyed by a seamless travel. A menu left open crosses into the next map along with whatever input state it set. Design for the crossing rather than assuming a clean slate.

A map not listed in `UProjectPackagingSettings::MapsToCook` (`Developer/DeveloperToolSettings/Classes/Settings/ProjectPackagingSettings.h`) is not cooked, and the failure appears only in a packaged build.

## Editor-only faults

Some networking faults exist only in PIE, because PIE has several worlds and a packaged build has one. A recurring case: an online services instance must carry the world context handle, or PIE cannot host, while a packaged build resolves regardless. Test both.

## Checklist

- [ ] Authority is stated in writing for every replicated system, along with what happens on disagreement
- [ ] Rules are in the GameMode, agreed state in the GameState, per-player state in the PlayerState
- [ ] Nothing on a client path reads the GameMode, or a PlayerController belonging to another player
- [ ] `bReplicates` is set where needed, and `GetLifetimeReplicatedProps` covers every replicated property
- [ ] No design expects a delegate to replicate, a USTRUCT to replicate as a unit, or `PlayerArray` to announce itself
- [ ] Every client-sendable RPC has `WithValidation`, and its sender has an owner
- [ ] Nothing depends on pause, and no per-player stop was implemented as a pause
- [ ] Movement, prediction and relevance were left to the engine rather than reimplemented
- [ ] The first travel is hard, later travels are seamless, `TransitionMap` is set, and UI crossing a travel is accounted for
- [ ] The feature was exercised in PIE and in a packaged build

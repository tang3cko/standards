# Subsystems

A subsystem is an auto-instanced UObject whose lifetime is tied to an engine construct. It is the answer to "this needs to be reachable from anywhere" that does not require a singleton, a manager Actor, or a reference chain.

This file is about **which subsystem to choose and why one at all**. The mechanics of writing one — the `Initialize`/`Deinitialize` contract, `ShouldCreateSubsystem` null-checking, world-subsystem specifics — are stated once in `unreal-core/references/class-design.md`, and repeated below only where the architectural choice depends on them.

## The five lifetimes

The official documentation lists four. **There are five** — the World row is missing from the page. See `unreal-core/references/official-doc-errata.md`.

| Base class | One per | Created | Destroyed |
|---|---|---|---|
| `UEngineSubsystem` | Engine | After its module's `StartupModule` returns | Engine shutdown |
| `UEditorSubsystem` | Editor | Editor start | Editor close |
| `UGameInstanceSubsystem` | GameInstance | GameInstance creation | Game exit — **survives travel** |
| `UWorldSubsystem` | World | World creation | Level teardown |
| `ULocalPlayerSubsystem` | Local player | Player added | Player removed |

`Subsystems/Subsystem.h` carries the canonical list in its header comment above `USubsystem`, naming all five lifetimes and their base classes.

Pick by what the state must outlive. Anything crossing a level travel is a GameInstance subsystem; anything that must die with the level is a World subsystem. Getting this wrong produces state that persists into a session it does not belong to, which reads as a haunting rather than a bug.

## The two choices that are architectural

Writing a subsystem is covered in `unreal-core/references/class-design.md` — the `Initialize`/`Deinitialize` contract, getting one, world-subsystem specifics, `UTickableWorldSubsystem`'s forwarding requirement. Two of those decisions are design decisions rather than mechanics, and belong here.

**`ShouldCreateSubsystem` is where a subsystem declines to exist.** Server-only, or not-in-this-world-type, is expressed by returning false rather than by making the subsystem tolerate a context it was never meant for. The cost is that **every call site must then null-check**, which is a real cost — decide deliberately rather than reaching for it by reflex.

**`InitializeDependency` is the only ordering tool, and it only orders construction.** It may be called from `Initialize` and nowhere else. If two subsystems need each other's *state* rather than each other's existence, that is a design problem the collection cannot solve for you: give one of them a delegate the other binds to, or move the shared state out of both.

## Why not a singleton

The classic C++ singleton has no defined teardown, initializes on first use in an order nobody chose, survives level travel whether or not that was wanted, and cannot be excluded from a test world. Every one of those is fixed by a subsystem.

A manager Actor is justified only when the thing must genuinely be in the world. Otherwise it adds a spawn order problem and a `GetWorld()` dependency to something that needed neither.

## When a subsystem does not appear

Almost always the module was never loaded. A subsystem in a module nobody loads is never created — the `UDynamicSubsystem` comment in `Subsystem.h` says so directly: "If instances of your subsystem aren't being created it maybe that the module they are in isn't being explicitly loaded". Check `LoadModule` before suspecting the subsystem.

Within one `LoadingPhase`, module load order is not deterministic. If one must precede another, call `LoadModule`/`LoadModuleChecked` explicitly rather than relying on the phase.

## Checklist

- [ ] The lifetime chosen matches what the state must outlive, from all five options
- [ ] `ShouldCreateSubsystem` returning false was a deliberate decision, and every call site null-checks
- [ ] `InitializeDependency` is used only to order construction, and no design depends on it to order state
- [ ] No singleton, and no manager Actor for something that never needed to be in the world
- [ ] A missing subsystem was diagnosed as a module-loading question before anything else
- [ ] The mechanics checklist in `unreal-core/references/class-design.md` was run too — this file deliberately does not repeat it

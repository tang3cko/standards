---
name: unreal-architecture
description: Design Unreal Engine systems following Tang3cko standards — where logic lives, how systems communicate, Core/Shell boundaries, replication and authority. Use when designing system communication, placing a new system, or reviewing architecture.
when_to_use: Triggers on "アーキテクチャ", "設計", "依存", "Subsystem", "Component", "GameMode", "replication", "レプリケーション", "delegate", "Game Feature", "architecture", "dependency".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unreal Architecture

Tang3cko's Unreal system architecture: logic is placed by authority and lifetime, systems talk through delegates and replicated state rather than direct references, and engine-facing shells stay thin. The Core Rules below are always in force; the decision tables and acceptance criteria live in references/.

The boundary broken most often is Core Rule 5: logic that accumulates on an Actor because the Actor was already there. Nothing objects — not the compiler, not the engine, not a running game — until the class is a thousand lines and only testable by loading a map.

The building blocks are **multicast delegates, subsystems and replicated state**. The multicast delegate is Unreal's event mechanism: the owner declares and broadcasts, subscribers bind. There is no engine-wide event bus beyond that, so a design assuming an asset-mediated channel is assuming something that is not there. See `communication.md` for what is and is not checkable about the plugin usually cited as the counter-example.

## Phase Tracking

**At workflow start (designing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load foundations    | architecture.md + placement.md"
TaskCreate: "Phase 2: Place the system    | Placement Guide + load its reference"
TaskCreate: "Phase 3: Wire communication  | communication.md + replication.md; authority and notification decided in writing"
TaskCreate: "Phase 4: Write/review code   | follow Core Rules"
TaskCreate: "Phase 5: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Placement questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `architecture.md` + `placement.md` | Always |
| 2 | Read the selected placement's reference | Designing |
| 3 | Read `communication.md` + `replication.md` | Any design where two systems or two machines must agree |
| 5 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

1. **Authority decides placement before convenience does** — server-only rules in the GameMode, shared state in the GameState, per-player state in the PlayerState. Choosing a class because it was already in scope is the defect
2. **Lifetime decides the class** — Component for reusable behaviour, WorldSubsystem for per-level services, GameInstanceSubsystem for anything crossing travel. Never a singleton
3. **Communicate through delegates and replicated state** — not through direct references between systems, and not by walking `GetGameMode()` from a widget
4. **Delegates never cross the network** — the engine refuses to replicate them. Anything a remote machine must learn goes through a replicated property or an RPC
5. **The Actor is a shell.** Rules live in plain types or Components that a test can construct; the Actor wires them together and does not decide

## Workflow

Identify the task type and follow the matching path.

### Designing a new system

1. Load `architecture.md` + `placement.md` for foundational context
2. Place the system with the Placement Guide below and load its reference
3. Load `communication.md` (same-machine notification) and `replication.md` (anything a remote machine must learn); load `core-shell.md` when the system carries rules worth testing
4. Generate code following the placement
5. Run the Self-Check below before presenting

### Reviewing architecture

1. Load `placement.md`, `replication.md`, and `core-shell.md`
2. Check authority, lifetime, dependency direction, and what the design assumes about the network, against each loaded reference's `## Checklist`
3. Note issues with priority (must fix, should fix, nice to have)

### Choosing a placement

1. Load `architecture.md` for the inventory of what the engine already provides
2. Load the candidate placements' references and explain trade-offs with code examples

## Placement Guide

Load `architecture.md` + `placement.md` first for foundational context, then place the system by what it needs:

| Need | Placement | Reference |
|------|-----------|-----------|
| Rules only the server may decide | GameMode | references/placement.md |
| State every machine must agree on | GameState | references/replication.md |
| State belonging to one player | PlayerState | references/replication.md |
| Reusable behaviour on something in the world | UActorComponent | references/placement.md |
| A service scoped to one level | UWorldSubsystem | references/subsystems.md |
| A service surviving level travel | UGameInstanceSubsystem | references/subsystems.md |
| A service scoped to one local player | ULocalPlayerSubsystem | references/subsystems.md |
| Notification between systems on one machine | Multicast delegate | references/communication.md |
| Notification across machines | Replicated property or RPC | references/replication.md |
| A feature that ships and loads as a unit | Game Feature plugin | references/modularity.md |

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] The reference files named by the chosen workflow path were actually Read this session. SKILL.md is only the index — the acceptance criteria live in references/, and several items below cannot be verified from this file alone
- [ ] The `## Checklist` at the end of every reference loaded this session was run item-by-item against the code you wrote or reviewed — those checklists are the per-placement acceptance criteria this Self-Check delegates to
- [ ] Placement follows authority first and lifetime second, not what was already in scope
- [ ] No singleton, and no manager Actor where a subsystem fits
- [ ] Cross-system communication goes through delegates or replicated state, not direct references
- [ ] Nothing assumes a delegate replicates, that a client can pause, or that a PlayerController exists for a remote player
- [ ] The Actor layer contains no decision worth asserting on
- [ ] Every replicated design states which machine is authoritative and what happens when the two disagree

## Reference Files

Each row lists the trigger for loading and what exists only in that file — skipping a triggered file means designing without its acceptance criteria.

| File | Load When |
|------|-----------|
| references/architecture.md | Starting any design or placement choice. Only place with the overall shape, the inventory of what Unreal gives you without writing it, and what it does not give you |
| references/placement.md | Placing any system, or reviewing where one landed. Only place with the authority-before-convenience procedure, the Actor/Component split criteria, and the Tick decision |
| references/subsystems.md | Placing a service on any subsystem rung. Only place with the five lifetimes, `ShouldCreateSubsystem` gating, `InitializeDependency` for construction order, world-subsystem specifics, and the subsystem-versus-manager-Actor argument |
| references/communication.md | Wiring notification between systems on one machine. Only place with delegate type selection, binding lifetime rules, and the evidence that there is no engine-wide event bus |
| references/replication.md | Any design where a remote machine must learn something, or any review of a networked system. Only place with the authority map, what replicates and what does not, RPC ownership requirements, pause being global, and travel rules |
| references/core-shell.md | Any system that carries rules worth testing, or any claim that a module boundary is enforced. Only place with what UBT does and does not enforce, the two traps that break the boundary, and why Lyra is not evidence for the pattern |
| references/modularity.md | Deciding between a module, a plugin, and a Game Feature. Only place with the split criteria and Game Feature component injection |

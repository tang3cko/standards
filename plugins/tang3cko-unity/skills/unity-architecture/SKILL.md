---
name: unity-architecture
description: Design Unity systems using ScriptableObject architecture following Tang3cko standards — EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO patterns. Use when designing system communication, choosing an SO pattern, or reviewing architecture.
when_to_use: Triggers on "アーキテクチャ", "EventChannel", "RuntimeSet", "Variable", "ActionSO", "ReactiveSO", "依存", "設計", "ScriptableObject", "SO", "architecture", "dependency".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Architecture

Tang3cko's Unity system architecture: systems are wired through ScriptableObject assets —
EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO — not through scene references. The Core
Rules below are always in force; the decision tables and acceptance criteria live in references/.

The boundary broken most often is Core Rule 5: a pure C# core that takes a ReactiveSO dependency.
The core exposes plain `event Action`; only the MonoBehaviour shell touches EventChannelSO.

## Phase Tracking

**At workflow start (designing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load foundations    | architecture.md + design-principles.md"
TaskCreate: "Phase 2: Pick SO pattern     | Pattern Selection Guide + load its reference"
TaskCreate: "Phase 3: Wire dependencies   | dependency-management.md"
TaskCreate: "Phase 4: Write/review code   | follow Core Rules"
TaskCreate: "Phase 5: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Pattern-choice questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `architecture.md` + `design-principles.md` | Always |
| 2 | Read the selected pattern's reference | Designing |
| 5 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

1. **ScriptableObject as architecture** — EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO are the building blocks; systems are wired through SO assets, not scene references
2. **Event-driven decoupling** — cross-system communication goes through SO channels, never direct references between systems
3. **Dependency priority** — EventChannel > SerializeField > Find/GetComponent > Singleton; each step down needs justification, Singleton is last resort
4. **Naming** — ScriptableObjects end in `SO`, event channels in `EventChannelSO` (see unity-core skill for the full naming contract)
5. **EventChannels are shell-layer wiring — never a dependency of the pure C# core.** An engine-agnostic core exposes plain C# `event Action`; the MonoBehaviour shell subscribes and relays into EventChannelSO. See `references/dependency-management.md` Core/Shell Boundary for the rationale and asmdef enforcement

## Workflow

Identify the task type and follow the matching path.

### Designing a new system

1. Load `architecture.md` + `design-principles.md` for foundational context
2. Pick the SO pattern from the Pattern Selection Guide below and load its reference
3. Load `dependency-management.md` for DI wiring
4. Generate code following the selected pattern
5. Run the Self-Check below before presenting

### Reviewing architecture

1. Load `design-principles.md` for the three pillars (Observability, Asset-based DI, DOD)
2. Check dependency direction and coupling
3. Verify SO pattern usage follows conventions
4. Note issues with priority (must fix, should fix, nice to have)

### Choosing a pattern

1. Load `architecture.md` for the system communication overview
2. Load candidate pattern references and explain trade-offs with code examples

## Pattern Selection Guide

Load `architecture.md` + `design-principles.md` first for foundational context, then pick a pattern:

| Need | Pattern | Reference |
|------|---------|-----------|
| Fire-and-forget messaging | EventChannel | references/event-channels.md |
| Track active objects in scene | RuntimeSet | references/runtime-sets.md |
| Share state with change notification | Variable | references/variables.md |
| Configurable commands / abilities | ActionSO | references/actions.md |
| Per-entity reactive state + Jobs | ReactiveSO | references/reactive-entity-sets.md |

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] The reference files named by the chosen workflow path were actually Read this session. SKILL.md is only the index — the acceptance criteria live in references/, and several items below cannot be verified from this file alone
- [ ] The `## Checklist` at the end of every reference loaded this session was run against the code you wrote or reviewed — those checklists are the per-pattern acceptance criteria this Self-Check delegates to
- [ ] Cross-system communication uses SO channels — no direct system-to-system references, no new Singletons
- [ ] Pure C# core code has no ReactiveSO dependency — shell relays core events into EventChannelSO (Core Rule 5)
- [ ] The chosen SO pattern matches the Pattern Selection Guide row for the need
- [ ] Dependencies sit on the highest viable rung of the Dependency Priority ladder
- [ ] SO assets follow naming (`SO` / `EventChannelSO` suffixes) and live in the conventional directories
- [ ] Event subscriptions are paired (`OnEnable` subscribe / `OnDisable` unsubscribe)

## Reference Files

Each row lists the trigger for loading and what exists only in that file — skipping a triggered file means designing without its acceptance criteria.

| File | Load When |
|------|-----------|
| references/architecture.md | Starting any design or pattern choice. Only place with the full decision tree, the approach-comparison table, and the four Singleton acceptance conditions |
| references/design-principles.md | Designing or reviewing any system. Only place defining the three pillars (Observability model, Asset-based DI, DOD) and the design acceptance checklist reviews are judged against |
| references/dependency-management.md | Wiring any dependency. Only place with the priority-ladder rationale, the SerializeField-with-Find-fallback and Singleton implementations, and the Core/Shell asmdef boundary rule |
| references/event-channels.md | Implementing pub/sub. Only place listing built-in channel types, the custom `EventChannelSO<T>` recipe, CreateAssetMenu path conventions, and Event Monitor debugging |
| references/runtime-sets.md | Tracking active objects. Only place with the self-registration lifecycle, custom query-set example, and the RuntimeSet-vs-ReactiveEntitySet decision table |
| references/variables.md | Sharing state with change notification. Only place with the Publisher Contract (Variable + EventChannel co-existence), change-detection semantics, and GPU sync rules |
| references/actions.md | Building data-driven commands. Only place with `ActionSO`/`ActionSO<T>` override signatures, the `#if UNITY_EDITOR` monitoring guard, and Sequence/Conditional composites |
| references/reactive-entity-sets.md | Per-entity state or Job System work. Only place with the RES API, struct field-only rules, double-buffer Orchestrator lifecycle, and snapshot/time-travel patterns |

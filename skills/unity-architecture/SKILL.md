---
name: unity-architecture
description: Design Unity systems using ScriptableObject architecture following Tang3cko standards — EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO patterns. Use when designing system communication, choosing an SO pattern, or reviewing architecture.
when_to_use: Triggers on "アーキテクチャ", "EventChannel", "RuntimeSet", "Variable", "ActionSO", "ReactiveSO", "依存", "設計", "ScriptableObject", "SO", "architecture", "dependency".
model: opus
effort: max
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Architecture

Help users design, implement, and review Unity system architecture using ScriptableObject-based patterns following Tang3cko standards.

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

- [ ] Cross-system communication uses SO channels — no direct system-to-system references, no new Singletons
- [ ] The chosen SO pattern matches the Pattern Selection Guide row for the need
- [ ] Dependencies sit on the highest viable rung of the Dependency Priority ladder
- [ ] SO assets follow naming (`SO` / `EventChannelSO` suffixes) and live in the conventional directories
- [ ] Event subscriptions are paired (`OnEnable` subscribe / `OnDisable` unsubscribe)

## Reference Files

| File | Use When |
|------|----------|
| references/architecture.md | System communication, dependency patterns |
| references/design-principles.md | Three pillars: Observability, Asset-based DI, DOD |
| references/dependency-management.md | DI priority: EventChannel > SerializeField > Find > Singleton |
| references/event-channels.md | Pub/sub implementation |
| references/runtime-sets.md | Object tracking |
| references/variables.md | Shared state with change events |
| references/actions.md | Data-driven commands (ActionSO) |
| references/reactive-entity-sets.md | Per-entity reactive state, Job System integration |

---
name: unity-architecture
description: Design Unity systems using ScriptableObject architecture following Tang3cko standards. EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO patterns. Use when designing system communication or reviewing architecture. Triggers on "アーキテクチャ", "EventChannel", "RuntimeSet", "Variable", "ActionSO", "ReactiveSO", "依存", "設計", "ScriptableObject", "SO", "architecture", "dependency".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Architecture

Help users design, implement, and review Unity system architecture using ScriptableObject-based patterns following Tang3cko standards.

## Core Principles

1. **ScriptableObject as architecture** - EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO patterns
2. **Event-driven decoupling** - Systems communicate through SO channels, never direct references
3. **Dependency priority** - EventChannel > SerializeField > Find > Singleton (avoid Singleton)

## When Invoked

### Step 1: Determine Task Type

- **Designing a new system?** → Go to Step 2a
- **Reviewing architecture?** → Go to Step 2b
- **Choosing a pattern?** → Go to Step 2c

### Step 2a: Designing a New System

1. Load `architecture.md` and `design-principles.md` for foundational context
2. Identify which SO pattern fits the use case:
   - System-to-system messaging → `event-channels.md`
   - Tracking live objects → `runtime-sets.md`
   - Shared state with change events → `variables.md`
   - Data-driven commands → `actions.md`
   - Per-entity reactive state → `reactive-entity-sets.md`
3. Load `dependency-management.md` for DI patterns
4. Generate code following the selected pattern

### Step 2b: Reviewing Architecture

1. Load `design-principles.md` for the three pillars (Observability, Asset-based DI, DOD)
2. Check dependency direction and coupling
3. Verify SO pattern usage follows conventions
4. Note issues with priority (P1/P2/P3)

### Step 2c: Choosing a Pattern

1. Load `architecture.md` for the system communication overview
2. Load candidate pattern files based on the question
3. Explain trade-offs with code examples

## Pattern Selection Guide

| Need | Pattern | Reference |
|------|---------|-----------|
| Fire-and-forget messaging | EventChannel | references/event-channels.md |
| Track active objects in scene | RuntimeSet | references/runtime-sets.md |
| Share state with change notification | Variable | references/variables.md |
| Configurable commands / abilities | ActionSO | references/actions.md |
| Per-entity reactive state + Jobs | ReactiveSO | references/reactive-entity-sets.md |

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

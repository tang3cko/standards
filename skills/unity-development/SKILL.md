---
name: unity-development
description: Write Unity C# code following Tang3cko standards. Architecture, naming, testing, UI patterns. Use when writing or reviewing Unity code. Triggers on "Unity", "C#", "アーキテクチャ", "EventChannel", "RuntimeSet", "テスト", "UI Toolkit", "ScriptableObject", "SO", "NUnit", "uGUI", "UniTask", "Humble Object", "ReactiveSO".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Development Skill

Help users write, review, and refactor Unity C# code following Tang3cko standards.

## Core Principles

1. **ScriptableObject architecture** - EventChannel, RuntimeSet, Variable patterns
2. **Testability first** - Humble Object pattern, Edit Mode tests preferred
3. **Event-driven** - Decouple systems, avoid Singleton

## When Invoked

### Step 1: Determine Task Type

- **Writing new code?** → Go to Step 2a
- **Reviewing existing code?** → Go to Step 2b
- **Architecture question?** → Go to Step 2c
- **Testing question?** → Go to Step 2d

### Step 2a: Writing New Code

1. `_core-rules.md` is auto-loaded with naming and structure rules
2. Load architecture references matching the feature domain
3. Generate code following loaded standards
4. Verify naming, dependency priority, and error handling

### Step 2b: Reviewing Code

1. `_core-rules.md` is auto-loaded with P1 rules
2. Load references relevant to the code under review
3. Check against rules, noting specific issues with line numbers
4. Categorize issues by priority (P1 must fix, P2 should fix, P3 nice to have)

### Step 2c: Architecture Question

1. Load `architecture.md` and `design-principles.md`
2. Load specific pattern files (EventChannel, RuntimeSet, Variable, Actions, ReactiveEntitySet)
3. Explain patterns with code examples from references

### Step 2d: Testing Question

1. Load `testing.md` for overview
2. Load specific testing files based on question (test-modes, patterns, test-doubles, nunit, assemblies, pitfalls)
3. Provide guidance with examples

## Reference Files

### Core

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: Essential naming and structure rules |
| references/naming.md | Naming conventions in detail |
| references/code-organization.md | Directory structure, namespaces |
| references/comments.md | XML docs, Header/Tooltip attributes |
| references/error-handling.md | Null safety, OnValidate |
| references/performance.md | Caching, pooling, event-driven |
| references/async.md | Awaitable, UniTask, Coroutines |
| references/input-system.md | InputReader pattern |

### Architecture

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


### Testing

| File | Use When |
|------|----------|
| references/testing.md | Testing fundamentals overview |
| references/principles.md | FIRST principles, AAA pattern, TDD |
| references/test-modes.md | Edit Mode vs Play Mode decision |
| references/patterns.md | Humble Object, Builder, Factory patterns |
| references/test-doubles.md | Mock, Stub, Spy, Fake |
| references/nunit.md | Attributes and assertions |
| references/assemblies.md | Test assembly definitions (asmdef) |
| references/pitfalls.md | Common testing mistakes |

### UI

| File | Use When |
|------|----------|
| references/ui-toolkit.md | UXML, USS, BEM naming |
| references/ugui.md | World Space UI, billboards |
| references/accessibility.md | Font sizes, contrast ratios |

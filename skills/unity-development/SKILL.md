---
name: unity-development
description: Write Unity C# code following Tang3cko standards. Architecture, naming, testing, UI patterns. Use when writing or reviewing Unity code. Triggers on "Unity", "C#", "アーキテクチャ", "EventChannel", "RuntimeSet", "テスト", "UI Toolkit".
model: sonnet
context: fork
agent: general-purpose
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Development Skill

Help users write, review, and refactor Unity C# code following Tang3cko standards.

## Core Principles

1. **ScriptableObject architecture** - EventChannel, RuntimeSet, Variable patterns
2. **Testability first** - Humble Object pattern, Edit Mode tests preferred
3. **Event-driven** - Decouple systems, avoid Singleton

## When Invoked

1. Read relevant reference files based on the user's request
2. Apply Unity standards to review or generate code
3. Ensure code follows naming, architecture, and testing guidelines

## Reference Files

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: Essential naming and structure rules |
| references/architecture.md | System communication, dependency patterns |
| references/event-channels.md | Pub/sub implementation |
| references/runtime-sets.md | Object tracking |
| references/variables.md | Shared state with change events |
| references/naming.md | Naming conventions |
| references/code-organization.md | Directory structure, namespaces |
| references/error-handling.md | Null safety, OnValidate |
| references/performance.md | Caching, pooling, event-driven |
| references/async.md | Awaitable, UniTask, Coroutines |
| references/input-system.md | InputReader pattern |
| references/testing.md | FIRST principles, AAA pattern |
| references/test-doubles.md | Mock, Stub, Spy, Fake |
| references/nunit.md | Attributes and assertions |
| references/ui-toolkit.md | UXML, USS, BEM naming |
| references/ugui.md | World Space UI |
| references/accessibility.md | Font sizes, contrast ratios |

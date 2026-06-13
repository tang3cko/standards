---
name: unity-testing
description: Write and review Unity tests following Tang3cko standards — Humble Object, Edit Mode vs Play Mode, NUnit, test doubles, test assemblies. Use when writing tests or reviewing test code.
when_to_use: Triggers on "テスト", "NUnit", "Humble Object", "mock", "stub", "asmdef", "Edit Mode", "Play Mode", "test", "testing", "TDD", "テスト駆動".
model: sonnet
allowed-tools: Read Glob Grep Edit Write
---

# Unity Testing

Help users write, review, and improve Unity tests following Tang3cko testing standards.

## Core Principles

1. **Testability first** - Humble Object pattern separates logic from MonoBehaviour
2. **Edit Mode preferred** - Faster, more reliable; use Play Mode only when necessary
3. **FIRST principles** - Fast, Isolated, Repeatable, Self-validating, Timely

## Workflow

Identify the task type and follow the matching path.

### Writing new tests

1. Load `testing.md` (overview) and `principles.md` (FIRST / AAA pattern)
2. Decide the mode with `test-modes.md` (Edit Mode vs Play Mode — see decision below)
3. Load pattern references as needed:
   - Making code testable → `patterns.md` (Humble Object, Builder, Factory)
   - Mocking dependencies → `test-doubles.md`
   - NUnit attributes/assertions → `nunit.md`
   - Test assembly setup → `assemblies.md`
4. Generate tests following AAA (Arrange, Act, Assert)

### Reviewing test code

1. Load `principles.md` (FIRST) and `pitfalls.md` (common mistakes checklist)
2. Check AAA adherence, test isolation, and naming
3. Categorize issues by priority (must fix, should fix, nice to have)

### A testing-strategy question

1. Load `testing.md` for fundamentals, plus the reference for the question domain
2. Explain with code examples

## Test Mode Decision

| Condition | Mode | Why |
|-----------|------|-----|
| Pure C# logic, no Unity APIs | Edit Mode | Fast, no scene setup |
| ScriptableObject creation/validation | Edit Mode | Works without Play Mode |
| MonoBehaviour lifecycle (Start, Update) | Play Mode | Requires runtime |
| Physics, coroutines, async timing | Play Mode | Needs Unity loop |

## Reference Files

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

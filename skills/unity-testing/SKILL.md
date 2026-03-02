---
name: unity-testing
description: Write and review Unity tests following Tang3cko standards. Humble Object, Edit Mode, NUnit, test doubles. Use when writing tests or reviewing test code. Triggers on "テスト", "NUnit", "Humble Object", "mock", "stub", "asmdef", "Edit Mode", "Play Mode", "test", "testing", "TDD", "テスト駆動".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Testing

Help users write, review, and improve Unity tests following Tang3cko testing standards.

## Core Principles

1. **Testability first** - Humble Object pattern separates logic from MonoBehaviour
2. **Edit Mode preferred** - Faster, more reliable; use Play Mode only when necessary
3. **FIRST principles** - Fast, Isolated, Repeatable, Self-validating, Timely

## When Invoked

### Step 1: Determine Task Type

- **Writing new tests?** → Go to Step 2a
- **Reviewing test code?** → Go to Step 2b
- **Testing strategy question?** → Go to Step 2c

### Step 2a: Writing New Tests

1. Load `testing.md` for overview and `principles.md` for FIRST/AAA pattern
2. Determine test mode: Load `test-modes.md` for Edit Mode vs Play Mode decision
3. Load pattern files based on need:
   - Making code testable → `patterns.md` (Humble Object, Builder, Factory)
   - Mocking dependencies → `test-doubles.md`
   - NUnit attributes/assertions → `nunit.md`
4. Load `assemblies.md` if setting up test assembly definitions
5. Generate tests following AAA pattern (Arrange, Act, Assert)

### Step 2b: Reviewing Test Code

1. Load `principles.md` for FIRST principles
2. Load `pitfalls.md` for common mistakes checklist
3. Check AAA pattern adherence, test isolation, naming
4. Categorize issues by priority (must fix, should fix, nice to have)

### Step 2c: Testing Strategy Question

1. Load `testing.md` for fundamentals overview
2. Load specific reference based on question domain
3. Explain with code examples

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

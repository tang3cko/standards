---
name: unity-testing
description: Write and review Unity tests following Tang3cko standards — Humble Object, Edit Mode vs Play Mode, NUnit, test doubles, test assemblies. Use when writing tests or reviewing test code.
when_to_use: Triggers on "テスト", "NUnit", "Humble Object", "mock", "stub", "asmdef", "Edit Mode", "Play Mode", "test", "testing", "TDD", "テスト駆動".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Testing

Help users write, review, and improve Unity tests following Tang3cko testing standards.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references    | testing.md + principles.md (+ patterns/doubles/nunit/assemblies as needed)"
TaskCreate: "Phase 2: Decide test mode   | test-modes.md decision table"
TaskCreate: "Phase 3: Write/review tests | AAA structure"
TaskCreate: "Phase 4: Self-Check         | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Strategy questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `principles.md` (and `pitfalls.md` when reviewing) | Always |
| 2 | Justify Edit Mode vs Play Mode per the decision table | New tests |
| 4 | Self-Check every written/edited test | Tests were written or edited |

## Core Rules (always apply)

1. **Testability first** — Humble Object pattern separates logic from MonoBehaviour; test the pure C# core, keep the MonoBehaviour shell thin
2. **Edit Mode preferred** — faster, more reliable; use Play Mode only when the Test Mode Decision below demands it
3. **FIRST principles** — Fast, Isolated, Repeatable, Self-validating, Timely
4. **AAA structure** — every test reads as Arrange, Act, Assert; one behavior per test; name = `Method_Condition_ExpectedResult`
5. **Test assemblies via asmdef** — test code never leaks into runtime assemblies (`UNITY_INCLUDE_TESTS` constraint)

Unity Test Framework note: UTF 2.0 is a core package since Unity 6.2 (docs live in the main Unity Manual under "Testing your code"). `[RequiresPlayMode]` exists for Edit Mode assemblies that need the player loop. For UI Toolkit interaction tests on Unity 6.3+, the separate UI Test Framework package exists.

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
5. Run the Self-Check below before presenting

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

## Self-Check (before presenting generated/edited tests)

Verify every test you wrote or changed. If any check fails, fix it first — do not present non-compliant tests.

- [ ] Each test has clear Arrange / Act / Assert sections and asserts one behavior
- [ ] Test names follow `Method_Condition_ExpectedResult`
- [ ] Mode choice matches the Test Mode Decision (no Play Mode test that could be Edit Mode)
- [ ] Tests are isolated — no shared mutable state, no order dependence, no leftover scene objects/assets
- [ ] Test asmdef references are correct and runtime assemblies gain no test dependencies

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

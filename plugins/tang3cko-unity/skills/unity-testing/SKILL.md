---
name: unity-testing
description: Write and review Unity tests following Tang3cko standards — Humble Object, Edit Mode vs Play Mode, NUnit, test doubles, test assemblies. Use when writing tests or reviewing test code.
when_to_use: Triggers on "テスト", "NUnit", "Humble Object", "mock", "stub", "asmdef", "Edit Mode", "Play Mode", "test", "testing", "TDD", "テスト駆動".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Testing

Tang3cko's Unity test standards: Humble Object, Edit Mode by default, AAA structure, asmdef-scoped
test assemblies. The Core Rules below are always in force; the acceptance criteria live in
references/.

The failure that costs the most is logic left inside a MonoBehaviour. It forces a slow, flaky Play
Mode test for something that should have been a sub-millisecond Edit Mode test — extract first,
then test.

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

Unity Test Framework note: UTF 2.0 is a core package since Unity 6.2 (docs live in the main Unity Manual under "Testing your code"). `[RequiresPlayMode]` lets an Editor-only test assembly run individual tests in the Editor's Play Mode (and `[RequiresPlayMode(false)]` exempts Edit Mode tests in platform assemblies). For UI Toolkit interaction tests on Unity 6.3+, the separate UI Test Framework package exists (`com.unity.ui.test-framework`, versioned with the Editor, e.g. 6.3.0).

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
- [ ] The references named by the chosen workflow path were actually Read this session, and the `## Checklist` at the end of every loaded reference was run against the result — SKILL.md is only the index; the acceptance criteria live in those checklists, so skipping them means presenting unreviewed work

## Reference Files

| File | Load When |
|------|-----------|
| references/testing.md | At the start of any writing or strategy path — the only file with the test-doubles quick-decision tree and the topic-to-file navigation map |
| references/principles.md | Before writing or reviewing any test — the only file with the name-as-specification rule (name must claim exactly what is asserted) and the 15ms unit-test budget |
| references/test-modes.md | When choosing Edit vs Play Mode for a new test — the only file with the full decision tree and the measured cost comparison (sub-ms vs multi-second player init) |
| references/patterns.md | When code under test touches MonoBehaviour/ScriptableObject or needs test data — the only file with the full `ForTesting` API rules (public, naming, placement) and Builder/Factory examples |
| references/test-doubles.md | When a dependency must be replaced — the only file with the 5-type comparison table, NSubstitute syntax, and the "don't mock what you don't own" rule |
| references/nunit.md | When writing assertions or attributes — the only file with the constraint-model syntax reference, `[TestCase]` vs `[ValueSource]` mode limits, and LogAssert |
| references/assemblies.md | When creating or fixing test asmdefs — the only file with the exact asmdef JSON (overrideReferences, precompiledReferences, UNITY_INCLUDE_TESTS) and troubleshooting fixes |
| references/pitfalls.md | When reviewing tests or before presenting written ones — the only file with the 8 bad/good pitfall pairs used as review criteria |

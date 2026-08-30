---
name: unreal-testing
description: Write and review Unreal Engine tests following Tang3cko standards — CQTest by default, the five test frameworks and when each applies, Humble Object, headless runs. Use when writing tests or reviewing test code in an Unreal project.
when_to_use: Triggers on "テスト", "CQTest", "Automation", "Functional Test", "Low Level Tests", "Gauntlet", "TEST_CLASS", "Humble Object", "test", "testing", "TDD", "ヘッドレス".
allowed-tools: Read Glob Grep Bash Edit Write TaskCreate TaskUpdate TaskList
---

# Unreal Testing

Tang3cko's Unreal test standards: CQTest by default, logic extracted out of Actors before it is tested, headless runs that fail loudly. The Core Rules below are always in force; the acceptance criteria live in references/.

The failure that costs the most is logic left inside an Actor or a Component. It forces a world, a tick loop and a map load for something that should have been a sub-millisecond test on a plain struct. Extract first, then test — the cheapest test that touches a world still pays editor startup.

The second failure is a green run that tested nothing. Unreal's automation runner exits **0 when a filter matches no tests at all**, and writes no report. A CI step that only checks the exit code will pass forever after a rename.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references      | frameworks.md + principles.md (+ cqtest/patterns/world-tests/headless/module-setup as needed)"
TaskCreate: "Phase 2: Choose the framework | Framework Decision table"
TaskCreate: "Phase 3: Write/review tests   | AAA structure"
TaskCreate: "Phase 4: Self-Check           | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Strategy questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `frameworks.md` + `principles.md` (and `pitfalls.md` when reviewing) | Always |
| 2 | Justify the framework choice per the decision table | New tests |
| 4 | Self-Check every written/edited test | Tests were written or edited |

## Core Rules (always apply)

1. **Testability first** — extract logic into plain C++ or USTRUCT types the test can construct directly; keep the Actor, Component and Subsystem shells thin
2. **CQTest is the default** — Epic's own xUnit-style framework, in-engine, no extra target
3. **Cheapest framework that answers the question** — a test that needs no world must not create one; a test that needs no editor must not start one
4. **AAA structure** — every test reads as Arrange, Act, Assert; one behaviour per test; name = `Method_Condition_ExpectedResult`
5. **A run that matched nothing is a failure** — assert on the test count, not on the exit code alone
6. **Tests live in `Private/Tests/`** — named `[ClassName]Test.cpp`, guarded so they never ship

## Workflow

Identify the task type and follow the matching path.

### Writing new tests

1. Load `frameworks.md` (decision table with evidence) and `principles.md` (FIRST / AAA / name-as-specification)
2. Choose the framework with the Framework Decision below
3. Load pattern references as needed:
   - Making code testable, or the code talks to an external service → `patterns.md`
   - Writing CQTest classes, fixtures, latent steps, assertions → `cqtest.md`
   - The test needs a world or a real level → `world-tests.md`
   - Running the tests from the command line or CI → `headless.md`
   - Creating or fixing a `.Build.cs` or file guard for tests → `module-setup.md`
4. Generate tests following AAA
5. Run the Self-Check below before presenting

### Reviewing test code

1. Load `principles.md` (FIRST) and `pitfalls.md` (the bad/good pairs used as review criteria)
2. Check AAA adherence, isolation, naming, and that the run would actually fail on regression
3. Categorize issues by priority (must fix, should fix, nice to have)

### A testing-strategy question

1. Load `frameworks.md` for the framework landscape, plus the reference for the question domain
2. Explain with code examples

## Framework Decision

| Condition | Framework | Why |
|---|---|---|
| Pure logic, no engine types | CQTest, or Low Level Tests where a separate target already exists | Fastest; no world |
| Logic on a UObject or subsystem, no level | CQTest | Setup/teardown, common state, latent steps |
| Needs a world but not a specific map | CQTest with a test world | `FTestWorldWrapper` builds and tears one down |
| Behaviour that must be seen in a real level | Functional Test (`AFunctionalTest`) | An Actor placed in the map, run by `ftest` |
| Multiple processes — client and server together | Gauntlet | The only one that orchestrates a session |
| Legacy tests already written with the macros | Automation (`IMPLEMENT_SIMPLE_AUTOMATION_TEST`) | Keep, do not extend |

Two paths look available and are not: the engine's networked automation macro, and a Low Level Tests target on a Launcher-installed engine. `frameworks.md` shows why, with the evidence.

## What cannot be automated here

Anything that needs a real session on an online service you do not own — a second account, a real backend, a live matchmaking lobby — is manual. With EOS in particular, P2P addresses peers by `ProductUserId`, so one account cannot stand in for two, and the second account must be a member of the Developer Portal organisation. This is SDK and account-administration behaviour, not anything visible in engine source — the project's own docs and decision records are the authority for it. See `patterns.md`.

Do not fake this with a mock of the online subsystem and call the feature tested. Extract the logic that decides *what* to do from the code that talks to the service, test the former, and record the latter as a manual check with a written procedure.

## Self-Check (before presenting generated/edited tests)

Verify every test you wrote or changed. If any check fails, fix it first — do not present non-compliant tests.

- [ ] Each test has clear Arrange / Act / Assert sections and asserts one behaviour
- [ ] Test names follow `Method_Condition_ExpectedResult`
- [ ] The framework is the cheapest one that answers the question, per the decision table
- [ ] No test creates a world, loads a map, or starts PIE that did not need to
- [ ] Tests are isolated — no shared mutable state, no order dependence, no actors or assets left behind
- [ ] Test code sits in `Private/Tests/` and cannot compile into a shipping build
- [ ] The run was executed, not assumed, and the observed test count is greater than zero
- [ ] The references named by the chosen workflow path were actually Read this session, and the `## Checklist` at the end of every loaded reference was run against the result — SKILL.md is only the index; the acceptance criteria live in those checklists, so skipping them means presenting unreviewed work

## Reference Files

| File | Load When |
|------|-----------|
| references/frameworks.md | At the start of any writing or strategy path — the only file with the five-framework comparison, what each costs to run, and the evidence that the networked automation macro and Launcher-engine LLT are dead ends |
| references/principles.md | Before writing or reviewing any test — the only file with FIRST, the name-as-specification rule, the per-tier time budgets, and why a green run proves nothing by itself |
| references/patterns.md | When code under test lives on an Actor/Component/Subsystem or talks to an external service — the only file with Humble Object in Unreal form, what is constructible by a test, the seam rules for online services, and test data construction |
| references/cqtest.md | When writing CQTest code — the only file with `TEST_CLASS` structure, `BEFORE_EACH`/`AFTER_EACH`, latent steps and their one real restriction, the assertion API, and file placement |
| references/world-tests.md | When a test needs a world or a real level — the only file with `FTestWorldWrapper` and its PIE limitation, Functional Test authoring, the latent-command ordering trap, and map requirements |
| references/headless.md | When running tests from the command line or CI — the only file with the exact command line, the zero-match failure mode and how to detect it, report parsing, screenshots, and the `-stdout -FullStdOutLogOutput` requirement |
| references/module-setup.md | When creating or fixing a test `.Build.cs` or file guard — the only file with what a `.Build.cs` needs to compile CQTest tests, how `WITH_DEV_AUTOMATION_TESTS`/`WITH_AUTOMATION_TESTS` are actually defined, and the guard that keeps test code out of Shipping |
| references/pitfalls.md | When reviewing tests or before presenting written ones — the only file with the twelve bad/good pairs used as review criteria, including the flags-API and documentation-sample traps |

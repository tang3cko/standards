# Pitfalls

Each pair is a review criterion. Every claim here is stated with its evidence in one of the other seven files — the pointer at the end of each pair says which, so that a finding raised from this file can be checked without hunting.

## 1. Trusting the exit code

**Bad** — CI runs the suite and checks `$?`. A rename drops every test from the filter; the run matches nothing, writes no report, and the step goes green forever.

**Good** — assert the report exists and the test count clears an expected minimum. The exit code is corroboration, not evidence. (`headless.md`)

## 2. Testing the Actor instead of the rule

**Bad** — a test spawns an Actor, ticks a world, and asserts that a member changed. A full editor startup plus a world, to check a comparison.

**Good** — the rule lives in a plain struct the test constructs directly, and the Actor is a `switch` nobody needs to assert on. (`patterns.md`, `principles.md`)

## 3. Mocking the SDK

**Bad** — a fake online subsystem returns success, the test passes, and the feature does not work because the real SDK behaves differently. This is the most common way an online feature reaches a build "tested" and broken.

**Good** — decisions tested as pure logic, translation reviewed, SDK behaviour written down as a manual procedure with the account requirements stated. (`patterns.md`)

## 4. Copying a sample from the documentation site

**Bad** — `EAutomationTestFlags::ApplicationContextMask`, copied from the Automation Spec page (`dev.epicgames.com/documentation/en-us/unreal-engine/automation-spec-in-unreal-engine`). It does not exist in 5.8; the masks moved to free constants with an underscore, and there are no deprecated aliases. Older samples use `ATF_Game`, which is gone entirely.

**Good** — samples taken from engine headers, and any documentation sample compiled before it is trusted. (`frameworks.md`)

## 5. Believing a skip

**Bad** — a test guarded by `SKIP` in a file built through the LLT adapter. `SKIP` expands to **nothing at all** there: the body runs, or the intent silently evaporates, depending on where it sat. `SECTION` degrades to an `AddInfo` call, so its grouping stops meaning anything.

**Good** — skips are expressed through the exclude list with a written reason, where they show up as Skipped rather than vanishing. (`frameworks.md`, `headless.md`)

## 6. `TEST_CASE` in a shared translation unit

**Bad** — `TEST_CASE` in the adapter generates its class name from `__COUNTER__`, which only works inside a unique namespace in the same translation unit; its own header says so. Two files collide and the second test disappears.

**Good** — `TEST_CASE_NAMED` with an explicit unique name. (`frameworks.md`)

## 7. Renaming a Functional Test actor

**Bad** — a Functional Test's name derives from `GetActorLabel()`. Someone renames the actor in the editor for clarity, the test path changes, and the CI filter naming it now matches nothing. See pitfall 1 for what happens next.

**Good** — the filter is broad enough to survive a label change, and the count check catches it if it does not. (`world-tests.md`)

## 8. Latent commands queued in the wrong place

**Bad** — commands added before `FStartFTestOnMap` run before the map is up, and the test asserts against an empty world. The engine's own source shouts about this in a comment.

**Good** — ordering understood and the wait command placed outside any latent action, as `FMapTestSpawner`'s header requires. (`world-tests.md`, `cqtest.md`)

## 9. Asking `FTestWorldWrapper` for PIE

**Bad** — `CreateTestWorld(EWorldType::PIE)`. The implementation is `// TODO Implement if needed` falling into `ensureMsgf(false, ...)`.

**Good** — any of the six world types the function actually accepts (`Game`, `Editor`, `EditorPreview`, `GamePreview`, `GameRPC`, `Inactive`), or the editor-common PIE commands with their timeout when PIE is genuinely what is being tested. (`world-tests.md`)

## 10. A UI claim from a screenshot API

**Bad** — "the menu renders correctly" backed by `HighResShot`, which forces `bShowUI = false`. The image shows a 3D scene and no menu, and proves nothing either way.

**Good** — the packaged build launched, driven with `-ExecCmds`, and its window photographed from outside; blank images rejected mechanically; the image actually opened by someone. (`headless.md`)

## 11. A tool that printed nothing

**Bad** — a Python tool run through `-run=pythonscript` exits 0 and prints nothing. It is recorded as passing. `unreal.log` writes at Display verbosity, which does not reach stdout.

**Good** — `-stdout -FullStdOutLogOutput` on anything whose output is read, and silence treated as a failure rather than a pass. (`headless.md`)

## 12. Planning around Low Level Tests on a Launcher engine

**Bad** — a plan that assumes an explicit LLT target. It does not build there: installed engines reject unique build environments, `TargetType.Program` is unsupported, and `LowLevelTestsRunner` is not shipped precompiled.

**Good** — CQTest, unless the project builds the engine from source. (`frameworks.md`)

## Checklist

- [ ] Reviewed against all twelve pairs above
- [ ] No finding was accepted on the basis of a passing run alone
- [ ] Every documentation-sourced identifier in the tests was checked against 5.8 headers

# Headless EditMode Test Execution

`scripts/test.sh <project_path> [test_filter] [test_platform]` runs this end-to-end. This file explains what it does and why, so you can debug it or extend it — don't reimplement its logic inline.

---

## Why EditMode (not PlayMode) for headless CI

EditMode tests run inside the Editor process without entering Play Mode — no player loop, no scene load required for pure-logic tests. They're the fast, deterministic default for headless runs, and `test_platform` defaults to `EditMode` for that reason. PlayMode tests can also run in `-batchmode` (pass `PlayMode` as the third argument), but need a Test Player build step first; prefer EditMode unless the suite specifically needs runtime lifecycle (see `unity-testing` skill's Test Mode Decision).

---

## What the script does, step by step

1. **Open-Editor check and lock** (`project_is_open` in `scripts/lib_resolve_unity.sh`, `acquire_batchmode_lock` in `scripts/lib_batchmode_lock.sh`) — exits 2 without starting Unity if an Editor has the project open, per Core Rule 1. Then takes a per-project `mkdir` lock so a second headless run against the same project waits (up to `UNITY_LOCK_WAIT`, default 900 s) instead of colliding inside Unity; a lock whose holder is dead is reclaimed. Giving up on the lock is exit 2 too — the run never happened
2. **Version detection** (`resolve_unity_executable`) — reads `m_EditorVersion:` from `ProjectSettings/ProjectVersion.txt` and resolves the matching Unity Hub install path per OS. Never hardcode a version or rely on whatever `unity` resolves to on `PATH` — a version mismatch either silently reimports the whole project or fails with a misleading error. `UNITY_PATH` overrides the lookup for non-Hub installs
3. **Invocation** — `-batchmode -nographics -runTests -testPlatform <platform> -testResults <path> -logFile <path>`, plus `-testFilter <filter>` when a filter argument was given (per [Unity's documented syntax](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/run-tests-from-command-line.html): semicolon-separated names, a regex, or `!Name` to exclude). **No `-quit`** — combined with `-runTests` the editor quits before the run produces anything. Measured 2026-08-19 on one project with Unity 6000.3.11f1, back to back: without `-quit`, 445/445 and a result XML; with `-quit`, `Exiting batchmode successfully now!` and no XML at all. `-runTests` exits on its own when the run finishes, so `-quit` buys nothing. Unity's docs don't mention `-quit` on the run-tests page — this is field measurement, not a documented contract. **Do not generalise this to `-executeMethod`**: `build.sh` needs `-quit` and keeps it. Measured the same day, same project — an `-executeMethod` run completes either way, but without `-quit` the editor never exits and sits there until something kills it. The difference is who owns the exit: the test framework ends a `-runTests` run and `-quit` races it, while nothing at all ends an `-executeMethod` run
4. **Result parsing** (`scripts/summarize_test_results.py`) — Unity's own process exit code is not authoritative for pass/fail (it can exit 0 with failures still recorded in the XML). The script parses the NUnit result XML instead and prints `Total/Passed/Failed/Skipped` plus each failing test's full name and message
5. **Missing-XML is exit 2, not a pass and not a test failure** — if Unity crashed or was killed before tests ran, no XML is produced; the script says so and dumps the tail of the Unity log rather than staying silent
6. **Zero collected tests is exit 2 too** — a `-testFilter` that matches nothing produces a valid XML reading `Total=0 Failed=0`, which grades as green while measuring nothing. A run that collected no tests is void, not a pass
7. **Watchdog** (`run_unity_watchdogged`) — Unity runs in the background and is SIGKILLed if still alive after `UNITY_TEST_TIMEOUT` seconds (default 1800). The kill reaches only the Editor pid: Unity's helpers (Licensing Client, Package Manager, ILPP runner, AutoQuitter) sit in their own process groups and may be left as orphans — an accepted cost, spelled out in `lib_batchmode_lock.sh` with the reason a machine-wide sweep is never coming back. A timeout ends in exit 2 with the ceiling named on stderr

| Exit | Meaning | What to report |
|------|---------|----------------|
| 0 | Every collected test passed | The `Total/Passed/...` line |
| 1 | The suite ran and at least one test failed | The `FAIL:` lines — this is a code result |
| 2 | Nothing was measured: Editor open, lock wait exhausted, watchdog kill, no XML, zero tests | The stderr reason — this is an environment result, not a test result |
| 3 | The test-count ratchet tripped (below) | How many tests disappeared |

---

## The test-count ratchet (opt-in)

Zero collected tests is the extreme case of a more general blind spot: **a suite that quietly got smaller still reports green**. A renamed class drops out of a filter, a file moves out of an assembly, a fixture stops being compiled — every remaining test passes, and nothing says that forty of them stopped existing.

Set `TEST_COUNT_FLOOR_DIR` to turn on the guard:

```bash
TEST_COUNT_FLOOR_DIR=<project>/TestResults bash scripts/test.sh <project_path>
```

| | |
|---|---|
| Where the floor lives | `<dir>/test-count-high-water-<platform>-<filter>`, one integer. **Point `<dir>` at a gitignored path** — the floor is an observation made on this machine, not a fact about the repository |
| One floor per filter | A filtered run is a subset; comparing it against the whole suite's mark would fail every time. The filter is part of the number's identity, not a detail about it |
| Below the floor | Exit 3, naming how many tests disappeared |
| No floor recorded yet | Exit 3, **not a warning**. "Every test passed" with no floor means nothing here checked that the whole suite ran |
| Above the floor | The floor is raised — but only when the run also passed. A failing run's total is not a number anyone should be held to next time |
| Shrinking on purpose | `ESTABLISH_BASELINE=1` records the current count in both directions. Deliberately a separate gesture from running the tests, so an intentional cut and a silent one never look alike |

Without `TEST_COUNT_FLOOR_DIR` none of this applies and the exit codes behave exactly as they did before the ratchet existed.

**CI trap.** A fresh CI checkout has no floor file, so the first CI run exits 3 for a reason unrelated to the code. The tempting fix — `ESTABLISH_BASELINE=1` in the pipeline — re-records the floor on every run and silently disables the check forever. Never put `ESTABLISH_BASELINE=1` in a pipeline definition. Carry the floor file across jobs in the CI cache, or leave the ratchet off in CI and keep it as a local guard; decide that when the CI system is chosen, not before.

**Do not replace this with a hardcoded expected-minimum constant.** That was the earlier shape of the same idea, and the way it fails is on record: the constant arrived copied from a previous project, was never bumped, sat eleven tests below the real suite, and stopped catching anything long before anyone noticed. A floor that is written by observation cannot drift from what was observed.

---

## Reading the Summary

Report to the user as the script's own `Total / Passed / Failed / Skipped` line, plus each `FAIL:` line verbatim. Don't paste raw XML or full batchmode logs unless asked — that defeats the point of headless summarization.

---

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-state boundary
- [pitfalls.md](pitfalls.md) - Project lock, licensing client issues
- [verification.md](verification.md) - Build-side verification (companion to test-side verification here)

---

## Checklist

- [ ] Tests were run via `bash ${CLAUDE_SKILL_DIR}/scripts/test.sh` — no hand-rolled `-batchmode -runTests` invocation
- [ ] `test_platform` matches the suite: EditMode by default, PlayMode only when the tests need runtime lifecycle
- [ ] Any `test_filter` uses Unity's documented `-testFilter` syntax (semicolon-separated names, regex, or `!Name` to exclude)
- [ ] Result reported as the script's `Total/Passed/Failed/Skipped` line plus `FAIL:` lines verbatim — not raw XML or full batchmode logs
- [ ] A missing result XML was treated as a hard failure and the dumped log tail was actually read, not skipped
- [ ] Exit 2 (never ran) was reported as an environment outcome, not folded into "tests failed"; after a watchdog kill, leftover Unity helper processes were checked for

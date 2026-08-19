# Headless EditMode Test Execution

`scripts/test.sh <project_path> [test_filter] [test_platform]` runs this end-to-end. This file explains what it does and why, so you can debug it or extend it — don't reimplement its logic inline.

---

## Why EditMode (not PlayMode) for headless CI

EditMode tests run inside the Editor process without entering Play Mode — no player loop, no scene load required for pure-logic tests. They're the fast, deterministic default for headless runs, and `test_platform` defaults to `EditMode` for that reason. PlayMode tests can also run in `-batchmode` (pass `PlayMode` as the third argument), but need a Test Player build step first; prefer EditMode unless the suite specifically needs runtime lifecycle (see `unity-testing` skill's Test Mode Decision).

---

## What the script does, step by step

1. **Lock check** (`check_editor_running` in `scripts/lib_resolve_unity.sh`) — refuses to proceed if a Unity process already appears to hold the project, per Core Rule 1
2. **Version detection** (`resolve_unity_executable`) — reads `m_EditorVersion:` from `ProjectSettings/ProjectVersion.txt` and resolves the matching Unity Hub install path per OS. Never hardcode a version or rely on whatever `unity` resolves to on `PATH` — a version mismatch either silently reimports the whole project or fails with a misleading error
3. **Invocation** — `-batchmode -nographics -runTests -testPlatform <platform> -testResults <path> -logFile <path>`, plus `-testFilter <filter>` when a filter argument was given (per [Unity's documented syntax](https://docs.unity3d.com/6000.3/Documentation/Manual/test-framework/run-tests-from-command-line.html): semicolon-separated names, a regex, or `!Name` to exclude). **No `-quit`** — combined with `-runTests` the editor quits before the run produces anything. Measured 2026-08-19 on one project with Unity 6000.3.11f1, back to back: without `-quit`, 445/445 and a result XML; with `-quit`, `Exiting batchmode successfully now!` and no XML at all. `-runTests` exits on its own when the run finishes, so `-quit` buys nothing. Unity's docs don't mention `-quit` on the run-tests page — this is field measurement, not a documented contract
4. **Result parsing** (`scripts/summarize_test_results.py`) — Unity's own process exit code is not authoritative for pass/fail (it can exit 0 with failures still recorded in the XML). The script parses the NUnit result XML instead and prints `Total/Passed/Failed/Skipped` plus each failing test's full name and message
5. **Missing-XML is a hard failure** — if Unity crashed or the project was locked before tests ran, no XML is produced; the script treats that as failure and dumps the tail of the Unity log rather than staying silent
6. **Zero collected tests is a hard failure too** (exit 2) — a `-testFilter` that matches nothing produces a valid XML reading `Total=0 Failed=0`, which grades as green while measuring nothing. A run that collected no tests is void, not a pass

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

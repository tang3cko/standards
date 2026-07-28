---
name: unity-headless
description: Run Unity tests and player builds headlessly (batchmode/CI) for any Unity game project via the skill's bundled scripts/test.sh and scripts/build.sh, executeMethod player builds, build artifact verification, and the Editor-open/closed boundary with unity-mcp. Use when setting up or debugging CLI/CI-driven Unity test or build execution.
when_to_use: Triggers on "batchmode", "-batchmode", "headless", "CUI実行", "ヘッドレス", "CI", "BuildPipeline", "executeMethod", "test.sh", "build.sh", "ProjectVersion.txt", "Licensing Client", "entitlement", "コマンドラインビルド", "コマンドラインテスト".
allowed-tools: Read Glob Grep Bash(bash:*) Write Edit TaskCreate TaskUpdate TaskList
---

# Unity Headless Operations

Set up and debug Editor-non-resident (CLI/CI) Unity test execution and player builds, following Tang3cko standards proven on ReactiveSO. Generalized for any Unity game project — not a specific sample's build.

**This skill ships working scripts — invoke them, don't reimplement their logic.** Version detection, OS resolution, project-lock checking, and result parsing are all handled by `scripts/`. The only things you write per-project are the pieces that are inherently project-specific: the `BuildScript.cs` entry point (scene list, output path, scripting backend) and the `Player.log` path (depends on company/product name).

## Phase Tracking

**At workflow start, create tasks for each phase:**

```
TaskCreate: "Phase 1: Identify task   | test run, build run, or debugging an existing failure"
TaskCreate: "Phase 2: Prepare inputs  | project path + (build only) BuildScript.cs entry point in place"
TaskCreate: "Phase 3: Run script      | scripts/test.sh or scripts/build.sh via Bash"
TaskCreate: "Phase 4: Verify result   | read script output + Self-Check"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Pure troubleshooting questions (pitfalls lookup) skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 2 | For builds: confirm `BuildScript.cs` (or equivalent) exists in the target project's `Assets/Editor/`; copy from `assets/BuildScript.cs.template` and fill in TODOs if missing | Build tasks |
| 3 | Invoke the bundled script via Bash — do not hand-roll the `-batchmode` invocation | Always |
| 4 | Read the script's own pass/fail output; don't declare success from the Bash exit status alone when the script prints failure details | Always |

## Core Rules (always apply)

1. **Editor state is mutually exclusive with batchmode** — a Unity project has a single lock (`Temp/UnityLockfile` / OS-level file lock). `-batchmode` execution requires the Editor for this project to be **closed**; `unity-mcp` requires the Editor to be **open and running**. Never attempt both against the same project directory at once — `scripts/test.sh` and `scripts/build.sh` both check for a running Editor process and refuse to proceed if one is found. If the task also involves live-Editor compile checking, that's `unity-mcp`'s job, not this skill's — sequence them (close Editor -> batchmode -> reopen Editor), don't interleave.
2. **Use the bundled scripts, don't reimplement them** — `scripts/test.sh` and `scripts/build.sh` already handle version detection (`ProjectVersion.txt`), per-OS executable resolution, `-nographics`/`-logFile` flags, and lock checking. Writing a fresh inline invocation reintroduces bugs these scripts already avoid.
3. **Parse output, don't trust the exit code alone** — Unity documents no common exit-code contract; it can exit 0 with test failures still present in the result XML, and exit nonzero for licensing/environment failures unrelated to the tests/build. Both scripts already do this parsing and print a clear summary — read it rather than re-deriving pass/fail from the raw Bash exit code.
4. **Verify, don't assume, build success** — a build entry point that doesn't call `EditorApplication.Exit(1)` on failure will exit 0 even when `BuildPipeline.BuildPlayer` failed (`assets/BuildScript.cs.template` already does this — keep it when customizing). When the task calls for a launch smoke test, confirm the player actually starts (see `references/verification.md`).

## Workflow

Identify the task type and follow the matching path. The workflow steps below name reference files at the point they become load-bearing — Read them there, not from memory: each holds material (filter syntax, template reasoning, log paths, diagnosis commands) that exists nowhere in this file.

### Running headless EditMode/PlayMode tests

1. Run: `bash ${CLAUDE_SKILL_DIR}/scripts/test.sh <project_path> [test_filter] [test_platform]` (argument semantics: `references/testing.md`)
2. Read the printed `Total/Passed/Failed/Skipped` summary and any `FAIL:` lines
3. If setup fails (no result XML, lock error), load `references/pitfalls.md`
4. Run the Self-Check below

### Running a headless player build

1. Confirm the target project has an Editor-only build entry point. If not, read `references/building.md`, then copy `assets/BuildScript.cs.template` into `<project>/Assets/Editor/BuildScript.cs` and resolve every `TODO`
2. Run: `bash ${CLAUDE_SKILL_DIR}/scripts/build.sh <project_path> <ExecuteMethod>`
3. Load `references/verification.md` and verify the artifact (and run `scripts/verify_player_log.sh <Player.log path>` if the task needs a launch smoke test)
4. Run the Self-Check below

### Debugging a headless run that fails or hangs

1. Load `references/pitfalls.md` and match the symptom (project lock, orphaned Licensing Client, misleading compile error, pdb false positive)
2. Apply the fix; re-run the same script
3. If the failure only reproduces with the Editor open/closed unexpectedly, re-check Core Rule 1

## Self-Check (before presenting results)

Verify every run you report on, and any project-specific file you wrote or changed. If any check fails, fix it first — do not present a result that violates these.

- [ ] Ran via `scripts/test.sh` / `scripts/build.sh` — no hand-rolled `-batchmode` invocation
- [ ] Reported the script's own summary (Total/Passed/Failed for tests; build result for builds), not just "exit code was 0"
- [ ] For builds: the entry point calls `EditorApplication.Exit(1)` on `BuildResult != Succeeded`
- [ ] Any customized `BuildScript.cs` has no leftover `TODO` markers
- [ ] Every reference file named by the workflow path taken was actually Read this session — the details they carry are not summarized here, so skipping them means working from guesses
- [ ] Each loaded reference's final `## Checklist` was run against the work it covers

## Reference Files

Each file is the only place its listed content exists — the table rows are triggers, not summaries.

| File | Load When |
|------|-----------|
| references/testing.md | Running or debugging a headless test run. Sole home of: test.sh's step-by-step internals, the EditMode-vs-PlayMode decision, `-testFilter` syntax, and how to report the summary |
| references/building.md | Any build task — especially before copying or customizing `BuildScript.cs`. Sole home of: the reasoning behind each template decision (explicit scene list, `EditorApplication.Exit(1)`, scripting backend per target) and build.sh invocation examples |
| references/verification.md | After a build, when verifying the artifact or startup. Sole home of: the `CompilePlayerScripts` snippet, the UTF-16LE literal-scan technique, and per-OS `Player.log` default paths |
| references/pitfalls.md | A headless run fails, hangs, or reports compile errors that make no sense. Sole home of: the symptom→cause table, diagnosis commands (Licensing Client scan, lock-check internals), and the pdb debug-directory false-positive rationale |

## Bundled Scripts

| File | Purpose |
|------|---------|
| scripts/test.sh | Run headless tests end-to-end: lock check, version detection, invocation, XML summary |
| scripts/build.sh | Run a headless player build end-to-end: lock check, version detection, invocation |
| scripts/verify_player_log.sh | Check a `Player.log` for clean startup |
| scripts/summarize_test_results.py | NUnit result XML parser used internally by `test.sh` |
| scripts/lib_resolve_unity.sh | Shared version-detection/lock-check helpers, sourced by `test.sh`/`build.sh` |
| assets/BuildScript.cs.template | Editor build entry point to copy into the target project and customize |

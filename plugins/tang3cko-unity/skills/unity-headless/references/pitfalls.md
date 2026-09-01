# Headless Operation Pitfalls

Common failure modes when running Unity from the CLI, and how to diagnose them.

---

## Summary

| Symptom | Cause | Fix |
|---------|-------|-----|
| Script exits 2 before Unity starts: "a Unity Editor has ... open" | Editor already has the project open | Close the Editor; see Editor-state boundary below |
| Script prints "waiting for batchmode lock", or exits 2 with "could not take the batchmode lock" | Another headless run against the same project is in progress (or a stale lock — reclaimed automatically once its holder is gone) | Let it finish, or raise `UNITY_LOCK_WAIT`; see Pitfall 1 |
| Script exits 2 with "watchdog ... killing it" | Unity never finished within `UNITY_TEST_TIMEOUT` / `UNITY_BUILD_TIMEOUT` | Read the log tail; check for orphaned helper processes (Pitfall 4) |
| Compile errors like `CS0246: type or namespace not found` on packages that clearly exist | Orphaned Licensing Client from a different Unity version can't resolve entitlements, breaking package resolution | Kill stray Licensing Client processes, re-login via Unity Hub |
| A literal-scan / diff check flags an absolute local path inside the pdb | `pdb` debug directory records the build machine's absolute path by design | Expected; exclude from failure criteria, don't chase it as a leak |
| `unity-mcp` calls fail while a headless script is also mid-run | Both trying to hold the same project simultaneously | Sequence them — never run concurrently against the same project path |

---

## Editor-Open/Closed Boundary (unity-mcp vs. this skill)

Unity holds a single project-level lock (`Temp/UnityLockfile` plus an OS-level advisory lock on most platforms). Only one Unity process — Editor UI or batchmode — can hold a given project directory at a time.

- **`unity-mcp`** talks to a **running** Editor via its MCP bridge. It requires the Editor to be open.
- **This skill (`unity-headless`)** runs Unity via `-batchmode`, which requires the Editor for that project to be **closed**. Launching batchmode against a project the Editor already has open fails fast (lock error), it does not queue or wait.

When a task needs both (e.g. "check compile errors live, then produce a CI build"): **sequence them**, don't interleave — close the Editor before batchmode, reopen it afterward if live editing resumes. Never attempt to hold both against the same project path at once.

---

## Pitfall 1: Project Lock (Editor Already Open)

### Problem

Running `scripts/test.sh` or `scripts/build.sh` while the Unity Editor has the same project open. Without the check below, the batchmode process would fail almost immediately, often with a terse error about the project being locked, or (on some versions) simply hang waiting for a lock that will never release.

### Diagnosis

Both bundled scripts run `project_is_open` (`scripts/lib_resolve_unity.sh`) before anything else and exit 2 if it matches. It identifies Editors by executable path (`ps -o comm`), and only then reads those pids' arguments for a `-projectpath <this project>` (case-insensitive, separator-terminated):

```bash
# what project_is_open does under the hood
ps -Ao pid=,comm= | awk '/\/Unity\.app\/Contents\/MacOS\/Unity$/ { print $1 }'   # Editor pids
ps -p <pid> -o args=                                                              # which project each holds
```

It deliberately does **not** use `pgrep -f 'Unity.*-projectPath'`. Unity Hub keeps `-projectPath <project>` in its own argv after launching an Editor and stays resident after that Editor closes, so the command-line match reported a lock that nobody held and a batchmode run waited forever (Puzzle_003, 2026-07-28). Still best-effort in one direction: a just-closed Editor can hold the OS-level file lock briefly after its process is gone.

Two headless runs against the same project are serialized by a `mkdir` lock (`scripts/lib_batchmode_lock.sh`), one per project path under `${TMPDIR:-/tmp}/unity-headless/`. The second run prints `waiting for batchmode lock` and polls; a lock whose `owner` pid is dead is reclaimed at once, and a lock directory with no `owner` file (holder killed between `mkdir` and the write) is reclaimed after `UNITY_LOCK_GRACE` seconds (30). Set `UNITY_LOCKDIR` to move it, `UNITY_LOCK_WAIT` to change the 900 s ceiling. Giving up is exit 2 — the run never happened, and the lock is left where it was.

If the script refuses to start, don't bypass the check and don't `rmdir` a lock by hand while its owner pid is alive — it's catching Core Rule 1.

### Solution

Close the Editor for this project (or wait for the other batchmode run to finish — the script does this on its own within `UNITY_LOCK_WAIT`), then re-run the same script. This is Core Rule 1 in `SKILL.md`.

---

## Pitfall 4: Watchdog Kill Leaves Unity Helpers Behind

### Problem

A run ends with `watchdog: Unity pid N still running after Ts - killing it` and exit 2. Afterwards `ps` shows `Unity.Licensing.Client`, `UnityPackageManager`, `Unity.ILPP.Runner`, or `UnityAutoQuitter` still alive with `ppid=1`, and the next run may hit Pitfall 2's `[Licensing::Module] Error: Access token is unavailable`.

### Why the script does not clean them up

Unity puts each helper in its own process group, so neither `kill <pid>` nor `kill -- -<pgid>` reaches them (measured on Puzzle_003, 2026-07-28: after a group kill all four helpers survived as orphans). The obvious fix — sweep the machine for processes matching a helper-name pattern — was tried there and shipped with an empty pattern that matched every process; it SIGKILLed the whole machine four times. The watchdog therefore kills exactly the pid it started, and the orphans are an accepted cost.

### Solution

List and remove the orphans by hand, checking each pid before killing it:

```bash
ps -Ao pid,ppid,args | grep -i '[U]nity' | grep -v selftest
```

Only kill processes whose argv shows they belong to this project's Editor run (e.g. a `Unity.Licensing.Client` under a `ppid` of 1 that appeared at the time of the kill). Other projects' Editors and a human's open Editor live on the same machine.

---

## Pitfall 2: Orphaned Licensing Client Misresolves as Compile Errors

### Problem

After switching Unity versions (e.g. via Hub, or running two versions on the same machine), a batchmode run reports package/type resolution failures that look like ordinary compile errors:

```
error CS0246: The type or namespace name 'SomeNamespace' could not be found
```

...on a package that is unambiguously installed and resolves fine in the Editor UI. This is misleading: the actual root cause is that the **Unity Licensing Client** (a separate background process handling entitlement/license checks) is still running under a different Unity version's session, and the new batchmode invocation can't get a clean entitlement resolution — which cascades into package manager failures that surface as `CS0246`.

### Diagnosis

Check for a running Licensing Client process, and check `Editor.log` / the batchmode `-logFile` for entitlement/licensing errors *above* the reported `CS0246` — the real error is earlier in the log than the symptom.

```bash
ps aux | grep -i "[U]nity Licensing Client"
```

### Solution

1. Kill the orphaned Licensing Client process
2. Re-authenticate via Unity Hub (log out/in, or `Unity -licensingClient` re-login per your Unity version's licensing flow)
3. Re-run the batchmode command

Don't chase this as a code problem — if the same symbol resolves fine in the Editor UI and only fails in batchmode with a *different* Unity version recently having run on the machine, suspect licensing before suspecting the code.

---

## Pitfall 3: pdb Debug Directory False Positive

### Problem

A build-artifact verification step (literal scan, artifact diff, or similar) flags an absolute filesystem path — often containing the build machine's username or CI runner workspace path — found inside a `.pdb` file or an assembly's debug directory record.

### Why it's not a bug

The PE/COFF debug directory format bakes in the absolute path to the original `.pdb` at compile time, by design, so debuggers can locate matching symbols. This is standard toolchain behavior for Mono/IL2CPP-produced debug symbols, not a data leak and not something under the project's control to "fix" — every managed build does this.

### Solution

Exclude debug-directory / pdb-path matches from failure criteria in any artifact-scanning check. If sensitive-path leakage into a *release* build is a genuine concern, address it by excluding `.pdb` files from the shipped artifact (most platforms already do this for non-Development builds) rather than trying to scrub the path.

---

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-state boundary
- [testing.md](testing.md) - Headless test execution
- [building.md](building.md) - Headless player builds
- [verification.md](verification.md) - Artifact and startup verification

---

## Checklist

- [ ] The symptom was matched against the Summary table before improvising a fix
- [ ] The scripts' lock check was never bypassed — the Editor was closed (or the other batchmode run finished) before retrying, and no lock directory with a live owner was removed by hand
- [ ] After a watchdog kill, orphaned Unity helper processes were listed and removed individually — never by a name-pattern sweep
- [ ] For `CS0246`-in-batchmode-only symptoms: a stray Licensing Client process was checked for, and the log was read *above* the first `CS0246` for the real licensing/entitlement error
- [ ] pdb debug-directory path findings were treated as expected toolchain behavior, not reported as defects or leaks
- [ ] `unity-mcp` and batchmode were sequenced against the project, never run concurrently

# Headless Operation Pitfalls

Common failure modes when running Unity from the CLI, and how to diagnose them.

---

## Summary

| Symptom | Cause | Fix |
|---------|-------|-----|
| Batchmode hangs or fails immediately with a lock error | Editor (or another batchmode process) already has the project open | Close the Editor / wait for the other process; see Editor-state boundary below |
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

Both bundled scripts already run this check via `check_editor_running` in `scripts/lib_resolve_unity.sh` and refuse to start if it detects a match — this is a best-effort `pgrep`/`ps` scan, not a guarantee (a just-closed Editor can still hold the OS-level file lock briefly after the process check would report clear):

```bash
# what check_editor_running does under the hood
ps aux | grep -i "[U]nity.*-projectPath.*$(basename "$PROJECT_PATH")"
```

If the script refuses to start, don't bypass the check — it's catching Core Rule 1.

### Solution

Close the Editor for this project (or wait for any other batchmode run against it to finish), then re-run the same script. This is Core Rule 1 in `SKILL.md`.

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
- [ ] The scripts' lock check was never bypassed — the Editor was closed (or the other batchmode run finished) before retrying
- [ ] For `CS0246`-in-batchmode-only symptoms: a stray Licensing Client process was checked for, and the log was read *above* the first `CS0246` for the real licensing/entitlement error
- [ ] pdb debug-directory path findings were treated as expected toolchain behavior, not reported as defects or leaks
- [ ] `unity-mcp` and batchmode were sequenced against the project, never run concurrently

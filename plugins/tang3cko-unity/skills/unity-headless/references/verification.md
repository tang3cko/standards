# Build Artifact & Startup Verification

`scripts/build.sh` only confirms the batchmode process exited 0 (and that requires the entry point to call `EditorApplication.Exit(1)` on failure — see `building.md`). That's not proof the player works. Verify further with the tools below.

---

## `PlayerBuildInterface.CompilePlayerScripts` artifact checks

For deeper verification than "the file exists," compile scripts for the target via `PlayerBuildInterface.CompilePlayerScripts` and inspect the resulting assemblies before or instead of a full player build — useful in CI for a fast script-only correctness pass without paying full build time. This is C# run inside the Editor (via a batchmode `-executeMethod`, same as the build entry point), not something `scripts/` wraps generically — write it ad hoc per check, since what you're inspecting varies by task:

```csharp
using UnityEditor.Build.Player;

var input = new ScriptCompilationSettings
{
    target = EditorUserBuildSettings.activeBuildTarget,
    group = EditorUserBuildSettings.selectedBuildTargetGroup,
};
var output = PlayerBuildInterface.CompilePlayerScripts(input, "Temp/ScriptCompile");
if (output.assemblies.Count == 0)
{
    Debug.LogError("script compilation produced no assemblies");
    EditorApplication.Exit(1);
}
```

### Scanning compiled output for stray strings (e.g. UTF-16 literals)

When checking whether a specific string constant or literal made it into the compiled managed assemblies (e.g. confirming a debug string was stripped, or a secret wasn't baked in), scan the assembly bytes for its UTF-16LE encoding — C# string literals are stored as UTF-16 in the managed image, so a plain ASCII byte search on the DLL will miss them:

```python
needle = "SomeMarkerString".encode("utf-16-le")
with open(assembly_path, "rb") as f:
    data = f.read()
found = needle in data
```

Do this only for a specific, deliberate check (e.g. "did the DEBUG_MENU string get compiled out of the release build") — don't scan for arbitrary substrings as a general lint, it produces noise (see the pdb false-positive note below).

---

## `Player.log` startup confirmation

`scripts/verify_player_log.sh <Player.log path>` checks for a clean `Initialize engine version` line with no immediately-following `Fatal Error`/`Unhandled Exception`. What it can't provide is the log's location — that depends on OS and `PlayerSettings.companyName`/`productName`, which are project-specific:

| Platform | Default `Player.log` location |
|----------|-------------------------------|
| macOS | `~/Library/Logs/<CompanyName>/<ProductName>/Player.log` |
| Windows | `%USERPROFILE%\AppData\LocalLow\<CompanyName>\<ProductName>\Player.log` |
| Linux | `~/.config/unity3d/<CompanyName>/<ProductName>/Player.log` |

Run the built player briefly (a few seconds is enough for startup), then check the log:

```bash
"$BUILD_OUTPUT/Game.app/Contents/MacOS/Game" -batchmode -nographics -logFile - &
PID=$!
sleep 5
kill "$PID" 2>/dev/null || true

bash ${CLAUDE_SKILL_DIR}/scripts/verify_player_log.sh "$HOME/Library/Logs/<CompanyName>/<ProductName>/Player.log"
```

Don't treat every warning in the log as a failure — Unity logs plenty of benign warnings on first launch (shader compilation, first-import asset warnings). `verify_player_log.sh` only fails on a missing "engine initialized" line or an explicit fatal/uncaught-exception line, which is the right bar for "starts cleanly."

---

## Known False Positives

See `pitfalls.md` for the pdb debug-directory path false positive — a literal-scan or artifact-diff check will flag the local build machine's absolute path baked into the pdb's debug directory record. This is expected toolchain behavior (not a leak, not a bug) and should be excluded from failure criteria rather than chased.

---

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-state boundary
- [building.md](building.md) - Producing the artifact this verifies
- [pitfalls.md](pitfalls.md) - False positives and other gotchas

---

## Checklist

- [ ] Build success was not claimed from `build.sh` exit 0 alone — at minimum the artifact's existence was verified
- [ ] `Player.log` path was derived from the OS table plus the project's actual `companyName`/`productName`, not guessed
- [ ] When a launch smoke test was requested: the player was actually run briefly and `verify_player_log.sh` passed on its log
- [ ] Any assembly literal scan targeted a specific, deliberate string and used its UTF-16LE encoding — no general substring linting
- [ ] pdb debug-directory path matches were excluded from failure criteria, not reported as leaks

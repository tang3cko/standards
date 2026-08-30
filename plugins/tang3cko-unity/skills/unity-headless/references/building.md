# Headless Player Builds

`scripts/build.sh <project_path> <ExecuteMethod>` runs this end-to-end, the same way `test.sh` does for tests: lock check, version detection, invocation. The one piece it can't provide for you is the Editor-only entry point itself — that's inherently project-specific (scenes, output path, scripting backend), which is why it lives as a template you copy in, not a generated script.

---

## `assets/BuildScript.cs.template`

Copy into `<project>/Assets/Editor/BuildScript.cs` and resolve every `TODO`:

```csharp
using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    public static void BuildDevelopment() => Build(development: true);
    public static void BuildRelease() => Build(development: false);

    private static void Build(bool development)
    {
        var scenes = new[]
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/Main.unity",
        };
        // ... BuildPlayerOptions, scripting backend, BuildPipeline.BuildPlayer,
        // EditorApplication.Exit(1) on failure - see the full template file.
    }
}
```

Points that matter, with the reasoning:

- **Explicit scene list, never `EditorBuildSettings.scenes`** — that's local Editor state that drifts between machines/CI runners; an explicit array in code is reproducible everywhere the repo is checked out
- **`EditorApplication.Exit(1)` on failure is not optional** — a batchmode process that reaches the end of `-executeMethod` exits 0 by default even if `BuildPipeline.BuildPlayer` reported failure. Without this call, `build.sh` will report "build finished (exit 0)" for a build that actually failed. The template includes it; keep it when customizing
- **Scripting backend per target, chosen explicitly** — Mono2x for fast Development Build iteration, IL2CPP for release (and required outright on some platforms, e.g. iOS — don't assume Mono2x is always a valid release option)
- **`Editor/` folder placement** — code under any `Editor/`-named folder compiles Editor-only and never ships in the player build

---

## Invoking it

```bash
bash ${CLAUDE_SKILL_DIR}/scripts/build.sh <project_path> BuildScript.BuildDevelopment
bash ${CLAUDE_SKILL_DIR}/scripts/build.sh <project_path> BuildScript.BuildRelease
```

The `ExecuteMethod` argument is a passthrough — the same `build.sh` drives dev smoke builds and release builds by selecting which static method to invoke. `build.sh` itself only reports the process exit code; that's why the entry point's own `EditorApplication.Exit(1)` matters (see above) and why the next step is always artifact verification.

---

## References

- [../SKILL.md](../SKILL.md) - Core Rules and Editor-state boundary
- [verification.md](verification.md) - Verifying the artifact this produces
- [pitfalls.md](pitfalls.md) - Project lock, licensing client issues

---

## Checklist

- [ ] The entry point lives under an `Editor/`-named folder (e.g. `Assets/Editor/BuildScript.cs`) so it never ships in the player
- [ ] Scene list is an explicit array in code — not read from `EditorBuildSettings.scenes`
- [ ] `EditorApplication.Exit(1)` on `BuildResult != Succeeded` survived any customization of the template
- [ ] Scripting backend is chosen explicitly per target (Mono2x for dev iteration, IL2CPP for release / platforms that require it)
- [ ] No leftover `TODO` markers in the customized `BuildScript.cs`
- [ ] Build was invoked via `build.sh`, and artifact verification (`verification.md`) is the next step — exit 0 alone was not reported as success

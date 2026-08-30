# Module Setup

Load this before writing the first test in a module, or when a test file fails to compile because the module cannot see `CQTest.h`.

A test is only ever compiled if the module depends on the test framework and the build configuration defines the guard macro as `1`. Neither is automatic — both need something explicit in the module's `.Build.cs` and in the test file itself.

## Depending on CQTest

CQTest lives in `Developer/CQTest` and is a normal engine module named `CQTest` (declared by `CQTest.Build.cs`, `public class CQTest : ModuleRules`). Add it to the owning module's `PrivateDependencyModuleNames`, alongside whatever else the test needs — `Core`, `CoreUObject`, `Engine` at minimum:

```csharp
PrivateDependencyModuleNames.AddRange(
    new string[] {
        "Core",
        "CoreUObject",
        "Engine",
        "CQTest"
    }
);
```

This is a normal module dependency, not conditional on `Target.bBuildEditor` or similar — `CQTest` itself only pulls in editor-only modules internally, guarded by `Target.bBuildEditor` in its own `.Build.cs`. A game module that depends on `CQTest` still builds correctly for Shipping; the macro guard described below is what actually keeps the test code out, not the module dependency.

## `WITH_DEV_AUTOMATION_TESTS` and `WITH_AUTOMATION_TESTS`

These are not `#define`d in engine headers — `Runtime/Core/Public/Misc/AutomationTest.h` only supplies a fallback (`#ifndef WITH_AUTOMATION_TESTS #define WITH_AUTOMATION_TESTS (WITH_DEV_AUTOMATION_TESTS || WITH_PERF_AUTOMATION_TESTS) #endif`) for the case where nothing set it first. The values that matter are compile definitions UnrealBuildTool adds per target, in `UEBuildTarget.SetupGlobalEnvironment` (`Programs/UnrealBuildTool/Configuration/UEBuildTarget.cs`):

```csharp
bool bCompileDevTests = (Configuration != UnrealTargetConfiguration.Test && Configuration != UnrealTargetConfiguration.Shipping);
bool bCompilePerfTests = bCompileDevTests;
...
GlobalCompileEnvironment.Definitions.Add("WITH_DEV_AUTOMATION_TESTS=" + (bCompileDevTests ? "1" : "0"));
GlobalCompileEnvironment.Definitions.Add("WITH_PERF_AUTOMATION_TESTS=" + (bCompilePerfTests ? "1" : "0"));
```

So `WITH_DEV_AUTOMATION_TESTS` is `1` for every configuration except `Test` and `Shipping`, and `0` there — Debug, DebugGame, Development and Editor all get it. `TargetRules.bForceDisableAutomationTests` (checked earlier in the same method) can force both to `0` regardless of configuration; this is what `TestTargetRules.SetupCommonProperties` does for a Low Level Tests target, which is why LLT and the automation framework never coexist in one binary. `TargetRules.bForceCompileDevelopmentAutomationTests` and `bForceCompilePerformanceAutomationTests` do the opposite — force them to `1` even in `Test` or `Shipping`, for a target that specifically wants tests there.

## Guarding a test file

Wrap the whole file in the macro, matching what the engine's own `Private/Tests/` files do — for example `Runtime/Engine/Private/Tests/InstancedStaticMeshTest.cpp` opens with `#if WITH_DEV_AUTOMATION_TESTS && WITH_EDITOR` and closes with the matching `#endif`:

```cpp
// Private/Tests/LobbyRulesTest.cpp
#include "CQTest.h"

#if WITH_DEV_AUTOMATION_TESTS

TEST_CLASS(LobbyRulesTests, "Project.Lobby.Rules")
{
    // ...
};

#endif // WITH_DEV_AUTOMATION_TESTS
```

Add `&& WITH_EDITOR` only when the test genuinely needs editor-only APIs (asset registry, editor subsystems, and similar) — do not add it by default, since it removes coverage from Development builds that are not the editor.

With both the module dependency and the file guard in place, the file compiles in Debug/DebugGame/Development/Editor and drops out entirely in Test and Shipping — the compiler never sees the body, not even as dead code, because the preprocessor removes it before the compiler runs.

## Checklist

- [ ] The module's `.Build.cs` adds `CQTest` to `PrivateDependencyModuleNames`
- [ ] Every file under `Private/Tests/` is wrapped in `#if WITH_DEV_AUTOMATION_TESTS` / `#endif`
- [ ] `&& WITH_EDITOR` is added to the guard only when the test genuinely needs editor-only APIs
- [ ] No assumption that `WITH_DEV_AUTOMATION_TESTS` is defined by an engine header — it is a UnrealBuildTool compile definition, set per target configuration
- [ ] If a target sets `bForceDisableAutomationTests`, tests in modules it links are understood to compile out entirely, not just skip at runtime

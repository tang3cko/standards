# Modules, Dependencies and Includes

Load this when creating a module, editing a `.Build.cs`, or sorting includes.

Unreal has no per-assembly access control the way C# does. A module boundary is enforced by *include visibility*, not by a compiler-checked reference list, and the only real lever you have is which directory a header sits in and which list a dependency goes in.

## Directories

| Directory | Visible to | Notes |
|---|---|---|
| `Public/` | This module and anything depending on it | The module's API |
| `Private/` | This module only | Everything else |
| `Internal/` | This module, and modules explicitly granted access | Cross-module includes inside one plugin without exposing them widely |
| `Classes/` | Legacy | **Do not add to it.** The Directory Structure page (`dev.epicgames.com/documentation/en-us/unreal-engine/unreal-engine-directory-structure`) says it "should not be added to in the future" |

These names have **nothing to do** with C++ `public` / `private`. They control availability of code to other modules.

For every folder you create under `Public/`, create the matching folder under `Private/`.

A module nobody will depend on — your game's primary module is the usual case — does not need the split at all. Code outside those folders behaves as if it were private.

## Public versus private dependencies

```
Does a PUBLIC header of this module name a type from that module?
├─ No  → PrivateDependencyModuleNames
└─ Yes → Can a forward declaration replace the include?
         ├─ Yes → forward declare, then PrivateDependencyModuleNames
         └─ No  → PublicDependencyModuleNames
```

Private is the default. Public dependencies propagate transitively to every dependent module, which is how a project ends up rebuilding the world for a one-line change. The Modules page (`dev.epicgames.com/documentation/en-us/unreal-engine/unreal-engine-modules`) puts it directly: "Private dependencies are preferred wherever possible, as they can reduce your project's compile times."

`PrivateIncludePathModuleNames` grants the include path without the link dependency — enough to forward declare, not enough to call.

Do not verify this by reading. Build with `-DisableUnity` and no PCH, which is also the only reliable way to prove that every header includes what it uses.

## When to split a module

Start with **one runtime module per project**. Split when there is a concrete reason, not preemptively: editor-only code mixed into runtime code, a system genuinely reused across projects, compile times that have become the bottleneck, or a subsystem with no dependencies on the rest.

A module holding one or two classes is not worth its overhead.

Dependencies flow one way. There is a mechanism for declaring a circular one — `ModuleRules.CircularlyReferencedDependentModules` — and Epic labels it in its own doc comment: **"Only for legacy reason, should not be used in new code."** It also only applies to modules already listed as a public or private dependency; it is not a general escape hatch.

Declaring one gets you a **warning**, not an error, from `UEBuildModuleCPP` (`Programs/UnrealBuildTool/Configuration/UEBuildModuleCPP.cs`): "Support for circular references is being phased out; please do not introduce new ones." Neither the warning nor the existence of the API is permission. If two modules need each other's types, the shared types belong in a third module both depend on.

Editor modules may depend on runtime modules. **Never the reverse.**

Plugins can only depend on plugins or modules at the same level or higher, and — unlike modules — plugins can contain assets. That, not code size, is usually what forces something to become a plugin. Most plugins need only one module.

`LoadingPhase` orders modules coarsely — the values are `ModuleLoadingPhase` in `Programs/UnrealBuildTool/ModuleDescriptor.cs`. Within a single phase the order is **not deterministic**; if one module must be up before another, call `LoadModule`/`LoadModuleChecked` explicitly.

Two things that are reported repeatedly but that this file cannot source from the engine, so treat them as leads rather than rules: persistent "cannot find C++ class in plugin" errors from the editor are usually answered by moving the plugin to `PreDefault`, and adding a plugin to the `.uproject` without rebuilding the editor target can take the editor down during startup with nothing useful printed. Record either in the project's own docs if you hit it, with what you actually observed.

## About a "pure logic core" module

The instinct to isolate engine-independent logic into a Core module is sound, and Unreal supports it only partially.

The dependency direction is real and the engine lives by it. The `Core` class in `Runtime/Core/Core.Build.cs` names neither `Engine` nor `CoreUObject` in its dependency lists; the `CoreUObject` class in `Runtime/CoreUObject/CoreUObject.Build.cs` lists `Core`, `TraceLog`, `CorePreciseFP` and `Json` in `PublicDependencyModuleNames`; the `Engine` class in `Runtime/Engine/Engine.Build.cs` lists `Core` and `CoreUObject` in its `PublicDependencyModuleNames`.

Shipped runtime modules do live above `Core` without `Engine`. `Runtime/MathCore/MathCore.Build.cs` and `Runtime/CorePreciseFP/CorePreciseFP.build.cs` list only `Core` in `PublicDependencyModuleNames`; `Runtime/Json/Json.Build.cs` lists `Core` and `RapidJSON` publicly plus `TraceLog` privately. So an engine-free layer is a real thing that exists — but note that even these are not single-dependency modules, and "depends on Core only" is worth checking rather than assuming.

What does **not** exist is enforcement. `bCompileAgainstEngine` and `bCompileAgainstCoreUObject` are declared as properties on `TargetRules` (`Programs/UnrealBuildTool/Configuration/Rules/TargetRules.cs`), not on `ModuleRules`, and both carry a `[RequiresUniqueBuildEnvironment]` attribute. Neither name appears anywhere in `Programs/UnrealBuildTool/Configuration/Rules/ModuleRules.cs` — search it and get zero hits, against several in `TargetRules.cs`. They are whole-target switches that every module in the build shares, so there is no way to declare "this module never sees Engine" and have UBT check it. The boundary is a convention you maintain by keeping that module's dependency list short and reviewing it.

Two traps when you try:

- **A clean `PublicDependencyModuleNames` does not mean engine-free.** The `GeometryCore` class in `Runtime/GeometryCore/GeometryCore.Build.cs` lists only `Core` in `PublicDependencyModuleNames` while naming `"Engine", // For GPUSkinPublicDefs.h` in `PublicIncludePathModuleNames`. Read all the lists, not the first one.
- **Reflection costs you the boundary immediately.** `GENERATED_BODY()` is defined in CoreUObject's `ObjectMacros.h`, so the moment a "pure logic" type becomes a USTRUCT it depends on CoreUObject. An engine-free layer must be plain C++ types, with reflection added at the layer above.

**Lyra does not have such a layer, and it is not an oversight to copy around.** Its entire game is one module with no `Public`/`Private` split, organised into feature folders, with the modular parts pushed into Game Feature plugins. `Source/` holds a `LyraGame.Build.cs` and a `LyraEditor.Build.cs` — the server variants are *targets*, not modules — and every plugin module depends on `Engine` directly. Classes under `System/` derive from `UBlueprintFunctionLibrary` and `UPrimaryDataAsset`, which are Engine types.

Lyra is not bundled with an installed engine, so this was not checked against anything on this machine. It was read from a third-party GitHub mirror — **not an Epic repository**, and therefore capable of having diverged from what Epic ships. Re-verify against whichever Lyra revision you actually have, from Epic's own distribution. Do not carry forward specific file counts, folder counts or a commit hash from a past check; confirm the shape of the claim (no `Public`/`Private` split, every plugin depends on `Engine`) instead of a number.

So: separate a pure-logic layer when the logic is genuinely testable in isolation and you are willing to keep it plain C++. Do not claim the engine or its flagship sample endorses the pattern, and do not expect the build system to hold the line for you.

## Includes

- `#pragma once` in every header.
- Include what you use. **Never rely on a header included indirectly** by something else you include.
- Be fine-grained. Do not include monolithic headers such as `Core.h`.
- A `.cpp` includes its matching header first — UBT warns if it does not.
- `Engine/Engine.h` (for `UEngine`/`GEngine`) is a different file from the monolithic `Runtime/Engine/Public/Engine.h`. Include the former.
- Prefer a forward declaration to an include, in headers especially — it is also what lets a dependency stay private.

The IWYU page (`dev.epicgames.com/documentation/en-us/unreal-engine/include-what-you-use-iwyu-for-unreal-engine-programming`) says "IWYU mode is disabled by default for games and game plugins; however, IWYU mode is enabled by default for the Engine and Engine plugins", and tells you to check `PCHUsage` to see which you have. **Do not read `PCHUsage` as the IWYU switch.** In `ModuleRules.PCHUsage` (`Programs/UnrealBuildTool/Configuration/Rules/ModuleRules.cs`), any module on `BuildSettingsVersion.V2` or later returns `UseExplicitOrSharedPCHs` regardless of whether it is an engine module or a game module; the engine/game split the page describes only appears on the pre-V2 branch. Separately, `ModuleRules.IWYUSupport` defaults to `IWYUSupport.Full` for everything, and `TargetRules.bIWYU` is tied to the intermediate environment being an explicit IWYU build.

What survives all that is the discipline, not the switch: include what you use, and **verify it with a non-unity build with PCHs disabled** rather than by reading a setting.

## Checklist

- [ ] Every dependency is private unless a public header of this module names the type, and forward declarations were tried first
- [ ] The module split was justified by a concrete reason, not created preemptively
- [ ] No circular module dependency; shared types were extracted rather than cross-referenced
- [ ] No runtime module depends on an editor module
- [ ] Nothing new was added under `Classes/`
- [ ] Every folder under `Public/` has its counterpart under `Private/`
- [ ] No load-order assumption within a single `LoadingPhase`
- [ ] Every header compiles standalone — verified with a non-unity, no-PCH build rather than by reading
- [ ] `.cpp` files include their own header first, and no monolithic header is included

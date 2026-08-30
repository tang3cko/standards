# Core and Shell

Load this when separating engine-free logic from engine-facing code.

Separating engine-free logic from engine-facing code is worth doing in Unreal, and **the toolchain will not enforce it for you.** Everything below follows from that one fact. The module mechanics behind this — directory visibility, public/private dependencies, `.Build.cs` details — live in `unreal-core/references/module-and-includes.md`. This file is about the architectural judgement: when to pay for the separation and what it costs.

## What is real

The dependency direction exists and the engine lives by it: `Runtime/Core` names neither `Engine` nor `CoreUObject` in its `.Build.cs`; `CoreUObject` depends on `Core`; `Engine` depends on both. Shipped runtime modules depend on `Core` alone — `MathCore`, `Json`, `CorePreciseFP`. `TraceLog` does not even take a link dependency, only an include path.

So an engine-free layer is a thing that exists in practice, at engine scale.

## What is not

`bCompileAgainstEngine` and `bCompileAgainstCoreUObject` are declared on **`TargetRules`, not `ModuleRules`** (`Programs/UnrealBuildTool/Configuration/Rules/TargetRules.cs`), and both require a unique build environment — they are whole-target switches shared by every module in the build. There is no way to declare "this module never sees Engine" and have UBT check it.

So a violation is never a compile error. The boundary holds only as long as someone keeps reading the dependency lists — put that in review, because nothing else will catch it.

## Two traps

**A clean `PublicDependencyModuleNames` does not mean engine-free.** `GeometryCore.Build.cs` (`Runtime/GeometryCore/`) lists only `Core` there while naming `Engine` in `PublicIncludePathModuleNames`, with a comment saying which header it wanted. Read every list — public, private, and the include-path variants.

**Reflection costs you the boundary immediately.** `GENERATED_BODY()` is defined in CoreUObject's `ObjectMacros.h`. The moment a "pure logic" type becomes a `USTRUCT`, that layer depends on CoreUObject. An engine-free layer must be plain C++ types, with reflection added one layer up.

That is a real trade: you give up `UPROPERTY` editing, Blueprint exposure and replication in the core layer. Decide deliberately, per system, rather than declaring a project-wide Core module and discovering the cost later.

## Lyra is not evidence for this pattern

Lyra is not bundled with an installed engine, so nothing about it is checkable from a project checkout — the description below was read from a mirror on GitHub, and you should read whichever revision you actually have before citing it.

With that limit stated: Lyra's game is one module, with no `Public`/`Private` split, and its plugin modules depend on `Engine` directly. Its modularity is horizontal — Game Feature plugins that load and unload — not layered. Copying it gives you feature isolation, not an engine-free core.

Confirm that shape rather than carrying forward file counts, plugin counts or a commit hash from a past check. See `modularity.md` for what Lyra's structure is evidence for, and `unreal-core/references/module-and-includes.md` for the module mechanics.

## When to do it anyway

Separate a pure-logic layer when all three hold:

1. The logic is genuinely decidable without engine state — rules, scoring, state machines, validation.
2. You want it tested in the sub-millisecond tier rather than behind editor startup. This is usually the real motive, and it is a good one.
3. You are willing to keep it plain C++ and add reflection in the shell above.

Otherwise keep the logic in a Component and test it there, accepting the cost.

## Shape

```
FLobbyRules            plain struct, static functions   <- tested directly
ULobbySubsystem        translates decisions into calls  <- reviewed, not unit tested
EOS SDK                not yours                        <- manual procedure
```

The shell contains no branching worth asserting on. When conditions start appearing in it, a decision has leaked back down.

## Checklist

- [ ] The separation was justified by testability, not by taste for layers
- [ ] The core layer is plain C++ — no `USTRUCT`, no `UObject`, nothing reflected
- [ ] Every dependency list of the core module was read, including the include-path variants
- [ ] No claim is made that UBT enforces the boundary, or that Lyra demonstrates it, and no unverifiable Lyra numbers are quoted
- [ ] The shell holds no branching logic worth asserting on
- [ ] What the boundary costs — reflection, Blueprint exposure, replication in that layer — was accepted knowingly

# Tests That Need a World

Load this when a test needs a world, a map, or PIE.

Everything here is expensive and most of it is avoidable. Read `patterns.md` first and confirm the logic genuinely cannot be lifted out.

## A world without a map

`FTestWorldWrapper` (`Runtime/Engine/Public/Tests/AutomationCommon.h`) builds and tears down a world for a test: `CreateTestWorld`, `DestroyTestWorld`, `BeginPlayInTestWorld`, `TickTestWorld`, `EndPlayInTestWorld`.

Three properties matter:

- **It does not support PIE worlds.** `FTestWorldWrapper::CreateTestWorld` (`Runtime/Engine/Private/Tests/AutomationCommon.cpp`) switches on the world type: `Game` gets a `UGameInstance`; `Editor`, `EditorPreview`, `GamePreview`, `GameRPC` and `Inactive` are accepted without one; `case EWorldType::PIE: // TODO Implement if needed` falls into `ensureMsgf(false, TEXT("Unsupported world type in CreateTestWorld"))`. PIE is the only unsupported type — not "Game and Editor only".
- **The world does not tick itself.** The same function creates it with `SetShouldTick(false)` and roots it; you drive it with `TickTestWorld`. That is a feature — time only advances when you say so.
- **Teardown collects garbage only if you ask.** `FTestWorldWrapper::DestroyTestWorld` runs `CollectGarbage` and `CheckAndHandleStaleWorldObjectReferences` only inside its `bForceGarbageCollect` branch, and the destructor calls `DestroyTestWorld(false)`. So the leak check does **not** run by default — pass `true` when you want it.

Console variables set for a test go through `FTestConsoleVariable`, whose class comment in `AutomationCommon.h` warns that "race conditions are possible when multiple `FTestConsoleVariable` objects refer to the same Console Variable". One owner per variable per test.

## A test in a real level

`AFunctionalTest` is an `AActor` you place in a map. Its result is one of `Default`, `Invalid`, `Error`, `Running`, `Failed`, `Succeeded`. Run them with the `ftest` console command.

Two traps:

**The test's name comes from the actor's label and the map it sits in.** `AFunctionalTest::GetBeautifiedName` (`Developer/FunctionalTesting/Private/FunctionalTest.cpp`) builds it as `Project.Functional Tests.` + a suite name derived from the level's package (`MapPackageToAutomationPath`) + `.` + `GetActorLabel()`. So **two things rename the test**: renaming the actor in the editor, and moving or renaming the map. Either makes a filter or CI list naming it go stale with no error.

**Latent command ordering.** `Developer/FunctionalTesting/Private/Tests/ClientFuncTestPerforming.cpp` carries the comment `// !!! ANY COMMANDS ADDED HERE WILL RUN BEFORE FStartFTestOnMap !!!` for exactly this reason. A command queued in the wrong place runs before the map is up.

## PIE

Starting PIE from a test is a dedicated path. `FStartPIEForAutomationCommand` (`Editor/UnrealEd/Public/Tests/AutomationEditorCommon.h`) exists for it, and its own comment states what it buys you: it "handles failure at all stages: if PIE fails to start, starts but ends before the world is ready" and "all failure paths are guarded by PIEStartTimeoutSeconds". It also defers the PIE start so latent waits can precede it. There is a simpler `FStartPIECommand` in the same header for cases that need none of that.

Use these rather than driving PIE by hand; the hand-rolled version hangs instead of failing.

For an online game, PIE is also where a whole class of fault appears that packaged builds never show — an online services instance must carry the world context handle, or PIE cannot host, while a packaged build has one world and resolves regardless. A test that only ever ran packaged will not catch it.

## Maps

A map that is not in `UProjectPackagingSettings::MapsToCook` (`Developer/DeveloperToolSettings/Classes/Settings/ProjectPackagingSettings.h`) is not cooked, and the failure appears only in a packaged build. If a test depends on a map, that map must be listed.

Seamless travel needs `UGameMapsSettings::TransitionMap` (`Runtime/EngineSettings/Classes/GameMapsSettings.h`) configured, or the engine substitutes a bare world. A test crossing a travel boundary and asserting on what survived needs both configured, or it is asserting on the transition, not the feature.

## What still cannot be tested here

A world is not a session. Two players means two processes and two Epic accounts; that is Gauntlet's territory or a manual procedure, not `FTestWorldWrapper`. See `frameworks.md`.

## Checklist

- [ ] The logic under test genuinely needs a world — `patterns.md` was applied first
- [ ] No test asks `FTestWorldWrapper` for a PIE world
- [ ] Time advances only through explicit `TickTestWorld` calls
- [ ] Nothing is left in the world at teardown, and `DestroyTestWorld(true)` was called where the leak check matters — it does not run on the default path
- [ ] Functional Test filters do not silently depend on an actor label that can be renamed unnoticed
- [ ] Latent commands are queued where they will run after the map is up
- [ ] PIE is started through the editor-common commands, with their timeout, not by hand
- [ ] Any map a test depends on is in `MapsToCook`, and any travel test has a `TransitionMap`

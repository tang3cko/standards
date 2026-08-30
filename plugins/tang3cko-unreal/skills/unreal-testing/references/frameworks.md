# The Test Frameworks

Load this at the start of any writing or strategy path.

Unreal ships five things that call themselves testing. They are not alternatives to one another — they sit at different costs and answer different questions. Choosing wrongly is how a suite ends up taking twenty minutes to tell you a pure function works.

## The five

| Framework | Lives in | Needs | Answers |
|---|---|---|---|
| **CQTest** | `Developer/CQTest` | Editor or game process | Does this class behave? Default choice |
| **Automation** | `Runtime/Core` (`AutomationTest.h`) | Editor or game process | Same, older macro API |
| **Functional Test** | `Developer/FunctionalTesting` | A loaded map | Does this behave in a real level? |
| **Low Level Tests** | `Developer/LowLevelTestsRunner` | Its own build target | Does this compile-and-run without the engine? |
| **Gauntlet** | `Programs/AutomationTool/Gauntlet` | Several processes | Does a session of client + server work? |

### CQTest — the default

Epic's xUnit-shaped framework. `TEST_CLASS` gives setup, teardown, shared state and grouping; `BEFORE_EACH` / `AFTER_EACH` run per test, optional `BEFORE_ALL` / `AFTER_ALL` run once. Member variables persist during a test and reset between tests. Latent steps are supported and each step completes before the next begins.

Its one real restriction is narrower than commonly stated: you *may* call `AddCommand` inside `AFTER_EACH` — the official example does exactly that. What you must not do is call `FMapTestSpawner::AddWaitUntilLoadedCommand` from inside a latent action. See `cqtest.md` for the exact wording and the macro-name slip in it.

### Automation — keep, don't extend

`IMPLEMENT_SIMPLE_AUTOMATION_TEST` and friends. Still fully supported and still what most engine tests use. Prefer CQTest for new work because setup/teardown and shared state are structural rather than hand-rolled.

**The flags API changed and the documentation did not.** Masks moved out of the enum into free constants with an underscore. `Runtime/Core/Public/Misc/AutomationTest.h` declares `inline constexpr EAutomationTestFlags EAutomationTestFlags_ApplicationContextMask = ...`; the scoped spelling `EAutomationTestFlags::ApplicationContextMask` returns **0 hits** in that header, as do `ATF_Game` and `UE_DEPRECATED.*AutomationTestFlags`. With no deprecated aliases, an old sample is a hard compile error rather than a warning.

The Automation Spec page still carries the old spelling — its `BEGIN_DEFINE_SPEC` example reads `EAutomationTestFlags::ProductFilter | EAutomationTestFlags::ApplicationContextMask` (`dev.epicgames.com/documentation/en-us/unreal-engine/automation-spec-in-unreal-engine`). Take samples from engine headers.

Automation Spec is a BDD-shaped variant of the same thing. Its files take the `.spec.cpp` extension. See `cqtest.md` for the naming, including where Epic's own advice and Epic's own files disagree.

### Functional Test — for things that need a level

`AFunctionalTest` is an `AActor` you place in a map. The engine discovers it through asset tags, and the test's name is built from the actor's label — so **renaming the actor renames the test**, and any filter naming it goes stale silently. Run them with the `ftest` console command.

Latent commands added at the wrong point run before the test starts; the engine's own source carries a shouted comment about it. See `world-tests.md`.

### Low Level Tests — not on a Launcher engine

Catch2-based, built as its own `TargetType.Program`, monolithic, console. The vendored copy under `ThirdParty/Catch2` carries group lifecycle macros — `GROUP_TEST_CASE`, `GROUP_BEFORE_ALL`, `GROUP_AFTER_ALL` and siblings, in `catch_test_macros.hpp` — added to Epic's copy of the library rather than to a wrapper around it. Whether upstream Catch2 has since grown equivalents is not checked here; assume the vendored copy is the contract.

`Programs/UnrealBuildTool/Configuration/Rules/TestTargetRules.cs` — the `TestTargetRules.SetupCommonProperties` method sets `Type = TargetType.Program`, `LinkType = TargetLinkType.Monolithic`, `bIsBuildingConsoleApplication = true` and `bForceDisableAutomationTests = true` — so **LLT and the automation framework do not coexist in one target.**

On a Launcher-installed engine an explicit LLT target does not build. Three independent blockers: installed engines reject unique build environments (`RulesAssembly.CreateTargetRulesInstance` in `Programs/UnrealBuildTool/Configuration/Rules/RulesAssembly.cs` throws "Targets with a unique build environment cannot be built with an installed engine."), the `TargetRules.BuildEnvironment` property in `Programs/UnrealBuildTool/Configuration/Rules/TargetRules.cs` returns `TargetBuildEnvironment.Unique` for a `TargetType.Program` that sits under the project directory — which is where an LLT target goes — and `LowLevelTestsRunner` is not shipped precompiled. A workaround exists — declare it as a Game target with your own `main()` — but it leaves `WITH_ENGINE` inconsistent between engine objects and project translation units. Do not plan around LLT unless the project builds the engine from source.

The adapter (`Runtime/Core/Public/Misc/LowLevelTestAdapter.h`) lets one file compile either way. Three of its degradations matter:

- `SECTION` becomes an `AddInfo` call — `#define SECTION(Text) FAutomationTestFramework::Get().GetCurrentTest()->AddInfo(TEXT(Text));`. Its grouping stops meaning anything.
- `SKIP` expands to **nothing at all** — `#define SKIP(Message)` with an empty body. A silently skipped test is a test you believe you have.
- `TEST_CASE` generates its class name from `__COUNTER__` (via the `LLT_JOIN` macro) and only its *string* form from `__FILE__`/`__LINE__`. The header's own comment immediately above `TEST_CASE` says the generated names "only work when used inside unique namespace in the same compilation unit" and to "Use TEST_CASE_NAMED instead". Do that.

Also note the `IMPLEMENT_SIMPLE_AUTOMATION_TEST_PRIVATE_LLT` macro, which rewrites `::` to `.` in the pretty name — the `A::B::C` hierarchy in an LLT test name becomes the dotted path the automation framework filters on.

### Gauntlet — for sessions, not for classes

The only thing here that starts several processes. Gauntlet imposes no game-side test framework; a `GauntletTestController` is offered for driving an instance, and `EndTest(int32 ExitCode)` ends the run.

The documentation page (`dev.epicgames.com/documentation/en-us/unreal-engine/gauntlet-automation-framework-in-unreal-engine`) describes the concept and contains **no command line at all** — the whole page is a few paragraphs, and the strings `RunUAT`, `RunUnreal`, `-test=` and `-Tests=` appear nowhere in it. The command line has to come from the source.

Both spellings work, and **`-test=` is the one checked first**. In `UnrealTestOptions.ParametersWereApplied` (`Programs/AutomationTool/Gauntlet/Unreal/Base/Gauntlet.UnrealTestContext.cs`):

```csharp
List<string> TestArgStrings = Params.ParseValues("test=");
...
if (TestArgStrings.Count == 0)
{
    TestArgStrings = Params.ParseValues("tests=");
}
```

`RunUnreal.cs` declares a `Tests` parameter with `[ParamHelp("Tests", "List of gauntlet tests to run", Required = true, MultiSelectSeparator = ",")]`, but that is help metadata for `-listargs`, not the parser. The same file carries the comment `// make -test=none implicit if no test is supplied`, which is written against the singular form.

## The one to avoid

`IMPLEMENT_NETWORKED_AUTOMATION_TEST` (defined in `Runtime/Core/Public/Misc/AutomationTest.h`) appears in exactly one file outside its own definition: `Runtime/Engine/Private/Tests/NetworkAutomationTests.cpp`. It asserts nothing, carries a `// TODO: Ask JB about this....` comment, and the map it opens — `AutomationTest` — does not exist anywhere in `Engine/Content` (searching for `AutomationTest*.umap` there returns **0 files**). It is abandoned. Multi-process testing is Gauntlet's job.

## Cost, by shape rather than number

Editor startup dominates everything a CQTest or Automation run does; the test body itself is cheap by orders of magnitude in comparison, so much so that the per-test cost is close to noise next to the per-invocation cost. Re-measure on the actual machine before quoting a figure — these numbers move with hardware, OS, and whether Unreal Trace Server is already warm — but the shape holds: pay editor startup once, then run everything you can in that one process.

That ratio is the argument for the Humble Object pattern. Every assertion you can move to a plain type is an assertion that does not pay for editor startup to be checked. It is also the argument against splitting a suite into many separate editor invocations.

## Checklist

- [ ] The chosen framework is the cheapest one that can answer the question
- [ ] No new test uses the Automation macros where CQTest would do
- [ ] No code sample was copied from the documentation site without checking its identifiers against 5.8 headers
- [ ] No design depends on `IMPLEMENT_NETWORKED_AUTOMATION_TEST`, or on an LLT target under a Launcher-installed engine
- [ ] Where the LLT adapter is used, no reliance is placed on `SKIP` or `SECTION`, and `TEST_CASE_NAMED` is used instead of `TEST_CASE`
- [ ] Functional Test names are understood to derive from actor labels, and no filter hard-codes one without a way to notice it going stale

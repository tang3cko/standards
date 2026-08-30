# Official Documentation Errata

Load this before quoting `dev.epicgames.com` as authority.

Epic's documentation is the right starting point and is wrong in specific, checkable places. Each entry below was confirmed twice: the page was fetched and the sentence read, and the corresponding UE 5.8 engine source was opened. Where they disagree, **the source wins** — it is what compiles.

The engine paths below are relative to `Engine/Source/` in an installed 5.8.

## 1. Programming Subsystems omits `UWorldSubsystem`

`dev.epicgames.com/documentation/en-us/unreal-engine/programming-subsystems-in-unreal-engine` lists four supported lifetimes — Engine, Editor, GameInstance, LocalPlayer — in a table introduced by "Currently supported subsystems lifetimes include:".

`Runtime/Engine/Public/Subsystems/Subsystem.h` carries near-identical wording and lists **five**, including `World -> inherit UWorldSubsystem`. `UWorldSubsystem` and `UTickableWorldSubsystem` both exist in `Runtime/Engine/Public/Subsystems/WorldSubsystem.h`.

Only the World row is missing. Treat the page as an incomplete list, not as a statement that world subsystems are unsupported.

## 2. Smart Pointers states the wrong default thread-safety mode

`dev.epicgames.com/documentation/en-us/unreal-engine/smart-pointers-in-unreal-engine`'s Thread Safety section says "By default, Smart Pointers are only safe to access on a single thread" and instructs you to write `TSharedPtr<T, ESPMode::ThreadSafe>` explicitly.

`Runtime/Core/Public/Templates/SharedPointerFwd.h` declares every one of `TSharedRef`, `TSharedPtr`, `TWeakPtr` and `TSharedFromThis` with `ESPMode Mode = ESPMode::ThreadSafe`, above the comment "By default, thread safety features are turned on."

**The page is backwards.** Writing `ESPMode::ThreadSafe` explicitly is harmless but redundant; believing you have a fast non-atomic pointer by default is not. If you want the cheaper mode, you must ask for `ESPMode::NotThreadSafe`.

## 3. Asserts overstates where `verify` runs

`dev.epicgames.com/documentation/en-us/unreal-engine/asserts-in-unreal-engine` says "Verify macros operate fully in Debug, Development, Test, and Shipping Editor builds".

In `Runtime/Core/Public/Misc/Build.h`, the `UE_BUILD_TEST` block defines `DO_CHECK` as `USE_CHECKS_IN_SHIPPING`, which the same file defaults to `0`. With `DO_CHECK == 0`, `Runtime/Core/Public/Misc/AssertionMacros.h` reduces `verify` to evaluating its expression and nothing else — no halt, no log.

So Test is wrong. The same page contradicts itself two sentences later: "In all other cases, Verify macros will evaluate their expressions, but will not halt execution or output text to the log." That later sentence is the accurate one. The page's separate claim that check macros operate only in Debug and Development matches the source.

The distinction that *does* hold, and is the reason to prefer `verify`: `check` removes its expression entirely when disabled, so side effects disappear with it. `verify` keeps evaluating.

## 4. Multicast Delegates claims delegates can be triggered remotely

`dev.epicgames.com/documentation/en-us/unreal-engine/multicast-delegates-in-unreal-engine`'s opening paragraph says multi-cast delegates "can be loaded/saved, and triggered remotely".

`Runtime/CoreUObject/Private/UObject/PropertyMulticastDelegate.cpp` and its single-cast counterpart `PropertyDelegate.cpp` both implement `NetSerializeItem` as one comment and `return 1;`:

> Do not allow replication of delegates, as there is no way to make this secure (it allows the
> execution of any function in any object, on the remote client/server)

**Delegates do not replicate.** Never design a network path around one. Cross-machine notification goes through replicated properties with `ReplicatedUsing`, or through RPCs.

## 5. Code samples that no longer compile

The Automation Spec page (`dev.epicgames.com/documentation/en-us/unreal-engine/automation-spec-in-unreal-engine`) has a `BEGIN_DEFINE_SPEC` example using `EAutomationTestFlags::ProductFilter | EAutomationTestFlags::ApplicationContextMask`.

`EAutomationTestFlags::ApplicationContextMask` does not exist in 5.8. The masks were moved out of the enum into free constants with an underscore — `Runtime/Core/Public/Misc/AutomationTest.h` declares `inline constexpr EAutomationTestFlags EAutomationTestFlags_ApplicationContextMask`, referenced from the `static_assert` inside the `BEGIN_DEFINE_SPEC`-family macros in the same header. There are no deprecated aliases, so the old spelling is a hard compile error rather than a warning.

Prefer samples taken from engine headers over samples taken from the documentation site.

## 6. Structs says UStructs "cannot be used inside of UClasses"

`dev.epicgames.com/documentation/en-us/unreal-engine/structs-in-unreal-engine` — read narrowly this is about the UObject type hierarchy, but as written it is misleading: USTRUCTs appear as `UPROPERTY` members of UCLASSes throughout the engine. `AActor` itself declares `UPROPERTY(EditDefaultsOnly, Category=Tick) struct FActorTickFunction PrimaryActorTick;` (`Runtime/Engine/Classes/GameFramework/Actor.h`).

The genuinely load-bearing sentences on that page are the ones about garbage collection ("are not managed by the Garbage Collection system") and replication ("UStructs ARE NOT considered for replication. However, UProperty variables ARE considered for replication.").

## A note on judging page freshness

`dev.epicgames.com` renders client-side. Every URL returns HTTP 200 — including URLs that do not exist. A nonsense slug returns 200 with a page shell and no article node. **Never report a documentation page as "404".** The correct statement is that the page returns 200 with no article body, which is indistinguishable from a slug that was never valid.

Each page carries an `updated_at` timestamp in its embedded state. Pages that share a timestamp with many others suggest a bulk migration rather than an individual edit; pages with a distinct, later date suggest the reverse. Either way, treat an old timestamp as a reason to check the source, not as proof the content is stale — and re-check the timestamps yourself rather than trusting a previously recorded date range, since the site re-migrates content over time.

## Checklist

- [ ] Any claim sourced from `dev.epicgames.com` that concerns subsystem lifetimes, smart-pointer thread safety, assert behaviour by build configuration, or delegate replication was checked against engine source before being written down
- [ ] No code sample was copied from the documentation site without compiling it or checking the identifiers against 5.8 headers
- [ ] No documentation page was described as returning 404
- [ ] Where documentation and source disagree, the standard states the source's behaviour and notes that the documentation reads otherwise

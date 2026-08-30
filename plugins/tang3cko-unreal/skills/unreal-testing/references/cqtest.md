# Writing CQTest

Load this when writing or reviewing CQTest.

CQTest is Epic's xUnit-shaped framework, in `Developer/CQTest`. It needs no extra build target and no world unless you ask for one.

## Shape

```cpp
#include "CQTest.h"

TEST_CLASS(LobbyRulesTests, "Project.Lobby.Rules")
{
    FLobbyState State;

    BEFORE_EACH()
    {
        State = LobbyWith(/*Members=*/2);
    }

    TEST_METHOD(CanStart_WithTooFewMembers_ReturnsFalse)
    {
        ASSERT_THAT(IsFalse(FLobbyRules::CanStart(1, 2, true)));
    }

    TEST_METHOD(CanStart_WithEnoughMembersAllReady_ReturnsTrue)
    {
        ASSERT_THAT(IsTrue(FLobbyRules::CanStart(2, 2, true)));
    }
};
```

- `TEST_CLASS(Name, "Dotted.Path")` — the path is what filters match.
- `BEFORE_EACH` / `AFTER_EACH` run per test method. `BEFORE_ALL` / `AFTER_ALL` are static and run once.
- Member variables persist within a test and **reset between tests**. Do not rely on carry-over.
- Protected helper methods on the class are the right place for shared arrange code.

## Naming

`Method_Condition_ExpectedResult`, one behaviour per method.

**This is a Tang3cko rule, not an Epic one.** The CQTest page (`dev.epicgames.com/documentation/en-us/unreal-engine/cqtest-test-framework-for-unreal-engine`) states no naming convention; its examples merely happen to look like `BeforeRunTest_CallsSetup`. The three-part name that circulates as evidence — `SpawnAndReplicatePawn_WithReplicatedPawn_ProvidesPawnToClients` — sits inside a `/* ... */` sample block in `Developer/CQTest/Public/Components/PIENetworkComponent.h`, as a `TEST_METHOD` inside the commented-out `NETWORK_TEST_CLASS(MyFixtureName, "Network.Example")` example, guarded by `#if ENABLE_PIE_NETWORK_TEST`. It is illustrative comment text, not a test the engine compiles and runs. Adopt the shape because it makes the name a specification, and do not cite Epic as the source.

The name is a specification. If the name claims more than the assertions check, fix the name or add the assertion.

## Latent steps

`AddCommand` queues work that completes before the next step begins. If an assertion fails inside a latent action, no further latent actions run — but `AFTER_EACH` **is still invoked**, so teardown stays reliable.

You may call `AddCommand` inside `AFTER_EACH`; the official example does. The restriction that actually exists is narrower: `FMapTestSpawner::AddWaitUntilLoadedCommand` (`Developer/CQTest/Public/Components/MapTestSpawner.h`) carries the note "Must be called outside of a latent action. Preferably within BEFORE_TEST." — **`BEFORE_TEST` is not a macro that exists**; CQTest declares `BEFORE_EACH` and `BEFORE_ALL`, and Epic's comment is a slip. Read it as `BEFORE_EACH`.

Use latent steps only for things that genuinely need frames to pass. A latent test costs a world and a tick loop; a synchronous one costs neither.

## Assertions

`ASSERT_THAT` takes a matcher. `ASSERT_THAT` is defined in `Developer/CQTest/Public/CQTest.h` as `if (!this->Assert._assertion) { return; }` — so **a failed assertion returns from the test method immediately**, and nothing after it runs. The matchers are members of the asserter in `Developer/CQTest/Public/Assert/NoDiscardAsserter.h`; read that header rather than guessing, since the set grows.

```cpp
ASSERT_THAT(IsTrue(Rules.CanStart()));
ASSERT_THAT(IsFalse(Rules.CanStart()));
ASSERT_THAT(AreEqual(ELobbyAction::EndSession, Action));      // expected first
ASSERT_THAT(AreNotEqual(OldState, NewState));                 // expected first here too
ASSERT_THAT(AreEqualIgnoreCase(TEXT("host"), Member.Role));
ASSERT_THAT(IsNear(1.0f, Rules.GetProgress(), 0.001f));       // float comparison
ASSERT_THAT(IsNull(Subsystem->FindMember(UnknownId)));
ASSERT_THAT(IsNotNull(Found));
```

`IsNear` is the one to reach for on floats — an `AreEqual` on a float is a test that fails on a different machine. `AreEqualIgnoreCase` and `AreNotEqualIgnoreCase` exist for strings.

The equality matchers take expected first, actual second — reversing them produces a failure message that reads backwards, which costs time when someone else is looking at the log.

Because a failure returns, anything the test needed to clean up must be in `AFTER_EACH`, not written after the assertion.

Assert the behaviour, not the implementation: prefer checking the value a function returns over checking that a member was written.

One behaviour per test method. When a test needs three unrelated assertions, it is three tests.

## Where the files go

`Private/Tests/` inside the owning module, named `[ClassName]Test.cpp`. When a test maps one-to-one onto a class, name it after that class — `LobbyRulesTest.cpp`.

Automation Spec files take `.spec.cpp` instead. The Automation Spec page also says a spec should "not have the word 'Test' in the name" — **treat that as advice, not as engine practice**: of the engine's own `.spec.cpp` files, `LinearRegressionTests.spec.cpp` and `ToolMenusTestUtilities.spec.cpp` both contain it. Follow it here for consistency, knowing Epic does not.

Nothing under `Private/Tests/` may compile into a shipping build. The engine's own tests are guarded by `WITH_DEV_AUTOMATION_TESTS`; do the same. See `module-setup.md` for how that macro gets defined and how to guard your own test files with it.

## Checklist

- [ ] Test class path is dotted and namespaced under the project, and filters match it
- [ ] Method names follow `Method_Condition_ExpectedResult` and claim exactly what is asserted
- [ ] One behaviour per test method
- [ ] No test relies on member state carrying over between test methods
- [ ] Latent steps are used only where frames must pass, and `AddWaitUntilLoadedCommand` is called outside any latent action
- [ ] Files sit in `Private/Tests/` as `[ClassName]Test.cpp`, guarded so they cannot ship
- [ ] No claim in a comment or commit message describes the three-part naming as an Epic convention

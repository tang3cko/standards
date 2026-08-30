# Test Principles

Load this before writing or reviewing any test.

## FIRST

**Fast** — see the budgets below. **Isolated** — no shared mutable state, no order dependence. **Repeatable** — same result on any machine, any run, any time of day. **Self-validating** — the test says pass or fail; nobody reads a log to decide. **Timely** — written with the code, not after the bug.

## AAA

Every test reads as three sections, in order:

```cpp
TEST_METHOD(OnMemberLeft_WhenHostLeaves_EndsTheSession)
{
    // Arrange
    const FLobbyState State = LobbyWith(/*Members=*/3, /*HostIndex=*/0);

    // Act
    const ELobbyAction Action = FLobbyRules::OnMemberLeft(State, State.Members[0].Id);

    // Assert
    ASSERT_THAT(AreEqual(ELobbyAction::EndSession, Action));
}
```

One behaviour per test. When a test needs three unrelated assertions it is three tests, and the names will tell you so.

## The name is the specification

`Method_Condition_ExpectedResult`. The name must claim **exactly** what the assertions check — no more, no less.

A name claiming more than the body checks is worse than no test: it is a false statement that survives review because the test passes. When you find one, either add the assertion or narrow the name. Do not leave it.

This applies to the condition too. `OnMemberLeft_EndsSession` hides which member; the test that matters is the one where the *host* leaves.

## Time budgets

Not aspirations — the shape separating tiers that should exist from tiers that should not.

| Tier | Budget | What it costs |
|---|---|---|
| Plain type, no engine | under 1 ms | Nothing |
| Any test at all, in-editor | Editor startup, paid once per invocation | Unavoidable, and it dominates everything below |
| Test with a world | startup + world construction + ticks | Add seconds |
| Functional test | startup + map load | Add tens of seconds |
| Gauntlet session | multiple processes | Minutes |

The first row is where a suite should mostly live. A trivial test's own body is cheap by orders of magnitude next to editor startup — which means **the per-test cost is nearly free once you are in, and the per-invocation cost is enormous.** Consequences: write many cheap tests rather than few big ones, and never split a suite across editor invocations to parallelise.

## A green run proves nothing by itself

The automation runner exits 0 when the filter matches no tests. Before believing a suite:

- Did it write a report? A zero-match run writes none.
- How many tests ran? Compare against an expected minimum.
- Would it fail if the code broke? A test with no assertion passes forever. So does a `SKIP` in the LLT adapter, which expands to nothing.

## Do not test what you do not own

No mock stands in for the EOS SDK, the engine's replication, or `UCharacterMovementComponent`. A mock of an API you do not own asserts your belief about that API, and with online services that belief is wrong more often than it is right.

Test the decision, review the translation, and write down the manual procedure for the rest.

## Checklist

- [ ] Every test has explicit Arrange / Act / Assert and asserts one behaviour
- [ ] Every name claims exactly what its body checks, including the condition
- [ ] No test shares mutable state with another or depends on ordering
- [ ] The suite lives mostly in the sub-millisecond tier; engine-touching tests are the exception and each is justified
- [ ] The suite is not split across editor invocations
- [ ] A regression in the code under test would actually fail the run — no assertion-free tests, no silent skips
- [ ] Nothing the project does not own is mocked

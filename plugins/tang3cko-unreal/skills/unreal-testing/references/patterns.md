# Making Unreal Code Testable

The rule is Humble Object: **the part that decides goes in a type a test can construct directly; the part that talks to the engine stays thin and is not unit tested.** The stakes are high in Unreal because the cheapest engine-touching test still pays for editor startup, while a plain type costs microseconds.

## What "constructible by a test" means here

| Kind | Constructible directly? | Use for |
|---|---|---|
| Plain `F`-struct / class, no reflection | Yes | Rules, calculations, state machines |
| `USTRUCT` | Yes, but drags in CoreUObject | Reflected data that also needs rules |
| `UObject` | `NewObject` needs an Outer | Things needing GC or Blueprint exposure |
| `UActorComponent` | Needs an owning Actor | Behaviour attached to something in the world |
| `AActor` | Needs a world | Things that exist in the level |
| `UWorldSubsystem` | Needs a world | Per-level services |

Every row down that table costs more to test. Push logic upward until the thing you assert on is in the first or second row.

## The shape

```cpp
// LobbyRules.h — no engine types beyond containers
struct FLobbyRules
{
    static bool CanStart(int32 MemberCount, int32 MinMembers, bool bAllReady);
    static ELobbyAction OnMemberLeft(const FLobbyState& State, const FGuid& Member);
};
```

```cpp
// LobbySubsystem.cpp — the shell
void ULobbySubsystem::HandleMemberLeft(const FGuid& Member)
{
    switch (FLobbyRules::OnMemberLeft(State, Member))
    {
        case ELobbyAction::EndSession: DestroySession(); break;
        case ELobbyAction::PromoteHost: PromoteHost();   break;
        case ELobbyAction::Nothing:                      break;
    }
}
```

The test asserts on `FLobbyRules`. The `switch` is the humble part: it has no branching logic of its own worth testing, and testing it would require a session.

If the shell starts growing conditions, that is the signal that a decision leaked back into it.

## External services — EOS above all

Do not mock the online subsystem and call the feature tested. A mock of an API you do not own tests your belief about that API, and this is precisely the area where that belief is most often wrong.

Split it in three:

1. **Decisions** — what to do given state. Plain types. Unit tested.
2. **Translation** — turning a decision into an SDK call, and an SDK callback into a state change. Thin, and reviewed rather than unit tested.
3. **The SDK** — not yours. Covered by a written manual procedure.

Record what is manual and how to check it. Two Epic accounts are required for anything real, the second must be a member of the Developer Portal organisation, and one account cannot stand in for two because EOS P2P addresses peers by `ProductUserId`. **None of that is checkable from engine source** — it is EOS SDK behaviour plus an account-administration requirement, and the project's own docs and decision records are the authority for it. Cite those, not the engine.

## Seams that work in Unreal

**A static function on a struct** is the cheapest seam. No injection, no lifetime, no mock — the test calls it and asserts the return value. This is the shape shown above, and it covers most cases.

**An interface** where the shell genuinely has two implementations. Do not create one for a single implementation just to enable mocking.

```cpp
UINTERFACE(MinimalAPI)
class UPartyRoster : public UInterface { GENERATED_BODY() };

class IPartyRoster
{
    GENERATED_BODY()
public:
    virtual int32 GetMemberCount() const = 0;
};
```

The shipping implementation reads the GameState; the test's implementation returns whatever the case needs. Because the `I` class is plain C++, a test double is a small class in the test file — no engine machinery.

**A delegate the shell binds** — the decision layer broadcasts, the shell acts.

```cpp
// The decision layer owns it and broadcasts.
DECLARE_MULTICAST_DELEGATE_OneParam(FOnSessionShouldEnd, ELobbyEndReason);

// The test binds a lambda and asserts it fired with the right reason.
ELobbyEndReason Observed = ELobbyEndReason::None;
Rules.OnSessionShouldEnd.AddLambda([&Observed](ELobbyEndReason R) { Observed = R; });
Rules.HandleMemberLeft(HostId);
ASSERT_THAT(AreEqual(ELobbyEndReason::HostLeft, Observed));
```

A *multicast* delegate cannot return a value, so this fits notification rather than queries; `DECLARE_DELEGATE_RetVal` and its parameterised siblings (`Runtime/Core/Public/Delegates/DelegateCombinations.h`) exist for the single-binding case where you do need an answer.

**`ShouldCreateSubsystem`** keeps a subsystem out of a test world entirely.

```cpp
// USubsystem::ShouldCreateSubsystem(UObject* Outer) const — Runtime/Engine/Public/Subsystems/Subsystem.h
bool ULobbySubsystem::ShouldCreateSubsystem(UObject* Outer) const
{
    if (!Super::ShouldCreateSubsystem(Outer))
    {
        return false;
    }
    return bWantedInThisContext;   // whatever the project's own condition is
}
```

Returning false here is cheaper than making the subsystem tolerate a world it was never meant for. Every call site must null-check the result once this can return false.

Avoid constructor injection into UObjects. Constructors run against the class default object at engine startup, and the only argument the engine ever passes is an `FObjectInitializer` — `UObject` declares both a default constructor and a `UObject(const FObjectInitializer&)` (`Runtime/CoreUObject/Public/UObject/Object.h`), and generated subclasses take the latter with a default argument. That is a fixed engine-supplied channel for configuring subobjects, not a place to hand a test its own collaborators.

## Test data

Build test state with a small factory or a builder in the test file, not by hand-filling a struct in every case:

```cpp
static FLobbyState LobbyWith(int32 Members, bool bAllReady = true);
```

One named helper per meaningful shape beats a parameter list nobody can read at the call site. This is the same rule as "avoid anonymous literals in function calls" from the core standard.

## Checklist

- [ ] The assertion target is a type the test constructs directly, not an Actor, Component or Subsystem
- [ ] The engine-facing shell contains no branching logic worth asserting on
- [ ] No mock stands in for the EOS SDK or any other API the project does not own
- [ ] Anything that cannot be automated is written down as a manual procedure, not left implied
- [ ] Interfaces exist because there are two implementations, not to enable a mock
- [ ] Test state is built through named helpers, not by hand-filled structs repeated per test

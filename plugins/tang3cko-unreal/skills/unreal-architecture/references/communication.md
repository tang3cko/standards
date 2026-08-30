# System Communication

Load this when wiring any notification between systems.

## The event mechanism

**The multicast delegate is Unreal's event.** The owner declares one and broadcasts it; interested parties bind and unbind themselves; a subscriber cannot broadcast on the owner's behalf.

That last property is what makes it an event rather than a shared callback list, and the engine says so in as many words. A class exposes `TMulticastDelegateRegistration` (`Runtime/Core/Public/Delegates/DelegateSignatureImpl.inl`) as the public face of its delegate so that others can bind without being able to broadcast. Its comment reads: "A function can expose this base class to users to let them bind against, but without letting them broadcast. This gives us the benefit of C# events."

```cpp
DECLARE_MULTICAST_DELEGATE_OneParam(FOnLobbyMemberLeft, const FGuid& /*Member*/);

class ULobbySubsystem : public UGameInstanceSubsystem
{
    FOnLobbyMemberLeft OnMemberLeft;   // subscribers bind here
};
```

`Broadcast(...)` raises it. `AddUObject` / `AddSP` / `AddWeakLambda` bind. `Remove` / `RemoveAll` unbind.

Three properties define what it can and cannot be used for:

- **It cannot return a value.** Enforced by `static_assert`, not convention. A design needing an answer back needs a single-cast delegate or an interface.
- **It is not visible in the editor** unless declared with the `DYNAMIC` variants, which are reflected, Blueprint-assignable, and meaningfully slower.
- **It does not cross the network.** See below.

There is no one-shot binding. Remove the binding yourself in the handler.

### There is no engine-wide event bus

Do not go looking for one. Notification is owner-declares, subscriber-binds; the subscriber reaches the owner through a subsystem.

- `GameplayMessageSubsystem` / `GameplayMessageRouter` is a **Lyra plugin**, not an engine feature. Searching an installed engine — `Engine/Source` and `Engine/Plugins` both — returns zero files for either name, which is the half that is checkable here. That the names are Lyra's is read from Lyra, which is not bundled with the engine; confirm against the revision you have.
- The engine's `EventChannel` types are all Sequencer curve channels — `FMovieSceneEventChannel` and friends. Unrelated.
- `IMessageBus` (`Runtime/Messaging/Public/IMessageBus.h`) is engine/editor infrastructure for inter-process messaging, not gameplay pub/sub.

A `UDataAsset` holding a delegate can imitate an asset-mediated channel, but that is a project invention with no engine support behind it. Do not present it as an Unreal idiom, and do not adopt it without a written reason.

## Delegate selection

| Need | Declare |
|---|---|
| One listener, may return a value | `DECLARE_DELEGATE_RetVal_*` |
| Many listeners, C++ only | `DECLARE_MULTICAST_DELEGATE_*` |
| Many listeners, Blueprint must bind | `DECLARE_DYNAMIC_MULTICAST_DELEGATE_*` |
| Many listeners, an event only the owner may broadcast | `DECLARE_EVENT_*` — **deprecated**, use multicast |

`DECLARE_EVENT_*` is deprecated — it exists only to restrict broadcasting to the owning class, and a plain multicast delegate declared in the owner's private section achieves the same thing. `DelegateCombinations.h` (`Runtime/Core/Public/Delegates/`) states this on the macro itself: the restriction "is not enforced and this type should be considered deprecated for new delegates, use normal multicast instead".

**Multicast delegates cannot return values.** This is a `static_assert`, not a convention. A design that needs an answer back needs an interface or a single-cast delegate, not an event.

Dynamic variants are reflected, Blueprint-assignable, serializable, and meaningfully slower. Use the non-dynamic form unless Blueprint must bind.

The macros in `Runtime/Core/Public/Delegates/DelegateCombinations.h` go up to nine parameters — `DECLARE_DELEGATE_NineParams` and its siblings exist — while the Delegates page (`dev.epicgames.com/documentation/en-us/unreal-engine/delegates-and-lambda-functions-in-unreal-engine`) says "Up to eight function parameters". Trust the header. If you are near either number, pass a struct.

## Binding, and unbinding

Bind with the variant that matches the subscriber's lifetime:

| Subscriber | Bind with |
|---|---|
| A `UObject` | `AddUObject` |
| A shared-pointer object | `AddSP` |
| A raw C++ object | `AddRaw` — **carries an explicit warning**; nothing unbinds it when the object dies |
| A lambda that runs now | `AddLambda` |
| A lambda that runs later, owned by a UObject | `AddWeakLambda` |
| A lambda that runs later, owned by a shared pointer | `AddSPLambda` |

`AddThreadSafeSP` is retained only for backwards compatibility — its doc comment in `DelegateSignatureImpl.inl` (`Runtime/Core/Public/Delegates/`) says so directly: "This function is redundant, but is retained for backwards compatibility. AddSP() works in both thread-safe and not-thread-safe modes and should be preferred." Prefer `AddSP`. `AddUnique` exists for the dynamic variants and does a linear scan; `RemoveSingleSwap` does not preserve order.

Broadcast order is not something to depend on — the engine iterates its invocation list in reverse, stated directly in the comment above the loop in `TMulticastDelegateBase::Broadcast` (`MulticastDelegateBase.h`, `Runtime/Core/Public/Delegates/`): it calls bound functions in reverse order so that any instance added by a callee during the broadcast is ignored. If two subscribers must run in an order, that ordering belongs in the code, not in the subscribe sequence.

Unsubscribe symmetrically. The weak-bound variants unbind themselves; the raw ones do not.

## Delegates and the network

**Delegates do not replicate.** The engine refuses on security grounds. The official multicast delegates page claims otherwise; see `unreal-core/references/official-doc-errata.md` for the source that contradicts it.

Anything a remote machine must learn goes through a replicated property with `ReplicatedUsing`, or an RPC. See `replication.md`.

## Interfaces versus delegates

Use a delegate when the owner does not care who is listening and does not need an answer.

Use an interface (`UINTERFACE` pair) when the caller needs a specific thing done and possibly an answer back — damage application, interaction, anything with a return. Do not create an interface with one implementation to enable a mock.

Use a Subsystem when the relationship is "anyone in this world may ask this service a question". A subsystem is reachable without a reference chain, which is what makes it the practical place to hang a delegate that unrelated systems need to bind to.

## What not to do

- A widget walking `GetGameMode()`, or any UI reaching into gameplay to read state. The GameMode does not exist on clients at all.
- A singleton. Subsystems exist precisely to avoid this and have engine-defined lifetimes.
- A manager Actor holding references to everything, spawned by whoever remembers.
- Polling in `Tick` for a change that could have been a delegate broadcast.
- Assuming `AGameStateBase::PlayerArray` announces changes. It is not registered in `AGameStateBase::GetLifetimeReplicatedProps`, so it is not replicated, and the engine raises nothing; each machine rebuilds it as PlayerStates arrive. A roster UI needs its own notification.

## Checklist

- [ ] No design claims an engine-provided pub/sub or message bus exists for gameplay
- [ ] Delegate type matches the need — non-dynamic unless Blueprint binds; nothing expects a return value from a multicast
- [ ] Every binding matches the subscriber's lifetime; no bare `AddRaw`, and deferred lambdas are weak-bound
- [ ] No behaviour depends on broadcast order
- [ ] Nothing expects a delegate to cross the network
- [ ] UI does not reach into gameplay classes that may not exist on its machine
- [ ] No singleton and no manager Actor where a subsystem fits
- [ ] Roster or list state has an explicit notification rather than relying on `PlayerArray`

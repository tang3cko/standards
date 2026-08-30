# Reflection Macros

Load this when writing `UPROPERTY`, `UFUNCTION`, `UCLASS`, `USTRUCT` or `UINTERFACE`.

These macros expand to nothing for the C++ compiler. Everything they do happens in Unreal Header Tool, which means a mistake here is never a compile error in the file you wrote it in — it is missing behaviour somewhere else.

Every gameplay class needs `GENERATED_BODY()` at the very beginning of the class body.

## `UPROPERTY` — editing visibility

Pick from this table, not from habit. The Edit and Visible families are mutually exclusive.

| Specifier | Archetype (Blueprint defaults, CDO) | Placed instance |
|---|---|---|
| `EditAnywhere` | editable | editable |
| `EditDefaultsOnly` | editable | not shown |
| `EditInstanceOnly` | not shown | editable |
| `VisibleAnywhere` | read-only | read-only |
| `VisibleDefaultsOnly` | read-only | not shown |
| `VisibleInstanceOnly` | not shown | read-only |

**Default to `EditDefaultsOnly`.** Tuning values belong on the class, where one edit changes every instance. Reach for `EditAnywhere` only when a designer genuinely needs to differ one placed actor from another — otherwise you have created state that exists in a level and nowhere else, which no one will find later.

`VisibleAnywhere` is for things the engine owns, components above all: the reference must not be reassigned in the editor, but seeing it is useful.

## `UPROPERTY` — the rest

| Specifier | Meaning |
|---|---|
| `BlueprintReadOnly` / `BlueprintReadWrite` | Blueprint access. `ReadOnly` unless writing is genuinely part of the contract |
| `Transient` | Not saved or loaded; **zero-filled at load**. Says nothing about GC |
| `Config` | Backed by an `.ini`. Cannot have a value in default properties; implies `BlueprintReadOnly` |
| `Instanced` | Each instance gets its own copy of the assigned object |
| `Replicated` | Replicated. Requires `GetLifetimeReplicatedProps` |
| `ReplicatedUsing=OnRep_X` | Replicated, and calls `OnRep_X` on arrival |
| `SaveGame` | Marks the field for a checkpoint/save system |
| `meta=(...)` | **Editor only. Never read metadata from game logic** |

Categories are not optional in practice — an uncategorised property lands in a default bucket and becomes unfindable.

## `UFUNCTION`

| Specifier | Meaning |
|---|---|
| `BlueprintCallable` | Callable from a graph |
| `BlueprintPure` | No side effects. **A `const` function becomes pure by default** — write `BlueprintPure=false` to prevent it |
| `BlueprintImplementableEvent` | No C++ body; Blueprint provides it |
| `BlueprintNativeEvent` | C++ default in `_Implementation`, overridable in Blueprint |
| `Server` / `Client` / `NetMulticast` | RPC direction |
| `Reliable` / `Unreliable` | Only valid with `Server`/`Client`/`NetMulticast` |
| `WithValidation` | Adds `_Validate`, returning whether the call may proceed |
| `BlueprintCosmetic` | Does not run on dedicated servers |
| `DevelopmentOnly` | Development builds only |
| `CallInEditor` | Adds a button in the Details panel |

**`BlueprintPure` does not cache.** Every node reading its output calls it again. A pure function doing non-trivial work, or returning an array, is a performance trap laid for whoever wires it up.

Two RPC facts that cost time when unknown.

A plain `AActor` with no `Owner` cannot send a server RPC, and the call is **silently absorbed** rather than failing. `AActor::GetFunctionCallspace` (`Runtime/Engine/Private/Actor.cpp`) decides this, and several of its branches return `FunctionCallspace::Absorbed`; the one that bites here logs "No Authority Server Call Absorbed". `AActor::HasNetOwner` in the same file states the rule outright: "all basic AActors are unable to call RPCs without special AActors as their owners (ie APlayerController)". Route through the PlayerController, or give the Actor an owner.

`WithValidation` is the only mechanism that stops a client sending nonsense.

## `UCLASS`

`Blueprintable` exposes the class as a base for Blueprints; the default is `NotBlueprintable` unless inherited otherwise. `Abstract` prevents placing it in a level. `Config=Name` allows `.ini`-backed properties, with `Engine`, `Editor`, `Input` and `Game` as the common names.

Gameplay classes are C++ and Blueprint is used where it fits — layout, data assets, animation. Each project records that decision once; do not re-litigate it in code review.

## `UINTERFACE`

```cpp
UINTERFACE(MinimalAPI, Blueprintable)
class UDamageable : public UInterface
{
    GENERATED_BODY()
};

class IDamageable
{
    GENERATED_BODY()
public:
    virtual void ApplyDamage(float Amount) = 0;
};
```

The `U` class exists only to be visible to reflection and needs nothing in it. The `I` class is the interface, must be abstract, and **must not have member variables**. Only `BlueprintImplementableEvent` and `BlueprintNativeEvent` functions may appear on an interface exposed to Blueprint.

`class-design.md`'s placement guidance for interfaces defers to this section for the mechanics — this is the canonical description of the U/I pair.

## Delegates

`DECLARE_MULTICAST_DELEGATE_*` and `DECLARE_DYNAMIC_MULTICAST_DELEGATE_*` are the Unreal equivalent of a C# `event`: the owner broadcasts, others subscribe, and subscribers cannot broadcast. Dynamic variants are reflected and Blueprint-assignable and cost more; use the non-dynamic form unless Blueprint needs to bind.

Multicast delegates **cannot return values** — enforced by a `static_assert(sizeof(RetValType) == 0, "The return type of a multicast delegate must be void")` in `Runtime/Core/Public/Delegates/DelegateSignatureImpl.inl`, not convention. If you need an answer back, you need an interface or a callback, not an event.

**Delegates never replicate.** The engine explicitly refuses, because a replicated delegate would let a remote machine invoke arbitrary functions. The official multicast page says they can be "triggered remotely"; it is wrong. See `official-doc-errata.md`.

Bind with the variant that matches the lifetime: `AddUObject` for a UObject, `AddSP` for a shared pointer, `AddWeakLambda`/`AddSPLambda` for deferred lambdas. `AddRaw` carries an explicit warning because nothing unbinds it when the object dies.

## Checklist

- [ ] Every property has a deliberate edit specifier, defaulting to `EditDefaultsOnly`, and a Category
- [ ] Engine-owned references such as components are `VisibleAnywhere`, not `EditAnywhere`
- [ ] No game logic reads `meta=`
- [ ] `Transient` was not used expecting it to affect garbage collection
- [ ] Every `const` `UFUNCTION` exposed to Blueprint was checked for accidental `BlueprintPure`, and no pure function does non-trivial work or returns an array
- [ ] Every client-callable `Server` RPC has `WithValidation`, and its owner can actually send it
- [ ] Interfaces are a U/I pair, the `I` class has no members, and Blueprint-exposed interfaces contain only implementable/native events
- [ ] No design depends on a delegate crossing the network or returning a value
- [ ] Every delegate binding matches the lifetime of what it binds to; no bare `AddRaw`

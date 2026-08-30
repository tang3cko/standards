# Memory, Pointers and Garbage Collection

Load this when choosing a pointer type, touching garbage collection, or referencing an asset.

Unreal has **two independent memory systems that do not overlap**: the UObject system with its garbage collector, and the smart pointer library for everything else. Mixing them is the single most expensive mistake in this file. `TSharedFromThis` on a UObject does not merely misbehave — it fails a `static_assert` reading "TSharedFromThis is not supported on UObjects", inside `TSharedFromThis` in `Runtime/Core/Public/Templates/SharedPointer.h`.

## Pointer decision table

| Situation | Use |
|---|---|
| Reflected member that must keep a UObject alive | `UPROPERTY() TObjectPtr<UThing> Thing;` |
| Reflected container of UObjects | `UPROPERTY() TArray<TObjectPtr<UThing>> Things;` |
| Reference to a UObject that must **not** keep it alive | `TWeakObjectPtr<UThing>` |
| Non-UObject class needs to keep a UObject alive | `TStrongObjectPtr<UThing>`, or `FGCObject` |
| Scoped protection while a function runs | `FGCObjectScopeGuard` |
| Asset that should load on demand, not with its owner | `TSoftObjectPtr<UThing>` / `TSoftClassPtr` |
| Shared ownership of a non-UObject | `TSharedPtr<FThing>`, or `TSharedRef` where null is impossible |
| Sole ownership of a non-UObject | `TUniquePtr<FThing>` |
| Observing a non-UObject you do not own | `TWeakPtr<FThing>` |
| Function parameter | The object itself, or `const FThing&`. Not a smart pointer |

A raw `UObject*` is acceptable as a local, as a function parameter, and as a return value. It is not acceptable as a member that outlives a stack frame.

### `TObjectPtr` specifics

Use it for reflected members and containers. In non-editor builds it is the same size as a raw pointer, serializes identically, and reads identically; in editor builds it adds late resolution, access tracking and type safety. Epic recommends it and the documentation calls it optional — but UHT can be configured per target to warn on or reject raw pointer members, so treat it as required.

The UE5 Migration Guide (`dev.epicgames.com/documentation/en-us/unreal-engine/unreal-engine-5-migration-guide`) says it performs "identically to raw pointers in non-editor builds". That is true of reads and of layout. It is not quite true of writes: the GC write barrier is gated on `UE_OBJECT_PTR_GC_BARRIER` (`Runtime/CoreUObject/Public/UObject/ObjectPtr.h`), which defaults to `1` and is **not** editor-only, so assignment carries a branch on an atomic flag for incremental reachability. The cost is negligible and predictable. Do not repeat "zero cost" as if it were unconditional.

Two mechanical consequences: capture `Find`-family results as `TObjectPtr<T>*`, not `T**`, and iterate reflected containers with `auto&` or `const auto&` rather than `auto*`, so the resolved address can be cached.

Passing a raw `UObject*` to `AddReferencedObject` is deprecated with a blunt message. `UE_REFERENCE_COLLECTOR_REQUIRE_OBJECTPTR_DEPRECATED` in `Runtime/CoreUObject/Public/UObject/UObjectGlobals.h` warns that the program "will randomly crash if this function is called when incremental gc is enabled". Pass `TObjectPtr`.

## Why `UPROPERTY` is load-bearing

The collector walks a per-`UClass` reference token stream. Tokens are emitted from reflected properties, and `UPROPERTY(...)` expands to nothing as far as the C++ compiler is concerned — it exists only for Unreal Header Tool. A member the header tool never saw contributes no token, so the collector never traverses it.

Two consequences follow, and the Unreal Object Handling page (`dev.epicgames.com/documentation/en-us/unreal-engine/unreal-object-handling-in-unreal-engine`) states both: a raw pointer reference "will not be automatically nulled, nor will it prevent garbage collection", and you "typically need to maintain a UPROPERTY reference to any Object you wish to keep alive". That page — not the UPROPERTY specifier reference — is where this rule lives.

`Transient` changes none of this. It suppresses serialization; the GC token is emitted as usual and the reference stays strong.

Actors and Components are frequently an apparent exception because the Level references them back to the root set. Do not build on that.

## Validity

```cpp
if (IsValid(Thing))   // null-safe; false for Garbage and Unreachable
if (IsValidChecked(Thing))  // asserts on null, otherwise the same test
UThing* Live = GetValid(Thing);  // returns the pointer or nullptr
```

`IsValid` tests the object's internal flags — it answers "has this been destroyed or marked garbage". It **cannot** answer "does this pointer still point at memory the engine owns", because it dereferences the pointer to find the object's index. A pointer left dangling by an unreflected member will crash inside `IsValid`, not return false. The fix is the `UPROPERTY`, not a better check.

`IsValid` is also not safe off the game thread: during the GC mark phase the Unreachable flag is set and the answer is wrong. `TWeakObjectPtr::IsValid` (`Runtime/Core/Public/UObject/WeakObjectPtrTemplates.h` — note that this one lives in Core, not CoreUObject) carries the same caveat in its own comment.

A `Destroy()`ed Actor is non-null and `IsValid`-false. Check before touching, not after.

## Casting

`Cast<T>` returns null on failure — branch on it. `CastChecked<T>` asserts on failure, but its `#else` path in Shipping is an unchecked C-style cast with no test at all. Use `CastChecked` only where you would have used `check` anyway, never as a safety net. `ECastCheckedType::NullAllowed` exists when null is a legitimate input.

## Constructing UObjects

Never `new`. `NewObject<T>()` at runtime; `CreateDefaultSubobject<T>()` inside a constructor. These are enforced, not advisory: calling `CreateDefaultSubobject` outside a constructor is `Fatal`, and calling the unnamed `NewObject` inside one asserts, because it would produce inconsistent names.

Constructors run at engine startup against the class default object. They may set defaults and create subobjects and load static assets, and nothing else. Real work belongs in `BeginPlay` for Actors and Components, or `Initialize` for Subsystems.

`CreateEditorOnlyDefaultSubobject` exists for editor-only components; they are stripped from packaged builds, so nothing on the runtime path may depend on one.

## Lambdas

Capture explicitly. `[&]` and `[=]` will capture `this` or a member and thereby a UObject pointer that the collector cannot see — and `[=]` reads as if it copied everything, which makes the bug harder to spot.

For anything deferred, use `CreateWeakLambda` (bound to a UObject) or `CreateSPLambda` (bound to a shared pointer). They unbind themselves when the owner dies. Any deferred lambda that does neither needs a comment explaining why its captures outlive it.

## Non-UObject classes holding UObjects

Derive from `FGCObject` and implement `AddReferencedObjects` and `GetReferencerName`. Note that `FGCObject` instances are **not trivially relocatable** — the `TIsTriviallyRelocatable<T>` specialization for `FGCObject`-derived types in `Runtime/CoreUObject/Public/UObject/GCObject.h` flags this explicitly — so storing them by value in an Unreal container can crash the collector during reachability analysis; hold them by pointer.

`TStrongObjectPtr` is the lighter option for a single reference; `FGCObjectScopeGuard` for a scope.

## Asset references

A hard reference loads its target when its owner loads. That is correct for things always needed and ruinous for things occasionally needed — the Referencing Assets page (`dev.epicgames.com/documentation/en-us/unreal-engine/referencing-assets-in-unreal-engine`) warns that "memory footprint can balloon due to many assets being loaded at once", which is the whole reason `TSoftObjectPtr` exists.

`TSoftObjectPtr` does not load anything by itself. `Get()` returns null if the asset is not resident; `LoadSynchronous()` blocks and will spike a frame. Load asynchronously through the streamable manager unless you know the call site tolerates a stall.

`ConstructorHelpers` results are null when the asset is missing or fails to load, and the crash lands wherever the pointer is first used. Assert at the point of load.

`FindObject` finds only what is already loaded; `LoadObject` loads. Choosing the wrong one gives you either a spurious null or an unintended load.

## Checklist

- [ ] Every UObject member that outlives a stack frame is a `UPROPERTY` with `TObjectPtr`, or a deliberate `TWeakObjectPtr`
- [ ] No smart pointer from the shared-pointer library points at a UObject
- [ ] Reflected containers are iterated with `auto&`, and `Find` results captured as `TObjectPtr<T>*`
- [ ] `IsValid` is called before touching any UObject not created in the same scope, and never off the game thread
- [ ] `CastChecked` appears only where a failed cast is a program error; every other cast branches
- [ ] No `new` on a UObject; no `CreateDefaultSubobject` outside a constructor; no real work in a constructor
- [ ] Every lambda captures explicitly, and every deferred lambda is weak-bound or has a comment justifying its captures
- [ ] Non-UObject holders of UObjects use `FGCObject` or `TStrongObjectPtr`, and no `FGCObject` is stored by value in a container
- [ ] Occasionally-needed assets are soft references, and no soft reference is resolved with `LoadSynchronous` on a frame that matters

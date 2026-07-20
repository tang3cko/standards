# Code Patterns

Recurring implementation patterns for Godot 4.x + .NET, with the reasoning behind each and where it applies.

## Interaction System

Decoupled structure based on `IInteractable` / `IInteractor` interfaces. Spatial context flows to the UI through a Resource-based channel.

### Layer layout

| Layer | Location | Role |
|-------|----------|------|
| Core abstractions | `Scripts/Interaction/Core/` | `IInteractable`, `IInteractor`, `InteractionContext`, `InteractionResult`, `InteractionContextEventChannel` |
| Validators | `Scripts/Interaction/Validators/` | Abstract `InteractionValidator : Resource` and concretes (`RequiredItemValidator`, …) |
| Implementations | `Scripts/Interaction/` | `InteractableComponent : Area3D`, `PickupInteractable`, `DoorInteractable`, … |
| Interactor | `Scripts/Player/PlayerInteractor.cs` | Raycast + focus tracking + hold state + context emission |
| UI | `Scripts/UI/InteractionPromptUI.cs` | Receives context via `InteractionContextEventChannel`, display only |

### IInteractable interface

```csharp
public interface IInteractable
{
    bool CanBeInteractedBy(IInteractor interactor);
    InteractionContext BuildContext(IInteractor interactor);
    void OnFocusEnter(IInteractor interactor);
    void OnFocusExit(IInteractor interactor);
    InteractionResult Interact(IInteractor interactor);
}

public enum InteractionResult { Success, InProgress, Rejected }
```

### Validator pattern

Give `InteractableComponent` an `[Export] InteractionValidator[] Validators` array and compose. Each validator is a Resource, hence reusable as a `.tres`.

```csharp
public abstract partial class InteractionValidator : Resource
{
    public abstract bool Validate(IInteractor interactor, IInteractable target);
    public abstract string GetBlockReason();
}
```

Concrete example: `RequiredItemValidator` checks `InventorySystem.HasItem(itemId)`.

## ReactiveSO (Resource-based EventChannel / Variable)

Shared patterns stored under `addons/ReactiveSO/`. Equivalent to Unity's ScriptableObject EventChannel.

### EventChannel

```csharp
[GlobalClass]
public partial class IntEventChannel : Resource
{
    [Signal] public delegate void RaisedEventHandler(int value);
    public void Raise(int value) => EmitSignal(SignalName.Raised, value);
}
```

Emitter and subscriber both `[Export]` the same `.tres`, fully decoupling the two sides.

### Variable

```csharp
[GlobalClass]
public partial class IntVariable : Resource
{
    [Export] private int _initialValue;
    [Export] private IntEventChannel _onValueChanged;

    public int Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            EmitSignal(SignalName.ValueChanged, _value);
            _onValueChanged?.Raise(_value);
        }
    }

    public void ResetToInitial() { Value = _initialValue; }
    public void SetWithoutNotify(int value) { _value = value; }
}
```

Provides two subscription routes — direct Signal and EventChannel — plus a `SetWithoutNotify(value)` escape hatch that skips notification.

### RuntimeSet

Collections like `Node3DRuntimeSet : Resource`. Emit `Added / Removed` signals; used for enemy pools and spawn management.

## Autoload Singletons

```ini
# project.godot
[autoload]
InventorySystem="*res://Scripts/Inventory/InventorySystem.cs"
```

- Node-based
- Static `Instance` property for convenient C#-side access
- State changes notified via Signals (`ItemAdded`, `ItemRemoved`, …)
- Provide read-only query APIs for external callers such as validators (`HasItem(string itemId)`)

## Player Controller Split

Separate nodes per responsibility.

| Node | Responsibility |
|------|----------------|
| `FirstPersonController : CharacterBody3D` | Movement and camera only |
| `PlayerInteractor : Node` | Raycast, focus, interact input only |

`PlayerInteractor` exposes `Body : Node3D` via a parent reference and implements `IInteractor`. The interaction side gets position data without knowing the player's internals.

**Intent**: Single Responsibility — movement and interaction stay independently extensible and testable.

## Cross-Scene Service via Group Lookup

When nodes living in different `.tscn` files (e.g. `Player` and `World`) need a mutual reference that does not warrant an autoload, use `AddToGroup()` + `GetFirstNodeInGroup()`.

```csharp
// Provider side
public override void _Ready()
{
    AddToGroup("voxel_world");
}

// Consumer side
public override void _Ready()
{
    _world = GetTree().GetFirstNodeInGroup("voxel_world") as VoxelWorld;
}
```

This avoids depending on a fixed `NodePath`, so scenes keep working when run standalone.

Applicability conditions:

- The target is unique in the scene
- Lookup failure is either tolerated as `null` or explicitly guarded
- The responsibility is a scene-local service reference, not a global-lifetime one

Caution: when a descendant group-looks-up an ancestor, the ancestor's `AddToGroup()` may not have run yet due to `_Ready()` ordering. See "Descendant `_Ready()` fires before the ancestor's" in `gotchas.md`.

## Signal-Driven State Propagation (Node Signal vs EventChannel)

A node that owns mutable world state emits its own Signal on change, and subscribers listen to it. Prefer the node's own Signal over an `EventChannel` when all of the following hold:

- There is a single emitter
- The emitting node is guaranteed to exist in the scene
- The dependency direction is limited to "observe that node's state"

```csharp
// Emitter
[Signal] public delegate void DigPerformedEventHandler(Vector3 worldPos, float radius);

public void DigAt(Vector3 worldPos, float radius)
{
    EmitSignal(SignalName.DigPerformed, worldPos, radius);
}

// Subscriber
public override void _Ready()
{
    _world.DigPerformed += OnDigPerformed;
}

public override void _ExitTree()
{
    if (_world != null)
        _world.DigPerformed -= OnDigPerformed;
}
```

The advantage of the node Signal over an EventChannel: the state owner and the event source coincide, keeping the dependency graph straightforward.

Caution: unsubscribe node Signals in `_ExitTree()` when the emitter outlives the subscriber — a lingering subscription risks keeping a GC root alive. See "Signal unsubscribe: only required for subscriptions that outlive you" in `gotchas.md` for the lifetime rule.

## Checklist

- [ ] Cross-system communication goes through a Resource-based EventChannel (`.tres` exported on both sides), not direct node references
- [ ] A node's own Signal is used instead of an EventChannel only when all three criteria hold: single emitter, guaranteed in scene, observe-only dependency
- [ ] Conditional behavior is composed from `Resource` validators (reusable as `.tres`), not hardcoded branch logic inside the interactable
- [ ] Player-facing systems are split by responsibility (movement node vs interaction node), each independently testable
- [ ] Group lookup is used only for a unique scene-local service, with lookup failure tolerated or guarded, and never from a descendant's `_Ready()` toward an ancestor
- [ ] Autoloads are reserved for global-lifetime systems and notify state changes via Signals with read-only query APIs

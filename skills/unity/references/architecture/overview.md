# Architecture Overview

## Purpose

Guide for selecting the appropriate ScriptableObject-based architecture pattern. All patterns are built on Tang3cko.ReactiveSO package.

---

## Pattern relationship diagram - P1

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        ScriptableObject-Driven Architecture                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────────┐      ┌──────────────────┐      ┌──────────────────┐   │
│  │   EventChannel   │      │     Variable     │      │  ScriptableObject│   │
│  │                  │      │                  │      │     (Data)       │   │
│  │  - Stateless     │      │  - Stateful      │      │                  │   │
│  │  - Fire & forget │      │  - Auto-notify   │      │  - Immutable     │   │
│  │  - N:N broadcast │      │  - Current value │      │  - Configuration │   │
│  └────────┬─────────┘      └────────┬─────────┘      └──────────────────┘   │
│           │                         │                                        │
│           │    ┌────────────────────┘                                        │
│           │    │                                                             │
│           ▼    ▼                                                             │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                         Collection Patterns                           │   │
│  ├──────────────────────────┬───────────────────────────────────────────┤   │
│  │       RuntimeSet         │          ReactiveEntitySet                │   │
│  │                          │                                           │   │
│  │  - Object tracking       │  - Per-entity state                       │   │
│  │  - No per-item state     │  - O(1) ID lookup                         │   │
│  │  - Simple add/remove     │  - State change callbacks                 │   │
│  │  - EventChannel notify   │  - Scene-persistent                       │   │
│  │                          │                                           │   │
│  │  Uses: onItemsChanged    │  Uses: SubscribeToEntity()                │   │
│  │        onCountChanged    │        OnItemAdded/Removed EventChannels  │   │
│  └──────────────────────────┴───────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

Communication Flow:
──────────────────

  Publisher ──RaiseEvent()──▶ EventChannel ──OnEventRaised──▶ Subscriber(s)
                                   ▲
                                   │
  Variable.Value = x ──(auto)──────┘
```

---

## Pattern selection flowchart - P1

```
START: What do you need?
│
├─▶ "Communication between systems"
│   │
│   └─▶ Need to store current value?
│       │
│       ├─▶ YES: Use Variable
│       │        (IntVariableSO, FloatVariableSO, etc.)
│       │        → Stores value + auto-notifies on change
│       │
│       └─▶ NO: Use EventChannel
│                (VoidEventChannelSO, IntEventChannelSO, etc.)
│                → Fire-and-forget notification
│
├─▶ "Track active objects in scene"
│   │
│   └─▶ Need per-object state?
│       │
│       ├─▶ YES: Use ReactiveEntitySet
│       │        → Per-entity data + O(1) lookup + state callbacks
│       │
│       └─▶ NO: Use RuntimeSet
│                → Simple collection + iteration + count
│
├─▶ "Store game data/configuration"
│   │
│   └─▶ Use ScriptableObject
│        → Immutable data assets (items, quests, enemies)
│
└─▶ "Manage dependencies"
    │
    └─▶ See Dependency Priority:
         1. EventChannel (decoupled)
         2. SerializeField (explicit)
         3. FindFirstObjectByType (fallback)
         4. Singleton (last resort)
```

---

## Quick comparison table - P1

| Pattern | State | Notification | Lookup | Use Case |
|---------|-------|--------------|--------|----------|
| **EventChannel** | None | Broadcast | N/A | System communication |
| **Variable** | Single value | On change | Direct | Shared state (score, health) |
| **RuntimeSet** | None | EventChannel | Iteration | Object tracking |
| **ReactiveEntitySet** | Per-entity | Per-entity + EventChannel | O(1) by ID | Entity state management |
| **ScriptableObject** | Immutable | None | Asset reference | Data configuration |

---

## Pattern summaries - P1

### EventChannel

**Purpose:** Decoupled event-driven communication

**When to use:**
- System A needs to notify System B without direct reference
- Multiple subscribers need the same notification
- One-time events (game start, enemy killed, button clicked)

**Key features:**
- 12 built-in types (Void, Int, Float, Bool, String, Vector2, Vector3, etc.)
- Complete decoupling (publisher doesn't know subscribers)
- Inspector-visible event flow
- Debug via Event Monitor Window

**Example:**
```csharp
// Publisher
onEnemyKilled?.RaiseEvent();

// Subscriber
onEnemyKilled.OnEventRaised += HandleEnemyKilled;
```

→ [Event Channels](event-channels.md)

---

### Variable

**Purpose:** Reactive state with automatic change notification

**When to use:**
- State needs to persist and be readable (score, health percentage)
- Multiple systems need to read the same value
- Value changes should trigger UI updates automatically

**Key features:**
- 11 built-in types (Int, Float, Bool, String, Vector2, Vector3, etc.)
- Automatic event firing on value change
- GPU Sync for shader integration
- Debug via Variable Monitor Window

**Example:**
```csharp
// Write (auto-notifies if EventChannel assigned)
playerScore.Value += 100;

// Read
int current = playerScore.Value;
```

→ [Variables System](variables.md)

---

### RuntimeSet

**Purpose:** Track active objects without expensive Find operations

**When to use:**
- Need to iterate over all active enemies, players, collectibles
- Simple add/remove lifecycle (OnEnable/OnDisable)
- No per-object state required
- Replace FindObjectsOfType or FindGameObjectsWithTag

**Key features:**
- Generic `RuntimeSetSO<T>` base class
- Built-in GameObjectRuntimeSetSO, TransformRuntimeSetSO
- EventChannel notifications (onItemsChanged, onCountChanged)
- Debug via Runtime Set Monitor Window

**Example:**
```csharp
// Registration
enemySet?.Add(this);    // OnEnable
enemySet?.Remove(this); // OnDisable

// Usage
foreach (var enemy in enemySet.Items) { }
```

→ [RuntimeSet Pattern](runtime-sets.md)

---

### ReactiveEntitySet

**Purpose:** Centralized per-entity state management

**When to use:**
- Each entity has individual state (health, mana, status)
- Need O(1) lookup by entity ID
- State should persist across scene operations
- External systems need to observe specific entity changes

**Key features:**
- Sparse Set data structure (O(1) operations)
- Per-entity subscription via `SubscribeToEntity()`
- ReactiveEntity<T> base class for automatic lifecycle
- EventChannel notifications (OnItemAdded, OnItemRemoved, OnDataChanged)

**Example:**
```csharp
// Registration
entitySet.Register(entityId, initialState);

// State update
entitySet.SetData(entityId, newState);

// Per-entity subscription
entitySet.SubscribeToEntity(entityId, (oldState, newState) => { });
```

→ [ReactiveEntitySet Pattern](reactive-entity-sets.md)

---

### ScriptableObject (Data)

**Purpose:** Immutable game data and configuration

**When to use:**
- Define item stats, quest data, enemy configurations
- Share data across multiple instances (100 enemies share 1 EnemyDataSO)
- Designer-friendly Inspector editing
- Hot reload during Play mode

**Key features:**
- Data-logic separation
- Memory efficient (shared references)
- Inspector-based configuration
- CreateAssetMenu for easy asset creation

**Example:**
```csharp
[CreateAssetMenu(menuName = "ProjectName/Data/Enemy")]
public class EnemyDataSO : ScriptableObject
{
    public int maxHealth = 100;
    public float moveSpeed = 3f;
}
```

→ [ScriptableObject Pattern](scriptableobject.md)

---

## Common combinations - P1

### Health system (Variable + EventChannel)

```
FloatVariableSO (playerHealth)
    │
    └──▶ FloatEventChannelSO (onHealthChanged)
              │
              ├──▶ HealthBarUI (update display)
              ├──▶ AudioManager (low health sound)
              └──▶ PostProcessing (damage vignette)
```

### Enemy tracking (RuntimeSet + EventChannel)

```
EnemyRuntimeSetSO (activeEnemies)
    │
    ├──▶ VoidEventChannelSO (onEnemiesChanged)
    │         └──▶ UI (update enemy count)
    │
    └──▶ IntEventChannelSO (onEnemyCountChanged)
              └──▶ WaveManager (check wave complete)
```

### Entity state management (ReactiveEntitySet)

```
EnemyEntitySetSO (enemyStates)
    │
    ├──▶ Per-entity subscription
    │         └──▶ EnemyHealthBar (track specific enemy)
    │
    └──▶ IntEventChannelSO (OnItemRemoved)
              └──▶ ScoreManager (add points on death)
```

---

## Debugging tools - P1

| Tool | Menu Path | Purpose |
|------|-----------|---------|
| Event Monitor | `Window/Reactive SO/Event Monitor` | Real-time event logging |
| Variable Monitor | `Window/Reactive SO/Variable Monitor` | View all Variable values |
| Runtime Set Monitor | `Window/Reactive SO/Runtime Set Monitor` | Track RuntimeSet contents |

---

## References

- [Event Channels](event-channels.md)
- [Variables System](variables.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [RES Job System](reactive-entity-sets-job-system.md) - Orchestrator, Burst
- [RES Persistence](reactive-entity-sets-persistence.md) - Snapshot, save/load
- [ScriptableObject Pattern](scriptableobject.md)
- [Dependency Management](dependency-management.md)
- [Extension Patterns](extension-patterns.md)

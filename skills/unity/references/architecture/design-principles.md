# Design Principles

## Purpose

Define the foundational design philosophy behind Tang3cko.ReactiveSO architecture. Understanding these principles is essential for making correct architectural decisions.

---

## Core principles - P1

### The three pillars

| Principle | Goal | Enables |
|-----------|------|---------|
| **Observability** | Capture and restore complete state | Time Travel, Flight Recorder, Golden Snapshots |
| **Asset-based DI** | Decouple systems via Inspector | Flexible testing without DI containers |
| **Data-Oriented Design** | Separate data from logic | Contiguous memory, deterministic behavior |

These principles are interconnected. Data-Oriented Design enables Observability. Asset-based DI enables testability through Observability.

---

## True Observability - P1

### Set Theoretic foundation

Observability is not just "seeing values in the Inspector." It has a mathematical foundation.

**The model:**

```
S_n(T_i) ⊆ T

T   = Total Set (all data in the system)
S_n = Observer (a filter or view)
i   = Time index
```

An observer never sees raw "Truth" directly. They see a subset filtered through a specific perspective:

- `S_Inspector`: Data formatted for Unity Inspector
- `S_GameView`: Data rendered as pixels on screen
- `S_Logic`: Data accessed by a specific algorithm

### True Observability defined

> **True Observability** means the ability to capture T_i (total state at moment i) deterministically, so any Observer S_n can be applied later to produce the exact same result.

### The problem with traditional Unity

Traditional OOP/Singleton architectures scatter T_i across:
- Private fields
- Object references
- Heap memory

**Result:**
- Cannot capture T_i (impossible to serialize entire heap)
- Cannot replay S_n(T_i) (bugs cannot be reproduced deterministically)

### The RES solution: Data as "Truth"

Reactive Entity Sets solves this by strictly separating **Data (T)** from **Behavior (S_n)**.

```
Data (T)
├── Contiguous memory layout (unmanaged struct)
├── Instant snapshots (MemCpy/Blit, not serialization)
└── Deterministic (pure functions produce same results)

Behavior (S_n)
├── Pure functions (no side effects)
├── Applied to data externally
└── Can be tested independently
```

### Practical applications

| Pattern | Description |
|---------|-------------|
| **Time Travel Debugging** | Store history of T_i, seek to any frame, inspect and intervene |
| **Flight Recorder** | Ring buffer of last 60s snapshots, dump on error for exact reproduction |
| **Hot Swapping** | Save T_now to binary, edit externally, inject back into running game |
| **Golden Snapshots** | Save complex states as test data, run logic assertions |

### Example: Time Travel Debugging

```csharp
// Store history
history.Add(entitySet.CreateSnapshot(Allocator.Persistent));

// Rewind time (0.0ms switching)
public void SeekToFrame(int frame)
{
    // Restoring is just a memory copy
    // The "Truth" of the world is instantly overwritten
    entitySet.RestoreSnapshot(history[frame]);
}
```

You are not replaying inputs; you are restoring **entire memory state**. Pause, inspect, intervene, observe butterfly effects.

---

## Asset-based Dependency Injection - P1

### DI vs DI Container

| Term | What it is |
|------|------------|
| **Dependency Injection** | The *pattern* of providing dependencies externally |
| **DI Container** | A *tool* (VContainer, Zenject) that automates DI |
| **Pure DI / Manual DI** | Practicing DI without a container |

Many developers conflate "DI = Zenject". This is incorrect. VContainer *performs* DI internally, but VContainer itself is not DI.

### Pure DI via Inspector

When you write:

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private IntVariableSO health;  // Dependency
}
```

And drag an asset into the Inspector field, you are practicing **Pure DI via Field Injection**. The Unity Inspector acts as the "assembler" that populates the dependency. No DI Container required.

### Traditional vs Asset-based

**Traditional (Tight Coupling):**

```csharp
public class EnemySpawner : MonoBehaviour
{
    void Spawn()
    {
        // "I need to find the specific Manager in this scene"
        GameManager.Instance.RegisterEnemy(newEnemy);
    }
}
```

**Asset-based DI (Loose Coupling):**

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObjectRuntimeSetSO enemySet;

    void Spawn()
    {
        // "I don't care who holds this list, I just add to it"
        enemySet.Add(newEnemy);
    }
}
```

### ScriptableObjects as stable anchors

Scenes load and unload. MonoBehaviours are created and destroyed. But **ScriptableObject assets remain constant** as stable connection points.

### Trade-offs

ScriptableObjects are not universally superior:

| Trade-off | Description |
|-----------|-------------|
| **Runtime data persistence** | Non-serialized data lost if asset unloaded |
| **Not scene-specific** | For per-scene state, MonoBehaviours may fit better |
| **Asset Management Hell** | Dozens/hundreds of assets to manage, find, name |

You are trading **code complexity** for **asset complexity**.

---

## Data-Oriented Design - P1

### The pattern

```
State (struct)        : Data only, no methods
Calculation logic     : Pure functions in separate classes
RES                   : Storage and notification
GameObject            : Visualization and reaction
```

### State structs: Fields only

```csharp
[Serializable]
public struct EnemyState
{
    public int health;
    public int maxHealth;
    public bool isStunned;
    public float stunEndTime;

    // NO methods that modify state
    // void TakeDamage(int damage) <- Don't do this
}
```

### Calculation logic: Pure functions

```csharp
public static class EnemyStateCalculator
{
    public static EnemyState ApplyDamage(EnemyState state, int damage)
    {
        state.health = Mathf.Max(0, state.health - damage);
        return state;
    }

    public static bool IsDead(EnemyState state)
    {
        return state.health <= 0;
    }
}
```

### Usage ties them together

```csharp
entitySet.UpdateData(enemyId, state =>
    EnemyStateCalculator.ApplyDamage(state, damage));
```

### Why this enables observability

| Property | Benefit |
|----------|---------|
| **unmanaged struct** | Contiguous memory, instant snapshot via MemCpy |
| **Pure functions** | Deterministic, same input = same output |
| **No side effects** | Logic can be tested without Unity |
| **Thread-safe** | Pure functions safe for Job System |

### Correspondence with other paradigms

| Paradigm | Data | Logic |
|----------|------|-------|
| Functional | Immutable data | Pure functions |
| ECS | Component | System |
| Redux | State | Reducer |
| **RES** | **State struct** | **Calculation classes** |

---

## Set Theory Foundation - P1

### ReactiveEntitySet as mathematical set

A ReactiveEntitySet is a mathematical set S where each element is (ID, Data):

```
S = { (id_1, data_1), (id_2, data_2), ..., (id_n, data_n) }
```

**Properties:**
- **Uniqueness**: Each ID appears at most once
- **Membership**: Entity is in the set or not (no partial)
- **Data association**: Each member has exactly one data value

### Views

Views are subsets defined by predicates:

**Static View R(c_0):**
```
R(c_0) = { (id, data) in S | P(data) = true }
```
Predicate depends only on data. Fixed at creation.

**Dynamic View R(c_1):**
```
R(c_1) = { (id, data) in S | P(data, context) = true }
```
Predicate requires external context to evaluate.

### Structural closure

Operations on a View cannot "escape" to the parent set:

```csharp
var view = enemies.CreateView(state => state.Health < 30);

// Access within view
view.EntityIds
view.Count
view.ForEach(...)

// No backflow to parent set (API not provided)
// view.ParentSet  <- Does not exist
```

### Performance implications

**Traditional:**
```
Query -> Traverse all entities -> Filter -> Result
Cost: O(n) per query
```

**Reactive View:**
```
Entity state change
  -> Evaluate view predicate
  -> Add/remove from view
  -> Notify subscribers

Cost: O(v) per change, where v = number of views
```

---

## Testability - P1

### Three pillars of testable code

| Pillar | Description |
|--------|-------------|
| **Modular** | Systems don't depend on each other directly |
| **Editable** | Data lives in SO assets, not hard-coded |
| **Debuggable** | Built-in monitoring tools for real-time observation |

### Inspector as DI Container

The Inspector itself becomes your DI container:

- **Zero setup** - Drag and drop asset assignment
- **Visual mocking** - Swap production assets with test assets
- **Designer-friendly** - Create test scenarios via new SO assets

### State Injection

Test complex states without playing through the game:

```csharp
[Test]
public void BossBattle_Enraged_Test()
{
    // Create fresh set instance
    var set = ScriptableObject.CreateInstance<EnemyEntitySetSO>();

    // Inject specific complex state instantly
    set.Register(bossId, new EnemyState { Health = 10, IsEnraged = true });

    // Run logic and verify
    damageSystem.ApplyDamage(bossId, 15);
    Assert.IsTrue(set[bossId].IsDead);
}
```

Hours of manual debugging become milliseconds of automated testing.

### Testing without mock libraries

No Moq, NSubstitute, or other mocking frameworks needed:

1. **ScriptableObjects are simple** - No complex behavior to mock
2. **Events are easy to verify** - Subscribe and check if called
3. **State is directly accessible** - No mock setups required

---

## Design checklist - P1

Before making architectural decisions, ask:

### 1. Observability

- Can the complete state T_i be captured at any moment?
- Can state be restored deterministically?
- Can any Observer S_n be applied to captured state?

### 2. Dependency

- Is the dependency provided externally (DI)?
- Is ScriptableObject used as stable anchor?
- Can dependencies be swapped for testing?

### 3. Data/Logic separation

- Is state stored in struct (no methods)?
- Is logic in pure functions (no side effects)?
- Can logic be tested without Unity?

### 4. Set membership

- Does entity exist in ReactiveEntitySet?
- Is state managed by RES, not GameObject?
- Can entity exist without visual representation?

---

## Entity vs Object vs View - P1

RES makes a clear distinction:

| Concept | Description | Lifecycle |
|---------|-------------|-----------|
| **Entity** | Logical unit with ID and state | Defined by RES registration |
| **Object** | Runtime representation (GameObject) | Defined by Unity instantiation |
| **View** | GameObject that displays entity data | Doesn't own the data |

### The key insight

Entity existence is determined by presence in ReactiveEntitySet, **not** by existence of Unity object:

```
Entity exists in RES         -> Entity is "alive"
GameObject exists in scene   -> Object is "visible"
```

These can be independent:
- **Entity without Object**: Data persists, no visual (network pre-spawn)
- **Object without Entity**: Visual exists, not tracked (decorations)

### GameObjects as Views

**Traditional Unity:**
```
GameObject owns its data
  └── MonoBehaviour holds state
      └── Destroyed when GameObject destroyed
```

**RES pattern:**
```
ReactiveEntitySet owns the data (ScriptableObject)
  └── Data persists across scenes
      └── GameObject is a "view" into that data
          └── Can be destroyed without losing state
```

---

## Data guidelines - P1

### The fundamental rule

> **Only include data in RES that is computed or managed OUTSIDE of GameObjects.**

### What belongs in RES

| Data | Computed by | In RES? | Reason |
|------|-------------|---------|--------|
| Health | Damage system | Yes | External logic |
| MaxHealth | Configuration | Yes | Reference data |
| SpeedMultiplier | Buff system | Yes | External logic |
| IsStunned | Status system | Yes | External logic |
| **Position** | Transform | **No** | GameObject owns it |
| **Rotation** | Transform | **No** | GameObject owns it |

### Exception: Network games

Server-authoritative position is different:

```csharp
[Serializable]
public struct NetworkEntityState
{
    public Vector3 Position;      // Server-computed, OK in RES
    public Quaternion Rotation;   // Server-computed, OK in RES
    public int Health;
}
```

GameObject's Transform becomes a **view** of authoritative position in RES.

### Decision flowchart

```
Should this data go in RES?
│
├── Computed by external system?
│   ├── NO  -> Keep as GameObject field
│   └── YES -> Continue
│
├── Change frequency <10% per frame?
│   ├── NO  -> Consider traditional approach
│   └── YES -> Continue
│
├── Multiple systems observe changes?
│   ├── NO  -> May not need RES
│   └── YES -> Add to RES
│
└── Need scene-independent persistence?
    └── YES -> RES provides this automatically
```

---

## References

- [Architecture Overview](overview.md) - Pattern selection
- [Event Channels](event-channels.md)
- [Variables System](variables.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [Dependency Management](dependency-management.md)

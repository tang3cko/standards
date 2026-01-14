# Reactive Entity Sets

## Purpose

Centralized per-entity state management using ScriptableObjects. Provides O(1) ID-based lookup, automatic change notifications, and scene-persistent data with clean separation between data and visualization.

---

## Design philosophy - P1

### Entity-Object-View model

RES inverts Unity's traditional ownership pattern.

**Traditional Unity:**

```
GameObject owns its data
  └── MonoBehaviour holds state
      └── Destroyed with GameObject
```

**RES pattern:**

```
ScriptableObject owns the data (persistent)
  └── Entity = logical unit with ID and state
      └── GameObject = view (displays data, doesn't own it)
          └── Can be destroyed without losing state
```

### Key insight

Entity existence is determined by RES registration, not GameObject existence.

| Concept | Description |
|---------|-------------|
| Entity | Logical unit with ID and state, exists in RES |
| Object | Runtime representation (GameObject), can exist independently |
| View | GameObject that displays entity data but doesn't own it |

### What this enables

- **Cross-scene persistence** - State survives scene transitions without DontDestroyOnLoad
- **Network synchronization** - Entity can exist before visual representation spawns
- **Object pooling** - Reuse GameObjects while maintaining entity identity

---

## When to use - P1

### Use RES when

- Per-entity state management (health, mana, status effects)
- ID-based lookup without Find operations
- State must persist across scenes
- External systems need entity data (UI, AI, networking)

### Use RuntimeSet when

- Simple object tracking (no per-entity state)
- Iterate over all active objects
- No ID-based lookup needed

### Comparison

| Feature | RuntimeSet | ReactiveEntitySet |
|---------|------------|-------------------|
| Stores | Object references | Per-entity data |
| Lookup | Iteration O(n) | ID-based O(1) |
| Per-entity state | No | Yes |
| Events | Collection level | Per-entity + collection |
| Use case | Object tracking | State management |

---

## Data struct rules - P1

### Requirements

RES data must be a struct with:

- `unmanaged` constraint (no managed references)
- **Fields only** (no properties, no methods)
- All logic in external pure functions

### Correct

```csharp
[Serializable]
public struct EnemyState
{
    public int health;
    public int maxHealth;
    public bool isStunned;
    public float stunEndTime;
}
```

### Wrong

```csharp
[Serializable]
public struct EnemyState
{
    public int health;
    public int maxHealth;

    // NG: Property is logic, even if read-only
    public float HealthPercent => (float)health / maxHealth;

    // NG: Method
    public void TakeDamage(int damage) { health -= damage; }
}
```

### Logic separation

All calculation logic belongs in separate utility classes as pure functions.

```csharp
public static class EnemyStateCalculator
{
    public static float GetHealthPercent(EnemyState state)
        => state.maxHealth > 0 ? (float)state.health / state.maxHealth : 0f;

    public static EnemyState ApplyDamage(EnemyState state, int damage)
    {
        state.health = Mathf.Max(0, state.health - damage);
        return state;
    }

    public static EnemyState ApplyStun(EnemyState state, float duration)
    {
        state.isStunned = true;
        state.stunEndTime = Time.time + duration;
        return state;
    }
}
```

Usage with RES:

```csharp
entitySet.UpdateData(entityId, state =>
    EnemyStateCalculator.ApplyDamage(state, damage));
```

---

## What data belongs in RES - P1

### Include

Data computed or managed by **external systems**.

| Data | Computed By | Reason |
|------|-------------|--------|
| health | Damage system | External logic modifies |
| maxHealth | Configuration | Reference data |
| isStunned | Status effect system | External logic controls |
| buffMultiplier | Buff system | External logic modifies |

### Exclude

Data owned by **GameObject itself**.

| Data | Owned By | Reason |
|------|----------|--------|
| Position | Transform | GameObject owns |
| Rotation | Transform | GameObject owns |
| Scale | Transform | GameObject owns |

### Exception: Server-authoritative networking

When server computes position, RES becomes the authoritative source.

```csharp
[Serializable]
public struct NetworkEntityState
{
    public float3 position;    // Server-computed, OK in RES
    public quaternion rotation; // Server-computed, OK in RES
    public int health;
}
```

In this case, GameObject's Transform is a **view** of the authoritative position in RES.

---

## Usage patterns - P1

### Pattern 1: Standalone data management

Pure data management without GameObjects. Ideal for simulations, AI systems, networking.

```csharp
public class SimulationManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;

    private void Start()
    {
        // Register entities with int IDs
        for (int i = 0; i < 10000; i++)
        {
            unitSet.Register(i, new UnitState
            {
                health = 100,
                teamId = i % 4
            });
        }
    }

    private void Update()
    {
        // Update all entities
        foreach (var id in unitSet.EntityIds)
        {
            unitSet.UpdateData(id, state =>
            {
                // Simulation logic
                return state;
            });
        }
    }

    private void OnDestroy()
    {
        unitSet.Clear();
    }
}
```

### Pattern 2: MonoBehaviour backend

Using `ReactiveEntity<T>` for automatic lifecycle management.

```csharp
public class Enemy : ReactiveEntity<EnemyState>
{
    [SerializeField] private EnemyStateSetSO enemySet;
    [SerializeField] private int maxHealth = 100;

    protected override ReactiveEntitySetSO<EnemyState> Set => enemySet;

    protected override EnemyState InitialState => new EnemyState
    {
        health = maxHealth,
        maxHealth = maxHealth,
        isStunned = false
    };

    // Auto Register on OnEnable
    // Auto Unregister on OnDisable

    public void TakeDamage(int damage)
    {
        State = EnemyStateCalculator.ApplyDamage(State, damage);

        if (State.health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
```

---

## Basic API - P1

### Creating the asset

```csharp
[CreateAssetMenu(
    fileName = "EnemyStateSet",
    menuName = "Reactive SO/Reactive Entity Sets/Enemy State")]
public class EnemyStateSetSO : ReactiveEntitySetSO<EnemyState>
{
}
```

### Registration

```csharp
// With int ID
entitySet.Register(entityId, initialState);
entitySet.Unregister(entityId);

// With MonoBehaviour (uses GetInstanceID internally)
entitySet.Register(this, initialState);
entitySet.Unregister(this);
```

### Data access

```csharp
// Read
var state = entitySet.GetData(entityId);

// Safe read
if (entitySet.TryGetData(entityId, out var state))
{
    // Use state
}

// Write (triggers events)
entitySet.SetData(entityId, newState);

// Update (functional pattern, recommended)
entitySet.UpdateData(entityId, state =>
{
    state.health -= damage;
    return state;
});
```

### Per-entity subscription

```csharp
// Subscribe to specific entity
entitySet.SubscribeToEntity(entityId, OnStateChanged);

// Always unsubscribe
entitySet.UnsubscribeFromEntity(entityId, OnStateChanged);

// Callback receives old and new state
private void OnStateChanged(EnemyState oldState, EnemyState newState)
{
    if (newState.health < oldState.health)
    {
        // Took damage
    }
}
```

### Set-level events (via Inspector)

Assign EventChannels to ReactiveEntitySetSO in Inspector:

- `OnItemAdded` (IntEventChannelSO) - Entity registered
- `OnItemRemoved` (IntEventChannelSO) - Entity unregistered
- `OnDataChanged` (IntEventChannelSO) - Entity state changed
- `OnSetChanged` (VoidEventChannelSO) - Any change occurred

---

## Anti-patterns - P1

### Direct mutation without SetData

```csharp
// NG: Changes not saved to RES
var state = entitySet.GetData(entityId);
state.health -= damage;
// state is a copy, RES not updated!

// OK: Use SetData or UpdateData
entitySet.UpdateData(entityId, state =>
{
    state.health -= damage;
    return state;
});
```

### Logic in struct

```csharp
// NG: Logic belongs in external utility
public struct EnemyState
{
    public float HealthPercent => (float)health / maxHealth;
}

// OK: External pure function
public static class EnemyStateCalculator
{
    public static float GetHealthPercent(EnemyState state) => ...;
}
```

### Transform data in RES

```csharp
// NG: GameObject owns Transform
public struct EnemyState
{
    public Vector3 position;
    public Quaternion rotation;
}

// OK: Only external-computed data
public struct EnemyState
{
    public int health;
    public bool isStunned;
}
```

### Forgetting to unsubscribe

```csharp
// NG: Memory leak
private void OnEnable()
{
    entitySet.SubscribeToEntity(entityId, OnStateChanged);
}
// Missing OnDisable!

// OK: Always unsubscribe
private void OnDisable()
{
    entitySet.UnsubscribeFromEntity(entityId, OnStateChanged);
}
```

---

## References

- [Job System Integration](reactive-entity-sets-job-system.md) - Orchestrator, double buffering, Burst
- [Persistence](reactive-entity-sets-persistence.md) - Snapshot, restore, save/load
- [RuntimeSet Pattern](runtime-sets.md) - Object tracking without per-entity state
- [Event Channels](event-channels.md) - Set-level notifications

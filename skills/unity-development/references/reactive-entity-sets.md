# ReactiveEntitySet details

## Design philosophy

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
```

**Key insight:** Entity existence is determined by RES registration, not GameObject existence.

## Data struct rules

RES data must be a struct with **fields only** (no properties, no methods).

```csharp
[Serializable]
public struct EnemyState
{
    public int health;
    public int maxHealth;
    public bool isStunned;
    public float stunEndTime;
}

// Logic in external pure functions
public static class EnemyStateCalculator
{
    public static float GetHealthPercent(EnemyState state)
        => state.maxHealth > 0 ? (float)state.health / state.maxHealth : 0f;

    public static EnemyState ApplyDamage(EnemyState state, int damage)
    {
        state.health = Mathf.Max(0, state.health - damage);
        return state;
    }

    public static bool IsDead(EnemyState state)
        => state.health <= 0;
}
```

## Basic API

```csharp
// Create asset
[CreateAssetMenu(fileName = "EnemyStateSet", menuName = "Reactive SO/Reactive Entity Sets/Enemy State")]
public class EnemyStateSetSO : ReactiveEntitySetSO<EnemyState> { }

// Registration
entitySet.Register(entityId, initialState);
entitySet.Unregister(entityId);

// Data access
var state = entitySet.GetData(entityId);
entitySet.TryGetData(entityId, out var state);
entitySet.SetData(entityId, newState);

// Update (functional pattern, recommended)
entitySet.UpdateData(entityId, state =>
{
    state.health -= damage;
    return state;
});

// Per-entity subscription
entitySet.SubscribeToEntity(entityId, OnStateChanged);
entitySet.UnsubscribeFromEntity(entityId, OnStateChanged);
```

## ReactiveEntity base class

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

    // Auto Register on OnEnable, Auto Unregister on OnDisable

    public void TakeDamage(int damage)
    {
        State = EnemyStateCalculator.ApplyDamage(State, damage);

        if (EnemyStateCalculator.IsDead(State))
        {
            Destroy(gameObject);
        }
    }
}
```

---

## Job System integration

Use Orchestrator when processing 1,000+ entities every frame.

### Double buffering

```
Frame N:
┌─────────────────┐     ┌─────────────────┐
│  Front Buffer   │     │  Back Buffer    │
│  (Current State)│     │  (Next State)   │
│  Read by:       │     │  Written by:    │
│  - Rendering    │     │  - Jobs         │
│  - UI           │     │  - Simulation   │
└─────────────────┘     └─────────────────┘

After CompleteAndApply(): Buffers swap via pointer exchange (O(1))
```

### Orchestrator lifecycle

```csharp
public class SimulationManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;
    private ReactiveEntitySetOrchestrator<UnitState> orchestrator;

    private void Start()
    {
        orchestrator = new ReactiveEntitySetOrchestrator<UnitState>(unitSet);
    }

    private void Update()
    {
        if (unitSet.Count == 0) return;
        JobHandle handle = ScheduleSimulation();
        orchestrator.ScheduleUpdate(handle, unitSet.Count);
    }

    private void LateUpdate()
    {
        orchestrator?.CompleteAndApply();
    }

    private void OnDestroy()
    {
        orchestrator?.Dispose();
        orchestrator = null;
    }
}
```

### Burst-compiled Job

```csharp
[BurstCompile]
public struct UnitSimulationJob : IJobParallelFor
{
    [ReadOnly] public NativeSlice<UnitState> input;
    public NativeArray<UnitState> output;
    public float deltaTime;

    public void Execute(int index)
    {
        UnitState unit = input[index];
        unit.position += unit.velocity * deltaTime;
        output[index] = unit;
    }
}
```

---

## Snapshot and persistence

### Creating and restoring snapshots

```csharp
// Capture current state
EntitySetSnapshot<UnitState> snapshot = unitSet.CreateSnapshot(Allocator.Persistent);

// Restore to captured state
unitSet.RestoreSnapshot(snapshot);

// Memory management
snapshot.Dispose(); // Required for Persistent allocator
```

### Time-travel pattern

```csharp
public class HistoryManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;
    private List<EntitySetSnapshot<UnitState>> history = new();
    private int maxSnapshots = 300;

    private void LateUpdate()
    {
        if (history.Count >= maxSnapshots)
        {
            history[0].Dispose();
            history.RemoveAt(0);
        }
        history.Add(unitSet.CreateSnapshot(Allocator.Persistent));
    }

    public void Rewind(int frames)
    {
        int targetIndex = history.Count - 1 - frames;
        if (targetIndex >= 0 && targetIndex < history.Count)
        {
            unitSet.RestoreSnapshot(history[targetIndex]);
        }
    }

    private void OnDestroy()
    {
        foreach (var snapshot in history) snapshot.Dispose();
        history.Clear();
    }
}
```

### ReactiveEntitySetHolder

Prevent ScriptableObject unload during scene transitions:

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private ReactiveEntitySetHolder holder;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
```

---

## Anti-patterns

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
// NG
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

### Forgetting to unsubscribe

```csharp
// NG: Memory leak
private void OnEnable()
{
    entitySet.SubscribeToEntity(entityId, OnStateChanged);
}
// Missing OnDisable!

// OK
private void OnDisable()
{
    entitySet.UnsubscribeFromEntity(entityId, OnStateChanged);
}
```

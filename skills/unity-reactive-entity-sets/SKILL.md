---
name: unity-reactive-entity-sets
description: ReactiveEntitySetSO for per-entity state. O(1) ID lookup, state change callbacks. Use when managing entity state in Unity.
---

# Reactive Entity Sets

## Purpose

Centralized per-entity state management using ScriptableObjects. Provides O(1) ID-based lookup, automatic change notifications, and scene-persistent data.

## Checklist

- [ ] Define state struct with fields only (no methods, no properties)
- [ ] Put logic in separate pure function classes
- [ ] Use UpdateData for functional state updates
- [ ] Subscribe/Unsubscribe in OnEnable/OnDisable
- [ ] Clear RES when scene unloads

---

## Design philosophy - P1

### Entity-Object-View model

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

### Key insight

Entity existence is determined by RES registration, not GameObject existence.

---

## Data struct rules - P1

### Requirements

RES data must be a struct with:
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

### Logic separation

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

    public static bool IsDead(EnemyState state)
        => state.health <= 0;
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

private void OnStateChanged(EnemyState oldState, EnemyState newState)
{
    if (newState.health < oldState.health)
    {
        // Took damage
    }
}
```

---

## Usage patterns - P1

### Pattern 1: ReactiveEntity base class

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

### Pattern 2: Standalone data management

```csharp
public class SimulationManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;

    private void Start()
    {
        // Register entities with int IDs (no GameObjects needed)
        for (int i = 0; i < 10000; i++)
        {
            unitSet.Register(i, new UnitState { health = 100 });
        }
    }

    private void OnDestroy()
    {
        unitSet.Clear();
    }
}
```

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

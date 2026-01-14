# ReactiveEntitySet Pattern

## Purpose

Implement centralized state management for game entities using ScriptableObjects, providing ID-based entity lookup, per-entity state tracking, and scene-persistent data with O(1) performance.

## Checklist

- [ ] Use ReactiveEntitySet for centralized entity state management
- [ ] Define custom data struct for entity state (must be a value type)
- [ ] Create ReactiveEntitySetSO asset via `Reactive SO/Reactive Entity Sets/...`
- [ ] Use `ReactiveEntity<TData>` base class for automatic lifecycle management
- [ ] Subscribe to per-entity events with `SubscribeToEntity`
- [ ] Assign EventChannels for set-level notifications (optional)
- [ ] Use unique entity IDs for lookups
- [ ] Consider scene persistence requirements

---

## ReactiveEntitySet vs RuntimeSet - P1

### When to use ReactiveEntitySet

```
✅ Per-entity state management (health, mana, status effects)
✅ ID-based entity lookup (get entity by ID)
✅ Scene-persistent state (state survives scene reloads)
✅ Complex state updates with per-entity events
✅ Centralized data for external systems (UI, AI, networking)
```

### When to use RuntimeSet

```
✅ Simple object tracking (no per-entity state)
✅ Iterate over all active objects
✅ Simple add/remove lifecycle
✅ No need for ID-based lookup
```

### Comparison

| Feature | RuntimeSet | ReactiveEntitySet |
|---------|------------|-------------------|
| State storage | None | Per-entity data |
| Lookup | Iteration | O(1) by ID |
| Events | EventChannel-based | Per-entity + EventChannel |
| Persistence | Scene lifecycle | ScriptableObject |
| Use case | Object tracking | State management |

---

## Core concepts - P1

### Architecture overview

```
ReactiveEntitySetSO<TData>
├── Sparse Set data structure (O(1) operations)
├── Per-entity state storage
├── Inspector-assignable EventChannels:
│   ├── OnItemAdded (IntEventChannelSO) - raised with entity ID
│   ├── OnItemRemoved (IntEventChannelSO) - raised with entity ID
│   ├── OnDataChanged (IntEventChannelSO) - raised with entity ID
│   └── OnSetChanged (VoidEventChannelSO) - raised on any change
└── Per-entity subscription via SubscribeToEntity()

ReactiveEntity<TData>
├── Automatic registration on OnEnable
├── Automatic unregistration on OnDisable
├── State access via protected State property
└── OnStateChanged event for per-entity observers
```

### Data flow

```
Entity spawns
    ↓
ReactiveEntity.OnEnable()
    ↓
EntitySet.Register(owner, initialData)
    ↓
OnItemAdded EventChannel fires (if assigned)
    ↓
Entity updates state
    ↓
EntitySet.SetData(owner, newData)
    ↓
Per-entity callback fires (if subscribed via SubscribeToEntity)
OnDataChanged EventChannel fires (if assigned)
    ↓
UI/Systems react to state change
```

---

## Basic implementation - P1

### Defining entity data

```csharp
using System;

namespace ProjectName.Enemy
{
    /// <summary>
    /// State data for enemy entities (must be a struct)
    /// </summary>
    [Serializable]
    public struct EnemyStateData
    {
        public int health;
        public int maxHealth;
        public bool isStunned;
        public float stunEndTime;

        public float HealthPercent => maxHealth > 0 ? (float)health / maxHealth : 0f;
        public bool IsDead => health <= 0;
    }
}
```

### Creating ReactiveEntitySetSO

```csharp
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Centralized state management for all enemies
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyEntitySet", menuName = "Reactive SO/Reactive Entity Sets/Enemy")]
    public class EnemyEntitySetSO : ReactiveEntitySetSO<EnemyStateData>
    {
        // Base class provides:
        // - Register(MonoBehaviour owner, TData initialData)
        // - Register(int id, TData initialData)
        // - Unregister(MonoBehaviour owner)
        // - Unregister(int id)
        // - GetData(MonoBehaviour owner) / GetData(int id)
        // - TryGetData(MonoBehaviour owner, out TData) / TryGetData(int id, out TData)
        // - SetData(MonoBehaviour owner, TData) / SetData(int id, TData)
        // - UpdateData(MonoBehaviour owner, Func<TData, TData>)
        // - Contains(MonoBehaviour owner) / Contains(int id)
        // - SubscribeToEntity(int id, Action<TData, TData>)
        // - UnsubscribeFromEntity(int id, Action<TData, TData>)
        // - Count, EntityIds, Data, Clear(), ForEach()
        // Inspector-assignable EventChannels:
        // - OnItemAdded (IntEventChannelSO)
        // - OnItemRemoved (IntEventChannelSO)
        // - OnDataChanged (IntEventChannelSO)
        // - OnSetChanged (VoidEventChannelSO)

        /// <summary>
        /// Get total health of all enemies
        /// </summary>
        public int GetTotalHealth()
        {
            int total = 0;
            foreach (var entityId in EntityIds)
            {
                if (TryGetData(entityId, out var state))
                {
                    total += state.health;
                }
            }
            return total;
        }

        /// <summary>
        /// Get all enemies below health threshold
        /// </summary>
        public List<int> GetLowHealthEnemies(float threshold)
        {
            var result = new List<int>();
            foreach (var entityId in EntityIds)
            {
                if (TryGetData(entityId, out var state) && state.HealthPercent < threshold)
                {
                    result.Add(entityId);
                }
            }
            return result;
        }
    }
}
```

### Manual registration pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Enemy with manual ReactiveEntitySet integration
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        [Header("Entity Set")]
        [SerializeField] private EnemyEntitySetSO entitySet;

        [Header("Settings")]
        [SerializeField] private int maxHealth = 100;

        private int entityId;

        private void OnEnable()
        {
            entityId = GetInstanceID();

            var initialState = new EnemyStateData
            {
                health = maxHealth,
                maxHealth = maxHealth,
                isStunned = false,
                stunEndTime = 0f
            };

            entitySet.Register(entityId, initialState);
        }

        private void OnDisable()
        {
            entitySet.Unregister(entityId);
        }

        public void TakeDamage(int damage)
        {
            if (entitySet.TryGetData(entityId, out var state))
            {
                state.health = Mathf.Max(0, state.health - damage);
                entitySet.SetData(entityId, state);

                if (state.IsDead)
                {
                    Die();
                }
            }
        }

        public void ApplyStun(float duration)
        {
            if (entitySet.TryGetData(entityId, out var state))
            {
                state.isStunned = true;
                state.stunEndTime = Time.time + duration;
                entitySet.SetData(entityId, state);
            }
        }

        private void Die()
        {
            Destroy(gameObject);
        }
    }
}
```

### Using ReactiveEntity base class

For cleaner code, extend `ReactiveEntity<TData>`:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Enemy using ReactiveEntity base class for automatic lifecycle
    /// </summary>
    public class EnemyEntity : ReactiveEntity<EnemyStateData>
    {
        [SerializeField] private EnemyEntitySetSO entitySet;
        [SerializeField] private int maxHealth = 100;

        // Required: specify which set to use
        protected override ReactiveEntitySetSO<EnemyStateData> Set => entitySet;

        // Required: specify initial state
        protected override EnemyStateData InitialState => new EnemyStateData
        {
            health = maxHealth,
            maxHealth = maxHealth,
            isStunned = false,
            stunEndTime = 0f
        };

        public void TakeDamage(int damage)
        {
            var state = State;
            state.health = Mathf.Max(0, state.health - damage);
            State = state;  // Automatically triggers events

            if (state.IsDead)
            {
                Destroy(gameObject);
            }
        }

        public void ApplyStun(float duration)
        {
            var state = State;
            state.isStunned = true;
            state.stunEndTime = Time.time + duration;
            State = state;
        }

        // Optional: cleanup before unregistration
        protected override void OnBeforeUnregister()
        {
            Debug.Log($"Enemy dying with {State.health} HP");
        }
    }
}
```

---

## State change events - P1

### Per-entity subscription

Use `SubscribeToEntity` for per-entity state change callbacks:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    /// <summary>
    /// Health bar that tracks a specific enemy
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("Entity Set")]
        [SerializeField] private EnemyEntitySetSO entitySet;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Image fillImage;

        private int trackedEntityId;

        public void SetTrackedEntity(int entityId)
        {
            // Unsubscribe from previous entity
            if (trackedEntityId != 0)
            {
                entitySet.UnsubscribeFromEntity(trackedEntityId, OnStateChanged);
            }

            trackedEntityId = entityId;

            // Subscribe to new entity (callback receives oldState, newState)
            entitySet.SubscribeToEntity(entityId, OnStateChanged);

            // Update immediately
            if (entitySet.TryGetData(entityId, out var state))
            {
                UpdateHealthBar(state);
            }
        }

        private void OnDisable()
        {
            if (trackedEntityId != 0)
            {
                entitySet.UnsubscribeFromEntity(trackedEntityId, OnStateChanged);
            }
        }

        // Callback signature: Action<TData, TData> (oldState, newState)
        private void OnStateChanged(EnemyStateData oldState, EnemyStateData newState)
        {
            UpdateHealthBar(newState);

            // Can compare old and new state
            if (newState.health < oldState.health)
            {
                Debug.Log($"Took {oldState.health - newState.health} damage");
            }
        }

        private void UpdateHealthBar(EnemyStateData state)
        {
            fillImage.fillAmount = state.HealthPercent;
        }
    }
}
```

### Using ReactiveEntity.OnStateChanged event

When using `ReactiveEntity<TData>` base class:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class EnemyStatusUI : MonoBehaviour
    {
        [SerializeField] private EnemyEntity trackedEnemy;
        [SerializeField] private UnityEngine.UI.Image healthFill;

        private void OnEnable()
        {
            if (trackedEnemy != null)
            {
                // ReactiveEntity exposes OnStateChanged event
                trackedEnemy.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDisable()
        {
            if (trackedEnemy != null)
            {
                trackedEnemy.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(EnemyStateData oldState, EnemyStateData newState)
        {
            healthFill.fillAmount = newState.HealthPercent;
        }
    }
}
```

### Set-level EventChannel notifications

Assign EventChannels in the ReactiveEntitySet Inspector for set-level notifications:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    /// <summary>
    /// Tracks all enemy spawns and deaths via EventChannels
    /// </summary>
    public class EnemyTracker : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onEnemyAdded;
        [SerializeField] private IntEventChannelSO onEnemyRemoved;

        private void OnEnable()
        {
            // Subscribe to EventChannels assigned to the EntitySet
            onEnemyAdded.OnEventRaised += HandleEnemySpawned;
            onEnemyRemoved.OnEventRaised += HandleEnemyRemoved;
        }

        private void OnDisable()
        {
            onEnemyAdded.OnEventRaised -= HandleEnemySpawned;
            onEnemyRemoved.OnEventRaised -= HandleEnemyRemoved;
        }

        private void HandleEnemySpawned(int entityId)
        {
            Debug.Log($"Enemy spawned: {entityId}");
        }

        private void HandleEnemyRemoved(int entityId)
        {
            Debug.Log($"Enemy removed: {entityId}");
        }
    }
}
```

---

## UpdateData pattern - P1

Use `UpdateData` for safer state modifications:

```csharp
// Using SetData (read-modify-write)
if (entitySet.TryGetData(entityId, out var state))
{
    state.health -= damage;
    entitySet.SetData(entityId, state);
}

// Using UpdateData (functional approach)
entitySet.UpdateData(entityId, state => {
    state.health -= damage;
    return state;
});

// Using UpdateData with MonoBehaviour
entitySet.UpdateData(this, state => {
    state.health = Mathf.Max(0, state.health - damage);
    state.isStunned = damage > 50;
    return state;
});
```

---

## ID management - P1

### Using InstanceID

For simple cases, use Unity's InstanceID:

```csharp
private void OnEnable()
{
    entityId = GetInstanceID();
    entitySet.Register(entityId, initialState);
}
```

### Using MonoBehaviour directly

For convenience, use MonoBehaviour overloads:

```csharp
private void OnEnable()
{
    // MonoBehaviour.GetInstanceID() is used internally
    entitySet.Register(this, initialState);
}

private void OnDisable()
{
    entitySet.Unregister(this);
}

public void TakeDamage(int damage)
{
    if (entitySet.TryGetData(this, out var state))
    {
        state.health -= damage;
        entitySet.SetData(this, state);
    }
}
```

### Using persistent IDs

For save/load systems, use persistent IDs:

```csharp
using UnityEngine;

namespace ProjectName.Enemy
{
    public class PersistentEnemy : MonoBehaviour
    {
        [SerializeField] private EnemyEntitySetSO entitySet;
        [SerializeField] private int persistentId;  // Set in Inspector or loaded from save

        private void OnEnable()
        {
            // Use persistent ID for save/load compatibility
            entitySet.Register(persistentId, LoadOrCreateState());
        }

        private EnemyStateData LoadOrCreateState()
        {
            if (SaveSystem.TryLoadEnemyState(persistentId, out var savedState))
            {
                return savedState;
            }

            return new EnemyStateData { health = 100, maxHealth = 100 };
        }
    }
}
```

---

## Performance characteristics - P1

### Sparse Set data structure

ReactiveEntitySet uses a Sparse Set internally for O(1) operations:

| Operation | Time Complexity |
|-----------|-----------------|
| Register | O(1) |
| Unregister | O(1) |
| GetData | O(1) |
| SetData | O(1) |
| Iteration | O(n) where n = registered entities |

### Memory efficiency

```csharp
// PagedSparseArray allocates memory in pages
// Only allocates pages for used ID ranges
// Efficient for sparse ID distributions

// Example: IDs 1, 2, 3, 1000, 1001
// Only allocates 2 pages (0-255 and 768-1023)
// Not 1002 individual slots
```

---

## Scene persistence - P1

### State survives scene changes

ReactiveEntitySet data is stored in ScriptableObjects:

```csharp
// Enemy in Scene A registers state
entitySet.Register(entityId, stateData);

// Scene B loads additively
// State is still accessible
if (entitySet.TryGetData(entityId, out var state))
{
    // State persists across scenes
}
```

### Cleanup on scene unload

```csharp
public class SceneCleanup : MonoBehaviour
{
    [SerializeField] private EnemyEntitySetSO entitySet;

    private void OnDestroy()
    {
        // Clear all entity data when scene unloads
        entitySet.Clear();
    }
}
```

---

## Anti-patterns - P1

### Direct state mutation

**❌ Bad:**

```csharp
// Getting state and modifying directly doesn't trigger events
var state = entitySet.GetData(entityId);
state.health -= damage;  // State not actually updated in the set!
```

**✅ Good:**

```csharp
// Always use SetData or UpdateData to trigger events
if (entitySet.TryGetData(entityId, out var state))
{
    state.health -= damage;
    entitySet.SetData(entityId, state);  // Triggers per-entity and EventChannel callbacks
}

// Or use UpdateData
entitySet.UpdateData(entityId, state => {
    state.health -= damage;
    return state;
});
```

### Forgetting to unsubscribe

**❌ Bad:**

```csharp
private void OnEnable()
{
    entitySet.SubscribeToEntity(entityId, OnStateChanged);
}
// Missing OnDisable - memory leak!
```

**✅ Good:**

```csharp
private void OnEnable()
{
    entitySet.SubscribeToEntity(entityId, OnStateChanged);
}

private void OnDisable()
{
    entitySet.UnsubscribeFromEntity(entityId, OnStateChanged);
}
```

---

## References

- [RuntimeSet Pattern](runtime-sets.md)
- [Event Channels](event-channels.md)
- [Variables System](variables.md)
- [ScriptableObject Pattern](scriptableobject.md)
- [Dependency Management](dependency-management.md)

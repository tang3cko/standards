# RuntimeSet Pattern

## Purpose

Manage dynamic collections of game objects using ScriptableObjects, avoiding expensive Find operations and enabling efficient iteration over active objects.

## Checklist

- [ ] Use RuntimeSets instead of FindObjectsOfType or FindGameObjectsWithTag
- [ ] Use built-in `GameObjectRuntimeSetSO` or `TransformRuntimeSetSO` when possible
- [ ] Create custom RuntimeSet types via `Reactive SO/Runtime Sets/...`
- [ ] Register objects in OnStartClient/OnEnable
- [ ] Unregister objects in OnStopClient/OnDisable
- [ ] Clear RuntimeSet when scene unloads
- [ ] Assign EventChannels to RuntimeSet for reactive updates (optional)
- [ ] Use Runtime Set Monitor Window for debugging (`Window/Reactive SO/Runtime Set Monitor`)

---

## Built-in RuntimeSet types - P1

### Available types

The package provides base classes and built-in types. Create assets via `Create → Reactive SO → Runtime Sets → [Type]`.

| RuntimeSet Type | Use Case | Example |
|-----------------|----------|---------|
| `RuntimeSetSO<T>` | Base class for custom types | Custom component tracking |
| `GameObjectRuntimeSetSO` | Track GameObjects | Active enemies, collectibles |
| `TransformRuntimeSetSO` | Track Transforms | Waypoints, spawn points |

### Using built-in types

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("RuntimeSet")]
        [SerializeField] private GameObjectRuntimeSetSO enemySet;

        private void OnEnable()
        {
            enemySet?.Add(gameObject);
        }

        private void OnDisable()
        {
            enemySet?.Remove(gameObject);
        }
    }
}
```

---

## Custom RuntimeSet types - P1

### Creating a custom RuntimeSet

Use `RuntimeSetSO<T>` base class for custom component tracking:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// RuntimeSet for managing active enemies
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "Reactive SO/Runtime Sets/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
        // Base class provides:
        // - Items (IReadOnlyList<T>)
        // - Count
        // - Add(T item)
        // - Remove(T item)
        // - Clear()
        // - Contains(T item)
        // Inspector-assignable EventChannels:
        // - onItemsChanged (VoidEventChannelSO)
        // - onCountChanged (IntEventChannelSO)

        /// <summary>
        /// Get the closest enemy to a position
        /// </summary>
        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }
    }
}
```

---

## Object registration - P1

### Register in OnEnable/OnStartClient

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("RuntimeSet")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        // For non-networked games
        private void OnEnable()
        {
            enemySet?.Add(this);
        }

        private void OnDisable()
        {
            enemySet?.Remove(this);
        }

        // For networked games (Mirror)
        public override void OnStartClient()
        {
            base.OnStartClient();
            enemySet?.Add(this);
        }

        public override void OnStopClient()
        {
            enemySet?.Remove(this);
            base.OnStopClient();
        }
    }
}
```

### Scene cleanup

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    /// <summary>
    /// Manages scene lifecycle and cleanup
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [Header("RuntimeSets to Clear")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;
        [SerializeField] private GameObjectRuntimeSetSO playerSet;

        private void OnDestroy()
        {
            // Clear all RuntimeSets when scene unloads
            enemySet?.Clear();
            playerSet?.Clear();
        }

        /// <summary>
        /// Destroy all items and clear the set
        /// </summary>
        public void DestroyAllEnemies()
        {
            // DestroyItems() destroys all GameObjects and clears the set
            // Only available on GameObjectRuntimeSetSO
            playerSet?.DestroyItems();
        }
    }
}
```

---

## Event notifications - P1

### EventChannel-based notifications

RuntimeSets use EventChannels for notifications. Assign EventChannel assets in the RuntimeSet Inspector:

- **onItemsChanged** (VoidEventChannelSO) - Raised when items are added or removed
- **onCountChanged** (IntEventChannelSO) - Raised with the new count when items change

### Setup

1. Create RuntimeSet asset via `Create → Reactive SO → Runtime Sets → [Type]`
2. Create EventChannel assets via `Create → Reactive SO → Channels → [Type]`
3. Assign EventChannels to the RuntimeSet asset in Inspector

### Subscribing to RuntimeSet events

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class EnemyCountDisplay : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onEnemiesChanged;
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;

        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO activeEnemies;

        [Header("UI")]
        [SerializeField] private TMPro.TextMeshProUGUI countText;

        private void OnEnable()
        {
            // Subscribe to the EventChannel assigned to the RuntimeSet
            onEnemiesChanged.OnEventRaised += UpdateCount;
            // Or subscribe to count changes
            onEnemyCountChanged.OnEventRaised += HandleCountChanged;

            UpdateCount();
        }

        private void OnDisable()
        {
            onEnemiesChanged.OnEventRaised -= UpdateCount;
            onEnemyCountChanged.OnEventRaised -= HandleCountChanged;
        }

        private void UpdateCount()
        {
            countText.text = $"Enemies: {activeEnemies.Count}";
        }

        private void HandleCountChanged(int newCount)
        {
            countText.text = $"Enemies: {newCount}";
        }
    }
}
```

### Combining with EventChannels for victory condition

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class EnemyManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;
        [SerializeField] private VoidEventChannelSO onAllEnemiesDefeated;

        private void OnEnable()
        {
            onEnemyCountChanged.OnEventRaised += CheckVictoryCondition;
        }

        private void OnDisable()
        {
            onEnemyCountChanged.OnEventRaised -= CheckVictoryCondition;
        }

        private void CheckVictoryCondition(int count)
        {
            if (count == 0)
            {
                onAllEnemiesDefeated?.RaiseEvent();
            }
        }
    }
}
```

---

## Usage examples - P1

### Bad: Using FindObjectsOfType

```csharp
// Heavy operation every frame
private void Update()
{
    // FindGameObjectsWithTag is very expensive
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

    foreach (GameObject enemy in enemies)
    {
        ProcessEnemy(enemy);
    }
}
```

### Good: Using RuntimeSet

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO activeEnemies;

        private void ProcessEnemies()
        {
            // Fast iteration over active enemies
            foreach (var enemy in activeEnemies.Items)
            {
                ProcessEnemy(enemy);
            }
        }

        private void ProcessEnemy(EnemyController enemy)
        {
            // Process logic here
        }
    }
}
```

---

## Debugging features - P1

### Runtime Set Monitor Window

Access via `Window → Reactive SO → Runtime Set Monitor` to monitor all RuntimeSets in real-time.

Features:
- View all RuntimeSet assets in project
- Real-time item count during Play Mode
- Filter by name or type
- Click to select asset in Project window

### Inspector debugging

Select any RuntimeSet asset in Inspector during Play Mode to see:
- Current item count
- List of all registered items (clickable)
- Clear button for testing

---

## Benefits - P1

### Performance comparison

```csharp
// Bad: O(n) search every frame
void Update()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
    // Search entire scene hierarchy
}

// Good: O(1) access
void ProcessEnemies()
{
    foreach (var enemy in activeEnemies.Items)
    {
        // Direct access to managed list
    }
}
```

### Inspector visibility

```
Assets/_Project/ScriptableObjects/RuntimeSets/
├── ActiveEnemies.asset (EnemyRuntimeSetSO)
├── ActivePlayers.asset (GameObjectRuntimeSetSO)
└── Waypoints.asset (TransformRuntimeSetSO)
```

- View current item count in Inspector
- See which objects are registered
- Monitor all sets in Runtime Set Monitor Window

### Decoupling

```csharp
// System A: Spawns enemies
public class EnemySpawner : MonoBehaviour
{
    // Enemies automatically register themselves
    // Spawner doesn't need to notify anyone
}

// System B: Tracks enemies
public class QuestManager : MonoBehaviour
{
    [SerializeField] private EnemyRuntimeSetSO activeEnemies;

    // Can access all enemies without knowing who spawned them
    private int GetRemainingEnemies()
    {
        return activeEnemies.Count;
    }
}
```

---

## Practical example - P1

### Complete enemy management system

```csharp
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Custom RuntimeSet for enemy management
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "Reactive SO/Runtime Sets/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
        /// <summary>
        /// Get the closest enemy to a position
        /// </summary>
        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = enemy;
                }
            }

            return closest;
        }

        /// <summary>
        /// Get all enemies within radius
        /// </summary>
        public List<EnemyController> GetEnemiesInRadius(Vector3 center, float radius)
        {
            var result = new List<EnemyController>();
            float radiusSqr = radius * radius;

            foreach (var enemy in Items)
            {
                if (enemy == null) continue;

                if ((enemy.transform.position - center).sqrMagnitude <= radiusSqr)
                {
                    result.Add(enemy);
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Enemy controller with RuntimeSet registration
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        private void OnEnable()
        {
            enemySet?.Add(this);
        }

        private void OnDisable()
        {
            enemySet?.Remove(this);
        }
    }
}
```

---

## RuntimeSet vs ReactiveEntitySet - P1

### When to use RuntimeSet

```
✅ Tracking active GameObjects/Components
✅ Simple add/remove lifecycle
✅ Need to iterate over all items
✅ No per-entity state required
```

### When to use ReactiveEntitySet

```
✅ Per-entity state management
✅ ID-based entity lookup
✅ Scene-persistent state
✅ Complex state updates with events
```

See [ReactiveEntitySet Pattern](reactive-entity-sets.md) for centralized state management.

---

## References

- [Event Channels](event-channels.md)
- [Variables System](variables.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [ScriptableObject Pattern](scriptableobject.md)
- [Dependency Management](dependency-management.md)

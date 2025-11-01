# RuntimeSet Pattern

## Purpose

Manage dynamic collections of game objects using ScriptableObjects, avoiding expensive Find operations and enabling efficient iteration over active objects.

## Checklist

- [ ] Use RuntimeSets instead of FindObjectsOfType or FindGameObjectsWithTag
- [ ] Register objects in OnStartClient/OnEnable
- [ ] Unregister objects in OnStopClient/OnDisable
- [ ] Clear RuntimeSet when scene unloads
- [ ] Expose IReadOnlyList for safe external access
- [ ] Use RuntimeSet with EventChannels for event-driven updates

---

## Dynamic Object Management

### RuntimeSet Implementation

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Enemy
{
    /// <summary>
    /// RuntimeSet for managing active enemies
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : ScriptableObject
    {
        private List<EnemyController> items = new List<EnemyController>();

        /// <summary>
        /// Read-only access to the collection
        /// </summary>
        public IReadOnlyList<EnemyController> Items => items;

        /// <summary>
        /// Add an enemy to the set
        /// </summary>
        public void Add(EnemyController item)
        {
            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Remove an enemy from the set
        /// </summary>
        public void Remove(EnemyController item)
        {
            items.Remove(item);
        }

        /// <summary>
        /// Clear all enemies from the set
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Get the count of active enemies
        /// </summary>
        public int Count => items.Count;
    }
}
```

---

## Object Registration

### Register in OnEnable/OnStartClient

```csharp
using UnityEngine;

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

### Scene Cleanup

```csharp
using UnityEngine;

namespace ProjectName.Core
{
    /// <summary>
    /// Manages scene lifecycle and cleanup
    /// </summary>
    public class SceneManager : MonoBehaviour
    {
        [Header("RuntimeSets to Clear")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;
        [SerializeField] private PlayerRuntimeSetSO playerSet;

        private void OnDestroy()
        {
            // Clear all RuntimeSets when scene unloads
            enemySet?.Clear();
            playerSet?.Clear();
        }
    }
}
```

---

## Usage Examples

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
using Tang3cko.EventChannels;

namespace ProjectName.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO activeEnemies;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onEnemyStateChanged;

        private void OnEnable()
        {
            // Only process when state changes (event-driven)
            onEnemyStateChanged.OnEventRaised += ProcessEnemies;
        }

        private void OnDisable()
        {
            onEnemyStateChanged.OnEventRaised -= ProcessEnemies;
        }

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

### Event-Driven with RuntimeSet

```csharp
namespace ProjectName.UI
{
    public class EnemyCountDisplay : MonoBehaviour
    {
        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO activeEnemies;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onEnemySpawned;
        [SerializeField] private VoidEventChannelSO onEnemyKilled;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI countText;

        private void OnEnable()
        {
            onEnemySpawned.OnEventRaised += UpdateCount;
            onEnemyKilled.OnEventRaised += UpdateCount;

            UpdateCount();
        }

        private void OnDisable()
        {
            onEnemySpawned.OnEventRaised -= UpdateCount;
            onEnemyKilled.OnEventRaised -= UpdateCount;
        }

        private void UpdateCount()
        {
            countText.text = $"Enemies: {activeEnemies.Count}";
        }
    }
}
```

---

## Benefits

### Performance Comparison

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

### Inspector Visibility

```csharp
// You can inspect the RuntimeSet asset to see:
// - How many items are currently in the set
// - Which objects are registered
// - Add/remove items manually for testing
```

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

## Practical Example

### Complete Enemy Management System

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Enemy
{
    /// <summary>
    /// RuntimeSet for enemy management
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : ScriptableObject
    {
        private List<EnemyController> items = new List<EnemyController>();

        public IReadOnlyList<EnemyController> Items => items;
        public int Count => items.Count;

        public void Add(EnemyController item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EnemyRuntimeSet] Attempted to add null enemy");
                return;
            }

            if (!items.Contains(item))
            {
                items.Add(item);
                Debug.Log($"[EnemyRuntimeSet] Added enemy. Total: {items.Count}");
            }
        }

        public void Remove(EnemyController item)
        {
            if (items.Remove(item))
            {
                Debug.Log($"[EnemyRuntimeSet] Removed enemy. Total: {items.Count}");
            }
        }

        public void Clear()
        {
            int count = items.Count;
            items.Clear();
            Debug.Log($"[EnemyRuntimeSet] Cleared {count} enemies");
        }

        public EnemyController GetClosestTo(Vector3 position)
        {
            EnemyController closest = null;
            float closestDistance = float.MaxValue;

            foreach (var enemy in items)
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

## References

- [Event Channels](event-channels.md)
- [ScriptableObject Pattern](scriptableobject.md)
- [Performance Optimization](../core/performance.md)

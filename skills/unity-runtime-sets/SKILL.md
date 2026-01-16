---
name: unity-runtime-sets
description: RuntimeSetSO for tracking active GameObjects. Self-registering collections, no FindObjectsOfType. Use when managing object collections in Unity.
---

# RuntimeSet Pattern

## Purpose

Manage dynamic collections of game objects using ScriptableObjects, avoiding expensive Find operations.

## Checklist

- [ ] Use RuntimeSets instead of FindObjectsOfType
- [ ] Register objects in OnEnable, unregister in OnDisable
- [ ] Clear RuntimeSet when scene unloads
- [ ] Assign EventChannels for reactive updates (optional)

---

## Design philosophy - P1

### Self-registering collections

**Traditional Unity:**
```
GameManager wants all enemies
         ↓
FindObjectsOfType<Enemy>()  ← O(n) scene scan every call
         ↓
Performance problem
```

**RuntimeSet pattern:**
```
Enemy OnEnable() → enemySet.Add(this)
Enemy OnDisable() → enemySet.Remove(this)
         ↓
GameManager → enemySet.Items  ← O(1) direct access
```

### Key insight

**Objects register themselves. The collection doesn't search for them.**

---

## Built-in RuntimeSet types - P1

| RuntimeSet Type | Use Case | Example |
|-----------------|----------|---------|
| `RuntimeSetSO<T>` | Base class for custom types | Custom component tracking |
| `GameObjectRuntimeSetSO` | Track GameObjects | Active enemies, collectibles |
| `TransformRuntimeSetSO` | Track Transforms | Waypoints, spawn points |

---

## Custom RuntimeSet types - P1

```csharp
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
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
}
```

---

## Object registration - P1

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    public class EnemyController : MonoBehaviour
    {
        [Header("RuntimeSet")]
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

## Event notifications - P1

RuntimeSets use EventChannels for notifications. Assign in Inspector:
- **onItemsChanged** (VoidEventChannelSO) - Items added or removed
- **onCountChanged** (IntEventChannelSO) - New count when items change

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class EnemyCountDisplay : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onEnemyCountChanged;

        [Header("RuntimeSets")]
        [SerializeField] private EnemyRuntimeSetSO activeEnemies;

        [Header("UI")]
        [SerializeField] private TMPro.TextMeshProUGUI countText;

        private void OnEnable()
        {
            onEnemyCountChanged.OnEventRaised += HandleCountChanged;
            UpdateCount(activeEnemies.Count);
        }

        private void OnDisable()
        {
            onEnemyCountChanged.OnEventRaised -= HandleCountChanged;
        }

        private void HandleCountChanged(int newCount)
        {
            countText.text = $"Enemies: {newCount}";
        }
    }
}
```

---

## Scene cleanup - P1

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class SceneManager : MonoBehaviour
    {
        [Header("RuntimeSets to Clear")]
        [SerializeField] private EnemyRuntimeSetSO enemySet;

        private void OnDestroy()
        {
            enemySet?.Clear();
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

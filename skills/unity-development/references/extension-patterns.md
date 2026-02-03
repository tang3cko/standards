# Extension patterns

## CreateAssetMenu conventions

| Category | Menu Path |
|----------|-----------|
| Data SO | `"ProjectName/Data/Category/Name"` |
| RuntimeSet | `"ProjectName/RuntimeSet/TypeName"` |
| EventChannel | `"ProjectName/Events/TypeName Event Channel"` |

---

## Adding new EventChannel

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Item
{
    [CreateAssetMenu(fileName = "ItemEventChannel", menuName = "ProjectName/Events/Item Event Channel")]
    public class ItemEventChannelSO : EventChannelSO<ItemSO>
    {
    }
}
```

Usage:

```csharp
// Publisher
[SerializeField] private ItemEventChannelSO onItemAcquired;
public void Pickup() => onItemAcquired?.RaiseEvent(item);

// Subscriber
private void OnEnable() => onItemAcquired.OnEventRaised += HandleItemAcquired;
private void OnDisable() => onItemAcquired.OnEventRaised -= HandleItemAcquired;
```

---

## Adding new RuntimeSet

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
    public class EnemyRuntimeSetSO : RuntimeSetSO<EnemyController>
    {
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

Registration:

```csharp
[SerializeField] private EnemyRuntimeSetSO enemySet;
private void OnEnable() => enemySet?.Add(this);
private void OnDisable() => enemySet?.Remove(this);
```

# RuntimeSet details

## Design philosophy

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

**Key insight:** Objects register themselves. The collection doesn't search for them.

## Custom RuntimeSet

```csharp
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    [CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "Reactive SO/Runtime Sets/Enemy")]
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

## Event notifications

RuntimeSets use EventChannels for notifications. Assign in Inspector:
- **onItemsChanged** (VoidEventChannelSO) - Items added or removed
- **onCountChanged** (IntEventChannelSO) - New count when items change

```csharp
public class EnemyCountDisplay : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onEnemyCountChanged;
    [SerializeField] private EnemyRuntimeSetSO activeEnemies;
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
```

## Scene cleanup

```csharp
private void OnDestroy()
{
    enemySet?.Clear();
}
```

## RuntimeSet vs ReactiveEntitySet

| Use RuntimeSet when | Use ReactiveEntitySet when |
|---------------------|---------------------------|
| Tracking active GameObjects/Components | Per-entity state management |
| Simple add/remove lifecycle | ID-based entity lookup |
| Need to iterate over all items | Scene-persistent state |
| No per-entity state required | Complex state updates with events |

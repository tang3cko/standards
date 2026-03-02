# EventChannel Details

EventChannel pub/sub pattern for decoupled communication.

---

## Built-in EventChannel Types

| Type | Use Case | Example |
|------|----------|---------|
| `VoidEventChannelSO` | No parameters | OnGameStart, OnEnemyKilled |
| `IntEventChannelSO` | Integer values, enums | OnScoreChanged, OnLevelUp |
| `FloatEventChannelSO` | Decimal values, ratios | OnHealthChanged |
| `BoolEventChannelSO` | Boolean flags | OnToggleChanged, OnGamePaused |
| `StringEventChannelSO` | Text data | OnMessageReceived |
| `Vector2EventChannelSO` | 2D positions, input | OnMouseMoved |
| `Vector3EventChannelSO` | 3D positions | OnPlayerMoved |
| `GameObjectEventChannelSO` | Object references | OnTargetSelected |

---

## Usage Pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onEnemyKilled;
        [SerializeField] private IntEventChannelSO onEnemyDamaged;

        private void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            onEnemyDamaged?.RaiseEvent(damage);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            onEnemyKilled?.RaiseEvent();
            Destroy(gameObject);
        }
    }
}
```

---

## Custom EventChannel Types

Create custom EventChannels when passing complex data structures.

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Quest
{
    [CreateAssetMenu(fileName = "QuestEventChannel", menuName = "Reactive SO/Channels/Quest Event Channel")]
    public class QuestEventChannelSO : EventChannelSO<QuestSO>
    {
    }
}
```

### Creating a new custom EventChannel

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

### CreateAssetMenu conventions

| Category | Menu Path |
|----------|-----------|
| Data SO | `"ProjectName/Data/Category/Name"` |
| RuntimeSet | `"ProjectName/RuntimeSet/TypeName"` |
| EventChannel | `"ProjectName/Events/TypeName Event Channel"` |

---

## Debugging

Access Event Monitor via `Window -> Reactive SO -> Event Monitor`:
- Real-time event logging
- Filter by event channel name
- View event values and timestamps
- Caller tracking

---

## References

- [dependency-management.md](dependency-management.md) - EventChannel as priority 1 pattern
- [variables.md](variables.md) - Variables vs EventChannels comparison
- [architecture.md](architecture.md) - Architecture decision tree

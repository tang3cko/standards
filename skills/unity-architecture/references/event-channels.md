# EventChannel details

## Custom EventChannel types

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

## Complete example

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

## Debugging

Access Event Monitor via `Window → Reactive SO → Event Monitor`:
- Real-time event logging
- Filter by event channel name
- View event values and timestamps
- Caller tracking

---
name: unity-event-channels
description: EventChannelSO for decoupled pub/sub communication. VoidEventChannelSO, IntEventChannelSO. Use when implementing events in Unity.
---

# Event-Driven Communication with EventChannels

## Purpose

Implement event-driven architecture using Tang3cko.ReactiveSO for decoupled communication between game systems.

## Checklist

- [ ] Use EventChannels for decoupled communication
- [ ] Subscribe in OnEnable, unsubscribe in OnDisable
- [ ] Use null conditional operator when raising events
- [ ] Follow naming conventions (on + PastTense)

---

## Design philosophy - P1

### Observer pattern with ScriptableObjects

**Traditional Unity:**
```
Publisher → Direct reference → Subscriber
             (tight coupling)
```

**EventChannel pattern:**
```
Publisher → ScriptableObject Asset → Subscriber(s)
             (complete decoupling)
```

### Key insight

**The publisher doesn't know who receives the event, and subscribers don't know who sent it.**

---

## Built-in EventChannel types - P1

| EventChannel Type | Use Case | Example |
|-------------------|----------|---------|
| `VoidEventChannelSO` | No parameters | OnGameStart, OnEnemyKilled |
| `IntEventChannelSO` | Integer values, enums | OnScoreChanged, OnLevelUp |
| `FloatEventChannelSO` | Decimal values, ratios | OnHealthChanged |
| `BoolEventChannelSO` | Boolean flags | OnToggleChanged, OnGamePaused |
| `StringEventChannelSO` | Text data | OnMessageReceived |
| `Vector2EventChannelSO` | 2D positions, input | OnMouseMoved |
| `Vector3EventChannelSO` | 3D positions | OnPlayerMoved |
| `GameObjectEventChannelSO` | Object references | OnTargetSelected |

---

## Basic implementation - P1

### Event publisher

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Quest
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        public void SelectQuest(QuestSO quest)
        {
            // Raise event - sender doesn't know who receives it
            onQuestSelected?.RaiseEvent(quest);
        }
    }
}
```

### Event subscriber

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class QuestUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        // Subscribe in OnEnable
        private void OnEnable()
        {
            onQuestSelected.OnEventRaised += HandleQuestSelected;
        }

        // Unsubscribe in OnDisable
        private void OnDisable()
        {
            onQuestSelected.OnEventRaised -= HandleQuestSelected;
        }

        private void HandleQuestSelected(QuestSO quest)
        {
            UpdateQuestDisplay(quest);
        }
    }
}
```

---

## Custom EventChannel types - P2

### When to create custom types

Create custom EventChannels only when passing complex data structures.

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Quest
{
    /// <summary>
    /// Custom EventChannel for Quest-related events
    /// </summary>
    [CreateAssetMenu(fileName = "QuestEventChannel", menuName = "Reactive SO/Channels/Quest Event Channel")]
    public class QuestEventChannelSO : EventChannelSO<QuestSO>
    {
    }
}
```

---

## Debugging features - P1

### Event Monitor Window

Access via `Window → Reactive SO → Event Monitor` to monitor events in real-time.

Features:
- Real-time event logging
- Filter by event channel name
- View event values and timestamps
- Caller tracking

---

## Complete example - P1

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

# Event-Driven Communication with EventChannels

## Purpose

Implement event-driven architecture using Tang3cko.EventChannels for decoupled communication between game systems.

## Checklist

- [ ] Use EventChannels for decoupled communication
- [ ] Subscribe in OnEnable, unsubscribe in OnDisable
- [ ] Use null conditional operator when raising events
- [ ] Create EventChannel ScriptableObject assets
- [ ] Follow EventChannel naming conventions (on + PastTense)
- [ ] Choose appropriate EventChannel type for data being passed

---

## EventChannel Pattern

### Basic Implementation

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    /// <summary>
    /// EventChannel for Quest-related events
    /// </summary>
    [CreateAssetMenu(fileName = "QuestEventChannel", menuName = "ProjectName/Events/Quest Event Channel")]
    public class QuestEventChannelSO : EventChannelSO<QuestSO>
    {
        // Tang3cko.EventChannels base class provides all functionality
    }
}
```

### Event Publisher

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    public class QuestManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;
        [SerializeField] private QuestEventChannelSO onQuestCompleted;

        public void SelectQuest(QuestSO quest)
        {
            // Raise event - sender doesn't know who receives it
            onQuestSelected?.RaiseEvent(quest);
        }

        public void CompleteQuest(QuestSO quest)
        {
            onQuestCompleted?.RaiseEvent(quest);
        }
    }
}
```

### Event Subscriber

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    public class QuestUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;
        [SerializeField] private QuestEventChannelSO onQuestCompleted;

        // Subscribe in OnEnable
        private void OnEnable()
        {
            onQuestSelected.OnEventRaised += HandleQuestSelected;
            onQuestCompleted.OnEventRaised += HandleQuestCompleted;
        }

        // Unsubscribe in OnDisable
        private void OnDisable()
        {
            onQuestSelected.OnEventRaised -= HandleQuestSelected;
            onQuestCompleted.OnEventRaised -= HandleQuestCompleted;
        }

        private void HandleQuestSelected(QuestSO quest)
        {
            // Update UI with quest info
            UpdateQuestDisplay(quest);
        }

        private void HandleQuestCompleted(QuestSO quest)
        {
            // Show completion message
            ShowCompletionMessage(quest);
        }
    }
}
```

---

## EventChannel Types

### Available Types

| EventChannel Type | Use Case | Example |
|-------------------|----------|---------|
| `VoidEventChannelSO` | No parameters | OnEnemyKilled, OnSettingsLoaded |
| `IntEventChannelSO` | Integer values, enums | OnCoinEarned(amount), OnScoreChanged |
| `FloatEventChannelSO` | Decimal values, ratios | OnHealthChanged(0-1), OnSpeedModified |
| `BoolEventChannelSO` | Boolean flags | OnToggleChanged, OnGamePaused |
| `StringEventChannelSO` | Text data | OnMessageReceived, OnPlayerNameChanged |
| Custom EventChannel | Complex data | OnEnemyDeath(EnemyData, Position) |

### VoidEventChannel Example

```csharp
// Definition
[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "ProjectName/Events/Void Event Channel")]
public class VoidEventChannelSO : EventChannelSO { }

// Publisher
[SerializeField] private VoidEventChannelSO onGameStart;

private void StartGame()
{
    onGameStart?.RaiseEvent();
}

// Subscriber
[SerializeField] private VoidEventChannelSO onGameStart;

private void OnEnable()
{
    onGameStart.OnEventRaised += HandleGameStart;
}

private void HandleGameStart()
{
    Debug.Log("Game started");
}
```

### IntEventChannel Example

```csharp
// Publisher
[SerializeField] private IntEventChannelSO onCoinEarned;

private void EarnCoins(int amount)
{
    onCoinEarned?.RaiseEvent(amount);
}

// Subscriber
[SerializeField] private IntEventChannelSO onCoinEarned;

private void OnEnable()
{
    onCoinEarned.OnEventRaised += HandleCoinEarned;
}

private void HandleCoinEarned(int amount)
{
    totalCoins += amount;
    UpdateCoinDisplay();
}
```

### Enum Events (Use IntEventChannel)

```csharp
// Definition
public enum GameEndReason
{
    PlayerDeath = 0,
    BossDefeated = 1,
    TimeExpired = 2
}

// Publisher
[SerializeField] private IntEventChannelSO onGameEnd;

private void EndGame(GameEndReason reason)
{
    onGameEnd?.RaiseEvent((int)reason);
}

// Subscriber
[SerializeField] private IntEventChannelSO onGameEnd;

private void OnEnable()
{
    onGameEnd.OnEventRaised += HandleGameEnd;
}

private void HandleGameEnd(int reasonInt)
{
    GameEndReason reason = (GameEndReason)reasonInt;

    switch (reason)
    {
        case GameEndReason.PlayerDeath:
            ShowDefeatScreen();
            break;
        case GameEndReason.BossDefeated:
            ShowVictoryScreen();
            break;
        case GameEndReason.TimeExpired:
            ShowTimeOutScreen();
            break;
    }
}
```

---

## Benefits

### Complete Decoupling

```csharp
// EnemyHealth doesn't know who cares about enemy deaths
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void Die()
    {
        onEnemyKilled?.RaiseEvent();  // Fire and forget
        Destroy(gameObject);
    }
}

// Multiple subscribers can listen independently
// GameStatsManager: tracks kill count
// QuestManager: checks quest objectives
// AudioManager: plays death sound
// ParticleManager: spawns death effect
```

### Multiple Subscribers

```csharp
// Subscriber 1: Stats tracking
public class GameStatsManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void OnEnable()
    {
        onEnemyKilled.OnEventRaised += IncrementKillCount;
    }

    private void IncrementKillCount()
    {
        totalKills++;
    }
}

// Subscriber 2: Quest progress
public class QuestManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void OnEnable()
    {
        onEnemyKilled.OnEventRaised += CheckQuestProgress;
    }

    private void CheckQuestProgress()
    {
        currentProgress++;
        if (currentProgress >= requiredKills)
        {
            CompleteQuest();
        }
    }
}

// Subscriber 3: Audio feedback
public class AudioManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void OnEnable()
    {
        onEnemyKilled.OnEventRaised += PlayDeathSound;
    }

    private void PlayDeathSound()
    {
        audioSource.PlayOneShot(enemyDeathClip);
    }
}
```

### Inspector Visibility

EventChannels are ScriptableObject assets, making event flow visible:

```
Assets/_Project/ScriptableObjects/Events/Enemy/
├── OnEnemyKilled.asset (VoidEventChannelSO)
├── OnEnemySpawned.asset (VoidEventChannelSO)
└── OnEnemyDamaged.asset (IntEventChannelSO)
```

You can:
- See all event channels in Project window
- Find all objects using an event channel (Select asset → Find References In Scene)
- Debug event flow by selecting the asset

---

## Practical Example

### Complete Enemy Death System

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Enemy
{
    /// <summary>
    /// Enemy health management
    /// </summary>
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

## References

- [ScriptableObject Pattern](scriptableobject.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [Dependency Management](dependency-management.md)

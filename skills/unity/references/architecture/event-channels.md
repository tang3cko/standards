# Event-Driven Communication with EventChannels

## Purpose

Implement event-driven architecture using Tang3cko.ReactiveSO for decoupled communication between game systems.

---

## Design philosophy - P1

### Observer pattern with ScriptableObjects

EventChannels implement the Observer pattern using ScriptableObject assets as the communication medium.

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

This enables:
- **N:N communication** - Multiple publishers, multiple subscribers
- **Scene independence** - Communication across scenes
- **Hot swappable** - Change wiring in Inspector without code
- **Testable** - Easy to mock events in tests

### Stateless by design

EventChannels are **fire-and-forget**. They don't store values:

```csharp
// EventChannel: Notification only
onEnemyKilled.RaiseEvent();  // No value stored

// If you need to store the value, use Variable instead
// Variable: State + notification
playerScore.Value = 100;  // Stores value AND notifies
```

---

## Checklist

- [ ] Use EventChannels for decoupled communication
- [ ] Subscribe in OnEnable, unsubscribe in OnDisable
- [ ] Use null conditional operator when raising events
- [ ] Create EventChannel ScriptableObject assets via `Reactive SO/Channels/...`
- [ ] Follow EventChannel naming conventions (on + PastTense)
- [ ] Choose appropriate built-in EventChannel type for data being passed
- [ ] Use Event Monitor Window for debugging (`Window/Reactive SO/Event Monitor`)

---

## EventChannel pattern - P1

### Basic implementation

Use built-in EventChannel types provided by the package. Custom EventChannels are only needed for complex data types.

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Quest
{
    /// <summary>
    /// Custom EventChannel for Quest-related events
    /// Only needed when built-in types are insufficient
    /// </summary>
    [CreateAssetMenu(fileName = "QuestEventChannel", menuName = "Reactive SO/Channels/Quest Event Channel")]
    public class QuestEventChannelSO : EventChannelSO<QuestSO>
    {
        // Tang3cko.ReactiveSO base class provides all functionality
    }
}
```

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

## Built-in EventChannel types - P1

### Available types (12 built-in)

All types are provided by the package. Create assets via `Create → Reactive SO → Channels → [Type]`.

| EventChannel Type | Use Case | Example |
|-------------------|----------|---------|
| `VoidEventChannelSO` | No parameters | OnGameStart, OnEnemyKilled |
| `IntEventChannelSO` | Integer values, enums | OnScoreChanged, OnLevelUp |
| `LongEventChannelSO` | Large integer values | OnExperienceGained, OnCurrencyChanged |
| `FloatEventChannelSO` | Decimal values, ratios | OnHealthChanged(0-1), OnSpeedModified |
| `DoubleEventChannelSO` | High-precision decimals | OnTimerUpdated, OnDistanceCalculated |
| `BoolEventChannelSO` | Boolean flags | OnToggleChanged, OnGamePaused |
| `StringEventChannelSO` | Text data | OnMessageReceived, OnPlayerNameChanged |
| `Vector2EventChannelSO` | 2D positions, input | OnMouseMoved, OnJoystickInput |
| `Vector3EventChannelSO` | 3D positions, directions | OnPlayerMoved, OnTargetChanged |
| `QuaternionEventChannelSO` | Rotations | OnCameraRotated, OnObjectRotated |
| `ColorEventChannelSO` | Color values | OnColorChanged, OnThemeUpdated |
| `GameObjectEventChannelSO` | Object references | OnTargetSelected, OnObjectSpawned |

### VoidEventChannel example

```csharp
// Publisher - use built-in VoidEventChannelSO
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

private void OnDisable()
{
    onGameStart.OnEventRaised -= HandleGameStart;
}

private void HandleGameStart()
{
    Debug.Log("Game started");
}
```

### IntEventChannel example

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

private void OnDisable()
{
    onCoinEarned.OnEventRaised -= HandleCoinEarned;
}

private void HandleCoinEarned(int amount)
{
    totalCoins += amount;
    UpdateCoinDisplay();
}
```

### Vector3EventChannel example

```csharp
// Publisher
[SerializeField] private Vector3EventChannelSO onPlayerMoved;

private void Update()
{
    if (transform.hasChanged)
    {
        onPlayerMoved?.RaiseEvent(transform.position);
        transform.hasChanged = false;
    }
}

// Subscriber
[SerializeField] private Vector3EventChannelSO onPlayerMoved;

private void OnEnable()
{
    onPlayerMoved.OnEventRaised += HandlePlayerMoved;
}

private void OnDisable()
{
    onPlayerMoved.OnEventRaised -= HandlePlayerMoved;
}

private void HandlePlayerMoved(Vector3 position)
{
    // Update minimap, AI awareness, etc.
    UpdateMinimapMarker(position);
}
```

### Enum events (use IntEventChannel)

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

private void OnDisable()
{
    onGameEnd.OnEventRaised -= HandleGameEnd;
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

## Custom EventChannel types - P2

### When to create custom types

Create custom EventChannels only when:
- Passing complex data structures
- Passing multiple values together
- Type safety is critical for the specific use case

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Combat
{
    /// <summary>
    /// Custom data for damage events
    /// </summary>
    [System.Serializable]
    public struct DamageEventData
    {
        public GameObject target;
        public int damage;
        public Vector3 hitPoint;
        public DamageType damageType;
    }

    public enum DamageType { Physical, Fire, Ice, Lightning }

    /// <summary>
    /// Custom EventChannel for complex damage events
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEventChannel", menuName = "Reactive SO/Channels/Damage Event Channel")]
    public class DamageEventChannelSO : EventChannelSO<DamageEventData>
    {
    }
}
```

---

## Debugging features - P1

### Event Monitor Window

Access via `Window → Reactive SO → Event Monitor` to monitor events in real-time during Play Mode.

Features:
- Real-time event logging
- Filter by event channel name
- View event values and timestamps
- See listener counts
- **Caller tracking** - shows which script raised the event
- Export to CSV for analysis
- Pause/resume logging

### Inspector debugging

Select any EventChannel asset in Inspector during Play Mode to see:
- Current subscriber list with clickable GameObject references
- Manual trigger button to test events
- Subscriber count

### Caller information

EventChannels automatically track caller information for debugging:

```csharp
// CallerInfo includes:
// - File path where RaiseEvent was called
// - Line number
// - Method name
// Visible in Event Monitor Window
```

### OnAnyEventRaised static event

Static event for global event monitoring (Editor only):

```csharp
#if UNITY_EDITOR
private void OnEnable()
{
    EventChannelSO.OnAnyEventRaised += HandleAnyEventRaised;
}

private void OnDisable()
{
    EventChannelSO.OnAnyEventRaised -= HandleAnyEventRaised;
}

private void HandleAnyEventRaised(EventChannelSO channel, object value, CallerInfo callerInfo)
{
    Debug.Log($"Event raised: {channel.name} with value {value} from {callerInfo}");
}
#endif
```

Use cases:
- Custom debugging tools
- Analytics integration
- Event logging systems

---

## Benefits - P1

### Complete decoupling

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

### Multiple subscribers

```csharp
// Subscriber 1: Stats tracking
public class GameStatsManager : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void OnEnable()
    {
        onEnemyKilled.OnEventRaised += IncrementKillCount;
    }

    private void OnDisable()
    {
        onEnemyKilled.OnEventRaised -= IncrementKillCount;
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

    private void OnDisable()
    {
        onEnemyKilled.OnEventRaised -= CheckQuestProgress;
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

    private void OnDisable()
    {
        onEnemyKilled.OnEventRaised -= PlayDeathSound;
    }

    private void PlayDeathSound()
    {
        audioSource.PlayOneShot(enemyDeathClip);
    }
}
```

### Inspector visibility

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
- Monitor events in Event Monitor Window

---

## Practical example - P1

### Complete enemy death system

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

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

- [Variables System](variables.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [Design Principles](design-principles.md)
- [Dependency Management](dependency-management.md)

# Dependency Management

## Purpose

Establish clear priorities for dependency injection to maximize visibility, testability, and maintainability in team development.

## Checklist

- [ ] Prefer EventChannels for decoupled communication (Priority 1)
- [ ] Use SerializeField for explicit dependencies (Priority 2)
- [ ] Use FindFirstObjectByType only as SerializeField fallback (Priority 3)
- [ ] Use Singleton only for truly global state (Priority 4 - last resort)
- [ ] Validate Singleton usage against all four criteria
- [ ] Subscribe to events in OnEnable, unsubscribe in OnDisable

---

## Dependency Priority

### Priority Order

```
1. EventChannel (Highest Priority) - Complete decoupling
2. SerializeField - Explicit dependencies
3. FindFirstObjectByType - SerializeField fallback only
4. Singleton - Truly global state only (Last Resort)
```

### Why This Order?

Team development considerations:

1. Visibility - Dependencies are clear to everyone
2. Change Safety - Impact scope is clear, refactoring is safe
3. Unity Editor Integration - Designers can work with it, warnings appear
4. Parallel Development - Less file conflicts
5. Testability - Quality assurance possible

---

## EventChannel Pattern (Priority 1)

### Complete Decoupling

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Enemy
{
    // Publisher doesn't know who receives
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO onEnemyKilled;

        private void Die()
        {
            onEnemyKilled?.RaiseEvent();  // Fire and forget
        }
    }
}

namespace ProjectName.Core
{
    // Subscriber doesn't know who publishes
    public class GameStatsManager : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO onEnemyKilled;

        private void OnEnable()
        {
            onEnemyKilled.OnEventRaised += OnEnemyKilled;
        }

        private void OnDisable()
        {
            onEnemyKilled.OnEventRaised -= OnEnemyKilled;
        }

        private void OnEnemyKilled()
        {
            currentKillCount++;
        }
    }
}
```

### Benefits

- Complete decoupling (sender and receiver don't know each other)
- Multiple receivers can subscribe
- Event flow visible in Inspector via ScriptableObject assets
- Easy to add new listeners without modifying existing code

---

## SerializeField Pattern (Priority 2)

### Explicit Dependencies

```csharp
using UnityEngine;

namespace ProjectName.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Manager References")]
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private GameStatsManager gameStatsManager;

        private void Start()
        {
            // Fallback: Auto-find only if not assigned in Inspector
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();

                if (playerHealth == null)
                {
                    Debug.LogWarning("[UIManager] PlayerHealth not found. Health display will not work properly.", this);
                }
            }

            if (gameStatsManager == null)
            {
                gameStatsManager = FindFirstObjectByType<GameStatsManager>();

                if (gameStatsManager == null)
                {
                    Debug.LogWarning("[UIManager] GameStatsManager not found. Timer display will not work properly.", this);
                }
            }
        }

        private void UpdateTimer()
        {
            if (timerText != null && gameStatsManager != null)
            {
                float gameTime = gameStatsManager.CurrentGameTime;
                // Display logic
            }
        }
    }
}
```

### Benefits

- Dependencies visible in Inspector
- Impact scope clear when refactoring
- IDE refactoring tools work
- Designers can assign references
- Yellow warning in Inspector if unassigned

---

## Singleton Pattern (Priority 4 - Last Resort)

### When to Use Singleton

Use ONLY when ALL four conditions are met:

1. Technically must be single instance
2. Same lifecycle as entire application
3. Truly global state (game-wide state management)
4. Referenced from 10+ locations (dependency injection would be too verbose)

### Project Singleton Usage

| Class | Use Singleton? | Reason |
|-------|----------------|--------|
| `GameManager` | ✅ | Game-wide state management, referenced everywhere |
| `EnemyPoolManager` | ✅ | All spawners reference it, technically requires 1 instance |
| `GameStatsManager` | ❌ | Only 2-3 references → Use SerializeField |
| `AudioManager` | ❌ | Access via EventChannel |
| `ShopManager` | ❌ | Contained within UI → Use SerializeField |
| `PlayerWallet` | ❌ | EventChannel subscription only |

### Singleton Implementation

```csharp
using UnityEngine;

namespace ProjectName.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton initialization
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);  // Persist across scenes
            }
            else if (Instance != this)
            {
                Debug.LogWarning($"[GameManager] Duplicate GameManager found on {gameObject.name}. Destroying.");
                Destroy(gameObject);
                return;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
```

---

## Comparison: Singleton vs SerializeField

### Visibility

**Singleton:**
```csharp
// Dependencies not visible in code or Inspector
public class UIManager : MonoBehaviour
{
    // Inspector shows nothing about dependencies
}
```

**SerializeField:**
```csharp
// Dependencies immediately visible in Inspector
[SerializeField] private GameStatsManager gameStatsManager;
```

### Impact Analysis

**Singleton:**
```
Change GameStatsManager method name
↓
Where is it used? → grep entire project
↓
Miss a reference? → Runtime error
```

**SerializeField:**
```
Find references from Scene/Prefab search
↓
Impact scope is clear
↓
IDE refactoring tools work
```

### Parallel Development

**Singleton:**
- Programmer A: Add feature to GameStatsManager
- Programmer B: Add different feature to GameStatsManager
- → Same file edited = Git conflicts

**SerializeField:**
- Programmer A: UIManager references GameStatsManager
- Programmer B: ResultPanel references GameStatsManager
- → Different files = No conflicts

### Unity Editor Integration

**Singleton:**
```
Designer: "Timer isn't working"
↓
Programmer: "Check if GameStatsManager is in the scene"
↓
Designer: "How do I check that?"
```

**SerializeField:**
```
Inspector: "Game Stats Manager: None (GameStatsManager)"
↓
Yellow warning icon
↓
Designer can notice the issue
```

### Testing

**Singleton:**
- Can only test in Play mode
- Must destroy Instance after each test
- Cannot use mocks/stubs
- No CI/CD automated tests

**SerializeField:**
- Test in Edit mode or Play mode
- Can inject mock objects
- Unit tests possible
- Automated test suites possible

---

## Practical Example

### Refactoring from Singleton to SerializeField

**Before (Singleton):**
```csharp
public class UIManager : MonoBehaviour
{
    private void Update()
    {
        // Hidden dependency - not visible anywhere
        float time = GameStatsManager.Instance.CurrentGameTime;
        timerText.text = FormatTime(time);
    }
}
```

**After (SerializeField + EventChannel):**
```csharp
using Tang3cko.EventChannels;

public class UIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameStatsManager gameStatsManager;

    [Header("Event Channels")]
    [SerializeField] private FloatEventChannelSO onGameTimeChanged;

    private void OnEnable()
    {
        // Event-driven update (best)
        onGameTimeChanged.OnEventRaised += UpdateTimer;
    }

    private void OnDisable()
    {
        onGameTimeChanged.OnEventRaised -= UpdateTimer;
    }

    private void UpdateTimer(float time)
    {
        timerText.text = FormatTime(time);
    }
}
```

---

## References

- [Event Channels](event-channels.md)
- [ScriptableObject Pattern](scriptableobject.md)
- [RuntimeSet Pattern](runtime-sets.md)

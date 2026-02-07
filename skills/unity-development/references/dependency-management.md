# Dependency Management

Dependency injection priority order and patterns.
For quick reference, see [_core-rules.md](_core-rules.md).

---

## Priority Order - P1

```
1. EventChannel (Highest) <- Complete decoupling
2. SerializeField         <- Explicit dependencies
3. FindFirstObjectByType  <- SerializeField fallback only
4. Singleton (Last Resort)<- Truly global state only
```

### Why this order?

1. **Visibility** - Dependencies are clear to everyone
2. **Change Safety** - Impact scope is clear, refactoring is safe
3. **Unity Editor Integration** - Designers can work with it
4. **Parallel Development** - Less file conflicts
5. **Testability** - Quality assurance possible

---

## EventChannel Pattern - P1

```csharp
// Publisher doesn't know who receives
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private VoidEventChannelSO onEnemyKilled;

    private void Die()
    {
        onEnemyKilled?.RaiseEvent();
    }
}

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
}
```

---

## SerializeField Pattern - P2

```csharp
public class UIManager : MonoBehaviour
{
    [Header("Manager References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameStatsManager gameStatsManager;

    private void Start()
    {
        // Fallback: Auto-find only if not assigned in Inspector
        if (gameStatsManager == null)
        {
            gameStatsManager = FindFirstObjectByType<GameStatsManager>();

            if (gameStatsManager == null)
            {
                Debug.LogWarning("[UIManager] GameStatsManager not found.", this);
            }
        }
    }
}
```

---

## Singleton Pattern - P2

### When to use Singleton

Use ONLY when ALL four conditions are met:

1. Technically must be single instance
2. Same lifecycle as entire application
3. Truly global state (game-wide state management)
4. Referenced from 10+ locations

| Class | Use Singleton? | Reason |
|-------|----------------|--------|
| `GameManager` | Yes | Game-wide state, referenced everywhere |
| `EnemyPoolManager` | Yes | All spawners reference it |
| `GameStatsManager` | No | Only 2-3 references -> Use SerializeField |
| `AudioManager` | No | Access via EventChannel |

### Implementation

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
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
```

---

## Refactoring Example - P2

### Before (Singleton)

```csharp
// Bad: Hidden dependency
public class UIManager : MonoBehaviour
{
    private void Update()
    {
        float time = GameStatsManager.Instance.CurrentGameTime;
        timerText.text = FormatTime(time);
    }
}
```

### After (SerializeField + EventChannel)

```csharp
// Good: Explicit, event-driven
public class UIManager : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private FloatEventChannelSO onGameTimeChanged;

    private void OnEnable()
    {
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

- [_core-rules.md](_core-rules.md) - Dependency priority quick reference
- [architecture.md](architecture.md) - Architecture decision tree
- [event-channels.md](event-channels.md) - EventChannel details
- [design-principles.md](design-principles.md) - Asset-based DI principles

---
name: unity-dependency-management
description: Dependency injection priority. EventChannel > SerializeField > FindFirstObjectByType > Singleton. Use when managing dependencies in Unity.
---

# Dependency Management

## Purpose

Establish clear priorities for dependency injection to maximize visibility, testability, and maintainability.

## Checklist

- [ ] Prefer EventChannels for decoupled communication (Priority 1)
- [ ] Use SerializeField for explicit dependencies (Priority 2)
- [ ] Use FindFirstObjectByType only as SerializeField fallback (Priority 3)
- [ ] Use Singleton only for truly global state (Priority 4 - last resort)

---

## Dependency priority - P1

```
1. EventChannel (Highest Priority) - Complete decoupling
2. SerializeField - Explicit dependencies
3. FindFirstObjectByType - SerializeField fallback only
4. Singleton - Truly global state only (Last Resort)
```

### Why this order?

1. **Visibility** - Dependencies are clear to everyone
2. **Change Safety** - Impact scope is clear, refactoring is safe
3. **Unity Editor Integration** - Designers can work with it
4. **Parallel Development** - Less file conflicts
5. **Testability** - Quality assurance possible

---

## EventChannel pattern (Priority 1) - P1

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

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

---

## SerializeField pattern (Priority 2) - P1

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

## Singleton pattern (Priority 4 - Last Resort) - P1

### When to use Singleton

Use ONLY when ALL four conditions are met:

1. Technically must be single instance
2. Same lifecycle as entire application
3. Truly global state (game-wide state management)
4. Referenced from 10+ locations

### Project Singleton usage

| Class | Use Singleton? | Reason |
|-------|----------------|--------|
| `GameManager` | ✅ | Game-wide state, referenced everywhere |
| `EnemyPoolManager` | ✅ | All spawners reference it |
| `GameStatsManager` | ❌ | Only 2-3 references → Use SerializeField |
| `AudioManager` | ❌ | Access via EventChannel |

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

## Comparison: Singleton vs SerializeField - P1

### Visibility

**Singleton:**
```csharp
// Dependencies not visible in code or Inspector
public class UIManager : MonoBehaviour
{
    // Inspector shows nothing
}
```

**SerializeField:**
```csharp
// Dependencies immediately visible
[SerializeField] private GameStatsManager gameStatsManager;
```

### Testing

**Singleton:**
- Can only test in Play mode
- Cannot use mocks/stubs
- No CI/CD automated tests

**SerializeField:**
- Test in Edit mode or Play mode
- Can inject mock objects
- Automated test suites possible

---

## Practical example - P1

### Refactoring from Singleton to SerializeField

**Before (Singleton):**
```csharp
public class UIManager : MonoBehaviour
{
    private void Update()
    {
        // Hidden dependency
        float time = GameStatsManager.Instance.CurrentGameTime;
        timerText.text = FormatTime(time);
    }
}
```

**After (SerializeField + EventChannel):**
```csharp
public class UIManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameStatsManager gameStatsManager;

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

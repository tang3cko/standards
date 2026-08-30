# Dependency Management

Dependency injection priority order and patterns.
For quick reference, see the `unity-core` skill's Core Rules (in its SKILL.md).

---

## Priority Order

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

## EventChannel Pattern

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

## SerializeField Pattern

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

## Singleton Pattern

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

## Core/Shell Boundary (pure C# core vs ReactiveSO)

A pure C# gameplay core — engine-agnostic logic such as code ported between engines or shared with CLI simulation tools — must NOT depend on the ScriptableObject framework (Tang3cko.ReactiveSO: `EventChannelSO`, `VariableSO`, `ActionSO`, etc.).

**Why:** SO channels decouple game systems from each other, but they re-couple everything that touches them to the ReactiveSO framework and to `UnityEngine.Object`. Letting that leak into the core destroys engine-independence and the ability to test the core without Unity. The EventChannel rung at the top of the Priority Order applies to *shell-layer* wiring between Unity systems, not to the core's outward API.

The core exposes plain C# events; the MonoBehaviour shell subscribes and relays into EventChannelSO:

```csharp
// Core (no UnityEngine, no ReactiveSO)
public class BoardSimulation
{
    public event Action<int> ScoreChanged;

    public void ApplyMove(Move move)
    {
        // ... pure logic ...
        ScoreChanged?.Invoke(score);
    }
}

// Shell (MonoBehaviour layer) — relays core events into SO channels
public class BoardPresenter : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onScoreChanged;
    private BoardSimulation simulation;

    private void OnEnable() => simulation.ScoreChanged += RelayScore;
    private void OnDisable() => simulation.ScoreChanged -= RelayScore;

    private void RelayScore(int score) => onScoreChanged?.RaiseEvent(score);
}
```

**Enforce structurally with asmdefs** — the core asmdef must not reference the ReactiveSO asmdef (and should carry `noEngineReferences` where feasible), so a violation is a compile error, not a review finding:

```json
// Game.Core.asmdef — engine-agnostic core
{
    "name": "Game.Core",
    "references": [],
    "noEngineReferences": true
}

// Game.Shell.asmdef — Unity-side wiring
{
    "name": "Game.Shell",
    "references": ["Game.Core", "Tang3cko.ReactiveSO"]
}
```

---

## Refactoring Example

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

- [architecture.md](architecture.md) - Architecture decision tree
- [event-channels.md](event-channels.md) - EventChannel details
- [design-principles.md](design-principles.md) - Asset-based DI principles

---

## Checklist

- [ ] Every dependency sits on the highest viable rung (EventChannel > SerializeField > Find > Singleton), with a stated reason for each step down
- [ ] `FindFirstObjectByType` appears only as a null-fallback after an unassigned SerializeField, and logs a warning when it also fails
- [ ] Any Singleton satisfies all four acceptance conditions and clears `Instance` in `OnDestroy`
- [ ] Pure C# core code has zero references to ReactiveSO types; the core asmdef does not reference the ReactiveSO asmdef
- [ ] The shell relays core `event Action` events into EventChannelSO with paired `OnEnable`/`OnDisable` subscribe/unsubscribe

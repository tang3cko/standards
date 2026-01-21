---
name: unity-architecture
description: Unity architecture design patterns. ScriptableObject patterns, dependency injection, event-driven design, Singleton alternatives, decoupling strategies. Use when designing system architecture, managing dependencies, or choosing communication patterns in Unity.
---

# Unity Architecture

Architecture patterns for scalable, testable Unity projects.

---

## Architecture decision - P1

```
What problem are you solving?
│
├─▶ "Systems need to communicate"
│   │
│   ├─▶ Tight coupling OK? → Direct reference (SerializeField)
│   └─▶ Need decoupling?
│       ├─▶ Fire-and-forget → Event/Message pattern
│       └─▶ Query current state → Shared data pattern
│
├─▶ "Need global access to something"
│   │
│   ├─▶ Static data (configs)? → ScriptableObject
│   ├─▶ Runtime service? → Service Locator or DI
│   └─▶ Truly unique manager? → Singleton (last resort)
│
├─▶ "Track objects in scene"
│   │
│   ├─▶ Find at runtime → RuntimeSet pattern
│   └─▶ Known at design time → SerializeField
│
└─▶ "Large-scale entity simulation"
    │
    ├─▶ < 1000 entities → Traditional OOP
    └─▶ 1000+ entities → ECS or Data-Oriented Design
```

---

## Approach comparison - P1

| Approach | Coupling | Testability | Use Case |
|----------|----------|-------------|----------|
| **Direct Reference** | Tight | Medium | Known, stable relationships |
| **Event Pattern** | Loose | High | One-to-many communication |
| **ScriptableObject** | Loose | High | Shared state, configs |
| **Singleton** | Very Tight | Low | Truly global state only |
| **DI Container** | Loose | High | Large projects, many services |

**Tang3cko Recommendation:** ScriptableObject-based patterns (EventChannel, Variable, RuntimeSet)

---

## Dependency priority - P1

```
1. Event-based      ← Complete decoupling (recommended)
2. SerializeField   ← Explicit, visible in Inspector
3. Find/GetComponent ← Fallback only, not primary strategy
4. Singleton        ← Last resort, truly global state only
```

---

## ScriptableObject patterns (Recommended) - P1

Tang3cko projects use ScriptableObject-based architecture via ReactiveSO package.

### Pattern selection

| Need | Pattern | Example |
|------|---------|---------|
| Fire-and-forget notification | EventChannel | `onEnemyKilled.RaiseEvent()` |
| Shared state with change events | Variable | `playerScore.Value += 10` |
| Track active objects | RuntimeSet | `enemies.Items` |
| Per-entity state | ReactiveEntitySet | Entity state management |
| Reusable commands | Actions | Quest rewards, loot tables |

### EventChannel (pub/sub)

```csharp
// Publisher
[SerializeField] private VoidEventChannelSO onEnemyKilled;
private void Die() => onEnemyKilled?.RaiseEvent();

// Subscriber
private void OnEnable() => onEnemyKilled.OnEventRaised += HandleEnemyKilled;
private void OnDisable() => onEnemyKilled.OnEventRaised -= HandleEnemyKilled;
```

### RuntimeSet (object tracking)

```csharp
// Self-registration
[SerializeField] private EnemyRuntimeSetSO enemySet;
private void OnEnable() => enemySet?.Add(this);
private void OnDisable() => enemySet?.Remove(this);

// Consumer (no Find operations needed)
foreach (var enemy in enemySet.Items) { }
```

See [references/](references/) for detailed patterns.

---

## Singleton alternatives - P1

### Why avoid Singleton?

- Hidden dependencies (not visible in Inspector)
- Tight coupling (hard to swap implementations)
- Testing difficulties (can't mock)
- Order of initialization issues

### Alternative: ScriptableObject as "Singleton"

```csharp
// ScriptableObject asset (drag to Inspector)
[CreateAssetMenu(menuName = "Game/Settings")]
public class GameSettingsSO : ScriptableObject
{
    public float masterVolume;
    public int difficulty;
}

// Usage (explicit, testable)
public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameSettingsSO settings;

    public void PlaySound(AudioClip clip)
    {
        audioSource.volume = settings.masterVolume;
        audioSource.PlayOneShot(clip);
    }
}
```

### When Singleton is acceptable

Use ONLY when ALL conditions are met:
1. Technically must be single instance
2. Same lifecycle as entire application
3. Truly global state
4. Referenced from 10+ locations

---

## Communication patterns - P1

### Direct Reference (tight coupling)

```csharp
// Simple, but creates dependency
[SerializeField] private PlayerHealth playerHealth;

private void Update()
{
    if (playerHealth.CurrentHealth < 20)
        ShowWarning();
}
```

### Event Pattern (loose coupling)

```csharp
// Publisher doesn't know subscribers
[SerializeField] private FloatEventChannelSO onHealthChanged;

private void OnEnable()
{
    onHealthChanged.OnEventRaised += HandleHealthChanged;
}

private void HandleHealthChanged(float health)
{
    if (health < 20) ShowWarning();
}
```

### Which to choose?

| Scenario | Recommendation |
|----------|----------------|
| UI updating game state | Event pattern |
| Parent-child relationship | Direct reference |
| Cross-system communication | Event pattern |
| Known, stable 1:1 relationship | Direct reference |
| Multiple listeners possible | Event pattern |

---

## References

| Topic | File | When to Read |
|-------|------|--------------|
| EventChannel details | [event-channels.md](references/event-channels.md) | Implementing pub/sub |
| Variable details | [variables.md](references/variables.md) | Shared state, GPU Sync |
| RuntimeSet details | [runtime-sets.md](references/runtime-sets.md) | Object tracking |
| ReactiveEntitySet | [reactive-entity-sets.md](references/reactive-entity-sets.md) | Per-entity state, ECS-like patterns |
| Actions | [actions.md](references/actions.md) | Command pattern |
| Extension patterns | [extension-patterns.md](references/extension-patterns.md) | Adding new EventChannel/RuntimeSet types |
| Dependency management | [dependency-management.md](references/dependency-management.md) | Refactoring from Singleton |
| Design principles | [design-principles.md](references/design-principles.md) | Observability, data-oriented design |

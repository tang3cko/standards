# Architecture Patterns

Architecture patterns for scalable, testable Unity projects.

---

## Architecture Decision

```
What problem are you solving?
│
├─▶ "Systems need to communicate"
│   ├─▶ Tight coupling OK? → Direct reference (SerializeField)
│   └─▶ Need decoupling?
│       ├─▶ Fire-and-forget → EventChannel
│       └─▶ Query current state → Variable
│
├─▶ "Need global access to something"
│   ├─▶ Static data (configs)? → ScriptableObject
│   ├─▶ Runtime service? → Service Locator or DI
│   └─▶ Truly unique manager? → Singleton (last resort)
│
├─▶ "Track objects in scene"
│   ├─▶ Find at runtime → RuntimeSet
│   └─▶ Known at design time → SerializeField
│
└─▶ "Large-scale entity simulation"
    ├─▶ < 1000 entities → Traditional OOP
    └─▶ 1000+ entities → ECS or Data-Oriented Design
```

---

## Approach Comparison

| Approach | Coupling | Testability | Use Case |
|----------|----------|-------------|----------|
| **Direct Reference** | Tight | Medium | Known, stable relationships |
| **EventChannel** | Loose | High | One-to-many communication |
| **ScriptableObject** | Loose | High | Shared state, configs |
| **Singleton** | Very Tight | Low | Truly global state only |
| **DI Container** | Loose | High | Large projects, many services |

**Tang3cko Recommendation:** ScriptableObject-based patterns (EventChannel, Variable, RuntimeSet)

---

## ScriptableObject Patterns

| Need | Pattern | Example |
|------|---------|---------|
| Fire-and-forget notification | EventChannel | `onEnemyKilled.RaiseEvent()` |
| Shared state with change events | Variable | `playerScore.Value += 10` |
| Track active objects | RuntimeSet | `enemies.Items` |
| Per-entity state | ReactiveEntitySet | Entity state management |
| Reusable commands | Actions | Quest rewards, loot tables |

---

## Singleton Alternatives

### Why Avoid Singleton?

- Hidden dependencies (not visible in Inspector)
- Tight coupling (hard to swap implementations)
- Testing difficulties (can't mock)
- Order of initialization issues

### Alternative: ScriptableObject as "Singleton"

```csharp
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

### When Singleton is Acceptable

Use ONLY when ALL conditions are met:
1. Technically must be single instance
2. Same lifecycle as entire application
3. Truly global state
4. Referenced from 10+ locations

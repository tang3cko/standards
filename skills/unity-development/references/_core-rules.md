# Core Unity Rules

Essential P1 rules that apply to all Unity development. Auto-loaded with every skill invocation.

---

## Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Class/Interface | PascalCase, `I` prefix | `PlayerController`, `IDamageable` |
| Private field | camelCase | `playerHealth`, `moveSpeed` |
| Public field/Property | PascalCase | `MaxHealth`, `IsAlive` |
| Method | PascalCase, verb | `TakeDamage()`, `GetInventory()` |
| Boolean | `is/has/can/should` prefix | `isAlive`, `hasKey`, `canJump` |
| Constant | UPPER_SNAKE_CASE | `MAX_HEALTH`, `DEFAULT_SPEED` |
| ScriptableObject | `SO` suffix | `EnemyDataSO`, `QuestSO` |
| EventChannel | `EventChannelSO` suffix | `VoidEventChannelSO` |
| Namespace | `ProjectName.Category` | `ProjectName.Core`, `ProjectName.UI` |

---

## File Structure Order

```csharp
using UnityEngine;

namespace ProjectName.Category
{
    public class ExampleClass : MonoBehaviour
    {
        // 1. Fields ([Header] groups, then private)
        [Header("Dependencies")]
        [SerializeField] private OtherComponent dependency;

        private bool isInitialized;

        // 2. Properties
        public bool IsInitialized => isInitialized;

        // 3. Unity Lifecycle (Awake → OnEnable → Start → Update → OnDisable → OnDestroy)
        private void Awake() { }
        private void OnEnable() { }

        // 4. Public Methods
        public void DoSomething() { }

        // 5. Private Methods
        private void HandleEvent() { }

        // 6. Editor Only
#if UNITY_EDITOR
        private void OnValidate() { }
#endif
    }
}
```

---

## Dependency Priority

```
1. Event-based      ← Complete decoupling (recommended)
2. SerializeField   ← Explicit, visible in Inspector
3. Find/GetComponent ← Fallback only
4. Singleton        ← Last resort
```

---

## Error Handling

```csharp
// Null conditional
eventChannel?.RaiseEvent();

// OnValidate validation
#if UNITY_EDITOR
private void OnValidate()
{
    if (target == null)
        Debug.LogWarning($"[{GetType().Name}] target not assigned on {gameObject.name}.", this);
}
#endif

// Early return
public void Process(QuestSO quest)
{
    if (quest == null) { Debug.LogError("Quest is null"); return; }
    if (!quest.IsActive) return;
    StartQuest(quest);
}
```

---

## Anti-Patterns

❌ **Don't:**
- Use Singleton for everything
- Poll in Update() when events work
- Use Find/GetComponent as primary strategy
- Hide dependencies (use [SerializeField])

✅ **Do:**
- Use EventChannel for cross-system communication
- Cache components in Awake()
- Use RuntimeSet instead of FindObjectsOfType
- Validate [SerializeField] in OnValidate()

---
name: unity-core
description: Unity C# fundamentals. Naming, project structure, error handling, performance, async, Input System. Use when writing or reviewing C# code in Unity. Triggers on "Unity C#", "命名規則", "naming convention", "パフォーマンス", "async", "Input System".
model: sonnet
context: fork
agent: general-purpose
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity C# Fundamentals

## Purpose

Apply Tang3cko coding standards for Unity C# code including naming conventions, project organization, error handling, performance optimization, async patterns, and Input System integration.

## Checklist

- [ ] Naming: PascalCase for classes/methods, camelCase for private fields
- [ ] Namespace: `ProjectName.Category` pattern
- [ ] File: One public class per file, filename matches class name
- [ ] Error: Null checks, OnValidate validation, early return
- [ ] Performance: Cache components, use RuntimeSet, event-driven updates
- [ ] Async: Choose Awaitable (Unity 6+), UniTask, or Coroutines

---

## Quick reference - P1

### Naming conventions

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

### Directory structure

```
Assets/_Project/
├── Scripts/
│   ├── Core/           # GameManager, SaveSystem
│   ├── Player/         # PlayerHealth, PlayerController
│   ├── Enemy/          # EnemyHealth, EnemyController
│   ├── UI/             # UIManager, MenuController
│   ├── Events/         # EventChannelSO definitions
│   └── Data/           # DataSO definitions
├── ScriptableObjects/
│   ├── Data/           # Static game data
│   ├── Events/         # EventChannel instances
│   └── RuntimeSets/    # Runtime collections
└── Prefabs/
```

### Error handling patterns

```csharp
// Null conditional operator
eventChannel?.RaiseEvent();

// OnValidate validation
#if UNITY_EDITOR
private void OnValidate()
{
    if (targetComponent == null)
        Debug.LogWarning($"[{GetType().Name}] targetComponent not assigned on {gameObject.name}.", this);
}
#endif

// Early return pattern
public void ProcessQuest(QuestSO quest)
{
    if (quest == null) { Debug.LogError("Quest is null"); return; }
    if (!quest.IsActive) { Debug.LogWarning("Quest not active"); return; }
    StartQuest(quest);
}
```

### Performance essentials

```csharp
// Cache components in Awake
private Rigidbody cachedRigidbody;
private void Awake() => cachedRigidbody = GetComponent<Rigidbody>();

// Event-driven UI (not Update polling)
[SerializeField] private IntEventChannelSO onHealthChanged;
private void OnEnable() => onHealthChanged.OnEventRaised += UpdateHealthUI;
private void OnDisable() => onHealthChanged.OnEventRaised -= UpdateHealthUI;

// Cache WaitForSeconds
private WaitForSeconds oneSecond;
private void Awake() => oneSecond = new WaitForSeconds(1f);
```

### Async pattern comparison

| Approach | Unity Version | Use Case |
|----------|---------------|----------|
| **Coroutines** | All | Simple delays, legacy code |
| **Awaitable** | Unity 6+ | Libraries, zero dependencies |
| **UniTask** | Any (package) | Production apps, full features |

```csharp
// Awaitable (Unity 6+)
private async Awaitable SpawnAsync()
{
    await Awaitable.WaitForSecondsAsync(1f);
    SpawnEnemy();
}

// UniTask
private async UniTask SpawnAsync()
{
    await UniTask.Delay(1000);
    SpawnEnemy();
}
```

### Input System (InputReader pattern)

```csharp
// Subscribe to InputReader events
[SerializeField] private InputReaderSO inputReader;

private void OnEnable()
{
    inputReader.OnMoveEvent += HandleMove;
    inputReader.OnJumpEvent += HandleJump;
}

private void OnDisable()
{
    inputReader.OnMoveEvent -= HandleMove;
    inputReader.OnJumpEvent -= HandleJump;
}
```

---

## Decision tree - P1

```
What are you doing?
│
├─▶ Naming something
│   └─ See [naming.md](references/naming.md)
│
├─▶ Organizing files/folders
│   └─ See [code-organization.md](references/code-organization.md)
│
├─▶ Adding comments/docs
│   └─ See [comments.md](references/comments.md)
│
├─▶ Handling errors/null checks
│   └─ See [error-handling.md](references/error-handling.md)
│
├─▶ Optimizing performance
│   └─ See [performance.md](references/performance.md)
│
├─▶ Implementing async operations
│   └─ See [async.md](references/async.md)
│
└─▶ Handling input
    └─ See [input-system.md](references/input-system.md)
```

---

## File structure order - P1

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Category
{
    public class ExampleClass : MonoBehaviour
    {
        // 1. Fields (Header groups)
        [Header("Dependencies")]
        [SerializeField] private OtherComponent dependency;

        [Header("Settings")]
        [SerializeField] private float speed = 5f;

        private bool isInitialized;

        // 2. Properties
        public bool IsInitialized => isInitialized;

        // 3. Unity Lifecycle (Awake → OnEnable → Start → Update → OnDisable → OnDestroy)
        private void Awake() { }
        private void OnEnable() { }
        private void Start() { }
        private void Update() { }
        private void OnDisable() { }
        private void OnDestroy() { }

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

## References

- [Naming conventions](references/naming.md) - PascalCase, camelCase, SO suffixes
- [Code organization](references/code-organization.md) - Directory structure, namespaces
- [Comments](references/comments.md) - XML docs, Header, Tooltip, TODO
- [Error handling](references/error-handling.md) - Null safety, try-catch, OnValidate
- [Performance](references/performance.md) - Caching, pooling, event-driven
- [Async patterns](references/async.md) - Awaitable, UniTask, Coroutines
- [Input System](references/input-system.md) - InputReader pattern, Action Maps

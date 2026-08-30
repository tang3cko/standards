# Naming Conventions

Detailed naming conventions extending the SKILL.md Core Rules summary.
For quick reference, see the SKILL.md Core Rules.

---

## Classes and Interfaces

- Use PascalCase
- Interfaces use `I` prefix

```csharp
public class PlayerController { }
public class GameManager { }
public interface IDamageable { }
public interface IInteractable { }
```

---

## Fields

```csharp
public class Example
{
    // Public fields: PascalCase
    public float MoveSpeed = 5f;

    // Private fields with SerializeField: camelCase
    [SerializeField] private float attackRange = 2f;

    // Private fields: camelCase
    private bool isMoving;

    // Constants (const / static readonly): PascalCase
    private const float MaxHealth = 100f;
    private static readonly Vector3 DefaultPosition = Vector3.zero;
}
```

---

## Methods

- PascalCase, start with verbs

```csharp
public void StartAttraction(Transform target) { }
private void UpdatePosition() { }
public bool ValidateInput(string input) { }
public List<Enemy> GetActiveEnemies() { }

// Event handlers
private void OnEnemyDeath() { }
private void HandleQuestCompleted(QuestSO quest) { }
```

---

## Properties

```csharp
public int Health { get; private set; }
public bool IsAlive => Health > 0;
public IReadOnlyList<Item> Items => items;
```

---

## Events and EventChannels

```csharp
// EventChannel fields: "on" prefix + past tense
[SerializeField] private VoidEventChannelSO onQuestCompleted;
[SerializeField] private IntEventChannelSO onScoreChanged;

// C# events: "On" prefix + past tense
public event Action OnDeath;
public event Action<int> OnHealthChanged;
```

---

## Namespace

```csharp
namespace ProjectName.Category

// Examples
namespace ProjectName.Player
namespace ProjectName.Quest
namespace ProjectName.UI
namespace ProjectName.Core
namespace ProjectName.Data
```

---

## Unity-Specific Suffixes

```csharp
// ScriptableObject: "SO" suffix
public class QuestSO : ScriptableObject { }
public class EnemyDataSO : ScriptableObject { }

// EventChannel: "EventChannelSO" suffix
public class VoidEventChannelSO : EventChannelSO { }
public class IntEventChannelSO : EventChannelSO<int> { }

// RuntimeSet: "RuntimeSetSO" suffix
public class EnemyRuntimeSetSO : ScriptableObject { }
```

---

## Boolean Variables

```csharp
// Use "is", "has", "can", "should" prefixes
private bool isAlive;
private bool hasKey;
private bool canJump;
private bool shouldSpawn;

// "was" / "were" / "are" are acceptable in past or state contexts
public void ArmAfterDig(bool wasSuccessful) { }
public bool ItemsAreReady => count > 0;
```

The same rule applies to method parameters and properties, not just fields.

---

## References


- [code-organization.md](code-organization.md) - File and namespace organization
- [comments.md](comments.md) - Documentation and comment conventions

## Checklist

- [ ] All booleans — fields, parameters, AND properties — carry `is/has/can/should` (or `was/were/are` in past/state contexts)
- [ ] Serialized EventChannel fields use `on` + past tense (`onQuestCompleted`); C# events use `On` + past tense (`OnHealthChanged`)
- [ ] Every ScriptableObject-derived type carries its suffix: `SO`, `EventChannelSO`, or `RuntimeSetSO`
- [ ] Constants (`const` / `static readonly`) are PascalCase — UPPER_SNAKE_CASE only for preprocessor symbols
- [ ] Methods start with a verb; event handlers use `On`/`Handle` prefixes

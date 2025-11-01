# Naming Conventions

## Purpose

Consistent naming conventions improve code readability and maintainability.

## Checklist

- [ ] Classes/Interfaces: PascalCase
- [ ] Interfaces: `I` prefix
- [ ] Fields: Public use PascalCase, Private use camelCase
- [ ] Methods: PascalCase, start with verb
- [ ] Properties: PascalCase, noun/adjective
- [ ] Events: `on` or `On` prefix + past tense
- [ ] Boolean variables: `is`, `has`, `can`, `should` prefix
- [ ] ScriptableObject: `SO` suffix
- [ ] EventChannel: `EventChannelSO` suffix
- [ ] Namespace: `ProjectName.Category` pattern

---

## Classes and Interfaces

### Basic Rules

- Use PascalCase
- Clear and descriptive names
- Interfaces use `I` prefix

### Implementation Examples

```csharp
// Classes
public class PlayerController { }
public class GameManager { }

// Interfaces
public interface IDamageable { }
public interface IInteractable { }

// ScriptableObject
public class EnemyDataSO : ScriptableObject { }
public class QuestSO : ScriptableObject { }
```

---

## Variables and Fields

### Field Naming Rules

```csharp
public class Example
{
    // Public fields: PascalCase
    public float MoveSpeed = 5f;

    // Private fields with SerializeField attribute: camelCase
    [SerializeField] private float attackRange = 2f;

    // Private fields: camelCase
    private bool isMoving;

    // Constants: UPPER_SNAKE_CASE or PascalCase
    private const float MAX_HEALTH = 100f;
    private const int MaxPlayers = 4;
}
```

### Local Variables

```csharp
public void ProcessData()
{
    // camelCase
    int playerCount = 0;
    float deltaTime = Time.deltaTime;
    var enemyList = new List<Enemy>();
}
```

---

## Methods

### Basic Rules

- Use PascalCase
- Start with verbs
- Clear action names

### Implementation Examples

```csharp
// Good: Start with verb, clear action
public void StartAttraction(Transform target) { }
private void UpdatePosition() { }
public bool ValidateInput(string input) { }
public List<Enemy> GetActiveEnemies() { }

// Event handlers: "On" or "Handle" prefix
private void OnEnemyDeath() { }
private void HandleQuestCompleted(QuestSO quest) { }

// Unity Callback Methods
private void OnTriggerEnter(Collider other) { }
private void OnValidate() { }
```

### Bad Examples

```csharp
// Bad: No verb
public void Position() { }

// Bad: Ambiguous
public void Process() { }
public void Do() { }

// Bad: Abbreviations
public void UpdPos() { }
```

---

## Properties

### Basic Rules

- Use PascalCase
- Nouns or adjectives

### Implementation Examples

```csharp
public class Player
{
    // Properties
    public int Health { get; private set; }
    public bool IsAlive => Health > 0;
    public Vector3 Position { get; set; }

    // Read-only properties
    public IReadOnlyList<Item> Items => items;
}
```

---

## Events

### EventChannel Naming

```csharp
// "on" prefix + past tense
[SerializeField] private VoidEventChannelSO onQuestCompleted;
[SerializeField] private IntEventChannelSO onScoreChanged;
[SerializeField] private CardEventChannelSO onCardPlayed;
```

### C# Event Naming

```csharp
// "On" prefix + past tense
public event Action OnDeath;
public event Action<int> OnHealthChanged;
```

---

## Namespace

### Project Structure-Based Naming

```csharp
namespace ProjectName.Category

// Examples
namespace ProjectName.Player      // Player-related
namespace ProjectName.Quest       // Quest system
namespace ProjectName.Network     // Network-related
namespace ProjectName.UI          // UI-related
namespace ProjectName.Core        // Core systems
namespace ProjectName.Data        // ScriptableObject definitions
```

---

## Unity-Specific Naming

### ScriptableObject

```csharp
// "SO" suffix
public class QuestSO : ScriptableObject { }
public class EnemyDataSO : ScriptableObject { }
public class PlayerHandSO : ScriptableObject { }
```

### EventChannel

```csharp
// "EventChannelSO" suffix
public class QuestEventChannelSO : EventChannelSO<QuestSO> { }
public class VoidEventChannelSO : EventChannelSO { }
public class IntEventChannelSO : EventChannelSO<int> { }
```

### RuntimeSet

```csharp
// "RuntimeSetSO" suffix
public class EnemyRuntimeSetSO : ScriptableObject { }
public class PlayerRuntimeSetSO : ScriptableObject { }
```

---

## Boolean Variable Naming

### Recommended Patterns

```csharp
// "is", "has", "can", "should" prefixes
private bool isAlive;
private bool hasKey;
private bool canJump;
private bool shouldSpawn;

// State flags
private bool isMoving;
private bool isGrounded;
private bool isInteractable;
```

---

## References

- [Code Organization](code-organization.md)
- [Comments Documentation](comments-documentation.md)

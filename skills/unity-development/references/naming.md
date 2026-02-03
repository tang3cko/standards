# Naming conventions

## Classes and interfaces

- Use PascalCase
- Interfaces use `I` prefix

```csharp
public class PlayerController { }
public class GameManager { }
public interface IDamageable { }
public interface IInteractable { }
```

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

    // Constants: UPPER_SNAKE_CASE
    private const float MAX_HEALTH = 100f;
}
```

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

## Properties

```csharp
public int Health { get; private set; }
public bool IsAlive => Health > 0;
public IReadOnlyList<Item> Items => items;
```

## Events and EventChannels

```csharp
// EventChannel fields: "on" prefix + past tense
[SerializeField] private VoidEventChannelSO onQuestCompleted;
[SerializeField] private IntEventChannelSO onScoreChanged;

// C# events: "On" prefix + past tense
public event Action OnDeath;
public event Action<int> OnHealthChanged;
```

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

## Unity-specific

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

## Boolean variables

```csharp
// Use "is", "has", "can", "should" prefixes
private bool isAlive;
private bool hasKey;
private bool canJump;
private bool shouldSpawn;
```

# Comments and documentation

## Purpose

Proper comments and documentation help code understanding and improve maintainability.

## Checklist

- [ ] XML documentation comments on public classes/methods
- [ ] Tooltip and Header on SerializeFields
- [ ] Explanatory comments on complex logic
- [ ] No comments on self-explanatory code
- [ ] Specific TODO comments

---

## XML documentation comments - P1

### Basic rules

- public classes, methods, and properties must have XML documentation comments
- Use `<summary>` tag for overview
- Use `<param>` tag for parameters
- Use `<returns>` tag for return values

### Class documentation

```csharp
/// <summary>
/// Manages experience orbs and spawns them when enemies die
/// </summary>
public class ExperienceOrbManager : MonoBehaviour
{
    // ...
}
```

### Method documentation

```csharp
/// <summary>
/// Spawns experience orbs at the specified position
/// </summary>
/// <param name="position">Spawn position</param>
/// <param name="experienceValue">Experience amount</param>
/// <returns>List of spawned orbs</returns>
public List<GameObject> SpawnOrbs(Vector3 position, int experienceValue)
{
    // Implementation
}
```

### Property documentation

```csharp
/// <summary>
/// Player's current health
/// </summary>
public int CurrentHealth { get; private set; }

/// <summary>
/// Whether the player is alive
/// </summary>
public bool IsAlive => CurrentHealth > 0;
```

---

## Header attribute - P1

### Inspector organization

```csharp
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat Stats")]
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float defense = 5f;

    [Header("Event Channels")]
    [SerializeField] private VoidEventChannelSO onDeath;
    [SerializeField] private FloatEventChannelSO onHealthChanged;
}
```

---

## Tooltip attribute - P1

### Inspector descriptions

```csharp
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]

    [Tooltip("Player's maximum health")]
    [SerializeField] private float maxHealth = 100f;

    [Tooltip("Movement speed per second (meters)")]
    [Range(1f, 10f)]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Attack power. Base damage dealt to enemies")]
    [SerializeField] private float attackPower = 10f;
}
```

---

## Inline comments - P1

### Usage guidelines

**Good: Explaining Complex Logic**

```csharp
public int GetCardStrength()
{
    // Card game rule: 2 is strongest, Ace is second strongest
    if (rank == 2) return 15;
    if (rank == 1) return 14;
    return rank;
}

// Reverse strength during revolution
if (isRevolutionActive)
{
    return 20 - strength;
}
```

**Bad: Obvious Code Comments**

```csharp
// Bad: Unnecessary comment
// Increase player health
health += 10;

// Good: No comment (code is self-explanatory)
playerHealth.Heal(10);
```

**Good: Explaining Intent**

```csharp
// Server-only state updates for network synchronization
if (!NetworkServer.active) return;

// UI update deferred to next frame (avoid layout recalculation)
StartCoroutine(UpdateUINextFrame());
```

---

## TODO comments - P1

### Usage rules

```csharp
// TODO: Add DOTween animation in Phase 2
public void PlayCard(CardSO card)
{
    // Currently executes immediately
    onCardPlayed?.RaiseEvent(card);
}

// FIXME: Incomplete cleanup handling on host disconnect
public override void OnStopServer()
{
    base.OnStopServer();
}

// NOTE: This process only runs on server side
[ServerCallback]
private void Update()
{
    // ...
}
```

---

## When documentation is unnecessary - P1

### Self-explanatory code

```csharp
// Bad: Unnecessary comment
// Get player position
public Vector3 GetPosition() => transform.position;

// Good: No comment (method name is sufficiently descriptive)
public Vector3 GetPlayerPosition() => transform.position;
```

### Unity event functions

```csharp
// Bad: Unnecessary
/// <summary>
/// Awake method
/// </summary>
private void Awake() { }

// Good: No comment (Unity standard method)
private void Awake()
{
    // Only comment initialization logic
}
```

---

## Practical example - P1

### Complete class documentation

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Quest
{
    /// <summary>
    /// ScriptableObject that manages quest progress
    /// </summary>
    [CreateAssetMenu(fileName = "QuestProgress", menuName = "ProjectName/Data/Quest/QuestProgress")]
    public class QuestProgressSO : ScriptableObject
    {
        [Header("Current Quest")]
        [Tooltip("Currently active quest")]
        [SerializeField] private QuestSO currentQuest;

        [Header("Progress")]
        [Tooltip("Current progress count")]
        [SerializeField] private int currentProgress;

        [Tooltip("Time remaining (seconds)")]
        [SerializeField] private float timeRemaining;

        /// <summary>
        /// Current quest state
        /// </summary>
        public QuestState State { get; private set; }

        /// <summary>
        /// Initializes the quest
        /// </summary>
        /// <param name="quest">Quest to start</param>
        public void Initialize(QuestSO quest)
        {
            currentQuest = quest;
            currentProgress = 0;
            timeRemaining = quest.timeLimit;
            State = QuestState.InProgress;
        }

        /// <summary>
        /// Adds progress to the quest
        /// </summary>
        /// <param name="amount">Progress amount to add</param>
        /// <returns>Whether the quest was completed</returns>
        public bool AddProgress(int amount = 1)
        {
            currentProgress += amount;

            // Check goal achievement
            if (currentProgress >= currentQuest.requiredCount)
            {
                State = QuestState.Success;
                return true;
            }

            return false;
        }
    }
}
```

---

## References

- [Naming Conventions](naming-conventions.md)
- [Code Organization](code-organization.md)

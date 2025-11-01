# Reactive variables with Variables System

## Purpose

Implement reactive state management using Tang3cko.EventChannels Variables for automatic event notification when values change.

## Checklist

- [ ] Use Variables for stateful data that requires change notification
- [ ] Use EventChannels for stateless events
- [ ] Access values via Variable.Value property
- [ ] Assign EventChannel optionally based on notification requirements
- [ ] Use appropriate Variable type for data being stored
- [ ] Follow Variable naming conventions (noun form)

---

## Variables vs EventChannels

### When to Use Variables

Use Variables when:

```
✅ State needs to be persisted (score, health, flags)
✅ Current value needs to be referenced (UI display, calculations)
✅ Value changes require automatic notification
✅ Multiple systems need to read the same value
```

### When to Use EventChannels

Use EventChannels when:

```
✅ State is not required (button clicks, enemy death)
✅ One-time notification is sufficient (game start, scene transition)
✅ Value does not need to be stored
✅ Pure event-driven communication
```

### Comparison Example

**Variables: Stateful Data**

```csharp
// Variable stores current score
IntVariableSO playerScore;  // Current value: 1500

// Anyone can read current value
int currentScore = playerScore.Value;

// Setting value automatically notifies subscribers
playerScore.Value += 100;  // Triggers onScoreChanged event
```

**EventChannels: Stateless Events**

```csharp
// EventChannel does not store state
VoidEventChannelSO onEnemyKilled;

// Just notification - no value to read
onEnemyKilled.RaiseEvent();  // Fire and forget
```

---

## Variable Types

### Available Types

| Variable Type | Use Case | Example |
|---------------|----------|---------|
| `IntVariableSO` | Integer values, counts, IDs | Score, kill count, level |
| `FloatVariableSO` | Decimal values, ratios | Health percentage, speed multiplier |
| `BoolVariableSO` | Boolean flags, toggles | Game paused, feature enabled |
| `StringVariableSO` | Text data, keys | Player name, active quest ID |
| `Vector2VariableSO` | 2D positions, input | Mouse position, joystick input |
| `Vector3VariableSO` | 3D positions, directions | Player position, look direction |
| `GameObjectVariableSO` | Object references | Active target, selected object |

---

## Basic Implementation

### Creating a Variable

**1. Create Variable Asset**

Right-click in Project window → Create → Event Channels → Variables → Int Variable

**2. Create EventChannel Asset (Optional)**

Right-click in Project window → Create → Event Channels → Channels → Int Event

**3. Assign EventChannel to Variable**

Select IntVariableSO asset → Assign IntEventChannelSO in "On Value Changed" field

### Publisher Pattern

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Core
{
    /// <summary>
    /// Score management using Variables
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        [Header("Variables")]
        [SerializeField] private IntVariableSO playerScore;

        private void Start()
        {
            // Initialize score
            playerScore.Value = 0;
        }

        public void AddScore(int points)
        {
            // Setting value automatically raises event if EventChannel is assigned
            playerScore.Value += points;
        }

        public void ResetScore()
        {
            // Reset to initial value defined in Inspector
            playerScore.ResetToInitial();
        }
    }
}
```

### Subscriber Pattern

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Score display subscribes to score change events
    /// </summary>
    public class ScoreUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreChanged;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            // Subscribe to EventChannel (not Variable directly)
            onScoreChanged.OnEventRaised += UpdateScoreDisplay;
        }

        private void OnDisable()
        {
            // Always unsubscribe to prevent memory leaks
            onScoreChanged.OnEventRaised -= UpdateScoreDisplay;
        }

        private void UpdateScoreDisplay(int newScore)
        {
            scoreText.text = $"Score: {newScore}";
        }
    }
}
```

---

## Value Change Detection

### Automatic Event Firing

Variables use `EqualityComparer<T>` to detect value changes:

```csharp
// Only raises event when value actually changes
playerScore.Value = 100;  // Raises event
playerScore.Value = 100;  // Does NOT raise event (same value)
playerScore.Value = 150;  // Raises event
```

### High-Frequency Updates

Safe to use in Update loop - events only fire on actual changes:

```csharp
private void Update()
{
    // Only raises event when mouse position changes
    mousePosition.Value = Input.mousePosition;
}
```

---

## EventChannel Assignment Patterns

### Pattern A: With EventChannel (Event-Driven)

Use when multiple systems need automatic notification:

```csharp
// Variable assigned with EventChannel
IntVariableSO playerHealth;  // EventChannel assigned in Inspector

// Publisher
playerHealth.Value -= damage;  // Automatically notifies subscribers

// Subscriber (UI)
onHealthChanged.OnEventRaised += UpdateHealthBar;

// Subscriber (Audio)
onHealthChanged.OnEventRaised += PlayLowHealthSound;
```

**Benefits:**
- Automatic notifications
- Decoupled communication
- Multiple subscribers

### Pattern B: Without EventChannel (Polling)

Use when only direct value reads are needed:

```csharp
// Variable without EventChannel
IntVariableSO playerHealth;  // EventChannel not assigned

// Direct value access
if (playerHealth.Value <= 0)
{
    Die();
}

// Polling in Update (use sparingly)
private void Update()
{
    healthText.text = $"HP: {playerHealth.Value}";
}
```

**Use Cases:**
- Simple value reads
- No notification required
- Single-system access

---

## Naming Conventions

### Variable Assets

Use noun form describing the data:

```
✅ Good:
- playerScore (IntVariableSO)
- playerHealth (FloatVariableSO)
- isGamePaused (BoolVariableSO)
- mousePosition (Vector2VariableSO)

❌ Bad:
- onScoreChanged (use EventChannel for this)
- ScoreValue (redundant suffix)
- score_player (use camelCase)
```

### EventChannel Assets

Use `on + PastTense` form:

```
✅ Good:
- onScoreChanged (IntEventChannelSO)
- onHealthChanged (FloatEventChannelSO)
- onGamePaused (BoolEventChannelSO)
```

---

## Practical Example

### Complete Health System

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Core
{
    /// <summary>
    /// Player health management with Variable
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Variables")]
        [SerializeField] private FloatVariableSO currentHealth;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;

        private void Start()
        {
            // Initialize health to max
            currentHealth.Value = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            // Clamp and update health
            currentHealth.Value = Mathf.Max(0, currentHealth.Value - damage);

            // Check death
            if (currentHealth.Value <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            currentHealth.Value = Mathf.Min(maxHealth, currentHealth.Value + amount);
        }

        private void Die()
        {
            Debug.Log("Player died");
        }
    }
}
```

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Health bar UI subscribes to health changes
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private Image fillImage;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;

        private void OnEnable()
        {
            onHealthChanged.OnEventRaised += UpdateHealthBar;
        }

        private void OnDisable()
        {
            onHealthChanged.OnEventRaised -= UpdateHealthBar;
        }

        private void UpdateHealthBar(float currentHealth)
        {
            // Update fill amount (0.0 to 1.0)
            fillImage.fillAmount = currentHealth / maxHealth;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[HealthBarUI] onHealthChanged is not assigned.", this);

            if (fillImage == null)
                Debug.LogWarning($"[HealthBarUI] fillImage is not assigned.", this);
        }
#endif
    }
}
```

---

## Benefits

### Centralized State

```csharp
// Single source of truth
IntVariableSO playerScore;  // ScriptableObject asset

// Multiple systems access the same instance
// - ScoreManager (writes)
// - ScoreUI (reads)
// - QuestManager (reads)
// - AchievementManager (reads)
```

### Persistent State

```csharp
// Variables are ScriptableObject assets
// Values survive scene reloads (during Play mode)
playerScore.Value = 1000;  // Additive scene loading preserves this value
```

### Inspector Visibility

```
Assets/_Project/ScriptableObjects/Variables/
├── PlayerScore.asset (IntVariableSO)
├── PlayerHealth.asset (FloatVariableSO)
└── IsGameActive.asset (BoolVariableSO)
```

- View current runtime value in Inspector during Play mode
- "Reset to Initial" button for testing
- Easily find all references to a variable

### Testability

```csharp
// Easy to create mock Variables for testing
IntVariableSO mockScore = ScriptableObject.CreateInstance<IntVariableSO>();
mockScore.Value = 1000;

// Test without EventChannel
scoreManager.playerScore = mockScore;
scoreManager.AddScore(100);
Assert.AreEqual(1100, mockScore.Value);
```

---

## Common Patterns

### Multiple Variables with Shared EventChannel

```csharp
// Two separate Variables
IntVariableSO playerScore;    // Current score
IntVariableSO highScore;      // Best score

// Both use the same EventChannel
IntEventChannelSO onScoreChanged;

// Both variables assigned same EventChannel in Inspector
// UI subscribes once, receives updates from both variables
```

### Conditional EventChannel Assignment

```csharp
// Debug-only EventChannel for development
#if UNITY_EDITOR
[SerializeField] private FloatEventChannelSO onDebugValueChanged;
#endif

public float DebugValue
{
    get => debugValue.Value;
    set
    {
        debugValue.Value = value;
#if UNITY_EDITOR
        // Optional debug notification
        onDebugValueChanged?.RaiseEvent(value);
#endif
    }
}
```

---

## Anti-Patterns

### Direct Variable Subscription

**❌ Bad:**

```csharp
// Subscribing directly to Variable property
playerScore.OnValueChanged.OnEventRaised += UpdateUI;
```

**Problem:** Tight coupling to Variable asset.

**✅ Good:**

```csharp
// Subscribe to EventChannel (SerializeField)
[SerializeField] private IntEventChannelSO onScoreChanged;
onScoreChanged.OnEventRaised += UpdateUI;
```

**Benefits:** Decoupling, flexibility, testability.

---

### Mixing Variables and Direct References

**❌ Bad:**

```csharp
public class GameManager : MonoBehaviour
{
    public int playerScore;  // Direct field
    public IntVariableSO playerHealth;  // Variable

    // Inconsistent state management
}
```

**✅ Good:**

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private IntVariableSO playerScore;
    [SerializeField] private IntVariableSO playerHealth;

    // Consistent Variable usage
}
```

---

### Ignoring Initial Value Reset

**❌ Bad:**

```csharp
private void Start()
{
    // Assumes Variable is 0, but previous Play session set it to 1000
    playerScore.Value += 10;  // Now 1010, not 10!
}
```

**✅ Good:**

```csharp
private void Start()
{
    // Explicitly reset to initial value
    playerScore.ResetToInitial();

    // Or set to specific value
    playerScore.Value = 0;
}
```

**Note:** Variables automatically reset to initial value when entering Play mode (Editor only).

---

## References

- [Event Channels](event-channels.md)
- [ScriptableObject Pattern](scriptableobject.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [Dependency Management](dependency-management.md)

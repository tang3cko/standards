# Reactive Variables with Variables System

## Purpose

Implement reactive state management using Tang3cko.ReactiveSO Variables for automatic event notification when values change.

## Checklist

- [ ] Use Variables for stateful data that requires change notification
- [ ] Use EventChannels for stateless events
- [ ] Access values via Variable.Value property
- [ ] Assign EventChannel optionally based on notification requirements
- [ ] Use appropriate Variable type for data being stored
- [ ] Create Variable assets via `Reactive SO/Variables/...`
- [ ] Follow Variable naming conventions (noun form)
- [ ] Use Variable Monitor Window for debugging (`Window/Reactive SO/Variable Monitor`)
- [ ] Consider GPU Sync for shader-related variables

---

## Variables vs EventChannels - P1

### When to use Variables

Use Variables when:

```
✅ State needs to be persisted (score, health, flags)
✅ Current value needs to be referenced (UI display, calculations)
✅ Value changes require automatic notification
✅ Multiple systems need to read the same value
```

### When to use EventChannels

Use EventChannels when:

```
✅ State is not required (button clicks, enemy death)
✅ One-time notification is sufficient (game start, scene transition)
✅ Value does not need to be stored
✅ Pure event-driven communication
```

### Comparison example

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

## Built-in Variable types - P1

### Available types (11 built-in)

All types are provided by the package. Create assets via `Create → Reactive SO → Variables → [Type]`.

| Variable Type | Use Case | Example |
|---------------|----------|---------|
| `IntVariableSO` | Integer values, counts, IDs | Score, kill count, level |
| `LongVariableSO` | Large integer values | Experience points, currency |
| `FloatVariableSO` | Decimal values, ratios | Health percentage, speed multiplier |
| `DoubleVariableSO` | High-precision decimals | Scientific calculations, timers |
| `BoolVariableSO` | Boolean flags, toggles | Game paused, feature enabled |
| `StringVariableSO` | Text data, keys | Player name, active quest ID |
| `Vector2VariableSO` | 2D positions, input | Mouse position, joystick input |
| `Vector3VariableSO` | 3D positions, directions | Player position, look direction |
| `QuaternionVariableSO` | Rotations | Camera rotation, object orientation |
| `ColorVariableSO` | Color values | Theme color, player color |
| `GameObjectVariableSO` | Object references | Active target, selected object |

---

## Basic implementation - P1

### Creating a Variable

**1. Create Variable Asset**

Right-click in Project window → Create → Reactive SO → Variables → Int Variable

**2. Create EventChannel Asset (Optional)**

Right-click in Project window → Create → Reactive SO → Channels → Int Event

**3. Assign EventChannel to Variable**

Select IntVariableSO asset → Assign IntEventChannelSO in "On Value Changed" field

### Publisher pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

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

### Subscriber pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

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

## GPU Sync feature - P1

### Overview

Variables can automatically synchronize their values to shader global properties, enabling data-driven shader effects without manual scripting.

### Supported types for GPU Sync

| Variable Type | Shader Property Type |
|---------------|---------------------|
| `IntVariableSO` | `int` |
| `FloatVariableSO` | `float` |
| `BoolVariableSO` | `int` (0 or 1) |
| `Vector2VariableSO` | `Vector2` |
| `Vector3VariableSO` | `Vector3` |
| `QuaternionVariableSO` | `Vector4` |
| `ColorVariableSO` | `Color` |

### Enabling GPU Sync

1. Select Variable asset in Project window
2. In Inspector, enable "GPU Sync Enabled"
3. Enter shader property name (MUST start with underscore `_`)

**Important:** Shader property names MUST start with underscore (`_`).

```csharp
// OK: Property name starts with underscore
_PlayerHealth
_TimeScale
_DamageIntensity

// NG: Property name without underscore (will not work)
PlayerHealth
TimeScale
DamageIntensity
```

### Example: Health-based shader effect

**Variable Configuration:**
- Type: `FloatVariableSO`
- GPU Sync Enabled: ✓
- Property Name: `_PlayerHealth`

**Shader Code:**

```hlsl
// Shader receives value automatically
float _PlayerHealth;

fixed4 frag(v2f i) : SV_Target
{
    // Use health value for visual effect
    fixed4 col = tex2D(_MainTex, i.uv);

    // Red tint when health is low
    if (_PlayerHealth < 0.3)
    {
        col.r += (0.3 - _PlayerHealth) * 2;
    }

    return col;
}
```

**C# Code:**

```csharp
// Just update the Variable - shader receives value automatically
playerHealth.Value = currentHealth / maxHealth;
```

### Use cases for GPU Sync

- Player health affecting post-processing
- Time-based shader animations
- Game state affecting environment shaders
- Color theme propagation to all materials

---

## Value change detection - P1

### Automatic event firing

Variables use `EqualityComparer<T>` to detect value changes:

```csharp
// Only raises event when value actually changes
playerScore.Value = 100;  // Raises event
playerScore.Value = 100;  // Does NOT raise event (same value)
playerScore.Value = 150;  // Raises event
```

### High-frequency updates

Safe to use in Update loop - events only fire on actual changes:

```csharp
private void Update()
{
    // Only raises event when mouse position changes
    mousePosition.Value = Input.mousePosition;
}
```

---

## EventChannel assignment patterns - P1

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

### Pattern B: Without EventChannel (polling)

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

## Debugging features - P1

### Variable Monitor Window

Access via `Window → Reactive SO → Variable Monitor` to monitor all Variables in real-time.

Features:
- View all Variable assets in project
- Real-time value display during Play Mode
- Filter by name or type
- Click to select asset in Project window

### Inspector debugging

Select any Variable asset in Inspector during Play Mode to see:
- Current runtime value (editable)
- Initial value
- "Reset to Initial" button
- Subscriber list for assigned EventChannel
- GPU Sync status and property name

### OnAnyValueChanged static event

Static event for global value monitoring (Editor only):

```csharp
#if UNITY_EDITOR
private void OnEnable()
{
    VariableSO.OnAnyValueChanged += HandleAnyValueChanged;
}

private void OnDisable()
{
    VariableSO.OnAnyValueChanged -= HandleAnyValueChanged;
}

private void HandleAnyValueChanged(VariableSO variable, object oldValue, object newValue)
{
    Debug.Log($"Variable changed: {variable.name} from {oldValue} to {newValue}");
}
#endif
```

Use cases:
- Custom debugging tools
- State change logging
- Analytics integration

---

## Naming conventions - P1

### Variable assets

Use noun form describing the data:

```
✅ Good:
- playerScore (IntVariableSO)
- playerHealth (FloatVariableSO)
- isGamePaused (BoolVariableSO)
- mousePosition (Vector2VariableSO)
- cameraRotation (QuaternionVariableSO)
- themeColor (ColorVariableSO)

❌ Bad:
- onScoreChanged (use EventChannel for this)
- ScoreValue (redundant suffix)
- score_player (use camelCase)
```

### EventChannel assets

Use `on + PastTense` form:

```
✅ Good:
- onScoreChanged (IntEventChannelSO)
- onHealthChanged (FloatEventChannelSO)
- onGamePaused (BoolEventChannelSO)
```

---

## Practical example - P1

### Complete health system

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

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
using Tang3cko.ReactiveSO;

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

## Benefits - P1

### Centralized state

```csharp
// Single source of truth
IntVariableSO playerScore;  // ScriptableObject asset

// Multiple systems access the same instance
// - ScoreManager (writes)
// - ScoreUI (reads)
// - QuestManager (reads)
// - AchievementManager (reads)
```

### Persistent state

```csharp
// Variables are ScriptableObject assets
// Values survive scene reloads (during Play mode)
playerScore.Value = 1000;  // Additive scene loading preserves this value
```

### Inspector visibility

```
Assets/_Project/ScriptableObjects/Variables/
├── PlayerScore.asset (IntVariableSO)
├── PlayerHealth.asset (FloatVariableSO)
├── CameraRotation.asset (QuaternionVariableSO)
├── ThemeColor.asset (ColorVariableSO)
└── IsGameActive.asset (BoolVariableSO)
```

- View current runtime value in Inspector during Play mode
- "Reset to Initial" button for testing
- Easily find all references to a variable
- Monitor all variables in Variable Monitor Window

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

## Common patterns - P1

### Multiple Variables with shared EventChannel

```csharp
// Two separate Variables
IntVariableSO playerScore;    // Current score
IntVariableSO highScore;      // Best score

// Both use the same EventChannel
IntEventChannelSO onScoreChanged;

// Both variables assigned same EventChannel in Inspector
// UI subscribes once, receives updates from both variables
```

### Conditional EventChannel assignment

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

## Anti-patterns - P1

### Direct Variable subscription

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

### Mixing Variables and direct references

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

### Ignoring initial value reset

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
- [RuntimeSet Pattern](runtime-sets.md)
- [ReactiveEntitySet Pattern](reactive-entity-sets.md)
- [Design Principles](design-principles.md)
- [Dependency Management](dependency-management.md)

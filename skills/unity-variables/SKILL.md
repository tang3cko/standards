---
name: unity-variables
description: IntVariableSO, FloatVariableSO for reactive state. Auto-notify on change, GPU Sync. Use when implementing shared state in Unity.
---

# Reactive Variables with Variables System

## Purpose

Implement reactive state management using Tang3cko.ReactiveSO Variables for automatic event notification when values change.

## Checklist

- [ ] Use Variables for stateful data that requires change notification
- [ ] Use EventChannels for stateless events
- [ ] Access values via Variable.Value property
- [ ] Assign EventChannel optionally based on notification requirements

---

## Variables vs EventChannels - P1

### When to use Variables

```
✅ State needs to be persisted (score, health, flags)
✅ Current value needs to be referenced (UI display, calculations)
✅ Value changes require automatic notification
```

### When to use EventChannels

```
✅ State is not required (button clicks, enemy death)
✅ One-time notification is sufficient
✅ Value does not need to be stored
```

---

## Built-in Variable types - P1

| Variable Type | Use Case | Example |
|---------------|----------|---------|
| `IntVariableSO` | Integer values, counts | Score, kill count, level |
| `FloatVariableSO` | Decimal values, ratios | Health percentage, speed |
| `BoolVariableSO` | Boolean flags | Game paused, feature enabled |
| `StringVariableSO` | Text data | Player name, active quest ID |
| `Vector2VariableSO` | 2D positions | Mouse position |
| `Vector3VariableSO` | 3D positions | Player position |
| `ColorVariableSO` | Color values | Theme color |

---

## Basic implementation - P1

### Publisher pattern

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Variables")]
        [SerializeField] private IntVariableSO playerScore;

        private void Start()
        {
            playerScore.Value = 0;
        }

        public void AddScore(int points)
        {
            // Setting value automatically raises event if EventChannel is assigned
            playerScore.Value += points;
        }

        public void ResetScore()
        {
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
    public class ScoreUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onScoreChanged;

        [Header("UI References")]
        [SerializeField] private TMPro.TextMeshProUGUI scoreText;

        private void OnEnable()
        {
            onScoreChanged.OnEventRaised += UpdateScoreDisplay;
        }

        private void OnDisable()
        {
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

Variables can automatically synchronize their values to shader global properties.

### Enabling GPU Sync

1. Select Variable asset in Project window
2. Enable "GPU Sync Enabled"
3. Enter shader property name (MUST start with underscore `_`)

```csharp
// OK: Property name starts with underscore
_PlayerHealth
_TimeScale

// NG: Property name without underscore (will not work)
PlayerHealth
```

### Shader Code

```hlsl
float _PlayerHealth;

fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);

    // Red tint when health is low
    if (_PlayerHealth < 0.3)
    {
        col.r += (0.3 - _PlayerHealth) * 2;
    }

    return col;
}
```

---

## Value change detection - P1

Variables use `EqualityComparer<T>` to detect value changes:

```csharp
// Only raises event when value actually changes
playerScore.Value = 100;  // Raises event
playerScore.Value = 100;  // Does NOT raise event (same value)
playerScore.Value = 150;  // Raises event
```

---

## Naming conventions - P1

### Variable assets

Use noun form describing the data:

```
✅ Good:
- playerScore (IntVariableSO)
- playerHealth (FloatVariableSO)
- isGamePaused (BoolVariableSO)

❌ Bad:
- onScoreChanged (use EventChannel for this)
- ScoreValue (redundant suffix)
```

### EventChannel assets

Use `on + PastTense` form:

```
✅ Good:
- onScoreChanged (IntEventChannelSO)
- onHealthChanged (FloatEventChannelSO)
```

---

## Anti-patterns - P1

### Direct Variable subscription

**❌ Bad:**

```csharp
playerScore.OnValueChanged.OnEventRaised += UpdateUI;
```

**✅ Good:**

```csharp
[SerializeField] private IntEventChannelSO onScoreChanged;
onScoreChanged.OnEventRaised += UpdateUI;
```

### Ignoring initial value reset

**❌ Bad:**

```csharp
private void Start()
{
    playerScore.Value += 10;  // Previous value may persist!
}
```

**✅ Good:**

```csharp
private void Start()
{
    playerScore.ResetToInitial();
    // Or: playerScore.Value = 0;
}
```

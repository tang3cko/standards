# Variable Details

VariableSO pattern for shared state with change detection.

---

## Built-in Variable Types

| Type | Use Case | Example |
|------|----------|---------|
| `IntVariableSO` | Integer values, counts | Score, kill count, level |
| `FloatVariableSO` | Decimal values, ratios | Health percentage, speed |
| `BoolVariableSO` | Boolean flags | Game paused, feature enabled |
| `StringVariableSO` | Text data | Player name, active quest ID |
| `Vector2VariableSO` | 2D positions | Mouse position |
| `Vector3VariableSO` | 3D positions | Player position |
| `ColorVariableSO` | Color values | Theme color |

---

## Usage

### Variables vs EventChannels

| Use Variables when | Use EventChannels when |
|-------------------|------------------------|
| State needs to persist | State is not required |
| Current value needs to be read | One-time notification is sufficient |
| Value changes require auto-notification | Value does not need to be stored |

### Value change detection

Variables use `EqualityComparer<T>` to detect changes:

```csharp
playerScore.Value = 100;  // Raises event
playerScore.Value = 100;  // Does NOT raise event (same value)
playerScore.Value = 150;  // Raises event
```

### Naming conventions

- **Variable assets:** Noun form (playerScore, playerHealth, isGamePaused)
- **EventChannel assets:** `on + PastTense` form (onScoreChanged, onHealthChanged)

---

## GPU Sync Feature

Variables can automatically sync to shader global properties.

### Setup

1. Select Variable asset in Project window
2. Enable "GPU Sync Enabled"
3. Enter shader property name (MUST start with underscore `_`)

```csharp
// Good: Property name starts with underscore
_PlayerHealth
_TimeScale

// Bad: Property name without underscore (will not work)
PlayerHealth
```

### Shader code

```hlsl
float _PlayerHealth;

fixed4 frag(v2f i) : SV_Target
{
    fixed4 col = tex2D(_MainTex, i.uv);

    if (_PlayerHealth < 0.3)
    {
        col.r += (0.3 - _PlayerHealth) * 2;
    }

    return col;
}
```

---

## Anti-Patterns

### Direct Variable subscription

```csharp
// Bad: Subscribing directly to Variable's internal event creates tight coupling
// between the subscriber and the Variable's implementation. If the Variable is
// swapped or refactored, all direct subscribers break. Using a separate
// EventChannel allows the Variable and its consumers to evolve independently.
playerScore.OnValueChanged.OnEventRaised += UpdateUI;

// Good: Subscribe to the dedicated EventChannel instead
[SerializeField] private IntEventChannelSO onScoreChanged;
onScoreChanged.OnEventRaised += UpdateUI;
```

### Ignoring initial value reset

```csharp
// Bad: Previous value may persist from last play session
private void Start()
{
    playerScore.Value += 10;
}

// Good: Reset to initial value first
private void Start()
{
    playerScore.ResetToInitial();
}
```

---

## Publisher Contract: Variable + EventChannel Co-existence

Variable and EventChannel have orthogonal responsibilities:

- **Variable** = single source of truth for the **value** (persisted, queryable)
- **EventChannel** = single notification path for the **change**

These are **not** redundant systems. The writer of a Variable is responsible for both: updating the Variable and raising the matched EventChannel in the same frame.

```csharp
// Settings UI (publisher)
public void OnVolumeSliderChanged(float value)
{
    masterVolume.Value = value;
    onMasterVolumeChanged.RaiseEvent(value);
}
```

Subscribers read the Variable for initial value (e.g. on `OnEnable`) and listen on the EventChannel for changes:

```csharp
private void OnEnable()
{
    onMasterVolumeChanged.OnEventRaised += ApplyVolume;
    ApplyVolume(masterVolume.Value);  // initial-value bootstrap
}
```

This pattern keeps the Variable consumers loosely coupled to the Variable implementation (rule from Anti-Patterns above) while preserving a single value as the source of truth.

---

## References

- [event-channels.md](event-channels.md) - EventChannel patterns and types
- [architecture.md](architecture.md) - ScriptableObject patterns overview
- [dependency-management.md](dependency-management.md) - Dependency priority

---

## Checklist

- [ ] Consumers subscribe to the paired EventChannel — never directly to the Variable's internal `OnValueChanged`
- [ ] The Variable writer honors the Publisher Contract: updates the Variable and raises the matched EventChannel in the same frame
- [ ] Subscribers bootstrap the initial value from the Variable in `OnEnable`, then listen on the channel for changes
- [ ] Session-scoped Variables call `ResetToInitial()` at startup so stale editor-session values cannot leak in
- [ ] GPU-synced shader property names start with underscore (`_PlayerHealth`)
- [ ] Naming: Variable assets are nouns (playerScore), paired channels are `on + PastTense` (onScoreChanged)

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

### Reaching through the Variable to get at its channel

`VariableSO<T>.OnValueChanged` exposes the paired channel asset, so subscribing through it works —
but it makes the subscriber depend on the Variable as well as on the channel, for no benefit.
Serialize the channel asset directly instead.

```csharp
// Bad: now the subscriber needs a reference to the Variable too
[SerializeField] private IntVariableSO playerScore;
playerScore.OnValueChanged.OnEventRaised += UpdateUI;

// Good: depend only on the channel asset
[SerializeField] private IntEventChannelSO onScoreChanged;
onScoreChanged.OnEventRaised += UpdateUI;
```

Both reach the same channel asset; the second one keeps the display decoupled from where the
value lives.

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

**The Variable owns the pairing, and raises the channel itself.** `VariableSO<T>` holds a serialized `EventChannelSO<T>` field, and the `Value` setter raises it whenever the value actually differs:

```csharp
// Tang3cko.ReactiveSO — VariableSO<T>
[SerializeField] private EventChannelSO<T> onValueChanged;   // an asset reference, not a C# event

public T Value
{
    set
    {
        if (!EqualityComparer<T>.Default.Equals(this.value, value))
        {
            this.value = value;
            RaiseValueChangedEvent(value);   // the Variable raises the paired channel
        }
    }
}
```

So the writer assigns `Value` and nothing else:

```csharp
// Good: the Variable raises onMasterVolumeChanged by itself
public void OnVolumeSliderChanged(float value)
{
    masterVolume.Value = value;
}

// Bad: fires the channel twice
public void OnVolumeSliderChanged(float value)
{
    masterVolume.Value = value;
    onMasterVolumeChanged.RaiseEvent(value);
}
```

Wire the channel asset into the Variable asset's `onValueChanged` field once, in the Inspector. Subscribers then listen on that channel asset and bootstrap the initial value from the Variable:

```csharp
private void OnEnable()
{
    onMasterVolumeChanged.OnEventRaised += ApplyVolume;
    ApplyVolume(masterVolume.Value);  // initial-value bootstrap
}
```

This keeps consumers coupled to a channel asset rather than to the Variable, while the Variable remains the single source of truth for the value.

> If a change must be announced *without* a Variable behind it — a transient event with no queryable state — raise a standalone EventChannel directly. The rule above applies only to a channel that is wired into a Variable's `onValueChanged`.

---

## References

- [event-channels.md](event-channels.md) - EventChannel patterns and types
- [architecture.md](architecture.md) - ScriptableObject patterns overview
- [dependency-management.md](dependency-management.md) - Dependency priority

---

## Checklist

- [ ] Consumers subscribe to the channel asset wired into the Variable's `onValueChanged`, not to the Variable object
- [ ] The writer only assigns `Value` — it never also raises that Variable's paired channel, which would fire it twice
- [ ] Subscribers bootstrap the initial value from the Variable in `OnEnable`, then listen on the channel for changes
- [ ] Session-scoped Variables call `ResetToInitial()` at startup so stale editor-session values cannot leak in
- [ ] GPU-synced shader property names start with underscore (`_PlayerHealth`)
- [ ] Naming: Variable assets are nouns (playerScore), paired channels are `on + PastTense` (onScoreChanged)

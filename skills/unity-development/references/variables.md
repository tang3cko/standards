# Variable details

## Built-in Variable types

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

## Variables vs EventChannels

| Use Variables when | Use EventChannels when |
|-------------------|------------------------|
| State needs to persist | State is not required |
| Current value needs to be read | One-time notification is sufficient |
| Value changes require auto-notification | Value does not need to be stored |

## GPU Sync feature

Variables can automatically sync to shader global properties.

### Setup

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

## Value change detection

Variables use `EqualityComparer<T>` to detect changes:

```csharp
playerScore.Value = 100;  // Raises event
playerScore.Value = 100;  // Does NOT raise event (same value)
playerScore.Value = 150;  // Raises event
```

## Anti-patterns

### Direct Variable subscription

```csharp
// NG
playerScore.OnValueChanged.OnEventRaised += UpdateUI;

// OK
[SerializeField] private IntEventChannelSO onScoreChanged;
onScoreChanged.OnEventRaised += UpdateUI;
```

### Ignoring initial value reset

```csharp
// NG: Previous value may persist
private void Start()
{
    playerScore.Value += 10;
}

// OK
private void Start()
{
    playerScore.ResetToInitial();
}
```

## Naming conventions

- **Variable assets:** Noun form (playerScore, playerHealth, isGamePaused)
- **EventChannel assets:** `on + PastTense` form (onScoreChanged, onHealthChanged)

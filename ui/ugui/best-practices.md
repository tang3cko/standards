# uGUI Best Practices

## Purpose

Define performance optimization techniques and best practices for uGUI implementation to ensure efficient World Space UI with EventChannel integration.

## Checklist

- [ ] Use EventChannels for all UI state changes
- [ ] Subscribe in OnEnable, unsubscribe in OnDisable
- [ ] Use TextMeshPro instead of Unity Text
- [ ] Cache Camera.main in Start
- [ ] Use Canvas.enabled for show/hide (not Destroy)
- [ ] Update UI only when EventChannels fire (not in Update)
- [ ] Implement OnValidate for Inspector validation
- [ ] Use World Space Canvas for 3D UI only
- [ ] Apply camera billboard in LateUpdate
- [ ] Ensure EventSystem exists in scene

---

## EventChannel Integration

### Always Use EventChannels

```csharp
// ✅ Good: EventChannel-driven updates
namespace ProjectName.UI
{
    public class HealthUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI healthText;

        private void OnEnable()
        {
            onHealthChanged.OnEventRaised += UpdateHealth;
        }

        private void OnDisable()
        {
            onHealthChanged.OnEventRaised -= UpdateHealth;
        }

        private void UpdateHealth(float health)
        {
            healthText.text = $"HP: {health:F0}";
        }
    }
}

// ❌ Bad: Direct dependency on business logic
public class HealthUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private void Update()
    {
        healthText.text = $"HP: {playerHealth.CurrentHealth:F0}";
    }
}
```

### Always Unsubscribe in OnDisable

```csharp
// ✅ Good: Proper subscription management
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}

private void OnDisable()
{
    // REQUIRED: Prevent memory leaks
    onHealthChanged.OnEventRaised -= UpdateHealth;
}

// ❌ Bad: Never unsubscribes (memory leak)
private void Start()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}
```

---

## Performance Optimization

### Avoid Update Loop UI Updates

```csharp
// ✅ Good: EventChannel-driven (efficient)
private void OnEnable()
{
    onScoreChanged.OnEventRaised += UpdateScore;
}

private void UpdateScore(int score)
{
    scoreText.text = $"Score: {score}";
}

// ❌ Bad: Update every frame (expensive)
private void Update()
{
    scoreText.text = $"Score: {gameManager.Score}";
    // Canvas rebuild every frame!
}
```

### Cache Camera Reference

```csharp
// ✅ Good: Cache in Start
private Camera mainCamera;

private void Start()
{
    mainCamera = Camera.main;
}

private void LateUpdate()
{
    if (mainCamera != null)
        ApplyBillboard();
}

// ❌ Bad: Query every frame (expensive)
private void LateUpdate()
{
    Camera cam = Camera.main; // FindObjectOfType every frame!
    ApplyBillboard();
}
```

### Use Canvas.enabled for Show/Hide

```csharp
// ✅ Good: Enable/disable canvas
private void ShowUI()
{
    canvas.enabled = true;
}

private void HideUI()
{
    canvas.enabled = false;
}

// ❌ Bad: Destroy and recreate (very expensive)
private void HideUI()
{
    Destroy(gameObject);
}

private void ShowUI()
{
    Instantiate(uiPrefab);
}
```

### Minimize Canvas Rebuilds

```csharp
// ✅ Good: Update only necessary elements
private void UpdateHealth(float health)
{
    healthText.text = $"{health:F0}";
    // Only healthText triggers Canvas rebuild
}

// ❌ Bad: Update all UI elements
private void Update()
{
    healthText.text = $"{playerHealth.CurrentHealth:F0}";
    manaText.text = $"{playerMana.CurrentMana:F0}";
    staminaText.text = $"{playerStamina.CurrentStamina:F0}";
    // Triggers multiple Canvas rebuilds per frame
}
```

---

## TextMeshPro Usage

### Always Use TextMeshPro

```csharp
// ✅ Good: TextMeshPro
using TMPro;

[SerializeField] private TextMeshProUGUI nameText;

// ❌ Bad: Unity Text (deprecated)
using UnityEngine.UI;

[SerializeField] private Text nameText;
```

### Why TextMeshPro?

| Feature | TextMeshPro | Unity Text |
|---------|-------------|------------|
| **Quality** | High (SDF rendering) | Low (bitmap) |
| **Performance** | Better (efficient batching) | Worse |
| **Rich Text** | Full support | Limited |
| **Effects** | Outline, shadow, gradient | Basic |
| **Status** | Recommended | Deprecated |

---

## Canvas Configuration

### World Space Setup

```csharp
// ✅ Good: Proper World Space setup
private void Start()
{
    if (canvas != null)
    {
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
    }
}

// ❌ Bad: Missing worldCamera assignment
private void Start()
{
    canvas.renderMode = RenderMode.WorldSpace;
    // worldCamera not assigned - UI won't render correctly
}
```

### Canvas Scaler (Screen Space Only)

Canvas Scaler is only relevant for Screen Space UI (not World Space):

```csharp
// World Space UI: Canvas Scaler not needed
// Screen Space UI: Use UI Toolkit instead
```

---

## Inspector Validation

### Use OnValidate

```csharp
#if UNITY_EDITOR
private void OnValidate()
{
    // Validate EventChannels
    if (onHealthChanged == null)
        Debug.LogWarning($"[{GetType().Name}] onHealthChanged is not assigned on {gameObject.name}.", this);

    // Validate UI references
    if (healthText == null)
        Debug.LogWarning($"[{GetType().Name}] healthText is not assigned on {gameObject.name}.", this);

    if (canvas == null)
        Debug.LogWarning($"[{GetType().Name}] canvas is not assigned on {gameObject.name}.", this);
}
#endif
```

### Benefits

- Catches missing references at edit time
- Prevents runtime errors
- Improves team collaboration
- Visible warnings in Inspector

---

## EventSystem Requirements

### Ensure EventSystem Exists

uGUI requires an EventSystem in the scene for button clicks and interactions:

```
Hierarchy:
├── Canvas
│   └── Button
└── EventSystem (required for interactions)
```

### Check in Code

```csharp
private void Start()
{
    // Check if EventSystem exists
    if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
    {
        Debug.LogError("[UIManager] EventSystem not found in scene. UI interactions will not work.");
    }
}
```

---

## Button to EventChannel Pattern

### Convert Button Events

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    public class ButtonEventChannel : MonoBehaviour
    {
        [Header("Event Channels - Output")]
        [SerializeField] private VoidEventChannelSO onButtonPressed;

        [Header("UI References")]
        [SerializeField] private Button button;

        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(HandleButtonClick);
            }
        }

        private void HandleButtonClick()
        {
            onButtonPressed?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onButtonPressed == null)
                Debug.LogWarning($"[ButtonEventChannel] onButtonPressed is not assigned.", this);

            if (button == null)
                Debug.LogWarning($"[ButtonEventChannel] button is not assigned.", this);
        }
#endif
    }
}
```

---

## Common Pitfalls

### Pitfall 1: Forgetting to Unsubscribe

```csharp
// ❌ Bad: Memory leak
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}
// OnDisable missing!

// ✅ Good: Proper cleanup
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}

private void OnDisable()
{
    onHealthChanged.OnEventRaised -= UpdateHealth;
}
```

### Pitfall 2: Querying Camera Every Frame

```csharp
// ❌ Bad: Camera.main is expensive
private void LateUpdate()
{
    Camera cam = Camera.main;
    ApplyBillboard(cam);
}

// ✅ Good: Cache reference
private Camera mainCamera;

private void Start()
{
    mainCamera = Camera.main;
}

private void LateUpdate()
{
    ApplyBillboard(mainCamera);
}
```

### Pitfall 3: Using Update for UI Updates

```csharp
// ❌ Bad: Canvas rebuild every frame
private void Update()
{
    timerText.text = $"{Time.time:F2}";
}

// ✅ Good: EventChannel-driven
[SerializeField] private FloatEventChannelSO onTimerTick;

private void OnEnable()
{
    onTimerTick.OnEventRaised += UpdateTimer;
}

private void UpdateTimer(float time)
{
    timerText.text = $"{time:F2}";
}
```

### Pitfall 4: Missing EventSystem

```
Scene without EventSystem:
├── Canvas
│   └── Button (doesn't work!)

Scene with EventSystem:
├── Canvas
│   └── Button (works!)
└── EventSystem
```

---

## Networking Considerations

### Mirror Integration

When integrating with Mirror Networking:

```csharp
using Mirror;

namespace ProjectName.UI
{
    public class PlayerNameLabel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameText;

        /// <summary>
        /// Called from NetworkBehaviour SyncVar hook
        /// </summary>
        public void SetPlayerName(string playerName)
        {
            if (nameText != null)
            {
                nameText.text = playerName;
            }
        }
    }
}
```

### EventChannels are Local

EventChannels do not propagate across the network. They are local events only:

```csharp
// ✅ EventChannel fires locally
onPlayerDamaged?.RaiseEvent(damage);
// Only this client receives the event

// For network events, use Mirror's ClientRpc
[ClientRpc]
private void RpcShowDamage(int damage)
{
    onPlayerDamaged?.RaiseEvent(damage);
}
```

---

## Complete Example

```csharp
using UnityEngine;
using TMPro;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Player HP bar with best practices
    /// </summary>
    public class PlayerHPBarUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Settings")]
        [SerializeField] private bool enableBillboard = true;

        private Camera mainCamera;

        private void Start()
        {
            // Cache camera reference
            mainCamera = Camera.main;

            // Configure Canvas
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
            }
        }

        private void OnEnable()
        {
            // Subscribe to EventChannel
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised += UpdateHealth;
        }

        private void OnDisable()
        {
            // Unsubscribe (prevent memory leaks)
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised -= UpdateHealth;
        }

        private void LateUpdate()
        {
            // Camera billboard (only when visible)
            if (enableBillboard && mainCamera != null && canvas.enabled)
            {
                transform.LookAt(
                    transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }

        private void UpdateHealth(float health)
        {
            if (healthText != null)
            {
                healthText.text = $"HP: {health:F0}";
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[PlayerHPBarUI] onHealthChanged is not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[PlayerHPBarUI] canvas is not assigned on {gameObject.name}.", this);

            if (healthText == null)
                Debug.LogWarning($"[PlayerHPBarUI] healthText is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## References

- [World Space UI](world-space-ui.md)
- [Billboard Pattern](billboard.md)
- [Event Channels](../../architecture/event-channels.md)
- [Performance Optimization](../../core/performance.md)

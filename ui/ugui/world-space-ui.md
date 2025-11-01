# World Space UI (uGUI)

## Purpose

Define implementation patterns for uGUI World Space Canvas to display UI elements in 3D space (player name plates, interaction prompts, HP bars) while maintaining EventChannel-driven architecture.

## Checklist

- [ ] Use uGUI only for World Space UI (not Screen Space)
- [ ] Set Canvas.renderMode to RenderMode.WorldSpace
- [ ] Subscribe to EventChannels in OnEnable
- [ ] Unsubscribe from EventChannels in OnDisable
- [ ] Use TextMeshPro instead of Unity Text
- [ ] Include OnValidate for Inspector validation
- [ ] Convert button clicks to EventChannel events
- [ ] Use Canvas.enabled for show/hide (not Destroy)

---

## When to Use uGUI

### Recommended Use Cases

- **Player name plates** - World space text above players
- **NPC name plates** - Character names in 3D space
- **Interaction prompts** - "Press E to interact" messages
- **Damage popups** - Floating damage numbers
- **Enemy HP bars** - Health bars above enemies
- **World space markers** - Quest objectives, waypoints

### Not Recommended

- **Screen space menus** - Use UI Toolkit instead
- **HUD elements** - Use UI Toolkit instead
- **Settings panels** - Use UI Toolkit instead

---

## Basic Template

### uGUI EventChannel Template

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Basic uGUI template with EventChannel integration
    ///
    /// Responsibilities:
    /// - UI display control only
    /// - Subscribe to EventChannels for state changes
    /// - No direct dependencies on business logic
    /// </summary>
    public class ExampleUI : MonoBehaviour
    {
        [Header("Event Channels - Input")]
        [Tooltip("EventChannel to subscribe to")]
        [SerializeField] private VoidEventChannelSO onSomeEvent;

        [Header("Event Channels - Output")]
        [Tooltip("EventChannel to raise")]
        [SerializeField] private VoidEventChannelSO onButtonClicked;

        [Header("UI References")]
        [SerializeField] private Button someButton;
        [SerializeField] private TextMeshProUGUI someText;

        private void OnEnable()
        {
            // Subscribe to EventChannels
            if (onSomeEvent != null)
                onSomeEvent.OnEventRaised += HandleSomeEvent;
        }

        private void OnDisable()
        {
            // Unsubscribe from EventChannels (required)
            if (onSomeEvent != null)
                onSomeEvent.OnEventRaised -= HandleSomeEvent;
        }

        private void Start()
        {
            // Convert button events to EventChannels
            if (someButton != null)
                someButton.onClick.AddListener(HandleButtonClick);
        }

        private void HandleSomeEvent()
        {
            // Update UI
            someText.text = "Updated!";
        }

        private void HandleButtonClick()
        {
            // Raise EventChannel
            onButtonClicked?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Validate EventChannels and UI references
            if (onSomeEvent == null)
                Debug.LogWarning($"[{GetType().Name}] onSomeEvent is not assigned on {gameObject.name}.", this);

            if (someButton == null)
                Debug.LogWarning($"[{GetType().Name}] someButton is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## World Space Canvas Setup

### Canvas Configuration

```csharp
using UnityEngine;

namespace ProjectName.UI
{
    public class WorldSpaceUI : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            // Configure Canvas for World Space
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
            }
        }
    }
}
```

### Canvas Hierarchy

```
GameObject (World Space UI)
├── Canvas (RenderMode: World Space)
│   └── TextMeshPro - Text (Player Name)
└── WorldSpaceUIController.cs
```

---

## Interaction Prompt Example

```csharp
using UnityEngine;
using TMPro;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Interaction prompt display (World Space UI)
    ///
    /// Responsibilities:
    /// - Display "Press E to interact" prompts
    /// - Subscribe to EventChannel for interactable state
    /// - Show/hide based on player proximity
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [Tooltip("Interactable state change event")]
        [SerializeField] private BoolEventChannelSO onInteractableStateChanged;

        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private TextMeshProUGUI promptText;

        [Header("Settings")]
        [SerializeField] private string promptMessage = "Press E to interact";

        private void Start()
        {
            // Initial state: hidden
            HidePrompt();
        }

        private void OnEnable()
        {
            if (onInteractableStateChanged != null)
                onInteractableStateChanged.OnEventRaised += HandleInteractableStateChanged;
        }

        private void OnDisable()
        {
            if (onInteractableStateChanged != null)
                onInteractableStateChanged.OnEventRaised -= HandleInteractableStateChanged;
        }

        private void HandleInteractableStateChanged(bool isInteractable)
        {
            if (isInteractable)
                ShowPrompt();
            else
                HidePrompt();
        }

        private void ShowPrompt()
        {
            if (promptText != null)
            {
                promptText.text = promptMessage;
                canvas.enabled = true;
            }
        }

        private void HidePrompt()
        {
            if (canvas != null)
            {
                canvas.enabled = false;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onInteractableStateChanged == null)
                Debug.LogWarning($"[InteractionPromptUI] onInteractableStateChanged is not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[InteractionPromptUI] canvas is not assigned on {gameObject.name}.", this);

            if (promptText == null)
                Debug.LogWarning($"[InteractionPromptUI] promptText is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Health Bar Example

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Enemy health bar (World Space UI)
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image fillImage;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private void Start()
        {
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = Camera.main;
            }

            // Initialize to full health
            UpdateHealthBar(maxHealth);
        }

        private void OnEnable()
        {
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised += UpdateHealthBar;
        }

        private void OnDisable()
        {
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised -= UpdateHealthBar;
        }

        private void UpdateHealthBar(float currentHealth)
        {
            if (fillImage == null) return;

            float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
            fillImage.fillAmount = fillAmount;

            // Color interpolation based on health
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount);

            // Hide health bar if at full health
            canvas.enabled = fillAmount < 1f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[HealthBarUI] onHealthChanged is not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[HealthBarUI] canvas is not assigned on {gameObject.name}.", this);

            if (fillImage == null)
                Debug.LogWarning($"[HealthBarUI] fillImage is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Button Event to EventChannel

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Convert uGUI button clicks to EventChannels
    /// </summary>
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
            // Raise EventChannel
            onButtonPressed?.RaiseEvent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onButtonPressed == null)
                Debug.LogWarning($"[ButtonEventChannel] onButtonPressed is not assigned on {gameObject.name}.", this);

            if (button == null)
                Debug.LogWarning($"[ButtonEventChannel] button is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Show/Hide Pattern

### Good: Use Canvas.enabled

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
```

### Bad: Destroy GameObject

```csharp
// ❌ Bad: Destroying and recreating is expensive
private void HideUI()
{
    Destroy(gameObject);
}
```

---

## Performance Considerations

### Minimize Canvas Rebuilds

```csharp
// ✅ Good: Update only when EventChannel fires
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealthBar;
}

private void UpdateHealthBar(float health)
{
    healthText.text = $"{health:F0}";
}

// ❌ Bad: Update every frame
private void Update()
{
    healthText.text = $"{enemy.CurrentHealth:F0}";
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

// ❌ Bad: Query every frame
private void Update()
{
    Camera cam = Camera.main; // Expensive!
}
```

---

## References

- [Billboard Pattern](billboard.md)
- [Best Practices](best-practices.md)
- [Event Channels](../../architecture/event-channels.md)

# Camera Billboard Pattern

## Purpose

Implement camera billboard behavior for World Space UI elements to ensure they always face the camera, improving readability for player names, HP bars, and interaction prompts.

## Checklist

- [ ] Implement billboard in LateUpdate (not Update)
- [ ] Cache Camera.main reference in Start
- [ ] Use transform.LookAt with camera rotation
- [ ] Add enableBillboard toggle for flexibility
- [ ] Only update when canvas is enabled
- [ ] Null-check camera reference before using
- [ ] Use for name plates, HP bars, and prompts

---

## When to Use Billboard

### Recommended Use Cases

- **Player name plates** - Always readable text above players
- **NPC name plates** - Character names in 3D space
- **Interaction prompts** - "Press E" messages
- **Enemy HP bars** - Health bars above enemies
- **Floating damage numbers** - Damage popups

### When Not to Use

- **Static UI elements** - Signs, billboards in-world
- **Directional indicators** - Arrows pointing to objectives
- **Screen space UI** - Use UI Toolkit instead

---

## Basic Billboard Pattern

### Implementation

```csharp
using UnityEngine;

namespace ProjectName.UI
{
    /// <summary>
    /// Basic camera billboard for World Space UI
    /// </summary>
    public class CameraBillboard : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Enable camera billboard behavior")]
        [SerializeField] private bool enableBillboard = true;

        private Camera mainCamera;

        private void Start()
        {
            // Cache camera reference
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogWarning($"[CameraBillboard] Main camera not found on {gameObject.name}.", this);
            }
        }

        private void LateUpdate()
        {
            // Billboard: Always face camera
            if (enableBillboard && mainCamera != null)
            {
                transform.LookAt(
                    transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }
    }
}
```

### Why LateUpdate?

Use `LateUpdate` instead of `Update` to ensure billboard happens after camera movement:

```csharp
// ✅ Good: Billboard after camera moves
private void LateUpdate()
{
    ApplyBillboard();
}

// ❌ Bad: Billboard before camera moves (jittery)
private void Update()
{
    ApplyBillboard();
}
```

---

## Player Name Plate Example

```csharp
using UnityEngine;
using TMPro;

namespace ProjectName.UI
{
    /// <summary>
    /// Player name display above player (World Space UI)
    ///
    /// Responsibilities:
    /// - Display player name
    /// - Camera billboard (always face camera)
    /// - Integration with networking (SyncVar monitoring)
    /// </summary>
    public class PlayerNameLabel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private Canvas canvas;

        [Header("Settings")]
        [Tooltip("Enable camera billboard")]
        [SerializeField] private bool enableBillboard = true;

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

        private void LateUpdate()
        {
            // Camera billboard (always face camera)
            if (enableBillboard && mainCamera != null)
            {
                transform.LookAt(
                    transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }

        /// <summary>
        /// Set player name
        /// Called from NetworkBehaviour SyncVar hook or initialization
        /// </summary>
        public void SetPlayerName(string playerName)
        {
            if (nameText != null)
            {
                nameText.text = playerName;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (nameText == null)
                Debug.LogWarning($"[PlayerNameLabel] nameText is not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[PlayerNameLabel] canvas is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Interaction Prompt with Billboard

```csharp
using UnityEngine;
using TMPro;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Interaction prompt display with camera billboard (World Space UI)
    ///
    /// Responsibilities:
    /// - Display "Press E to interact" prompts
    /// - Subscribe to EventChannel for interactable state
    /// - Camera billboard for readability
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
        [SerializeField] private bool enableBillboard = true;

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

## Health Bar with Billboard

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.EventChannels;

namespace ProjectName.UI
{
    /// <summary>
    /// Enemy health bar with camera billboard
    /// </summary>
    public class HealthBarBillboard : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private Image fillImage;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool enableBillboard = true;
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color lowHealthColor = Color.red;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
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
                Debug.LogWarning($"[HealthBarBillboard] onHealthChanged is not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[HealthBarBillboard] canvas is not assigned on {gameObject.name}.", this);

            if (fillImage == null)
                Debug.LogWarning($"[HealthBarBillboard] fillImage is not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Performance Optimization

### Conditional Billboard Update

Only apply billboard when canvas is visible:

```csharp
// ✅ Good: Only billboard when visible
private void LateUpdate()
{
    if (enableBillboard && mainCamera != null && canvas.enabled)
    {
        ApplyBillboard();
    }
}

// ❌ Bad: Always billboard even when hidden
private void LateUpdate()
{
    if (enableBillboard && mainCamera != null)
    {
        ApplyBillboard();
    }
}
```

### Distance-Based Culling

Disable billboard for far-away objects:

```csharp
[Header("Settings")]
[SerializeField] private bool enableBillboard = true;
[SerializeField] private float maxBillboardDistance = 50f;

private void LateUpdate()
{
    if (!enableBillboard || mainCamera == null || !canvas.enabled)
        return;

    // Distance check
    float distance = Vector3.Distance(transform.position, mainCamera.transform.position);

    if (distance > maxBillboardDistance)
    {
        canvas.enabled = false;
        return;
    }

    // Apply billboard
    transform.LookAt(
        transform.position + mainCamera.transform.rotation * Vector3.forward,
        mainCamera.transform.rotation * Vector3.up
    );
}
```

---

## Alternative: Constraints Component

Unity's Look At Constraint can replace code-based billboard:

### Setup

1. Add `Look At Constraint` component to UI GameObject
2. Set Source: Main Camera
3. Configure:
   - `Weight`: 1
   - `Lock`: None
   - `Rotation Offset`: (0, 180, 0) if UI appears backwards

### Pros and Cons

| Approach | Pros | Cons |
|----------|------|------|
| **Code (LateUpdate)** | Full control, conditional logic, easy to debug | Requires code maintenance |
| **Constraint Component** | No code needed, visual setup | Less flexible, harder to add conditions |

**Recommendation:** Use LateUpdate for most cases to maintain full control and EventChannel integration.

---

## Common Issues

### Issue 1: UI Appears Backwards

```csharp
// ❌ Bad: UI is backwards
transform.LookAt(mainCamera.transform.position);

// ✅ Good: UI faces camera correctly
transform.LookAt(
    transform.position + mainCamera.transform.rotation * Vector3.forward,
    mainCamera.transform.rotation * Vector3.up
);
```

### Issue 2: Jittery Movement

```csharp
// ❌ Bad: Update before camera moves
private void Update()
{
    ApplyBillboard();
}

// ✅ Good: Update after camera moves
private void LateUpdate()
{
    ApplyBillboard();
}
```

### Issue 3: Missing Camera Reference

```csharp
// ✅ Good: Null check
private void LateUpdate()
{
    if (mainCamera == null)
    {
        mainCamera = Camera.main;
        return;
    }

    ApplyBillboard();
}
```

---

## References

- [World Space UI](world-space-ui.md)
- [Best Practices](best-practices.md)
- [Event Channels](../../architecture/event-channels.md)

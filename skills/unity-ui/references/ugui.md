# uGUI (World Space UI)

## When to use uGUI

### Recommended use cases

- Player name plates
- NPC name plates
- Interaction prompts ("Press E to interact")
- Damage popups
- Enemy HP bars
- World space markers

### Not recommended

- Screen space menus → Use UI Toolkit
- HUD elements → Use UI Toolkit
- Settings panels → Use UI Toolkit

---

## Basic template

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class WorldSpaceUI : MonoBehaviour
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
            mainCamera = Camera.main;

            if (canvas != null)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.worldCamera = mainCamera;
            }
        }

        private void OnEnable()
        {
            if (onHealthChanged != null)
                onHealthChanged.OnEventRaised += UpdateHealth;
        }

        private void OnDisable()
        {
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
                healthText.text = $"HP: {health:F0}";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[{GetType().Name}] onHealthChanged not assigned on {gameObject.name}.", this);

            if (canvas == null)
                Debug.LogWarning($"[{GetType().Name}] canvas not assigned on {gameObject.name}.", this);
        }
#endif
    }
}
```

---

## Camera billboard pattern

### Why LateUpdate?

Use `LateUpdate` to ensure billboard happens after camera movement:

```csharp
// Good: Billboard after camera moves
private void LateUpdate()
{
    ApplyBillboard();
}

// Bad: Billboard before camera moves (jittery)
private void Update()
{
    ApplyBillboard();
}
```

### Billboard implementation

```csharp
private void LateUpdate()
{
    if (enableBillboard && mainCamera != null && canvas.enabled)
    {
        transform.LookAt(
            transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up
        );
    }
}
```

### Common issues

```csharp
// Bad: UI appears backwards
transform.LookAt(mainCamera.transform.position);

// Good: UI faces camera correctly
transform.LookAt(
    transform.position + mainCamera.transform.rotation * Vector3.forward,
    mainCamera.transform.rotation * Vector3.up
);
```

---

## Performance optimization

### Cache camera reference

```csharp
// Good: Cache in Start
private Camera mainCamera;

private void Start()
{
    mainCamera = Camera.main;
}

// Bad: Query every frame
private void LateUpdate()
{
    Camera cam = Camera.main; // Expensive!
}
```

### Use Canvas.enabled for show/hide

```csharp
// Good: Enable/disable canvas
private void ShowUI() => canvas.enabled = true;
private void HideUI() => canvas.enabled = false;

// Bad: Destroy and recreate (very expensive)
private void HideUI() => Destroy(gameObject);
private void ShowUI() => Instantiate(uiPrefab);
```

### Event-driven updates

```csharp
// Good: Update only when EventChannel fires
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}

private void UpdateHealth(float health)
{
    healthText.text = $"HP: {health:F0}";
}

// Bad: Update every frame
private void Update()
{
    healthText.text = $"HP: {enemy.CurrentHealth:F0}";
}
```

### Conditional billboard

```csharp
// Good: Only billboard when visible
private void LateUpdate()
{
    if (enableBillboard && mainCamera != null && canvas.enabled)
    {
        ApplyBillboard();
    }
}

// Bad: Always billboard even when hidden
private void LateUpdate()
{
    if (enableBillboard && mainCamera != null)
    {
        ApplyBillboard();
    }
}
```

---

## TextMeshPro

### Always use TextMeshPro

```csharp
// Good
using TMPro;
[SerializeField] private TextMeshProUGUI nameText;

// Bad (deprecated)
using UnityEngine.UI;
[SerializeField] private Text nameText;
```

### Why TextMeshPro?

| Feature | TextMeshPro | Unity Text |
|---------|-------------|------------|
| Quality | High (SDF) | Low (bitmap) |
| Performance | Better | Worse |
| Rich Text | Full | Limited |
| Status | Recommended | Deprecated |

---

## Health bar example

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class HealthBarUI : MonoBehaviour
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
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, fillAmount);

            // Hide at full health
            canvas.enabled = fillAmount < 1f;
        }
    }
}
```

---

## Common pitfalls

### Forgetting to unsubscribe

```csharp
// Bad: Memory leak
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealth;
}
// OnDisable missing!

// Good
private void OnDisable()
{
    onHealthChanged.OnEventRaised -= UpdateHealth;
}
```

### Missing EventSystem

```
Scene without EventSystem:
├── Canvas
│   └── Button (doesn't work!)

Scene with EventSystem:
├── Canvas
│   └── Button (works!)
└── EventSystem
```

# uGUI (World Space UI)

Tang3cko's uGUI conventions for the one case uGUI is still used: world-space UI on projects that cannot use UI Toolkit world-space panels.

Canvas setup, CanvasScaler, Layout Groups, RectTransform anchoring, ScrollRect, TextMeshPro essentials import, and interaction readiness (EventSystem, GraphicRaycaster, Raycast Target) are covered by Unity's official `ui-ugui` skill (Unity-Technologies/skills). Do not restate them here. This file holds only what Tang3cko decides differently or additionally, and where the two disagree, this file wins.

> **Version note**: Unity 6.2+ supports World Space UI natively in UI Toolkit (Panel Renderer in 6.5 improves it further). On 6.2+ projects, prefer UI Toolkit for new world-space UI — use this file for projects pinned to Unity 6000.0/6000.1 or when extending existing uGUI systems.

---

## Overrides of the official `ui-ugui` defaults

| Topic | Official `ui-ugui` default | Tang3cko |
|-------|----------------------------|----------|
| Default runtime system | uGUI when the user has no preference | **UI Toolkit**; uGUI only for the cases below |
| Prefab layout | Feature folders (`Assets/UI/Inventory/`) | `Assets/_Project/Prefabs/UI/<Feature>/`, scripts under `Assets/_Project/Scripts/UI/<Feature>/` |
| Updating UI state | Not prescribed | EventChannel subscription in `OnEnable`/`OnDisable`; never per-frame polling |

---

## When to Use uGUI

### Recommended use cases (Unity 6000.0/6000.1, or existing uGUI systems)

- Player name plates
- NPC name plates
- Interaction prompts ("Press E to interact")
- Damage popups
- Enemy HP bars
- World space markers

### Not recommended

- Screen space menus -> Use UI Toolkit
- HUD elements -> Use UI Toolkit
- Settings panels -> Use UI Toolkit
- New world-space UI on Unity 6.2+ -> Use UI Toolkit world-space panels

---

## World Space Template

`Camera.main` cached once in `Start`; billboard in `LateUpdate` with the rotation-based form; show/hide via `Canvas.enabled`; state via EventChannel; text via `TextMeshProUGUI`.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI healthText;

        [Header("Settings")]
        [SerializeField] private float maxHealth = 100f;
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
            UpdateHealth(maxHealth);
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
            if (enableBillboard && mainCamera != null && canvas.enabled)
            {
                transform.LookAt(
                    transform.position + mainCamera.transform.rotation * Vector3.forward,
                    mainCamera.transform.rotation * Vector3.up
                );
            }
        }

        private void UpdateHealth(float currentHealth)
        {
            float fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
            if (fillImage != null)
                fillImage.fillAmount = fillAmount;
            if (healthText != null)
                healthText.text = $"HP: {currentHealth:F0}";
            canvas.enabled = fillAmount < 1f;
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

## Billboard Pattern

Billboard in `LateUpdate`, after the camera has moved for the frame; doing it in `Update` jitters.

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

## Show / Hide

```csharp
// Good: Enable/disable canvas
private void ShowUI() => canvas.enabled = true;
private void HideUI() => canvas.enabled = false;

// Bad: Destroy and recreate (very expensive)
private void HideUI() => Destroy(gameObject);
```

---

## References

- Official `ui-ugui` skill (Unity-Technologies/skills) — Canvas, CanvasScaler, Layout Groups, anchoring, ScrollRect, TMP essentials, interaction readiness
- [ui-toolkit.md](ui-toolkit.md) - UI Toolkit for screen-space UI
- [accessibility.md](accessibility.md) - Font size requirements
- See `unity-architecture` skill for EventChannel integration
- See `unity-async` skill for caching and event-driven patterns

---

## Checklist

Run against every uGUI file written or reviewed with this reference loaded:

- [ ] uGUI is actually warranted here (Unity 6000.0/6000.1 world space, or extending an existing uGUI system) — otherwise use UI Toolkit
- [ ] `Camera.main` is cached once in `Start`, never queried per frame
- [ ] Billboarding runs in `LateUpdate` using the rotation-based form, not `LookAt(camera.position)`
- [ ] Every `OnEnable` EventChannel subscription has a matching `OnDisable` unsubscription
- [ ] Show/hide uses `Canvas.enabled`, not Destroy/Instantiate; updates are event-driven, not per-frame polling
- [ ] Text uses `TextMeshProUGUI` (not `UnityEngine.UI.Text`); EventSystem/GraphicRaycaster/Raycast Target per the official `ui-ugui` interaction-readiness list

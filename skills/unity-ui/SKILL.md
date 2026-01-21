---
name: unity-ui
description: Unity UI implementation. UI Toolkit (UXML, USS, BEM naming, design tokens), uGUI (World Space, billboard), accessibility (font sizes, contrast). Use when creating or styling UI in Unity.
---

# Unity UI

UI implementation patterns for UI Toolkit and uGUI.

---

## Framework decision - P1

```
What UI are you building?
│
├─▶ Screen Space UI (menus, HUD, settings)
│   │
│   └─▶ Use UI Toolkit (UXML + USS)
│
└─▶ World Space UI (name plates, HP bars, prompts)
    │
    └─▶ Use uGUI (Canvas + TextMeshPro)
```

| Framework | Use Case | Render Mode |
|-----------|----------|-------------|
| **UI Toolkit** | Menus, HUD, settings panels | Screen Space |
| **uGUI** | Name plates, HP bars, interaction prompts | World Space |

---

## BEM naming - P1

```css
/* Block: Component name */
.quest-result { }

/* Element: Part of block (double underscore) */
.quest-result__title { }
.quest-result__status { }

/* Modifier: State variation (double dash) */
.quest-result__status--success { }
.quest-result__status--failed { }
```

```xml
<!-- Always include both base and modifier -->
<ui:Label class="quest-result__status quest-result__status--success" />
```

See [references/ui-toolkit.md](references/ui-toolkit.md) for complete BEM guide.

---

## Design tokens - P1

Define all design values in `Common.uss` `:root`:

```css
:root {
    /* Colors */
    --color-primary: #FFD700;
    --color-success: #32CD32;
    --color-danger: #FF6347;
    --color-text: #FFFFFF;
    --color-text-muted: rgba(255, 255, 255, 0.5);

    /* Font Sizes (PC minimum 18px, Console minimum 26px) */
    --font-size-large: 24px;
    --font-size-medium: 18px;
    --font-size-small: 18px;

    /* Spacing */
    --padding-large: 16px;
    --padding-medium: 12px;
    --padding-small: 8px;
}
```

```css
/* Usage */
.quest-title {
    color: var(--color-primary);
    font-size: var(--font-size-large);
    padding: var(--padding-medium);
}
```

---

## Panel Settings - P1

Configure for responsive scaling:

```yaml
m_ScaleMode: 2                    # Scale With Screen Size
m_ReferenceResolution: {x: 1920, y: 1080}
m_Match: 0                        # Match width (PC landscape)
```

---

## USS Flexbox quick reference - P1

```css
/* Equal distribution */
.column {
    flex-grow: 1;
    flex-basis: 0;     /* Required for true equal distribution */
}

/* Responsive panel */
.panel {
    width: 80%;
    max-width: 1200px;  /* Prevent stretching on ultrawide */
    min-width: 400px;   /* Maintain readability */
}

/* Flexbox alignment */
.container {
    flex-direction: row;         /* row | column */
    justify-content: center;     /* Main axis */
    align-items: center;         /* Cross axis */
}
```

---

## uGUI World Space pattern - P1

```csharp
public class WorldSpaceUI : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private FloatEventChannelSO onHealthChanged;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
    }

    private void OnEnable()
    {
        onHealthChanged.OnEventRaised += UpdateHealth;
    }

    private void OnDisable()
    {
        onHealthChanged.OnEventRaised -= UpdateHealth;
    }

    private void LateUpdate()
    {
        // Camera billboard
        if (mainCamera != null && canvas.enabled)
        {
            transform.LookAt(
                transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up
            );
        }
    }
}
```

See [references/ugui.md](references/ugui.md) for complete patterns.

---

## Font size accessibility - P1

| Platform | 1080p Minimum | 4K Minimum |
|----------|---------------|------------|
| **Console** | **26px** | **52px** |
| **PC/VR** | **18px** | **36px** |

Additional requirements:
- 200% text resizing support
- 4.5:1 contrast ratio
- 1.5x line spacing

See [references/accessibility.md](references/accessibility.md) for full guidelines.

---

## References

| Topic | File | When to Read |
|-------|------|--------------|
| UI Toolkit (UXML, BEM, USS) | [ui-toolkit.md](references/ui-toolkit.md) | Screen Space UI development |
| uGUI (World Space, billboard) | [ugui.md](references/ugui.md) | World Space UI, name plates |
| Accessibility | [accessibility.md](references/accessibility.md) | Font sizes, contrast ratios |

# UI Toolkit

UI Toolkit patterns: BEM naming, design tokens, USS responsive design, and C# controllers.

---

## BEM Naming Convention

### Structure

```css
/* Block: Independent component */
.quest-result { }

/* Element: Part of block, double underscore */
.quest-result__header { }
.quest-result__title { }
.quest-result__status { }

/* Modifier: State variation, double dash */
.quest-result--active { }
.quest-result__status--success { }
.quest-result__status--failed { }
```

### Anti-patterns

```css
/* Bad: Generic global classes */
.panel { }
.title { }
.status { }

/* Good: Component-specific */
.quest-result__panel { }
.quest-result__title { }
.quest-result__status { }

/* Bad: Too deep nesting */
.quest-result__panel__content__stats__item { }

/* Good: Flatten structure */
.quest-result__stat { }
```

### C# modifier toggling

```csharp
public void ShowSuccess()
{
    statusText.RemoveFromClassList("quest-result__status--failed");
    statusText.AddToClassList("quest-result__status--success");
    statusText.text = "SUCCESS";
}
```

---

## UXML Structure

### File template

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <!-- Reference USS files -->
    <Style src="../../USS/Common.uss" />
    <Style src="../../USS/QuestUI.uss" />

    <!-- Root container -->
    <ui:VisualElement name="QuestProgressPanel" class="quest-progress__panel">
        <ui:Label name="QuestNameText" class="quest-progress__title" text="Quest Name" />
        <ui:Label name="ProgressText" class="quest-progress__progress" text="0/5" />
    </ui:VisualElement>
</ui:UXML>
```

### Naming conventions

- **name attribute**: PascalCase (for C# queries)
- **class attribute**: kebab-case with BEM

```xml
<ui:VisualElement name="QuestResultPanel" class="quest-result__panel">
    <ui:Label name="StatusText" class="quest-result__status" />
</ui:VisualElement>
```

### UXML example

```xml
<!-- Always include both base class and modifier -->
<ui:VisualElement class="quest-result__panel">
    <ui:Label class="quest-result__title" />
    <ui:Label class="quest-result__status quest-result__status--success" />
</ui:VisualElement>
```

---

## Design Tokens

### Common.uss structure

```css
:root {
    /* Color Tokens */
    --color-primary: #FFD700;
    --color-primary-hover: #FFA500;
    --color-success: #32CD32;
    --color-danger: #FF6347;
    --color-background: rgba(0, 0, 0, 0.8);
    --color-text: #FFFFFF;
    --color-text-muted: rgba(255, 255, 255, 0.5);

    /* Font Size Tokens */
    --font-size-large: 24px;
    --font-size-medium: 18px;
    --font-size-small: 18px;

    /* Spacing Tokens */
    --padding-large: 16px;
    --padding-medium: 12px;
    --padding-small: 8px;

    /* Border Tokens */
    --border-radius: 4px;
    --border-radius-large: 8px;

    /* Animation Tokens */
    --transition-fast: 0.1s;
    --transition-normal: 0.3s;
}
```

### Token usage

```css
/* Good: Use tokens */
.quest-result__status--success {
    color: var(--color-success);
    font-size: var(--font-size-large);
    margin-bottom: var(--padding-large);
}

/* Bad: Hardcoded values */
.quest-result__status--success {
    color: #32CD32;
    font-size: 24px;
    margin-bottom: 16px;
}
```

### Naming pattern

```
--category-name-variant: value;

Examples:
--color-primary
--color-primary-hover
--font-size-large
--padding-button
--border-radius-large
```

---

## USS Responsive Design

### Panel Settings

```yaml
m_ScaleMode: 2                    # Scale With Screen Size
m_ReferenceResolution: {x: 1920, y: 1080}
m_ScreenMatchMode: 0              # Match Width Or Height
m_Match: 0                        # Match width (PC landscape)
```

### Flexbox patterns

```css
/* Equal distribution */
.column {
    flex-grow: 1;
    flex-basis: 0;     /* Required for true equal distribution */
}

/* Responsive panel */
.quest-board__panel {
    width: 80%;
    max-width: 1200px;
    min-width: 400px;
    height: 80%;
    max-height: 900px;
    align-self: center;
}

/* Flexbox alignment */
.container {
    flex-direction: row;           /* row | column */
    justify-content: center;       /* flex-start | center | flex-end | space-between */
    align-items: center;           /* stretch | flex-start | center | flex-end */
}
```

### Anti-patterns

```css
/* Bad: Fixed pixel sizes */
.quest-board__panel {
    width: 1200px;
}

/* Good: Responsive with constraints */
.quest-board__panel {
    width: 80%;
    max-width: 1200px;
    min-width: 400px;
}

/* Bad: flex-grow without flex-basis */
.column {
    flex-grow: 1;
}

/* Good: Guaranteed equal distribution */
.column {
    flex-grow: 1;
    flex-basis: 0;
}
```

---

## C# Controller Pattern

```csharp
using UnityEngine;
using UnityEngine.UIElements;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class QuestProgressUI : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onProgressUpdated;

        private UIDocument uiDocument;
        private Label progressText;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            var root = uiDocument.rootVisualElement;

            // Query by name attribute
            progressText = root.Q<Label>("ProgressText");

            // Subscribe to EventChannels
            onProgressUpdated.OnEventRaised += HandleProgressUpdated;
        }

        private void OnDisable()
        {
            onProgressUpdated.OnEventRaised -= HandleProgressUpdated;
        }

        private void HandleProgressUpdated(int progress)
        {
            progressText.text = $"{progress}/5";
        }
    }
}
```

### Element query best practices

```csharp
// Good: Type-safe query
Label titleLabel = root.Q<Label>("TitleLabel");
Button okButton = root.Q<Button>("OKButton");

// Bad: Untyped query
VisualElement titleLabel = root.Q("TitleLabel");

// Query with null check
if (progressText == null)
{
    Debug.LogError($"[{GetType().Name}] ProgressText not found in UXML", this);
    return;
}
```

---

## Performance Tips

### Cache element references

```csharp
// Good: Cache in OnEnable
private Label statusText;

private void OnEnable()
{
    statusText = root.Q<Label>("StatusText");
}

private void UpdateStatus(string status)
{
    statusText.text = status;
}

// Bad: Query every time
private void UpdateStatus(string status)
{
    var statusText = root.Q<Label>("StatusText"); // Expensive!
    statusText.text = status;
}
```

---

## Directory Structure

```
Assets/_Project/
├── UI/
│   ├── UXML/                    # UI structure definitions
│   │   ├── Quest/
│   │   │   └── QuestProgressHUD.uxml
│   │   └── Menu/
│   │       └── TitleScreen.uxml
│   │
│   └── USS/                     # Style definitions
│       ├── Common.uss           # Design tokens, common components
│       └── QuestUI.uss          # Quest-specific styles
│
└── Scripts/
    └── UI/                      # UI logic
        └── Quest/
            └── QuestProgressUI.cs
```

---

## Input System Pitfall: UIDocument Lifecycle Kills Project-Wide Actions

Unity 6.2+ intentionally disables the **entire project-wide InputActionAsset**
(`InputSystem.actions`) when the last active UIDocument's runtime panel is
disposed. Internally: `PanelSettings.DisposePanel -> EventProvider.Shutdown ->
InputSystemProvider.UnregisterActions -> InputActionAsset.Disable()`. If
gameplay actions live in the same asset, all gameplay input (Move/Look/Jump)
silently dies the moment the last UI Toolkit panel disappears — scene
transitions away from a UI Toolkit title screen, or `SetActive(false)` on a
fullscreen overlay, are typical triggers. Nothing re-enables the maps.

### Symptom

- Input works until a UI Toolkit screen/overlay closes, then everything stops.
- `InputActionMap.enabled` is `false` for every map; devices and bindings look healthy.
- Re-enabling the maps manually restores input immediately.

### Countermeasures (in order of preference)

```text
1. Separate assets        : Keep gameplay actions in their own InputActionAsset
                            (referenced via InputActionReference). Leave the
                            project-wide asset to UI Toolkit only.
2. Keep a UIDocument alive: Persistent scene with one UIDocument, or hide
                            overlays with style.display = DisplayStyle.None
                            instead of SetActive(false).
3. Unify on UI Toolkit    : Every screen keeps at least one UIDocument active.
```

```csharp
// Bad: gameplay reads actions from the project-wide asset (InputSystem.actions).
// UI Toolkit disables this entire asset when its last panel is disposed.
[SerializeField] private InputActionReference moveActionRef; // -> InputSystem_Actions (project-wide)

// Good: gameplay references a dedicated asset that UI Toolkit never touches.
[SerializeField] private InputActionReference moveActionRef; // -> Gameplay_Actions (gameplay-only)
```

Duplicating the .inputactions file preserves action GUIDs, so existing
InputActionReference fileIDs stay valid — only the asset GUID in the
referencing .asset needs to change.

Reference: https://tang3cko.com/posts/unity-inputactionasset-uidocument-scene-transition

---

## References

- [accessibility.md](accessibility.md) - Font size and contrast requirements
- See `unity-core` skill for general naming conventions
- See `unity-architecture` skill for EventChannel integration with UI

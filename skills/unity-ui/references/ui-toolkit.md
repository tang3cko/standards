# UI Toolkit

## Directory structure

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

## BEM naming convention

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

### UXML example

```xml
<!-- Always include both base class and modifier -->
<ui:VisualElement class="quest-result__panel">
    <ui:Label class="quest-result__title" />
    <ui:Label class="quest-result__status quest-result__status--success" />
</ui:VisualElement>
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

## Design tokens

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

## USS responsive design

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

## UXML structure

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

---

## C# controller pattern

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

### Performance: Cache element references

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

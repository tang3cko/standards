# UI Toolkit

Tang3cko's UI Toolkit conventions: BEM naming, design-token naming, the EventChannel controller, and the input-kill pitfall.

The mechanics of UI Toolkit — USS restrictions and brevity, UXML file rules, PanelSettings setup, flexible layouts, transitions, 9-slice, runtime data binding, custom elements, Painter2D, manipulators — are covered by Unity's official `ui-uitk` skill (Unity-Technologies/skills). Do not restate them here. This file holds only what Tang3cko decides differently or additionally, and where the two disagree, this file wins.

> **Version notes**: World Space UI Toolkit shipped in Unity 6.2 — UI elements can be positioned, rotated, and scaled alongside 3D objects, replacing the old "world space = uGUI" rule for 6.2+ projects. Unity 6.5 adds **Panel Renderer** (supersedes `UIDocument` — same functionality plus better world-space compatibility; a migration guide exists) and makes the **Advanced Text Generator** the default text system. Unity 6.3 adds the UI Test Framework package and USS CSS-style filters (blur/grayscale). All BEM/UXML/USS conventions in this file apply to both screen-space and world-space panels.

---

## Overrides of the official `ui-uitk` defaults

| Topic | Official `ui-uitk` default | Tang3cko |
|-------|----------------------------|----------|
| UXML `name` attribute | camelCase (`submitButton`) | **PascalCase** (`SubmitButton`) — matches the C# `root.Q<T>("SubmitButton")` call site |
| USS class naming | kebab-case, project patterns first | kebab-case **BEM** (`block__element--modifier`), mandatory |
| File layout | Feature folders (`Assets/UI/Inventory/`) | `Assets/_Project/UI/{UXML,USS}/<Feature>/` + `Assets/_Project/Scripts/UI/<Feature>/` (see Directory Structure) |
| Default runtime system | uGUI when the user has no preference | **UI Toolkit** for all new UI (world space too on 6.2+); uGUI only per `SKILL.md`'s decision table |
| C# scripts | Only when requested, in the UXML folder | Controllers subscribe via EventChannel (see C# Controller Pattern), live under `Scripts/UI/` |

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

## UXML Naming

- **name attribute**: PascalCase (for C# queries)
- **class attribute**: kebab-case BEM; always include the base class alongside a modifier

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="../../USS/Common.uss" />
    <Style src="../../USS/QuestUI.uss" />

    <ui:VisualElement name="QuestResultPanel" class="quest-result__panel">
        <ui:Label name="TitleText" class="quest-result__title" />
        <ui:Label name="StatusText" class="quest-result__status quest-result__status--success" />
    </ui:VisualElement>
</ui:UXML>
```

---

## Design Tokens

Tokens live in `Common.uss` under `:root`, named `--category-name-variant`; component USS never hardcodes a color, font size, spacing, or radius.

```css
:root {
    --color-primary: #FFD700;
    --color-primary-hover: #FFA500;
    --color-text-muted: rgba(255, 255, 255, 0.5);
    --font-size-large: 24px;
    --padding-large: 16px;
    --border-radius-large: 8px;
    --transition-fast: 0.1s;
}

/* Good */
.quest-result__status--success {
    color: var(--color-success);
    font-size: var(--font-size-large);
}

/* Bad: hardcoded values in component USS */
.quest-result__status--success {
    color: #32CD32;
    font-size: 24px;
}
```

Platform- and resolution-specific font-size tokens are defined in `accessibility.md`.

---

## Layout Defaults

Panel Settings for a PC-landscape project:

```yaml
m_ScaleMode: 2                    # Scale With Screen Size
m_ReferenceResolution: {x: 1920, y: 1080}
m_ScreenMatchMode: 0              # Match Width Or Height
m_Match: 0                        # Match width (PC landscape)
```

Panels size with `%` plus `min-`/`max-` constraints, never a bare fixed width. Equal distribution needs `flex-basis: 0` next to `flex-grow: 1` — `flex-grow` alone distributes only the leftover space.

```css
.quest-board__panel {
    width: 80%;
    max-width: 1200px;
    min-width: 400px;
}

.column {
    flex-grow: 1;
    flex-basis: 0;
}
```

---

## C# Controller Pattern

Controllers receive state through EventChannels (`unity-architecture`), never by polling. Element queries are typed, cached once in `OnEnable`, and null-checked; re-querying per update is a violation.

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

            progressText = root.Q<Label>("ProgressText");
            if (progressText == null)
            {
                Debug.LogError($"[{GetType().Name}] ProgressText not found in UXML", this);
                return;
            }

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

- Official `ui-uitk` skill (Unity-Technologies/skills) — USS restrictions, UXML rules, PanelSettings, binding, custom elements, Painter2D, manipulators
- [accessibility.md](accessibility.md) - Font size and contrast requirements
- See `unity-core` skill for general naming conventions
- See `unity-architecture` skill for EventChannel integration with UI

---

## Checklist

Run against every UXML/USS/C# file written or reviewed with this reference loaded:

- [ ] Every USS class is BEM (`block__element--modifier`); no generic global classes (`.panel`, `.title`), no chained elements (`a__b__c`)
- [ ] UXML `name` attributes are PascalCase (for C# queries); `class` attributes are kebab-case BEM
- [ ] Colors, font sizes, spacing, and radii use `var(--token)` from Common.uss — no hardcoded values in component USS
- [ ] Panels size responsively (`%` with `min-/max-` constraints); `flex-grow: 1` is paired with `flex-basis: 0` wherever equal distribution is intended
- [ ] C# element queries are typed (`root.Q<Label>(...)`), cached in `OnEnable`, and null-checked — never re-queried per update; state arrives via EventChannel, not polling
- [ ] Files sit under `Assets/_Project/UI/{UXML,USS}/` and `Assets/_Project/Scripts/UI/`, not the official skill's feature-folder default
- [ ] Gameplay input does not share the project-wide InputActionAsset with UI Toolkit if the last UIDocument panel can be disposed (input-kill pitfall above)

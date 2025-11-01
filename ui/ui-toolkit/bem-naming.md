# BEM Naming Convention for UI Toolkit

## Purpose

Establish consistent and scalable naming conventions for UI Toolkit classes using BEM (Block Element Modifier) methodology. This prevents style conflicts, improves maintainability, and makes component scope immediately clear.

## Checklist

- [ ] Use BEM naming for all UI Toolkit class names
- [ ] Follow Block__Element--Modifier pattern
- [ ] Use kebab-case for class names
- [ ] Use PascalCase for name attributes
- [ ] Avoid global class names like `.status` or `.title`
- [ ] Document component structure in USS comments
- [ ] Keep modifiers for state variations only

---

## BEM Overview

BEM (Block Element Modifier) is a CSS naming convention consisting of three parts:

```
.block__element--modifier
```

### Naming Components

- **Block**: Independent component name (e.g., `quest-result`, `quest-board`)
- **Element**: Part of a Block, connected with `__` (e.g., `quest-result__header`, `quest-result__button`)
- **Modifier**: State or variation, connected with `--` (e.g., `quest-result__status--success`, `quest-result__status--failed`)

---

## Basic Pattern

### Structure

```css
/* Block */
.component-name { }

/* Element */
.component-name__element { }
.component-name__element-subpart { }

/* Modifier */
.component-name--modifier { }
.component-name__element--modifier { }
```

### Example

```xml
<!-- ❌ Bad: Non-BEM -->
<ui:VisualElement class="panel">
    <ui:Label class="title" />
    <ui:Label class="status success" />
</ui:VisualElement>

<!-- ✅ Good: BEM Compliant -->
<ui:VisualElement class="quest-result__panel">
    <ui:Label class="quest-result__title" />
    <ui:Label class="quest-result__status quest-result__status--success" />
</ui:VisualElement>
```

---

## Complete Example: Quest Result Panel

### UXML Structure

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements">
    <Style src="Common.uss" />
    <Style src="QuestUI.uss" />

    <!-- Block: quest-result -->
    <ui:VisualElement name="QuestResultOverlay" class="quest-result__overlay">
        <ui:VisualElement name="QuestResultPanel" class="quest-result__panel">

            <!-- Element: __header -->
            <ui:VisualElement class="quest-result__header">
                <ui:Label text="Quest Result" class="quest-result__header-title" />
            </ui:VisualElement>

            <!-- Element: __content -->
            <ui:VisualElement class="quest-result__content">

                <!-- Element: __status + Modifier: --success/--failed -->
                <ui:Label name="ResultStatusText" text="SUCCESS"
                          class="quest-result__status quest-result__status--success" />

                <!-- Element: __title -->
                <ui:Label name="QuestNameText" text="Quest Name"
                          class="quest-result__title" />

                <!-- Element: __stats -->
                <ui:VisualElement class="quest-result__stats">
                    <ui:Label name="ProgressText" text="Progress: 5/5"
                              class="quest-result__stat" />
                    <ui:Label name="TimeText" text="Time: 02:35"
                              class="quest-result__stat" />
                </ui:VisualElement>

                <!-- Element: __rewards -->
                <ui:VisualElement class="quest-result__rewards">
                    <ui:Label text="── Rewards ──"
                              class="quest-result__rewards-title" />
                    <ui:Label text="(Coming Soon)"
                              class="quest-result__rewards-hint" />
                </ui:VisualElement>
            </ui:VisualElement>

            <!-- Element: __footer -->
            <ui:VisualElement class="quest-result__footer">
                <ui:Button name="OKButton" text="OK"
                           class="button-primary quest-result__button" />
                <ui:Label name="AutoCloseHint" text="Returning to lobby in 3s..."
                          class="quest-result__hint" />
            </ui:VisualElement>
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>
```

### USS Implementation

```css
/* ========================================
   Quest Result UI (BEM Compliant)
   Block: quest-result
   ======================================== */

/* Overlay (Full-screen background) */
.quest-result__overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.8);
    display: none;
    justify-content: center;
    align-items: center;
}

/* Panel (Main container) */
.quest-result__panel {
    width: 600px;
    height: 550px;
    background-color: rgba(40, 30, 20, 0.95);
    border-width: 3px;
    border-color: rgba(200, 150, 100, 0.8);
    border-radius: 12px;
    flex-direction: column;
    padding: var(--padding-large);
}

/* Element: Header */
.quest-result__header {
    flex-direction: row;
    justify-content: center;
    padding-bottom: var(--padding-large);
    border-bottom-width: 2px;
    border-bottom-color: rgba(200, 150, 100, 0.5);
}

.quest-result__header-title {
    font-size: var(--font-size-large);
    color: var(--color-primary);
    -unity-font-style: bold;
}

/* Element: Status (SUCCESS/FAILED) */
.quest-result__status {
    font-size: 64px;
    -unity-font-style: bold;
    margin-bottom: var(--padding-large);
    -unity-text-align: middle-center;
}

/* Modifier: Success state */
.quest-result__status--success {
    color: var(--color-success);
}

/* Modifier: Failed state */
.quest-result__status--failed {
    color: var(--color-danger);
}

/* Element: Title */
.quest-result__title {
    font-size: 24px;
    color: var(--color-primary);
    -unity-font-style: bold;
    margin-bottom: var(--padding-medium);
    -unity-text-align: middle-center;
}

/* Element: Stats Container */
.quest-result__stats {
    flex-direction: column;
    align-items: center;
    margin-bottom: var(--padding-large);
}

/* Element: Individual Stat */
.quest-result__stat {
    font-size: 18px;
    color: var(--color-text);
    margin-bottom: var(--padding-small);
}

/* Element: Rewards Container */
.quest-result__rewards {
    flex-direction: column;
    align-items: center;
    margin-bottom: var(--padding-large);
}

.quest-result__rewards-title {
    font-size: 20px;
    color: var(--color-primary);
    -unity-font-style: bold;
    margin-bottom: var(--padding-small);
}

.quest-result__rewards-hint {
    font-size: 14px;
    color: rgba(255, 255, 255, 0.5);
}

/* Element: Footer */
.quest-result__footer {
    flex-direction: column;
    align-items: center;
    padding-top: var(--padding-medium);
}

.quest-result__button {
    width: 150px;
    height: 50px;
    font-size: 20px;
    margin-bottom: var(--padding-small);
}

.quest-result__hint {
    font-size: var(--font-size-small);
    color: rgba(255, 255, 255, 0.6);
}
```

---

## Benefits of BEM

### Scope Clarity

```css
/* ✅ Immediately clear: __status belongs to quest-result */
.quest-result__status { }

/* ❌ Unclear: Which component does this belong to? */
.status { }
```

### Collision Avoidance

```css
/* ✅ No conflicts between different components */
.quest-result__title { }
.inventory__title { }

/* ❌ Potential conflicts */
.title { }  /* Which title? */
```

### Maintainability

```css
/* ✅ Easy to find all related styles */
.quest-board__panel { }
.quest-board__list { }
.quest-board__details { }

/* Search for "quest-board" finds everything */
```

### Consistency

```css
/* ✅ Consistent pattern across project */
.component-name__element--modifier

/* Team members immediately understand the structure */
```

---

## Common Patterns

### State Modifiers

```css
/* Button states */
.button-primary { }
.button-primary--disabled { }
.button-primary--loading { }

/* Quest status */
.quest-item { }
.quest-item--active { }
.quest-item--completed { }
.quest-item--failed { }
```

### Theme Modifiers

```css
/* Color themes */
.panel { }
.panel--dark { }
.panel--light { }

/* Size variations */
.button { }
.button--small { }
.button--large { }
```

### Combined Modifiers

```xml
<!-- Multiple modifiers can be applied together -->
<ui:Button class="button-primary button-primary--large button-primary--disabled" />
```

---

## Anti-Patterns

### Avoid Generic Global Classes

```css
/* ❌ Bad: Too generic */
.panel { }
.title { }
.button { }
.text { }

/* ✅ Good: Component-specific */
.quest-result__panel { }
.quest-result__title { }
.quest-result__button { }
.quest-result__text { }
```

### Avoid Deep Nesting

```css
/* ❌ Bad: Too deep */
.quest-result__panel__content__stats__item { }

/* ✅ Good: Flatten structure */
.quest-result__panel { }
.quest-result__content { }
.quest-result__stats { }
.quest-result__stat { }
```

### Avoid Modifier Without Base Class

```xml
<!-- ❌ Bad: Missing base class -->
<ui:Label class="quest-result__status--success" />

<!-- ✅ Good: Include both base and modifier -->
<ui:Label class="quest-result__status quest-result__status--success" />
```

---

## C# Integration

### Toggling Modifiers

```csharp
using UnityEngine;
using UnityEngine.UIElements;

namespace ProjectName.UI
{
    public class QuestResultUI : MonoBehaviour
    {
        private Label statusText;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            statusText = root.Q<Label>("ResultStatusText");
        }

        public void ShowSuccess()
        {
            // Remove failed modifier, add success modifier
            statusText.RemoveFromClassList("quest-result__status--failed");
            statusText.AddToClassList("quest-result__status--success");
            statusText.text = "SUCCESS";
        }

        public void ShowFailure()
        {
            // Remove success modifier, add failed modifier
            statusText.RemoveFromClassList("quest-result__status--success");
            statusText.AddToClassList("quest-result__status--failed");
            statusText.text = "FAILED";
        }
    }
}
```

---

## References

- [Design Tokens](design-tokens.md)
- [UXML Structure](uxml-structure.md)
- [USS Responsive Design](uss-responsive.md)

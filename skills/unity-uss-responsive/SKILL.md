---
name: unity-uss-responsive
description: Responsive USS layouts with Flexbox and Panel Settings. Use when creating scalable UI for multiple resolutions in Unity.
---

# USS Responsive Design

## Purpose

Implement responsive UI layouts using Unity UI Toolkit's Flexbox system and Panel Settings to support multiple screen resolutions while avoiding absolute positioning and fixed pixel values.

## Checklist

- [ ] Use Panel Settings with Scale With Screen Size mode
- [ ] Set reference resolution (e.g., 1920x1080)
- [ ] Prefer percentages over fixed pixel values
- [ ] Use flex-grow with flex-basis: 0 for equal distribution
- [ ] Add max-width/min-width for large/small screens
- [ ] Use Flexbox instead of absolute positioning
- [ ] Combine flex properties with padding/margin design tokens
- [ ] Test on multiple resolutions

---

## Panel Settings configuration - P1

Panel Settings control how UI Toolkit scales across different resolutions.

### Recommended settings

```yaml
# DefaultPanelSettings.asset
m_ScaleMode: 2                           # Scale With Screen Size (required)
m_ReferenceResolution: {x: 1920, y: 1080}  # Reference resolution
m_ScreenMatchMode: 0                      # Match Width Or Height
m_Match: 0                                # Match width (PC landscape)
```

### Scale mode options

| Mode | Value | Description | Use Case |
|------|-------|-------------|----------|
| Constant Pixel Size | 0 | No scaling | Not recommended |
| Constant Physical Size | 1 | Physical size scaling | Mobile/tablet |
| **Scale With Screen Size** | 2 | **Resolution scaling** | **PC games (recommended)** |

### Match value

- `0.0` = Match width (optimal for PC landscape)
- `0.5` = Balance width and height (mobile)
- `1.0` = Match height

**For PC games targeting 16:9 displays, use Match: 0.**

---

## Supported units - P1

Unity UI Toolkit supports limited units compared to web CSS.

### Supported

| Unit | Description | Example |
|------|-------------|---------|
| `px` | Absolute pixels (scales with Panel Settings) | `width: 300px;` |
| `%` | Percentage of parent | `width: 50%;` |
| `0` | Zero (no unit needed) | `margin: 0;` |

### Not supported

| Unit | Description | Alternative |
|------|-------------|-------------|
| `em`, `rem` | Font-relative | Use Panel Settings scaling |
| `vh`, `vw` | Viewport units | Use percentages + Flexbox |
| `vmin`, `vmax` | Viewport min/max | Not available |

**Important:** USS `px` values automatically scale with Panel Settings.

---

## Flexbox layout patterns - P1

Unity UI Toolkit uses Yoga (Flexbox subset) for layout.

### Equal distribution pattern

Use `flex-grow: 1` with `flex-basis: 0` for consistent distribution:

```css
/* ❌ Bad: Content size affects distribution */
.column {
    flex-grow: 1;
}

/* ✅ Good: Equal distribution regardless of content */
.column {
    flex-grow: 1;
    flex-basis: 0;
}
```

**UXML example:**

```xml
<ui:VisualElement style="flex-direction: row;">
    <ui:VisualElement class="column" style="flex-grow: 1; flex-basis: 0;">
        <ui:Label text="Left Column" />
    </ui:VisualElement>
    <ui:VisualElement class="column" style="flex-grow: 1; flex-basis: 0;">
        <ui:Label text="Right Column" />
    </ui:VisualElement>
</ui:VisualElement>
```

### Nested container pattern

Avoid absolute positioning; use nested Flexbox containers:

```xml
<!-- ✅ Good: Flexbox for bottom-right button placement -->
<ui:VisualElement style="flex-direction: column; flex-grow: 1;">
    <!-- Content area -->
    <ui:VisualElement style="flex-grow: 1;">
        <ui:Label text="Content" />
    </ui:VisualElement>

    <!-- Footer (bottom placement) -->
    <ui:VisualElement style="flex-direction: row; justify-content: flex-end;">
        <ui:Button text="Cancel" />
        <ui:Button text="OK" />
    </ui:VisualElement>
</ui:VisualElement>
```

```css
/* ❌ Bad: Absolute positioning (low maintainability) */
.button {
    position: absolute;
    bottom: 20px;
    right: 20px;
}
```

### Responsive panel pattern

Combine percentages with max-width/min-width:

```css
/* ✅ Good: Responsive panel */
.quest-board__panel {
    width: 80%;              /* 80% of screen width */
    max-width: 1200px;       /* Maximum width for 4K screens */
    height: 80%;             /* 80% of screen height */
    max-height: 900px;       /* Maximum height */
    align-self: center;      /* Center alignment */
}

/* ❌ Bad: Fixed size */
.quest-board__panel {
    width: 1200px;           /* Doesn't fit on smaller screens */
    height: 900px;
}
```

### Ultrawide support pattern

Use max-width to prevent excessive stretching:

```css
/* ✅ Good: Ultrawide-friendly */
.title-screen__ui-container {
    width: 25%;              /* 25% of screen width */
    max-width: 500px;        /* Prevent stretching on ultrawide */
    min-width: 350px;        /* Maintain readability on small screens */
}

/* ❌ Bad: Fixed value (looks small on ultrawide) */
.title-screen__ui-container {
    width: 400px;            /* Relatively small on 3440x1440 */
}
```

---

## Flexbox property reference - P1

### Container properties

```css
.container {
    flex-direction: row;           /* row | column */
    justify-content: flex-start;   /* flex-start | center | flex-end | space-between */
    align-items: stretch;          /* stretch | flex-start | center | flex-end */
    flex-wrap: nowrap;             /* nowrap | wrap */
}
```

### Item properties

```css
.item {
    flex-grow: 0;                  /* Growth ratio (0 = no growth) */
    flex-shrink: 1;                /* Shrink ratio (0 = no shrink) */
    flex-basis: auto;              /* Base size (auto | 0 | 100px | 50%) */
    align-self: auto;              /* Individual alignment */
}
```

### Common patterns

```css
/* Fill remaining space */
.fill-space {
    flex-grow: 1;
    flex-basis: 0;                 /* Equal distribution */
}

/* Fixed size (don't shrink) */
.fixed-size {
    flex-shrink: 0;
    width: 200px;
}

/* Center on cross-axis */
.centered {
    align-self: center;
}
```

---

## Anti-patterns - P1

### Pattern 1: Excessive absolute positioning

```css
/* ❌ Bad: Absolute positioning for non-overlay elements */
.debug-ui__panel {
    position: absolute;      /* Low maintainability, difficult to make responsive */
    top: 10px;
    left: 10px;
}
```

**Fix:**

```css
/* ✅ Good: Flexbox positioning */
.debug-ui__container {
    flex-direction: column;
    align-items: flex-start; /* Left alignment */
    justify-content: flex-start; /* Top alignment */
    padding: 10px;
}
```

### Pattern 2: Fixed pixel value abuse

```css
/* ❌ Bad: All fixed values */
.title-screen__ui-container {
    width: 400px;
    bottom: 80px;
    right: 60px;
}
```

**Fix:**

```css
/* ✅ Good: Percentage-based */
.title-screen__ui-container {
    width: 25%;
    max-width: 450px;
    min-width: 350px;
    /* If absolute positioning is necessary, use percentages */
    bottom: 5%;
    right: 3%;
}
```

### Pattern 3: flex-grow without flex-basis

```css
/* ❌ Bad: Distribution affected by content size */
.column {
    flex-grow: 1;
}
```

**Fix:**

```css
/* ✅ Good: Guaranteed equal distribution */
.column {
    flex-grow: 1;
    flex-basis: 0;           /* Ignore content size */
}
```

---

## Media query alternative - P1

**Unity UI Toolkit does not support CSS media queries.**

### Alternative 1: Panel Settings scaling (recommended)

```yaml
m_ScaleMode: 2  # Automatic scaling
```

### Alternative 2: C# dynamic class switching

```csharp
namespace ProjectName.UI
{
    public class ResponsiveUI : MonoBehaviour
    {
        private UIDocument uiDocument;

        private void Start()
        {
            uiDocument = GetComponent<UIDocument>();
            AdjustForScreenSize();
        }

        private void AdjustForScreenSize()
        {
            var root = uiDocument.rootVisualElement;

            // Remove all screen size classes
            root.RemoveFromClassList("small-screen");
            root.RemoveFromClassList("large-screen");

            // Add appropriate class
            if (Screen.width < 1280)
                root.AddToClassList("small-screen");
            else if (Screen.width >= 2560)
                root.AddToClassList("large-screen");
        }
    }
}
```

**USS:**

```css
/* Default styles */
.panel {
    width: 80%;
}

/* Small screen adjustments */
.small-screen .panel {
    width: 95%;
}

/* Large screen adjustments */
.large-screen .panel {
    max-width: 1600px;
}
```

---

## Resolution testing checklist - P1

Test your UI on these common resolutions:

| Resolution | Aspect Ratio | Priority | Notes |
|------------|-------------|----------|-------|
| 1920x1080 (Full HD) | 16:9 | Required | Reference resolution |
| 2560x1440 (2K) | 16:9 | Required | Common high-res |
| 1366x768 | 16:9 | Medium | Laptop standard |
| 3840x2160 (4K) | 16:9 | Medium | High-res displays |
| 3440x1440 (Ultrawide) | 21:9 | Low | Nice to have |

---

## Complete responsive example - P1

### Quest progress HUD

```css
/* QuestUI.uss */
.quest-progress-hud__panel {
    /* Responsive width */
    width: 20%;
    max-width: 350px;
    min-width: 250px;

    /* Positioning (top-left corner) */
    position: absolute;
    top: 20px;
    left: 20px;

    /* Visual styling */
    background-color: var(--color-background);
    border-radius: var(--border-radius);
    padding: var(--padding-medium);
}

.quest-progress-hud__title {
    font-size: var(--font-size-medium);
    color: var(--color-primary);
    margin-bottom: var(--margin-small);
}

.quest-progress-hud__progress {
    font-size: var(--font-size-small);
    color: var(--color-success);
}
```

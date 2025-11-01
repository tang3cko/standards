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

## Panel Settings Configuration

Panel Settings control how UI Toolkit scales across different resolutions.

### Recommended Settings

```yaml
# DefaultPanelSettings.asset
m_ScaleMode: 2                           # Scale With Screen Size (required)
m_ReferenceResolution: {x: 1920, y: 1080}  # Reference resolution
m_ScreenMatchMode: 0                      # Match Width Or Height
m_Match: 0                                # Match width (PC landscape)
```

### Scale Mode Options

| Mode | Value | Description | Use Case |
|------|-------|-------------|----------|
| Constant Pixel Size | 0 | No scaling | Not recommended |
| Constant Physical Size | 1 | Physical size scaling | Mobile/tablet |
| **Scale With Screen Size** | 2 | **Resolution scaling** | **PC games (recommended)** |

### Match Value

- `0.0` = Match width (optimal for PC landscape)
- `0.5` = Balance width and height (mobile)
- `1.0` = Match height

**For PC games targeting 16:9 displays, use Match: 0.**

---

## Supported Units

Unity UI Toolkit supports limited units compared to web CSS.

### Supported ✅

| Unit | Description | Example |
|------|-------------|---------|
| `px` | Absolute pixels (scales with Panel Settings) | `width: 300px;` |
| `%` | Percentage of parent | `width: 50%;` |
| `0` | Zero (no unit needed) | `margin: 0;` |

### Not Supported ❌

| Unit | Description | Alternative |
|------|-------------|-------------|
| `em`, `rem` | Font-relative | Use Panel Settings scaling |
| `vh`, `vw` | Viewport units | Use percentages + Flexbox |
| `vmin`, `vmax` | Viewport min/max | Not available |

**Important:** USS `px` values automatically scale with Panel Settings.

---

## Flexbox Layout Patterns

Unity UI Toolkit uses Yoga (Flexbox subset) for layout.

### Equal Distribution Pattern

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

**UXML Example:**

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

### Nested Container Pattern

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

### Responsive Panel Pattern

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

### Ultrawide Support Pattern

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

## Complete Responsive Example

### Quest Board Panel

```css
/* QuestUI.uss */
.quest-board__panel {
    width: 80%;
    max-width: 1400px;       /* 4K support */
    height: 80%;
    max-height: 900px;
    background-color: var(--color-panel);
    border-radius: var(--border-radius-large);
    padding: var(--padding-large);
}

.quest-board__list-container {
    flex-basis: 350px;       /* Base size */
    flex-shrink: 0;          /* Don't shrink */
    min-width: 300px;        /* Minimum width */
}

.quest-board__details {
    flex-grow: 1;
    flex-basis: 0;           /* Fill remaining space */
    min-width: 400px;        /* Minimum width for details */
}
```

---

## Flexbox Property Reference

### Container Properties

```css
.container {
    flex-direction: row;           /* row | column */
    justify-content: flex-start;   /* flex-start | center | flex-end | space-between */
    align-items: stretch;          /* stretch | flex-start | center | flex-end */
    flex-wrap: nowrap;             /* nowrap | wrap */
}
```

### Item Properties

```css
.item {
    flex-grow: 0;                  /* Growth ratio (0 = no growth) */
    flex-shrink: 1;                /* Shrink ratio (0 = no shrink) */
    flex-basis: auto;              /* Base size (auto | 0 | 100px | 50%) */
    align-self: auto;              /* Individual alignment */
}
```

### Common Patterns

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

## Anti-Patterns

### Pattern 1: Excessive Absolute Positioning

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

### Pattern 2: Fixed Pixel Value Abuse

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

### Pattern 3: flex-grow Without flex-basis

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

## Media Query Alternative

**Unity UI Toolkit does not support CSS media queries.**

### Alternative 1: Panel Settings Scaling (Recommended)

```yaml
m_ScaleMode: 2  # Automatic scaling
```

### Alternative 2: C# Dynamic Class Switching

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

### Alternative 3: Multiple USS Files

```csharp
// Platform-specific USS switching
#if UNITY_STANDALONE
    uiDocument.panelSettings = pcPanelSettings;
#endif
```

---

## Resolution Testing Checklist

Test your UI on these common resolutions:

| Resolution | Aspect Ratio | Priority | Notes |
|------------|-------------|----------|-------|
| 1920x1080 (Full HD) | 16:9 | 🔴 Required | Reference resolution |
| 2560x1440 (2K) | 16:9 | 🔴 Required | Common high-res |
| 1366x768 | 16:9 | 🟡 Medium | Laptop standard |
| 3840x2160 (4K) | 16:9 | 🟡 Medium | High-res displays |
| 3440x1440 (Ultrawide) | 21:9 | 🟢 Low | Nice to have |

---

## Practical Example: Quest Progress HUD

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

---

## References

- [BEM Naming](bem-naming.md)
- [Design Tokens](design-tokens.md)
- [UXML Structure](uxml-structure.md)

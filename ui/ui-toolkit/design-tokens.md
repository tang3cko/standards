# Design Tokens (USS Variables)

## Purpose

Centrally manage design values (colors, font sizes, spacing) using USS variables to ensure consistency, maintainability, and easy theme switching across all UI Toolkit interfaces.

## Checklist

- [ ] Define all design tokens in Common.uss :root section
- [ ] Use var(--token-name) instead of hardcoded values
- [ ] Follow --category-name-variant naming pattern
- [ ] Add comments explaining token purpose
- [ ] Check for existing tokens before creating new ones
- [ ] Use design tokens in all USS files
- [ ] Never hardcode color/size values in USS

---

## What Are Design Tokens?

Design tokens are design values (colors, font sizes, spacing) managed as variables in USS (Unity Style Sheets). They are defined in the `:root` selector and referenced throughout all stylesheets.

### Benefits

1. **Consistency**: Unified design across the entire project
2. **Maintainability**: Change colors/sizes in one place
3. **Readability**: `var(--color-primary)` makes intent clear
4. **Theme Support**: Easy dark/light theme switching in the future

---

## Token Definition

### Common.uss Structure

All design tokens must be defined in `Common.uss` within the `:root` section:

```css
/* Common.uss */

/* ========================================
   Design Tokens
   ======================================== */
:root {
    /* Color Tokens */
    --color-primary: #FFD700;         /* Gold - Primary accent */
    --color-secondary: #1E90FF;       /* Blue - Secondary accent */
    --color-success: #32CD32;         /* Green - Success state */
    --color-danger: #FF6347;          /* Red - Danger/failure state */
    --color-background: rgba(0, 0, 0, 0.8); /* Background */
    --color-text: #FFFFFF;            /* Text color */
    --color-text-muted: rgba(255, 255, 255, 0.5); /* Muted text */

    /* Font Size Tokens */
    --font-size-large: 24px;          /* Large heading */
    --font-size-medium: 18px;         /* Body text, buttons */
    --font-size-small: 14px;          /* Supplementary text */

    /* Spacing Tokens */
    --padding-large: 16px;            /* Large padding */
    --padding-medium: 12px;           /* Medium padding */
    --padding-small: 8px;             /* Small padding */
    --margin-large: 16px;             /* Large margin */
    --margin-medium: 12px;            /* Medium margin */
    --margin-small: 8px;              /* Small margin */
}

/* ========================================
   Common Components
   ======================================== */
/* Panel, button, text styles follow... */
```

---

## Token Usage

### Good Examples

```css
/* QuestUI.uss */
.quest-result__status--success {
    color: var(--color-success);
    font-size: var(--font-size-large);
    margin-bottom: var(--padding-large);
}

.quest-result__title {
    color: var(--color-primary);
    font-size: var(--font-size-medium);
    padding: var(--padding-medium);
}

.quest-result__hint {
    color: var(--color-text-muted);
    font-size: var(--font-size-small);
}
```

### Bad Examples

```css
/* ❌ Bad: Hardcoded values */
.quest-result__status--success {
    color: #32CD32;              /* Don't hardcode colors */
    font-size: 24px;             /* Don't hardcode sizes */
    margin-bottom: 16px;         /* Don't hardcode spacing */
}
```

---

## Naming Conventions

### Pattern

```css
--category-name-variant: value;
```

### Category Guidelines

| Category | Examples | Purpose |
|----------|----------|---------|
| `color-*` | `--color-primary`, `--color-text` | Color values |
| `font-size-*` | `--font-size-large`, `--font-size-heading-1` | Font sizes |
| `padding-*` | `--padding-large`, `--padding-button` | Padding values |
| `margin-*` | `--margin-section`, `--margin-small` | Margin values |
| `border-*` | `--border-width-thin`, `--border-radius` | Border properties |
| `transition-*` | `--transition-fast`, `--transition-normal` | Animation timing |

### Examples

```css
:root {
    /* Color Tokens */
    --color-primary: #FFD700;
    --color-primary-hover: #FFA500;
    --color-text: #FFFFFF;
    --color-text-muted: rgba(255, 255, 255, 0.5);

    /* Font Size Tokens */
    --font-size-large: 24px;
    --font-size-heading-1: 32px;
    --font-size-heading-2: 28px;

    /* Spacing Tokens */
    --padding-large: 16px;
    --padding-button: 12px;
    --margin-section: 24px;

    /* Border Tokens */
    --border-width-thin: 1px;
    --border-width-thick: 3px;
    --border-radius: 4px;
    --border-radius-large: 8px;

    /* Animation Tokens */
    --transition-fast: 0.1s;
    --transition-normal: 0.3s;
    --transition-slow: 0.5s;
}
```

---

## Common Components with Tokens

### Panel

```css
.panel {
    background-color: var(--color-background);
    border-radius: var(--border-radius-large);
    padding: var(--padding-large);
    border-width: var(--border-width-thin);
    border-color: var(--color-primary);
}
```

### Button

```css
.button-primary {
    background-color: var(--color-primary);
    color: #000000;
    font-size: var(--font-size-medium);
    padding: var(--padding-medium);
    border-radius: var(--border-radius);
    border-width: 0;
    transition-duration: var(--transition-fast);
}

.button-primary:hover {
    background-color: var(--color-primary-hover);
}

.button-primary:active {
    opacity: 0.8;
}
```

### Text Styles

```css
.text-title {
    font-size: var(--font-size-large);
    color: var(--color-primary);
    -unity-font-style: bold;
    margin-bottom: var(--margin-medium);
}

.text-body {
    font-size: var(--font-size-medium);
    color: var(--color-text);
}

.text-small {
    font-size: var(--font-size-small);
    color: var(--color-text-muted);
}
```

---

## Adding New Tokens

### Rules

1. **Define in Common.uss**: All tokens must live in `Common.uss` `:root` section
2. **Follow naming convention**: Use `--category-name-variant` pattern
3. **Add comments**: Explain the token's purpose
4. **Check for existing tokens**: Verify similar tokens don't already exist

### Example: Adding Animation Tokens

```css
/* Common.uss */
:root {
    /* Existing tokens... */

    /* Animation Tokens */
    --transition-fast: 0.1s;      /* Fast animations (hover effects) */
    --transition-normal: 0.3s;    /* Normal animations (panel open/close) */
    --transition-slow: 0.5s;      /* Slow animations (page transitions) */

    /* Easing Functions */
    --ease-out: ease-out;         /* Standard easing */
    --ease-in-out: ease-in-out;   /* Smooth start and end */
}
```

### Usage of New Tokens

```css
/* QuestUI.uss */
.quest-board__panel {
    transition-property: opacity;
    transition-duration: var(--transition-normal);
    transition-timing-function: var(--ease-in-out);
}

.quest-button:hover {
    transition-duration: var(--transition-fast);
}
```

---

## Theme Support (Future)

Design tokens enable easy theme switching. While not currently implemented, the structure supports it:

### Dark Theme Example

```css
/* Common.uss */
:root {
    /* Default (Dark) Theme */
    --color-background: rgba(0, 0, 0, 0.8);
    --color-text: #FFFFFF;
    --color-panel: rgba(40, 30, 20, 0.95);
}

/* Light Theme (Future) */
.theme-light {
    --color-background: rgba(255, 255, 255, 0.95);
    --color-text: #000000;
    --color-panel: rgba(240, 240, 240, 0.95);
}
```

### C# Theme Switching (Future)

```csharp
namespace ProjectName.UI
{
    public class ThemeManager : MonoBehaviour
    {
        private UIDocument uiDocument;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        public void SetTheme(string themeName)
        {
            var root = uiDocument.rootVisualElement;

            // Remove all theme classes
            root.RemoveFromClassList("theme-light");
            root.RemoveFromClassList("theme-dark");

            // Apply new theme
            root.AddToClassList($"theme-{themeName}");
        }
    }
}
```

---

## Token Organization Best Practices

### Group by Category

```css
:root {
    /* ===== Color Tokens ===== */
    --color-primary: #FFD700;
    --color-secondary: #1E90FF;

    /* ===== Font Size Tokens ===== */
    --font-size-large: 24px;
    --font-size-medium: 18px;

    /* ===== Spacing Tokens ===== */
    --padding-large: 16px;
    --padding-medium: 12px;
}
```

### Semantic vs Literal Naming

```css
/* ✅ Good: Semantic naming */
--color-primary: #FFD700;
--color-success: #32CD32;
--color-danger: #FF6347;

/* ❌ Bad: Literal naming */
--color-gold: #FFD700;
--color-green: #32CD32;
--color-red: #FF6347;
```

Semantic naming makes intent clear and allows color changes without token renaming.

---

## Complete Common.uss Example

```css
/* Common.uss */

/* ========================================
   Design Tokens
   ======================================== */
:root {
    /* Color Tokens */
    --color-primary: #FFD700;         /* Gold - Primary accent */
    --color-primary-hover: #FFA500;   /* Orange - Primary hover */
    --color-secondary: #1E90FF;       /* Blue - Secondary accent */
    --color-success: #32CD32;         /* Green - Success state */
    --color-danger: #FF6347;          /* Red - Danger/failure */
    --color-warning: #FFA500;         /* Orange - Warning */
    --color-background: rgba(0, 0, 0, 0.8);
    --color-panel: rgba(40, 30, 20, 0.95);
    --color-text: #FFFFFF;
    --color-text-muted: rgba(255, 255, 255, 0.5);

    /* Font Size Tokens */
    --font-size-large: 24px;
    --font-size-medium: 18px;
    --font-size-small: 14px;

    /* Spacing Tokens */
    --padding-large: 16px;
    --padding-medium: 12px;
    --padding-small: 8px;
    --margin-large: 16px;
    --margin-medium: 12px;
    --margin-small: 8px;

    /* Border Tokens */
    --border-width: 2px;
    --border-radius: 4px;
    --border-radius-large: 8px;

    /* Animation Tokens */
    --transition-fast: 0.1s;
    --transition-normal: 0.3s;
}

/* ========================================
   Common Components
   ======================================== */

.panel {
    background-color: var(--color-background);
    border-radius: var(--border-radius-large);
    padding: var(--padding-large);
}

.button-primary {
    background-color: var(--color-primary);
    color: #000000;
    font-size: var(--font-size-medium);
    padding: var(--padding-medium);
    border-radius: var(--border-radius);
    border-width: 0;
}

.button-primary:hover {
    background-color: var(--color-primary-hover);
}

.text-title {
    font-size: var(--font-size-large);
    color: var(--color-primary);
    -unity-font-style: bold;
}

.text-body {
    font-size: var(--font-size-medium);
    color: var(--color-text);
}
```

---

## References

- [BEM Naming](bem-naming.md)
- [UXML Structure](uxml-structure.md)
- [USS Responsive Design](uss-responsive.md)

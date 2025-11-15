# Font Size Accessibility Guidelines

## Purpose

Ensure text readability across all platforms (Console, PC/VR, Mobile) by following Microsoft Xbox Accessibility Guidelines (XAG) for minimum font sizes. These standards are designed to make games accessible to players with visual impairments and ensure comfortable reading distances for each platform.

## Checklist

- [ ] Use minimum 26px font size for console (1080p)
- [ ] Use minimum 18px font size for PC/VR (1080p)
- [ ] Scale font sizes proportionally for 4K (2x)
- [ ] Allow players to resize text up to 200% of minimum
- [ ] Ensure 4.5:1 contrast ratio for all text
- [ ] Use design tokens for font sizes (not hardcoded values)
- [ ] Test on target platform's typical viewing distance
- [ ] Avoid relying on platform screen magnification tools

---

## Platform-specific requirements

### Console (Xbox, PlayStation)

Console gaming assumes a **10-foot experience** where players sit several feet away from a TV screen.

| Resolution | Minimum Font Size | Recommended |
|------------|------------------|-------------|
| **1080p** | **26px** | 28px or larger |
| **4K (2160p)** | **52px** | 56px or larger |

**Why larger sizes?**
- Players sit 6-10 feet away from TV
- Couch gaming experience requires larger text
- Accessibility compliance for console certification

**Example (USS):**
```css
/* Console UI - Quest Title */
.quest-title--console {
    font-size: 28px;              /* Exceeds 26px minimum ✅ */
    color: var(--color-primary);
}

.quest-description--console {
    font-size: 26px;              /* Meets 26px minimum ✅ */
    color: var(--color-text);
}
```

---

### PC/VR

PC gaming assumes players sit **2-3 feet** from a monitor, allowing smaller font sizes.

| Resolution | Minimum Font Size | Recommended |
|------------|------------------|-------------|
| **1080p** | **18px** | 20px or larger |
| **4K (2160p)** | **36px** | 40px or larger |

**Why smaller than console?**
- Players sit closer to screens (arm's length)
- Higher pixel density on monitors
- Mouse/keyboard precision

**Example (USS):**
```css
/* PC UI - Quest Title */
.quest-title--pc {
    font-size: 24px;              /* Exceeds 18px minimum ✅ */
    color: var(--color-primary);
}

.quest-description--pc {
    font-size: 18px;              /* Meets 18px minimum ✅ */
    color: var(--color-text);
}
```

---

### Mobile / Xbox Game Streaming

Mobile devices and cloud gaming (Xbox Game Streaming) use **DPI-based scaling**.

| DPI | Minimum Font Size | Common Devices |
|-----|------------------|----------------|
| **100 DPI** | **18px** | Low-res tablets |
| **200 DPI** | **36px** | Standard phones |
| **400 DPI** | **72px** | High-res phones (iPhone, Pixel) |

**Scaling Rule:**
Font sizes scale **linearly** with DPI increases.

**Important:**
Mobile devices have higher DPI than TVs/PC displays. A 100 DPI mobile device requires the same 18px minimum as PC, but 400 DPI devices need 72px.

**Example (C#):**
```csharp
namespace ProjectName.UI
{
    public class MobileFontSizer : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;
        [SerializeField] private Label questTitle;

        private void Start()
        {
            ApplyDPIScaling();
        }

        private void ApplyDPIScaling()
        {
            float dpi = Screen.dpi;
            float baseFontSize = 18f;  // 100 DPI baseline
            float scaleFactor = dpi / 100f;

            // Linear scaling
            float scaledFontSize = baseFontSize * scaleFactor;

            questTitle.style.fontSize = new Length(scaledFontSize, LengthUnit.Pixel);
        }
    }
}
```

---

## Additional accessibility requirements

### Text resizing (200% requirement)

Players **must** be able to resize text up to **200%** of the minimum font sizes without loss of content, functionality, or meaning.

**Example Implementation:**
```csharp
namespace ProjectName.UI
{
    public class FontScaler : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        [Header("Font Scale Settings")]
        [Range(1.0f, 2.0f)]
        [SerializeField] private float fontScale = 1.0f;  // 100% to 200%

        private void OnValidate()
        {
            ApplyFontScale();
        }

        private void ApplyFontScale()
        {
            if (uiDocument == null) return;

            var root = uiDocument.rootVisualElement;

            // Apply scale to all text elements
            root.style.fontSize = new Length(18f * fontScale, LengthUnit.Pixel);
        }

        public void SetFontScale(float scale)
        {
            fontScale = Mathf.Clamp(scale, 1.0f, 2.0f);
            ApplyFontScale();
        }
    }
}
```

---

### Contrast ratio (4.5:1 minimum)

Visible text must have a **minimum luminosity contrast ratio of 4.5:1** against the background.

**Good Contrast Examples:**
```css
/* ✅ Good: White text on dark background (high contrast) */
.high-contrast-text {
    color: #FFFFFF;                    /* White */
    background-color: rgba(0, 0, 0, 0.8);  /* Dark background */
    /* Contrast ratio: ~15:1 ✅ */
}

/* ✅ Good: Dark text on light background */
.dark-on-light {
    color: #000000;                    /* Black */
    background-color: rgba(255, 255, 255, 0.9);  /* Light background */
    /* Contrast ratio: ~15:1 ✅ */
}
```

**Bad Contrast Examples:**
```css
/* ❌ Bad: Gray text on dark gray background (low contrast) */
.low-contrast-text {
    color: rgba(255, 255, 255, 0.5);   /* Muted white */
    background-color: rgba(50, 50, 50, 0.8);  /* Dark gray */
    /* Contrast ratio: ~2:1 ❌ */
}

/* ❌ Bad: Gold text on yellow background */
.bad-contrast {
    color: #FFD700;                    /* Gold */
    background-color: #FFFF00;         /* Yellow */
    /* Contrast ratio: ~1.5:1 ❌ */
}
```

**Online Contrast Checker:**
- WebAIM Contrast Checker: https://webaim.org/resources/contrastchecker/

---

### Line spacing and width

**Line Spacing:**
- Line spacing (leading) should be at least **1.5x** within paragraphs
- Paragraph spacing should be at least **1.5x** larger than line spacing

**Line Width:**
- Maximum **80 characters or glyphs** per line
- Maximum **40 characters** for Chinese, Japanese, Korean

**Example (USS):**
```css
.quest-description {
    font-size: 18px;
    line-height: 27px;             /* 1.5x line spacing ✅ */
    max-width: 600px;              /* Limit line length */
    white-space: normal;           /* Allow wrapping */
}

.quest-paragraph {
    margin-bottom: 40px;           /* 1.5x larger than line spacing ✅ */
}
```

---

## Integration with design tokens

### Recommended design tokens

Update `design-tokens.md` to include accessibility-compliant tokens:

```css
/* Common.uss */
:root {
    /* ===== PC/VR Font Sizes (Accessibility Baseline) ===== */
    --font-size-pc-large: 24px;        /* Exceeds 18px minimum */
    --font-size-pc-medium: 20px;       /* Exceeds 18px minimum */
    --font-size-pc-small: 18px;        /* Meets 18px minimum */

    /* ===== Console Font Sizes (10-foot experience) ===== */
    --font-size-console-large: 32px;   /* Exceeds 26px minimum */
    --font-size-console-medium: 28px;  /* Exceeds 26px minimum */
    --font-size-console-small: 26px;   /* Meets 26px minimum */

    /* ===== 4K Scaling (2x baseline) ===== */
    --font-size-4k-pc-large: 48px;
    --font-size-4k-pc-medium: 40px;
    --font-size-4k-pc-small: 36px;
    --font-size-4k-console-large: 64px;
    --font-size-4k-console-medium: 56px;
    --font-size-4k-console-small: 52px;
}
```

### Platform detection pattern

```csharp
namespace ProjectName.UI
{
    public class PlatformFontManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            ApplyPlatformFontTokens();
        }

        private void ApplyPlatformFontTokens()
        {
            var root = uiDocument.rootVisualElement;

            // Detect platform
#if UNITY_STANDALONE || UNITY_EDITOR
            root.AddToClassList("platform-pc");
#elif UNITY_CONSOLE || UNITY_PS4 || UNITY_XBOXONE
            root.AddToClassList("platform-console");
#elif UNITY_IOS || UNITY_ANDROID
            root.AddToClassList("platform-mobile");
#endif

            // Detect resolution
            if (Screen.height >= 2160)
                root.AddToClassList("resolution-4k");
            else if (Screen.height >= 1080)
                root.AddToClassList("resolution-1080p");
        }
    }
}
```

**USS with Platform Classes:**
```css
/* Default: PC/VR */
.quest-title {
    font-size: var(--font-size-pc-large);
}

/* Console override */
.platform-console .quest-title {
    font-size: var(--font-size-console-large);
}

/* 4K override */
.resolution-4k .quest-title {
    font-size: var(--font-size-4k-pc-large);
}

.resolution-4k.platform-console .quest-title {
    font-size: var(--font-size-4k-console-large);
}
```

---

## Common violations and fixes

### Violation 1: small supplementary text

```css
/* ❌ Bad: 14px on console (below 26px minimum) */
.quest-hint--console {
    font-size: 14px;  /* Accessibility violation ❌ */
}

/* ✅ Fix: Use 26px minimum */
.quest-hint--console {
    font-size: 26px;  /* Meets minimum ✅ */
    color: var(--color-text-muted);  /* Use color to de-emphasize */
}
```

### Violation 2: hardcoded font sizes

```css
/* ❌ Bad: Hardcoded values (no platform scaling) */
.title-screen__title {
    font-size: 48px;
}

/* ✅ Fix: Use design tokens */
.title-screen__title {
    font-size: var(--font-size-pc-large);
}

.platform-console .title-screen__title {
    font-size: var(--font-size-console-large);
}
```

### Violation 3: relying on screen magnification

```csharp
// ❌ Bad: Expecting platform magnifier to fix small text
// "Users can zoom in if text is too small"

// ✅ Fix: Use correct minimum font sizes from the start
// Implement proper font sizes for each platform
```

---

## Testing checklist

### Console testing

- [ ] Test at 6-10 feet from a 55" TV
- [ ] All UI text is 26px or larger at 1080p
- [ ] All UI text is 52px or larger at 4K
- [ ] Critical information (health, ammo, objectives) uses larger sizes (28px+)

### PC/VR testing

- [ ] Test at 2-3 feet from a 24-27" monitor
- [ ] All UI text is 18px or larger at 1080p
- [ ] All UI text is 36px or larger at 4K
- [ ] Test on ultrawide monitors (3440x1440)

### Mobile testing

- [ ] Test on 100 DPI, 200 DPI, 400 DPI devices
- [ ] Font sizes scale linearly with DPI
- [ ] Text remains readable on smallest supported screen

### Accessibility testing

- [ ] Text can be resized to 200% without breaking layout
- [ ] Contrast ratio is 4.5:1 or higher
- [ ] Line spacing is at least 1.5x
- [ ] Lines are no longer than 80 characters (40 for CJK)

---

## Platform screen magnification is not a mitigation

**Important:** Microsoft XAG explicitly states that platform-provided screen magnification tools (like Xbox Magnifier) are **not** an appropriate substitute for proper font sizes.

**Why?**
- Magnification crops the screen, losing peripheral information
- Requires manual activation and configuration
- Degrades user experience
- Does not meet accessibility compliance

**Correct Approach:**
Implement proper minimum font sizes **from the start** for each platform.

---

## References

### Official guidelines

- **Xbox Accessibility Guidelines (XAG) 101**: https://learn.microsoft.com/en-us/gaming/accessibility/xbox-accessibility-guidelines/101
- **Game Accessibility Guidelines**: https://gameaccessibilityguidelines.com/use-an-easily-readable-default-font-size/

### Related standards

- [Design Tokens](../ui-toolkit/design-tokens.md) - USS variables and theming
- [USS Responsive Design](../ui-toolkit/uss-responsive.md) - Resolution scaling
- [uGUI Best Practices](../ugui/best-practices.md) - TextMeshPro usage

### Tools

- **WebAIM Contrast Checker**: https://webaim.org/resources/contrastchecker/
- **Unity UI Toolkit**: Built-in Panel Settings for resolution scaling

---

## Summary table

| Platform | 1080p Minimum | 4K Minimum | Viewing Distance | Priority |
|----------|--------------|------------|------------------|----------|
| **Console** | **26px** | **52px** | 6-10 feet | 🔴 Critical (certification requirement) |
| **PC/VR** | **18px** | **36px** | 2-3 feet | 🔴 Critical |
| **Mobile (100 DPI)** | **18px** | **36px** | 1-2 feet | 🟡 Medium |
| **Mobile (200 DPI)** | **36px** | **72px** | 1-2 feet | 🟡 Medium |
| **Mobile (400 DPI)** | **72px** | **144px** | 1-2 feet | 🟡 Medium |

**Additional Requirements (All Platforms):**
- ✅ 200% text resizing support
- ✅ 4.5:1 contrast ratio
- ✅ 1.5x line spacing
- ✅ Max 80 characters per line (40 for CJK)

---

## Quick decision matrix

**When designing UI text, ask:**

1. **What platform is this for?**
   - Console → Use 26px minimum (1080p) or 52px (4K)
   - PC/VR → Use 18px minimum (1080p) or 36px (4K)
   - Mobile → Calculate based on DPI (18px @ 100 DPI baseline)

2. **Is this critical information?**
   - Yes → Use larger than minimum (e.g., 28px on console)
   - No → Use minimum (e.g., 26px on console)

3. **Can players resize this text?**
   - Yes → Ensure it scales to 200% without breaking layout
   - No → **Fix this** - resizing is required

4. **What's the contrast ratio?**
   - 4.5:1 or higher → ✅ Good
   - Below 4.5:1 → ❌ Adjust colors

---

## Enforcement strategy

### Code review checklist

When reviewing UI implementations, verify:

- [ ] All font-size values use design tokens (no hardcoded px)
- [ ] Design tokens meet platform minimums
- [ ] Platform detection logic is implemented
- [ ] Text resizing to 200% is supported
- [ ] Contrast ratio meets 4.5:1

### Unity editor validation

Consider creating an Editor script to validate font sizes:

```csharp
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;

namespace ProjectName.Editor
{
    public class AccessibilityValidator
    {
        [MenuItem("Tools/Validate Font Sizes")]
        public static void ValidateFontSizes()
        {
            // Find all UIDocuments in project
            var uiDocuments = Resources.FindObjectsOfTypeAll<UIDocument>();

            foreach (var doc in uiDocuments)
            {
                ValidateDocument(doc);
            }
        }

        private static void ValidateDocument(UIDocument doc)
        {
            var root = doc.rootVisualElement;
            var labels = root.Query<Label>().ToList();

            foreach (var label in labels)
            {
                var fontSize = label.resolvedStyle.fontSize;

                // Check against platform minimum (assuming PC baseline)
                if (fontSize < 18f)
                {
                    Debug.LogWarning(
                        $"[Accessibility] Font size {fontSize}px is below 18px minimum " +
                        $"in {doc.name} - {label.name}",
                        doc
                    );
                }
            }
        }
    }
}
#endif
```


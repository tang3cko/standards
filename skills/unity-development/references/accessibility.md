# Font Size Accessibility Guidelines

## Platform requirements

### Console (Xbox, PlayStation)

10-foot experience: players sit 6-10 feet from TV.

| Resolution | Minimum | Recommended |
|------------|---------|-------------|
| **1080p** | **26px** | 28px+ |
| **4K** | **52px** | 56px+ |

### PC/VR

2-3 feet from monitor.

| Resolution | Minimum | Recommended |
|------------|---------|-------------|
| **1080p** | **18px** | 20px+ |
| **4K** | **36px** | 40px+ |

### Mobile

DPI-based scaling: `fontSize = 18px × (DPI / 100)`

| DPI | Minimum |
|-----|---------|
| 100 | 18px |
| 200 | 36px |
| 400 | 72px |

---

## Design tokens for accessibility

```css
:root {
    /* PC/VR Font Sizes */
    --font-size-pc-large: 24px;
    --font-size-pc-medium: 20px;
    --font-size-pc-small: 18px;

    /* Console Font Sizes */
    --font-size-console-large: 32px;
    --font-size-console-medium: 28px;
    --font-size-console-small: 26px;

    /* 4K Scaling (2x) */
    --font-size-4k-pc-large: 48px;
    --font-size-4k-pc-medium: 40px;
    --font-size-4k-pc-small: 36px;
    --font-size-4k-console-large: 64px;
    --font-size-4k-console-medium: 56px;
    --font-size-4k-console-small: 52px;
}
```

---

## Platform detection

```csharp
namespace ProjectName.UI
{
    public class PlatformFontManager : MonoBehaviour
    {
        [SerializeField] private UIDocument uiDocument;

        private void Start()
        {
            var root = uiDocument.rootVisualElement;

#if UNITY_STANDALONE || UNITY_EDITOR
            root.AddToClassList("platform-pc");
#elif UNITY_PS4 || UNITY_XBOXONE
            root.AddToClassList("platform-console");
#endif

            if (Screen.height >= 2160)
                root.AddToClassList("resolution-4k");
        }
    }
}
```

```css
/* Default: PC */
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
```

---

## Additional requirements

### Text resizing (200%)

Players must be able to resize text up to 200% without breaking layout.

```csharp
public class FontScaler : MonoBehaviour
{
    [Range(1.0f, 2.0f)]
    [SerializeField] private float fontScale = 1.0f;

    public void SetFontScale(float scale)
    {
        fontScale = Mathf.Clamp(scale, 1.0f, 2.0f);
        ApplyFontScale();
    }
}
```

### Contrast ratio (4.5:1 minimum)

```css
/* Good: High contrast */
.high-contrast-text {
    color: #FFFFFF;
    background-color: rgba(0, 0, 0, 0.8);
    /* Contrast ratio: ~15:1 */
}

/* Bad: Low contrast */
.low-contrast-text {
    color: rgba(255, 255, 255, 0.5);
    background-color: rgba(50, 50, 50, 0.8);
    /* Contrast ratio: ~2:1 - VIOLATION */
}
```

Tool: [WebAIM Contrast Checker](https://webaim.org/resources/contrastchecker/)

### Line spacing and width

```css
.quest-description {
    font-size: 18px;
    line-height: 27px;     /* 1.5x line spacing */
    max-width: 600px;      /* Max 80 characters per line */
}

.quest-paragraph {
    margin-bottom: 40px;   /* 1.5x larger than line spacing */
}
```

---

## Common violations

### Small supplementary text

```css
/* Bad: 14px on console */
.quest-hint--console {
    font-size: 14px;  /* Below 26px minimum */
}

/* Fix */
.quest-hint--console {
    font-size: 26px;
    color: var(--color-text-muted);  /* Use color to de-emphasize */
}
```

### Hardcoded font sizes

```css
/* Bad */
.title-screen__title {
    font-size: 48px;
}

/* Fix */
.title-screen__title {
    font-size: var(--font-size-pc-large);
}

.platform-console .title-screen__title {
    font-size: var(--font-size-console-large);
}
```

---

## Testing checklist

### Console testing

- [ ] Test at 6-10 feet from 55" TV
- [ ] All text ≥ 26px at 1080p
- [ ] All text ≥ 52px at 4K

### PC/VR testing

- [ ] Test at 2-3 feet from 24-27" monitor
- [ ] All text ≥ 18px at 1080p
- [ ] All text ≥ 36px at 4K
- [ ] Test on ultrawide (3440x1440)

### Accessibility testing

- [ ] Text resizes to 200% without breaking
- [ ] Contrast ratio ≥ 4.5:1
- [ ] Line spacing ≥ 1.5x
- [ ] Lines ≤ 80 characters (40 for CJK)

---

## Summary

| Platform | 1080p Min | 4K Min | Distance |
|----------|-----------|--------|----------|
| Console | 26px | 52px | 6-10 feet |
| PC/VR | 18px | 36px | 2-3 feet |
| Mobile (100 DPI) | 18px | 36px | 1-2 feet |
| Mobile (400 DPI) | 72px | 144px | 1-2 feet |

**All platforms:**
- 200% text resizing
- 4.5:1 contrast ratio
- 1.5x line spacing
- Max 80 chars/line (40 CJK)

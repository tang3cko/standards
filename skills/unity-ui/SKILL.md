---
name: unity-ui
description: Unity UI standards for UI Toolkit and uGUI. Covers BEM naming, USS styling, UXML structure, responsive layouts, World Space UI, and accessibility. Use when creating or modifying Unity UI.
---

# Unity UI standards

Apply Tang3cko UI standards when working with Unity UI Toolkit or uGUI.

## Quick reference

### UI Toolkit - BEM naming

```
block__element--modifier

Examples:
- inventory-panel
- inventory-panel__item-slot
- inventory-panel__item-slot--selected
```

### Font size guidelines (accessibility)

| Platform | Minimum body | Minimum UI |
|----------|--------------|------------|
| Desktop  | 16px         | 14px       |
| Mobile   | 16px         | 12px       |
| VR       | 24px         | 20px       |

### Priority levels

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

## Detailed guides

Read these files for complete standards:

### UI Toolkit

- [BEM naming](../../unity/ui/ui-toolkit/bem-naming.md)
- [Design tokens](../../unity/ui/ui-toolkit/design-tokens.md)
- [UXML structure](../../unity/ui/ui-toolkit/uxml-structure.md)
- [USS responsive](../../unity/ui/ui-toolkit/uss-responsive.md)

### uGUI

- [Best practices](../../unity/ui/ugui/best-practices.md)
- [World Space UI](../../unity/ui/ugui/world-space-ui.md)
- [Billboard](../../unity/ui/ugui/billboard.md)

### Accessibility

- [Font size guidelines](../../unity/ui/accessibility/font-size-guidelines.md)

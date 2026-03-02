---
name: unity-ui
description: Build Unity UI following Tang3cko standards. UI Toolkit (UXML/USS), uGUI, accessibility. Use when creating or reviewing UI code. Triggers on "UI Toolkit", "uGUI", "UXML", "USS", "BEM", "アクセシビリティ", "World Space UI", "UI", "ユーザーインターフェース", "accessibility", "billboard".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity UI

Help users build, review, and improve Unity UI following Tang3cko standards.

## Core Principles

1. **UI Toolkit for new UI** - UXML structure, USS styling, BEM naming convention
2. **uGUI for World Space** - Billboards, in-game HUD, spatial UI
3. **Accessibility always** - Minimum font sizes, contrast ratios, scalable layouts

## When Invoked

### Step 1: Determine Task Type

- **Building new UI?** → Go to Step 2a
- **Reviewing UI code?** → Go to Step 2b
- **UI technology question?** → Go to Step 2c

### Step 2a: Building New UI

1. Determine which UI system to use:
   - Screen-space menus, HUD, settings → UI Toolkit (`ui-toolkit.md`)
   - World Space UI, billboards → uGUI (`ugui.md`)
2. Load `accessibility.md` for font size and contrast requirements
3. Generate UI code following loaded standards
4. Verify BEM naming (UI Toolkit) or component structure (uGUI)

### Step 2b: Reviewing UI Code

1. Load the relevant UI system reference
2. Load `accessibility.md` for compliance check
3. Check naming conventions, layout patterns, styling approach
4. Categorize issues by priority (must fix, should fix, nice to have)

### Step 2c: UI Technology Question

1. Load relevant references based on question
2. Explain with examples, showing recommended patterns

## UI System Decision

| Use Case | System | Reference |
|----------|--------|-----------|
| Screen-space menus, HUD | UI Toolkit | references/ui-toolkit.md |
| Settings panels, inventory | UI Toolkit | references/ui-toolkit.md |
| World Space UI, billboards | uGUI | references/ugui.md |
| In-game floating labels | uGUI | references/ugui.md |

## Reference Files

| File | Use When |
|------|----------|
| references/ui-toolkit.md | UXML, USS, BEM naming |
| references/ugui.md | World Space UI, billboards |
| references/accessibility.md | Font sizes, contrast ratios |

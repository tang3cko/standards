---
name: unity-ui
description: Build Unity UI following Tang3cko standards — UI Toolkit (UXML/USS, BEM naming), uGUI for World Space, accessibility. Use when creating or reviewing UI code.
when_to_use: Triggers on "UI Toolkit", "uGUI", "UXML", "USS", "BEM", "アクセシビリティ", "World Space UI", "UI", "ユーザーインターフェース", "accessibility", "billboard".
model: sonnet
allowed-tools: Read Glob Grep Edit Write
---

# Unity UI

Help users build, review, and improve Unity UI following Tang3cko standards.

## Core Principles

1. **UI Toolkit for new UI** - UXML structure, USS styling, BEM naming convention
2. **uGUI for World Space** - Billboards, floating world labels, spatial UI (screen-space HUD belongs to UI Toolkit)
3. **Accessibility always** - Minimum font sizes, contrast ratios, scalable layouts

## Workflow

Identify the task type and follow the matching path. Load `accessibility.md` in every path.

### Building new UI

1. Pick the UI system from the UI System Decision below and load its reference
2. Load `accessibility.md` for font size and contrast requirements
3. Generate UI code following the loaded standards
4. Verify BEM naming (UI Toolkit) or component structure (uGUI)

### Reviewing UI code

1. Load the relevant UI system reference and `accessibility.md`
2. Check naming conventions, layout patterns, and styling approach
3. Categorize issues by priority (must fix, should fix, nice to have)

### A UI-technology question

1. Load the relevant references and explain with recommended-pattern examples

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

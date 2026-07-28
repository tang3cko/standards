---
name: unity-ui
description: Build Unity UI following Tang3cko standards — UI Toolkit (UXML/USS, BEM naming, world-space support on Unity 6.2+), uGUI for legacy World Space, accessibility. Use when creating or reviewing UI code.
when_to_use: Triggers on "UI Toolkit", "uGUI", "UXML", "USS", "BEM", "アクセシビリティ", "World Space UI", "UI", "ユーザーインターフェース", "accessibility", "billboard".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity UI

Tang3cko's Unity UI standards: UI Toolkit by default (UXML/USS, BEM), uGUI only where UI Toolkit
cannot go, accessibility in every path. The Core Rules below are always in force; the naming rules,
size minimums, and billboard math live in references/.

Two things decide most reviews: USS classes that skip BEM, and a world-space decision made without
first checking the project's Unity version — World Space UI Toolkit requires 6.2+.

## Phase Tracking

**At workflow start (building or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Pick UI system      | UI System Decision table (check project Unity version)"
TaskCreate: "Phase 2: Load references     | system reference + accessibility.md"
TaskCreate: "Phase 3: Write/review UI     | BEM naming, accessibility minimums"
TaskCreate: "Phase 4: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. UI-technology questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Confirm the project's Unity version before choosing world-space tech | World Space UI |
| 2 | Read `accessibility.md` | Always |
| 4 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

1. **UI Toolkit is the default for new UI** — UXML structure, USS styling, BEM naming convention. On Unity 6.2+ this includes World Space UI (shipped in 6.2, matured through 6.5's Panel Renderer)
2. **uGUI only where UI Toolkit can't go** — projects pinned to Unity 6000.0/6000.1 needing world-space UI, or existing uGUI systems being extended
3. **Accessibility always** — minimum font sizes, contrast ratios, scalable layouts (`references/accessibility.md`)
4. **BEM naming is mandatory in USS** — `block__element--modifier`; no generic global classes, no deep nesting

## Workflow

Identify the task type and follow the matching path. Load `accessibility.md` in every path.

### Building new UI

1. Pick the UI system from the UI System Decision below and load its reference
2. Load `accessibility.md` for font size and contrast requirements
3. Generate UI code following the loaded standards
4. Run the Self-Check below before presenting

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
| World Space UI (Unity 6.2+) | UI Toolkit (world-space panel) | references/ui-toolkit.md |
| World Space UI (Unity 6000.0/6000.1) | uGUI | references/ugui.md |
| Extending an existing uGUI system | uGUI | references/ugui.md |

**Version note**: check the project's Unity version before choosing world-space tech — World Space UI Toolkit requires Unity 6.2+. Per-version renderer/text-system specifics (6.5 Panel Renderer, Advanced Text Generator) live in `references/ui-toolkit.md`.

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] The references named by the workflow path were actually Read this session — `accessibility.md` in every path, plus the chosen UI system's reference. The domain specifics (naming details, size minimums, billboard math) exist only in those files, so skipping them means checking against nothing
- [ ] The final `## Checklist` of each loaded reference was run against every written/edited file
- [ ] USS classes follow BEM (`block__element--modifier`), no generic global classes
- [ ] World Space choice matches the project's Unity version (UI Toolkit on 6.2+, uGUI below)

## Reference Files

| File | Load When |
|------|-----------|
| references/ui-toolkit.md | Any UI Toolkit work (build, review, or question). Only place with: full BEM/UXML naming rules, design-token structure, Panel Settings + flexbox patterns, 6.2–6.5 version specifics (world space, Panel Renderer, Advanced Text Generator), and the UIDocument/project-wide-InputActionAsset input-kill pitfall |
| references/ugui.md | Any uGUI world-space work (Unity 6000.0/6000.1, or extending existing uGUI). Only place with: billboard LookAt math, Camera.main caching, Canvas.enabled show/hide, TextMeshPro rule, EventSystem pitfall |
| references/accessibility.md | Every path, before writing or judging any text/color styling. Only place with: per-platform font minimums (console 26px / PC 18px at 1080p, DPI-scaled mobile), 4.5:1 contrast, 200% resize, line-length limits |

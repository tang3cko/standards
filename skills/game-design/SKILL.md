---
name: game-design
description: Organize game concepts and review designs using established game design theories — Sakurai (risk-reward, accessibility, game feel), Tajiri's verb theory (mechanics as verbs), Miyazaki (accomplishment through adversity, fragmentary narrative, interconnected world design), and concept frameworks (pillars, core loop, GDD). Use when designing, reviewing, or brainstorming game concepts, mechanics, difficulty, narrative, or world design.
when_to_use: Triggers on "game design", "game concept", "verb theory", "Sakurai", "risk reward", "core loop", "GDD", "game review", "mechanics", "game feel", "pillars", "difficulty", "narrative", "world design", "level design", "environmental storytelling", "ゲームデザイン", "ゲーム設計", "動詞理論", "コアループ", "ゲームコンセプト", "Miyazaki", "宮崎".
model: opus # deep cross-philosophy design reasoning; guidance-only, no ARL/file writes
effort: max
allowed-tools: Read Glob Grep TaskCreate TaskUpdate TaskList
---

# Game Design

Help users organize game concepts, review game designs, and apply established design theories to their projects.

## Phase Tracking

**At workflow start (concept organizing, design review, or mechanics path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load theory references | per the workflow path below"
TaskCreate: "Phase 2: Apply the framework    | verbs / pitch / pillars / loop, or review checklist"
TaskCreate: "Phase 3: Validate               | MUST check against Sakurai (+ Miyazaki where relevant) checkpoints"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Single design-theory questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the references named by the chosen path | Always |
| 3 | Validate against `sakurai-philosophy.md` checkpoints (+ `miyazaki-philosophy.md` for difficulty/narrative/world) | Concept or review output |

## Core Principles

1. **Sakurai Philosophy** - Risk-reward, player perspective, accessible depth, squeeze-and-release
2. **Verb Theory** - Define mechanics as verbs; design actions before objects
3. **Miyazaki Philosophy** - Accomplishment through adversity, fragmentary narrative, interconnected world design, dignified grotesque
4. **Concept Organization** - Elevator pitch, design pillars, core loop, GDD structure

## Workflow

Identify the task type and follow the matching path.

### Organizing a new game concept

1. `references/verb-theory.md` — extract core verbs from the idea
2. `references/concept-organization.md` — walk through the concept steps
3. Guide through: core verbs → elevator pitch → design pillars → core loop → one-sheet
4. Validate against `references/sakurai-philosophy.md` review checkpoints
5. For difficulty, narrative, or world design, also validate against `references/miyazaki-philosophy.md`

### Reviewing an existing design

1. `references/sakurai-philosophy.md` — Consolidated Design Review Checklist
2. `references/miyazaki-philosophy.md` — checklist for narrative, difficulty, and world design
3. `references/verb-theory.md` — verify core verbs are clear and well-combined
4. Evaluate core game feel first, then feedback/polish, then production
5. Give actionable feedback with priority labels (must fix, should fix, nice to have)

### Defining or refining mechanics

1. `references/verb-theory.md` — verb extraction and combination workflow
2. Identify core verbs, supporting verbs, and verb-object interactions
3. Apply `references/sakurai-philosophy.md` (risk-reward, squeeze-release)
4. Apply `references/miyazaki-philosophy.md` where relevant (combat commitment, death design, discovery)
5. Output: verb list, verb-object matrix, verb combination analysis

### A design-theory question

1. Identify which theory applies, then load the relevant reference(s)
2. Explain with concrete examples and apply them to the user's context
3. When comparing philosophies, cross-reference Sakurai and Miyazaki for contrasting views

### Understanding design influences

1. `references/miyazaki-influences.md` — how specific media shaped design decisions
2. Connect the influence to the design principle, and show how it manifests in specific games

## Reference Files

| File | Use When |
|------|----------|
| references/sakurai-philosophy.md | Reviewing designs, evaluating game feel, checking risk-reward balance |
| references/miyazaki-philosophy.md | Evaluating difficulty design, narrative structure, world design, boss design, aesthetic direction |
| references/miyazaki-influences.md | Tracing design decisions to their origins (gamebooks, literature, manga) |
| references/verb-theory.md | Defining mechanics, extracting core verbs, designing interactions |
| references/concept-organization.md | Structuring ideas, writing pitches, building GDD sections |

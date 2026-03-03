---
name: game-design
description: Organize game concepts and review designs using established game design theories. Sakurai's philosophy (risk-reward, accessibility, game feel), Tajiri's verb theory (mechanics as verbs), Miyazaki's philosophy (accomplishment through adversity, fragmentary narrative, interconnected world design), and concept organization frameworks (pillars, core loop, GDD). Use when designing, reviewing, or brainstorming game concepts. Triggers on "game design", "game concept", "verb theory", "Sakurai", "risk reward", "core loop", "GDD", "game review", "mechanics", "game feel", "pillars", "ゲームデザイン", "ゲーム設計", "動詞理論", "コアループ", "ゲームコンセプト", "Miyazaki", "宮崎", "difficulty", "narrative", "world design", "level design", "environmental storytelling", "fragmentary", "interconnected".
model: sonnet
allowed-tools: Read, Glob, Grep
---

# Game Design

Help users organize game concepts, review game designs, and apply established design theories to their projects.

## Core Principles

1. **Sakurai Philosophy** - Risk-reward, player perspective, accessible depth, squeeze-and-release
2. **Verb Theory** - Define mechanics as verbs; design actions before objects
3. **Miyazaki Philosophy** - Accomplishment through adversity, fragmentary narrative, interconnected world design, dignified grotesque
4. **Concept Organization** - Elevator pitch, design pillars, core loop, GDD structure

## When Invoked

### Step 1: Determine Task Type

- **Organizing a new game concept?** -> Go to Step 2a
- **Reviewing an existing design?** -> Go to Step 2b
- **Defining or refining mechanics?** -> Go to Step 2c
- **Question about design theory?** -> Go to Step 2d

### Step 2a: Organizing a New Game Concept

1. Load `references/verb-theory.md` - Extract core verbs from the idea
2. Load `references/concept-organization.md` - Walk through concept steps
3. Guide through: Core verbs -> Elevator pitch -> Design pillars -> Core loop -> One-sheet
4. Validate against `references/sakurai-philosophy.md` design review checkpoints
5. If the game involves difficulty, narrative design, or world design, also validate against `references/miyazaki-philosophy.md`

### Step 2b: Reviewing an Existing Design

1. Load `references/sakurai-philosophy.md` - Use the Consolidated Design Review Checklist
2. Load `references/miyazaki-philosophy.md` - Use the Consolidated Design Review Checklist for narrative, difficulty, and world design evaluation
3. Load `references/verb-theory.md` - Verify core verbs are clear and well-combined
4. Evaluate core game feel items first, then feedback/polish, then production
5. Provide actionable feedback with priority labels (must fix, should fix, nice to have)

### Step 2c: Defining or Refining Mechanics

1. Load `references/verb-theory.md` - Use the verb extraction and combination workflow
2. Identify core verbs, supporting verbs, and verb-object interactions
3. Apply `references/sakurai-philosophy.md` principles (risk-reward, squeeze-release)
4. Apply `references/miyazaki-philosophy.md` principles where relevant (combat commitment, death design, discovery)
5. Output: verb list, verb-object matrix, verb combination analysis

### Step 2d: Question About Design Theory

1. Identify which theory applies to the question
2. Load the relevant reference file(s)
3. Explain with concrete examples from the reference
4. Show application to the user's specific context
5. When comparing philosophies, cross-reference Sakurai and Miyazaki for contrasting perspectives

### Step 2e: Understanding Design Influences

1. Load `references/miyazaki-influences.md` - For questions about how specific media influenced game design
2. Explain the connection between the influence and the design principle
3. Show how the influence manifests in specific games

## Reference Files

| File | Use When |
|------|----------|
| references/sakurai-philosophy.md | Reviewing designs, evaluating game feel, checking risk-reward balance |
| references/miyazaki-philosophy.md | Evaluating difficulty design, narrative structure, world design, boss design, aesthetic direction |
| references/miyazaki-influences.md | Understanding how gamebooks, literature, and manga influenced Miyazaki's design; tracing design decisions to their origins |
| references/verb-theory.md | Defining mechanics, extracting core verbs, designing interactions |
| references/concept-organization.md | Structuring ideas, writing pitches, building GDD sections |

---
name: game-design
description: Organize game concepts and review designs using established game design theories. Sakurai's philosophy (risk-reward, accessibility, game feel), Tajiri's verb theory (mechanics as verbs), and concept organization frameworks (pillars, core loop, GDD). Use when designing, reviewing, or brainstorming game concepts. Triggers on "game design", "game concept", "verb theory", "Sakurai", "risk reward", "core loop", "GDD", "game review", "mechanics", "game feel", "pillars", "ゲームデザイン", "ゲーム設計", "動詞理論", "コアループ", "ゲームコンセプト".
model: sonnet
allowed-tools: Read, Glob, Grep
---

# Game Design

Help users organize game concepts, review game designs, and apply established design theories to their projects.

## Core Principles

1. **Sakurai Philosophy** - Risk-reward, player perspective, accessible depth, squeeze-and-release
2. **Verb Theory** - Define mechanics as verbs; design actions before objects
3. **Concept Organization** - Elevator pitch, design pillars, core loop, GDD structure

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

### Step 2b: Reviewing an Existing Design

1. Load `references/sakurai-philosophy.md` - Use the Consolidated Design Review Checklist
2. Load `references/verb-theory.md` - Verify core verbs are clear and well-combined
3. Evaluate core game feel items first, then feedback/polish, then production
4. Provide actionable feedback

### Step 2c: Defining or Refining Mechanics

1. Load `references/verb-theory.md` - Use the verb extraction and combination workflow
2. Identify core verbs, supporting verbs, and verb-object interactions
3. Apply `references/sakurai-philosophy.md` principles (risk-reward, squeeze-release)
4. Output: verb list, verb-object matrix, verb combination analysis

### Step 2d: Question About Design Theory

1. Identify which theory applies to the question
2. Load the relevant reference file
3. Explain with concrete examples from the reference
4. Show application to the user's specific context

## Reference Files

| File | Use When |
|------|----------|
| references/sakurai-philosophy.md | Reviewing designs, evaluating game feel, checking risk-reward balance |
| references/verb-theory.md | Defining mechanics, extracting core verbs, designing interactions |
| references/concept-organization.md | Structuring ideas, writing pitches, building GDD sections |

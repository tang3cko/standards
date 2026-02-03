---
name: documentation
description: Write and review documentation following Tang3cko standards. Use when creating or reviewing markdown files. Triggers on "ドキュメント", "documentation", "README", "markdown", "docs", "文書作成", "ドキュメントレビュー".
model: haiku
context: fork
allowed-tools: Read, Glob, Grep
---

# Documentation Skill

Write and review documentation following Tang3cko standards.

## Core Principles

1. **Clarity over completeness** - Write what is necessary, no more
2. **Consistent structure** - Follow established patterns
3. **Actionable content** - Every document should help readers accomplish something

## When Invoked

### Step 1: Identify Task Type

- **Writing new docs?** → Go to Step 2a
- **Reviewing existing?** → Go to Step 2b
- **Formatting questions?** → Load markdown-formatting.md directly

### Step 2a: Writing New Documentation

1. Load `references/document-structure.md` for structure template
2. Apply writing principles from `_core-rules.md` (auto-loaded)
3. Draft document following templates

### Step 2b: Reviewing Documentation

1. `_core-rules.md` is auto-loaded with P1 rules
2. Check against rules, noting specific issues with line numbers
3. Categorize issues by priority (P1 must fix, P2 should fix, P3 nice to have)

### Step 3: Provide Output

**For writing:** Provide draft following templates
**For review:** List specific issues with:
- Line number
- Issue description
- Suggested fix

## Reference Files

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: Essential P1 rules for all documentation |
| references/writing-principles.md | Voice, tone, grammar, inclusive language |
| references/document-structure.md | Structuring documents |
| references/markdown-formatting.md | Markdown syntax questions |
| references/file-organization.md | Organizing files/folders |
| references/code-examples.md | Including code in docs |

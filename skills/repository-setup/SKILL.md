---
name: repository-setup
description: Repository setup standards for Tang3cko projects. Covers AGENTS.md, Git submodules, Claude Code skills, and Renovate configuration. Use when setting up new repositories or onboarding projects.
---

# Repository setup standards

Apply Tang3cko repository setup standards when initializing or configuring projects.

## Decision tree

```
What are you setting up?
│
├─▶ AI agent context (CLAUDE.md, GEMINI.md)
│   └─ [AI Agents Setup](./references/ai-agents.md)
│
└─▶ Dependency automation
    └─ [Renovate Setup](./references/renovate.md)
```

---

## Checklist

```
New repository setup:
[ ] AGENTS.md created with symlinks (CLAUDE.md, GEMINI.md)
[ ] Renovate configured (optional)
```

---

## Quick reference

### AGENTS.md template

```markdown
# Project Name

Brief project description.

## Important

- Think in English
- Respond in user's language

## Before editing

- Read relevant documentation before making changes
- Follow existing patterns and structure
```

```bash
ln -s AGENTS.md CLAUDE.md
ln -s AGENTS.md GEMINI.md
```

### Renovate

Create `.github/renovate.json5`:

```json5
{
  $schema: 'https://docs.renovatebot.com/renovate-schema.json',
  extends: ['config:best-practices'],
  timezone: 'Asia/Tokyo',
  schedule: ['before 9am on Monday'],
  automerge: true,
  automergeType: 'pr',
  matchUpdateTypes: ['minor', 'patch'],
}
```

---

## References

- [AI Agents Setup](./references/ai-agents.md) - AGENTS.md, CLAUDE.md, GEMINI.md
- [Renovate Setup](./references/renovate.md) - Dependency automation

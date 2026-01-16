---
name: repository-setup
description: Repository setup standards. AGENTS.md, symlinks, Renovate configuration. Use when setting up new repositories.
---

# Repository Setup Standards

## Purpose

Apply Tang3cko repository setup standards when initializing or configuring projects. This skill covers AI agent context files and dependency automation with Renovate.

## Checklist

- [ ] AGENTS.md created with project context
- [ ] Symlinks created (CLAUDE.md, GEMINI.md)
- [ ] Renovate configured (optional)

---

## AI Agents Setup - P1

### Create AGENTS.md

Create a central context file that AI agents read for project-specific instructions:

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

### Create symlinks

Different AI agents look for different files. Use symlinks to maintain a single source of truth:

```bash
# Claude Code reads CLAUDE.md
ln -s AGENTS.md CLAUDE.md

# Gemini CLI reads GEMINI.md
ln -s AGENTS.md GEMINI.md
```

### Best practices

**Keep it concise:**
- Under 150 lines recommended
- Focus on project-specific context
- Avoid duplicating general coding standards

**Use actionable instructions:**

```markdown
# ✅ Good
- Read `src/core/` before modifying core logic
- Run `npm test` before committing

# ❌ Bad
- Always write clean code
- Never introduce bugs
```

**Include project-specific context:**

```markdown
## Architecture

- Frontend: React + TypeScript
- Backend: Node.js + Express
- Database: PostgreSQL

## Key directories

- `src/components/` - React components
- `src/api/` - API endpoints
- `src/db/` - Database models
```

### Supported AI agents

| Agent | Context file | Documentation |
|-------|--------------|---------------|
| Claude Code | CLAUDE.md | [Claude Code Docs](https://docs.anthropic.com/en/docs/claude-code) |
| Gemini CLI | GEMINI.md | [Gemini CLI Docs](https://github.com/google-gemini/gemini-cli) |

---

## Renovate Setup - P2

### Create config file

Create `.github/renovate.json5`:

```json5
{
  $schema: 'https://docs.renovatebot.com/renovate-schema.json',
  extends: [
    'config:best-practices',
  ],
  timezone: 'Asia/Tokyo',
  schedule: ['before 9am on Monday'],
  automerge: true,
  automergeType: 'pr',
  matchUpdateTypes: ['minor', 'patch'],
}
```

### Enable Renovate

1. Install [Renovate GitHub App](https://github.com/apps/renovate)
2. Grant access to your repository
3. Renovate will create an onboarding PR

### Configuration options

**Schedule:**

```json5
// Weekly on Monday morning (JST)
schedule: ['before 9am on Monday'],

// Daily
schedule: ['before 9am every weekday'],

// Monthly
schedule: ['before 9am on the first day of the month'],
```

**Automerge:**

```json5
// Automerge minor and patch updates
automerge: true,
automergeType: 'pr',
matchUpdateTypes: ['minor', 'patch'],
```

Major updates require manual review by default. This is intentional - breaking changes need human verification.

### Best practices

- Review major updates: Read changelogs, check for breaking changes
- Monitor PRs: Failed CI blocks automerge
- Lock file maintenance: Renovate automatically updates lock files

---

## Quick reference - P1

### New repository setup

```bash
# 1. Create AGENTS.md with project context
touch AGENTS.md

# 2. Create symlinks for AI agents
ln -s AGENTS.md CLAUDE.md
ln -s AGENTS.md GEMINI.md

# 3. (Optional) Configure Renovate
mkdir -p .github
touch .github/renovate.json5
```

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

## Architecture

- [Technology stack here]

## Key directories

- `src/` - Source code
- `docs/` - Documentation
```

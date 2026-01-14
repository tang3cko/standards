# AI Agents Setup

## Purpose

Configure context files for AI coding assistants (Claude Code, Gemini CLI).

---

## AGENTS.md - P1

Create a central context file that AI agents read for project-specific instructions.

### Create AGENTS.md

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

---

## Best practices - P1

### Keep it concise

- Under 150 lines recommended
- Focus on project-specific context
- Avoid duplicating general coding standards

### Use actionable instructions

```markdown
# Good
- Read `src/core/` before modifying core logic
- Run `npm test` before committing

# Bad
- Always write clean code
- Never introduce bugs
```

### Include project-specific context

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

---

## Supported AI agents

| Agent | Context file | Documentation |
|-------|--------------|---------------|
| Claude Code | CLAUDE.md | [Claude Code Docs](https://docs.anthropic.com/en/docs/claude-code) |
| Gemini CLI | GEMINI.md | [Gemini CLI Docs](https://github.com/google-gemini/gemini-cli) |

---

## References

- [Renovate Setup](renovate.md)

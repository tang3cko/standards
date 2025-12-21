# Setup

## Purpose

Checklist for setting up new repositories.

---

## AGENTS.md - P1

Create AGENTS.md for AI agent context.

**Create `AGENTS.md`:**

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

**Create symlink for Claude Code:**

```bash
ln -s AGENTS.md CLAUDE.md
```

**Notes:**

- Keep concise (under 150 lines)
- Use specific, actionable instructions
- Avoid "always" / "never" phrasing

---

## Coding standards submodule - P1

Add coding-standards as a Git submodule.

```bash
git submodule add https://github.com/tang3cko/coding-standards.git docs/05_standards
```

**Verify:**

```bash
git submodule status
```

**Update submodule:**

```bash
git submodule update --remote docs/05_standards
```

**Clone project with submodules:**

```bash
git clone --recursive https://github.com/tang3cko/[project].git
```

---

## Claude Code skills - P2

Link coding standards skills to enable automatic context loading.

**Create skills directory:**

```bash
mkdir -p .claude/skills
```

**Link required skills:**

```bash
# Unity projects
ln -s ../../docs/05_standards/skills/unity-csharp .claude/skills/unity-csharp
ln -s ../../docs/05_standards/skills/unity-testing .claude/skills/unity-testing
ln -s ../../docs/05_standards/skills/unity-ui .claude/skills/unity-ui

# Mirror networking projects
ln -s ../../docs/05_standards/skills/mirror-networking .claude/skills/mirror-networking

# All projects
ln -s ../../docs/05_standards/skills/documentation .claude/skills/documentation
```

**Available skills:**

| Skill | Use case |
|-------|----------|
| `unity-csharp` | C# coding standards, naming, architecture |
| `unity-testing` | NUnit, FIRST principles, test patterns |
| `unity-ui` | UI Toolkit, uGUI, accessibility |
| `mirror-networking` | Mirror multiplayer patterns |
| `documentation` | Technical writing standards |

**Notes:**

- Skills are auto-invoked by Claude based on context
- Only link skills relevant to your project
- Symlinks are relative to `.claude/skills/` directory

---

## Renovate - P2

Set up automated dependency updates.

**Create `.github/renovate.json5`:**

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

**Notes:**

- Automerge for minor/patch updates
- Major updates require manual review
- Runs weekly on Monday morning

---

## References

- [Claude Code Memory Documentation](https://code.claude.com/docs/en/memory)
- [AGENTS.md Specification](https://agents.md/)
- [Renovate Documentation](https://docs.renovatebot.com/)
- [Git Submodules](https://git-scm.com/book/en/v2/Git-Tools-Submodules)

# File Organization

File naming and directory structure guidelines.

---

## File Naming

- Use kebab-case: `naming-conventions.md`
- Keep under 30 characters
- No spaces, underscores, or special characters

```
✅ Good:
naming-conventions.md
event-channels.md

❌ Bad:
Naming_Conventions.md
eventChannels.md
```

---

## Directory Structure

- Organize files by type or topic
- Limit directory depth to 3-4 levels maximum
- Use numbered prefixes for ordering: `00_overview/`, `01_architecture/`

```
docs/
├── 00_overview/
├── 01_architecture/
├── 02_game-design/
├── 03_development/
│   └── testing/
└── 05_standards/
```

---

## Directory Naming

- Use kebab-case for directory names
- Keep names short and descriptive

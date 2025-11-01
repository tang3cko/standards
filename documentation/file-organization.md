# File Organization

## Purpose

This document defines standards for file naming, directory structure, and version control practices for documentation. Consistent file organization improves discoverability, maintainability, and collaboration across all projects.

## Checklist

- [ ] Use kebab-case for file names
- [ ] Keep file names under 30 characters
- [ ] Avoid spaces and special characters in file names
- [ ] Organize files by type or feature
- [ ] Keep directory hierarchy shallow (max 3-4 levels)
- [ ] Maintain CHANGELOG.md for significant updates
- [ ] Use Git for version history (avoid "Last Updated" sections)

---

## File Naming - P1

### Kebab-Case Convention

**Rule:**
- Use lowercase letters with hyphens (`kebab-case`)
- No spaces, underscores, or camelCase
- Use descriptive names that indicate content

**✅ Good:**

```
naming-conventions.md
event-channels.md
scriptableobject-patterns.md
ui-toolkit-best-practices.md
```

**❌ Bad:**

```
Naming_Conventions.md
eventChannels.md
ScriptableObject Patterns.md
UI-Toolkit-Best-Practices.MD
```

---

### File Name Length

**Rule:**
- Keep file names under 30 characters (excluding extension)
- Use abbreviations sparingly and only when widely understood
- Prioritize clarity over brevity

**✅ Good:**

```
error-handling.md           (15 chars)
dependency-management.md    (21 chars)
ui-responsive-design.md     (20 chars)
```

**❌ Bad:**

```
error-handling-patterns-and-best-practices-guide.md  (51 chars - too long)
err-hdl.md                                           (7 chars - unclear)
```

---

### Special Characters

**Rule:**
- Avoid spaces, underscores, and special characters
- Do not use: `!@#$%^&*()+=[]{}|;:'",<>?/\`
- Exception: Use `-` (hyphen) as word separator

**✅ Good:**

```
scriptableobject-design.md
event-driven-architecture.md
```

**❌ Bad:**

```
scriptable_object_design.md
event & messaging.md
architecture (v2).md
```

---

### File Extensions

**Rule:**
- Use `.md` for Markdown documentation
- Use `.json` for JSON configuration
- Use `.yaml` or `.yml` for YAML configuration
- Use `.txt` only for plain text (rare)

---

## Directory Structure - P1

### Type-Based Organization

**Rule:**
- Organize files by type or topic
- Keep related files together
- Use clear, descriptive directory names

**Example Structure:**

```
docs/
├── 00_spec/              # Specifications
│   ├── features/
│   └── clarifications/
├── 01_overview/          # Project overview
├── 02_game_design/       # Game design documents
├── 03_technical/         # Technical documentation
│   ├── coding_standards/
│   │   ├── core/
│   │   ├── architecture/
│   │   ├── ui/
│   │   ├── documentation/
│   │   └── examples/
│   └── architecture.md
├── 98_plans/             # Implementation plans
│   └── tasks/
└── 99_ideas/             # Ideas and brainstorming
```

---

### Shallow Hierarchy

**Rule:**
- Limit directory depth to 3-4 levels maximum
- Avoid deeply nested structures
- Use flat structures when possible

**✅ Good:**

```
docs/03_technical/coding_standards/core/naming-conventions.md  (4 levels)
```

**❌ Bad:**

```
docs/technical/standards/coding/unity/csharp/core/naming/conventions.md  (9 levels)
```

---

### Index Files

**Rule:**
- Use `README.md` as the index for each directory
- README should provide overview and navigation
- Link to subdirectories and key files

**Example:**

```markdown
# Core Coding Standards

## Purpose

This directory contains core coding standards applicable to all Unity projects.

## Contents

- [Naming Conventions](naming-conventions.md)
- [Code Organization](code-organization.md)
- [Error Handling](error-handling.md)
- [Performance](performance.md)
```

---

## Version Control - P2

### Git as Source of Truth

**Rule:**
- Use Git commit history for tracking changes
- Avoid "Last Updated" or "Version" sections in documents
- Exception: YAML frontmatter for publishing systems

**✅ Good:**

```bash
# View document history
git log --follow docs/03_technical/coding_standards/core/naming-conventions.md

# View specific changes
git diff HEAD~1 docs/03_technical/coding_standards/core/naming-conventions.md
```

**❌ Bad:**

```markdown
# Naming Conventions

**Last Updated:** 2025-01-15
**Version:** 1.2.3

## Purpose
...
```

---

### CHANGELOG.md

**Rule:**
- Maintain `CHANGELOG.md` at project root for significant updates
- Follow [Keep a Changelog](https://keepachangelog.com/) format
- Use Semantic Versioning for releases

**Example:**

```markdown
# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Documentation writing guide with 5 core documents

### Changed
- Restructured coding standards into shared submodule

### Deprecated
- Old singleton-based patterns

### Removed
- Redundant UI standards documentation

### Fixed
- Broken links in architecture documentation

## [1.0.0] - 2025-01-15

### Added
- Initial coding standards documentation
- EventChannel pattern examples
- ScriptableObject best practices
```

---

### Semantic Versioning

**Rule:**
- Use `MAJOR.MINOR.PATCH` format
- MAJOR: Breaking changes or major restructuring
- MINOR: New content or non-breaking additions
- PATCH: Fixes, typos, minor clarifications

**Examples:**
- `1.0.0` - Initial release
- `1.1.0` - Added new documentation section
- `1.1.1` - Fixed typos and broken links
- `2.0.0` - Major restructure of documentation

---

## File Metadata - P3

### YAML Frontmatter

**Rule:**
- Use YAML frontmatter for metadata when needed by publishing tools
- Place at the very beginning of the document
- Keep minimal and relevant

**Example:**

```markdown
---
title: Naming Conventions
date: 2025-01-15
author: Development Team
status: Active
version: 1.0.0
tags: [coding-standards, naming, conventions]
---

# Naming conventions

## Purpose
...
```

**Common Fields:**
- `title`: Document title
- `date`: Creation or publication date (YYYY-MM-DD)
- `author`: Author or team name
- `status`: Active, Draft, Deprecated, Archived
- `version`: Semantic version number
- `tags`: Array of relevant tags

---

## Directory Naming - P1

### Kebab-Case for Directories

**Rule:**
- Use kebab-case for directory names
- Keep names short and descriptive
- Use numbers for ordering when needed

**✅ Good:**

```
docs/
├── 00_spec/
├── 01_overview/
├── 02_game_design/
├── 03_technical/
│   ├── coding_standards/
│   │   ├── documentation/
│   │   └── examples/
```

**❌ Bad:**

```
docs/
├── Specifications/
├── Project_Overview/
├── gameDesign/
├── Technical Documentation/
```

---

### Numbered Prefixes

**Rule:**
- Use numbered prefixes for enforcing order
- Format: `00_`, `01_`, `02_`, etc.
- Use for sequential or priority-based organization

**Example:**

```
docs/
├── 00_spec/           # First: Specifications
├── 01_overview/       # Second: Overview
├── 02_game_design/    # Third: Game Design
├── 03_technical/      # Fourth: Technical
├── 98_plans/          # Near-end: Plans
└── 99_ideas/          # Last: Ideas
```

---

## Asset Organization - P2

### Documentation Assets

**Rule:**
- Store images in `images/` or `assets/` subdirectory
- Use descriptive file names for assets
- Keep asset files close to referencing documents

**Example:**

```
docs/03_technical/coding_standards/architecture/
├── event-channels.md
├── images/
│   ├── event-channel-flow.png
│   ├── scriptableobject-graph.png
│   └── runtime-set-diagram.png
```

---

### Asset File Size

**P2 Guidelines:**
- Images: < 1MB per file
- Diagrams: Prefer SVG when possible
- Screenshots: Optimize PNG compression

**P3 Guidelines:**
- Use lossy compression for photos (JPEG 80-90% quality)
- Use lossless compression for diagrams (PNG)
- Consider WebP format for modern browsers

---

## Cross-Project Reusability - P1

### Shared Documentation Repositories

**Rule:**
- Use Git submodules for shared documentation
- Keep shared content project-agnostic
- Use `ProjectName` placeholder in examples

**Example:**

```
ProjectA/
└── docs/
    └── 03_technical/
        └── coding_standards/  (Git submodule)

ProjectB/
└── docs/
    └── 03_technical/
        └── coding_standards/  (Same submodule)
```

---

### Submodule Configuration

**`.gitmodules` Example:**

```ini
[submodule "docs/03_technical/coding_standards"]
    path = docs/03_technical/coding_standards
    url = https://github.com/your-org/coding-standards.git
```

**Commands:**

```bash
# Add submodule
git submodule add https://github.com/your-org/coding-standards.git docs/03_technical/coding_standards

# Update submodule
git submodule update --remote docs/03_technical/coding_standards

# Clone project with submodules
git clone --recursive https://github.com/your-org/project.git
```

---

## Archiving and Deprecation - P3

### Marking Deprecated Documents

**Rule:**
- Add deprecation notice at the top
- Link to replacement document
- Keep file accessible for reference

**Example:**

```markdown
# Old Architecture Pattern

> **⚠️ DEPRECATED:** This document is deprecated as of 2025-01-15. See [Event-Driven Architecture](event-channels.md) for the current approach.

## Purpose

This document describes the legacy architecture pattern...
```

---

### Moving to Archive

**Rule:**
- Create `archive/` directory for old documents
- Move deprecated files instead of deleting
- Update links to archived files

**Example:**

```
docs/03_technical/
├── coding_standards/
│   ├── archive/
│   │   ├── old-singleton-pattern.md
│   │   └── legacy-ui-guidelines.md
│   ├── core/
│   └── architecture/
```

---

## Verification Checklist

Before committing documentation files:

- [ ] File names use kebab-case
- [ ] File names are under 30 characters
- [ ] No spaces or special characters in file names
- [ ] Directory structure is shallow (max 3-4 levels)
- [ ] README.md exists in major directories
- [ ] Git history used instead of "Last Updated" sections
- [ ] CHANGELOG.md updated for significant changes
- [ ] Assets organized in dedicated subdirectories
- [ ] Deprecated documents marked clearly

---

## References

- [Keep a Changelog](https://keepachangelog.com/)
- [Semantic Versioning](https://semver.org/)
- [Git Submodules Documentation](https://git-scm.com/book/en/v2/Git-Tools-Submodules)
- [GitHub Documentation Best Practices](https://github.blog/2021-11-18-repository-structure-best-practices/)

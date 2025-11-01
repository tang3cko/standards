# Documentation Writing Guide

## Purpose

This guide defines standards for writing technical documentation across all projects using these coding standards. It ensures consistency, clarity, and maintainability of documentation.

## Contents

### Core Guides (P1 Priority)

1. **[Document Structure](document-structure.md)** - Standard document organization and formatting
2. **[Code Examples](code-examples.md)** - Writing clear and consistent code examples
3. **[Markdown Formatting](markdown-formatting.md)** - Markdown syntax and formatting rules
4. **[File Organization](file-organization.md)** - File naming and directory structure
5. **[Writing Principles](writing-principles.md)** - Voice, tone, and style guidelines

---

## Quick Reference

### Document Template

All documentation should follow this structure:

```markdown
# Title

## Purpose

[What this document provides and why it exists]

## Checklist

- [ ] [Actionable item 1]
- [ ] [Actionable item 2]

---

## Content

[Main content sections]

---

## References

- [External resource 1]
- [External resource 2]
```

### Key Rules

- **Language**: Write all documentation in English
- **Code Namespace**: Use `ProjectName` as placeholder namespace
- **Headings**: Use ATX-style (`#`, `##`, `###`) with sentence case
- **File Names**: Use kebab-case, keep under 30 characters
- **Checklist Placement**: Always place immediately after Purpose section

---

## Priority Levels

- **P1 (Required)**: Required standards that must be followed
- **P2 (Recommended)**: Recommended standards that should be followed when possible
- **P3 (Optional)**: Optional enhancements for comprehensive documentation

Each guide indicates priority levels for specific rules.

---

## Integration with SpecKit

This documentation guide integrates with [SpecKit](https://github.com/github/spec-kit) workflow:

- **Spec documents**: Follow document-structure.md template
- **Plan documents**: Follow document-structure.md template
- **Tasks documents**: Follow document-structure.md template
- **Code examples**: Follow code-examples.md standards

---

## References

- [SpecKit - Spec-Driven Development](https://github.com/github/spec-kit)
- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/)
- [GitLab Documentation Style Guide](https://docs.gitlab.com/ee/development/documentation/styleguide/)
- [Write the Docs Best Practices](https://www.writethedocs.org/guide/writing/beginners-guide-to-docs/)

# Documentation writing guide

## Purpose

This guide defines standards for writing technical documentation across all projects. It ensures consistency, clarity, and maintainability of documentation.

This page summarizes the most important rules. For details, see each guide.

---

## Highlights

### Language and grammar

- Write all documentation in English using American spelling
- Use active voice instead of passive voice
- Address the reader as "you" (second person)
- Use present tense for describing behavior
- Keep sentences short and simple (one idea per sentence)
- Use serial comma (Oxford comma) in lists (A, B, and C)
- Define technical terms on first use

### Formatting

- Use ATX-style headings (`#`, `##`, `###`) with sentence case
- Use proper heading hierarchy (don't skip levels like H1 → H3)
- Use `-` for unordered lists (not `*` or `+`)
- Always specify language identifier in code blocks (e.g., ```csharp)
- Use backticks for inline code, class names, methods, and technical terms
- Use `---` horizontal rules to separate major sections
- Add blank lines before and after code blocks, lists, and headings

### Links and images

- Use descriptive link text that explains the destination (avoid "click here")
- Use relative paths for internal documentation links
- Provide descriptive alt text for all images

### Structure

- Start with a Purpose section explaining what the document covers
- Use H2 (`##`) for main sections, H3 (`###`) for subsections
- Place References section at the end
- Keep content focused on a single topic

### Files and naming

- Use kebab-case for file and directory names (e.g., `api-reference.md`)
- Keep file names under 30 characters
- Keep directory hierarchy shallow (max 3-4 levels)
- Use `README.md` only for top-level `docs/` directories

### Code examples

- Use `ProjectName` as placeholder namespace in examples
- Include both good (✅) and bad (❌) examples when showing patterns
- Provide context (imports, class declarations) for code snippets
- Ensure all examples are correct and follow coding standards
- Add comments to explain non-obvious logic

### Inclusivity and accessibility

- Use inclusive technical terms (allowlist/blocklist, primary/replica, main branch)
- Use gender-neutral language (they/their for singular indefinite pronouns)
- Avoid ableist terms (use "validation check" instead of "sanity check")
- Don't rely on color alone to convey meaning

---

## Guides

| Guide | Description |
|-------|-------------|
| [Writing principles](writing-principles.md) | Voice, tone, grammar, and style guidelines |
| [Document structure](document-structure.md) | Standard document organization |
| [Markdown formatting](markdown-formatting.md) | Markdown syntax and formatting rules |
| [File organization](file-organization.md) | File naming and directory structure |
| [Code examples](code-examples.md) | Writing clear and consistent code examples |

---

## References

- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/)
- [GitLab Documentation Style Guide](https://docs.gitlab.com/ee/development/documentation/styleguide/)
- [Write the Docs Best Practices](https://www.writethedocs.org/guide/writing/beginners-guide-to-docs/)

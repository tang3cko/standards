# Document structure

## Purpose

This document defines the standard structure for technical documentation. Following this structure ensures consistency, scannability, and maintainability across all projects.

---

## Standard structure - P1

All documentation should follow this basic structure:

```markdown
# Title

## Purpose

Brief explanation of what this document covers and why it matters.
Optionally, state what is NOT covered (scope boundaries).

---

## Section 1

Content...

---

## Section 2

Content...

---

## References

- [Related Document 1](path/to/doc1.md)
- [External Resource](https://example.com)
```

---

## Sections

### Title (H1) - P1

**Rules:**
- Single H1 heading at the document start
- Use sentence case (e.g., "Error handling patterns" not "Error Handling Patterns")
- Clear and descriptive
- No emoji or decorations

**Example:**

```markdown
# API authentication guide
```

---

### Purpose - P1

**Rules:**
- Always the first section after the title
- Explain what the document covers and why it exists
- Optionally include what is NOT covered (scope boundaries)
- Keep it brief (1-3 sentences)

**Example:**

```markdown
## Purpose

This document defines error handling patterns for backend services. It covers exception handling, logging standards, and error response formats.

This document does not cover client-side error handling or UI error messages.
```

---

### Content sections - P1

**Rules:**
- Use H2 (`##`) for main sections
- Use H3 (`###`) for subsections
- Separate major H2 sections with `---` horizontal rules
- Keep hierarchy shallow (avoid H4 and deeper when possible)
- Use descriptive section names

**Example:**

```markdown
---

## Authentication

### API keys

Use API keys for server-to-server communication...

### OAuth 2.0

Use OAuth 2.0 for user-facing applications...

---

## Error handling

### Error response format

All errors should return a consistent JSON structure...
```

---

### References - P2

**Rules:**
- Place at the end of the document
- Use simple markdown links
- Link to related documents and external resources
- Separate with `---` horizontal rule before this section

**Example:**

```markdown
---

## References

- [Error codes reference](error-codes.md)
- [REST API Guidelines](https://github.com/microsoft/api-guidelines)
```

---

## Section separators - P1

**Rules:**
- Use `---` to separate major H2 sections
- Place exactly one blank line before and after `---`
- Do not overuse (only between major sections, not between every subsection)

**Example:**

```markdown
## Section 1

Content...

---

## Section 2

Content...
```

---

## YAML frontmatter - P3

For documents that require metadata (e.g., for static site generators or documentation tools):

```markdown
---
title: API authentication guide
status: active
---

# API authentication guide

## Purpose

...
```

**Common fields:**
- `title`: Document title
- `status`: active, draft, deprecated, archived

**Note:** Git history is the primary source of truth for authorship and dates. Only use frontmatter when required by tooling.

---

## Anti-patterns

### Unnecessary decoration - P1

**❌ Bad:**
```markdown
### 🔹 Core features

**Important:** Use consistent naming.
```

**✅ Good:**
```markdown
### Core features

Use consistent naming.
```

---

### Overuse of bold - P1

**❌ Bad:**
```markdown
Use **camelCase** for **variables** and **PascalCase** for **classes**.
```

**✅ Good:**
```markdown
Use camelCase for variables and PascalCase for classes.
```

Use bold sparingly, only for critical emphasis or UI element names.

---

### Skipping heading levels - P1

**❌ Bad:**
```markdown
# Title

### Subsection (skipped H2)
```

**✅ Good:**
```markdown
# Title

## Section

### Subsection
```

---

## References

- [Google developer documentation style guide](https://developers.google.com/style)
- [GitLab documentation structure](https://docs.gitlab.com/ee/development/documentation/structure.html)

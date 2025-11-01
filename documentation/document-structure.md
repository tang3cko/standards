# Document Structure

## Purpose

This document defines the standard structure for all technical documentation, including coding standards, SpecKit documents (spec/plan/tasks), and general documentation files. Following this structure ensures consistency, scannability, and maintainability across all projects.

## Checklist

- [ ] Follow Title → Purpose → Checklist → Content → References structure
- [ ] Place Checklist immediately after Purpose section
- [ ] Use ATX-style headings (`#`, `##`, `###`)
- [ ] Separate major sections with `---` horizontal rules
- [ ] Place References as the last section
- [ ] Keep checklist items actionable and specific

---

## Standard Document Template

All documentation must follow this structure:

```markdown
# Title

## Purpose

Brief explanation of what this document covers and why it matters.

## Checklist

- [ ] Actionable item 1
- [ ] Actionable item 2
- [ ] Actionable item 3

---

## Section 1

### Subsection 1.1

Content...

### Subsection 1.2

Content...

---

## Section 2

### Subsection 2.1

Content...

---

## References

- [Related Document 1](path/to/doc1.md)
- [Related Document 2](path/to/doc2.md)
```

---

## Required Sections

### 1. Title (H1) - P1

**Rules:**
- Single H1 heading at the document start
- Use sentence case (e.g., "Naming conventions" not "Naming Conventions")
- Clear and descriptive
- No emoji or decorations

**Example:**

```markdown
# Error handling patterns
```

---

### 2. Purpose (H2) - P1

**Rules:**
- Always the second section (immediately after Title)
- Explains what the document covers and why it exists
- 1-3 sentences for clarity
- Use clear, concise language

**Example:**

```markdown
## Purpose

This document defines error handling patterns for Unity projects. It covers null safety, try-catch usage, and logging standards to prevent runtime errors and improve debugging efficiency.
```

---

### 3. Checklist (H2) - P1

**Rules:**
- Always the third section (immediately after Purpose)
- Never place at the end as "Summary"
- Use GitHub-style task list syntax (`- [ ]`)
- Items must be actionable and independently checkable
- Order from most important to least important
- Use imperative mood (e.g., "Use X", "Avoid Y", "Cache Z")

**Example:**

```markdown
## Checklist

- [ ] Use null conditional operator (?.) for safe member access
- [ ] Null check SerializeFields in Start/Awake
- [ ] Wrap file I/O operations in Try-Catch blocks
- [ ] Include [ClassName] prefix in all Debug.Log messages
```

**Anti-Pattern (❌ Bad):**

```markdown
---

## Summary

### Checklist

- [ ] Item 1
```

---

### 4. Content Sections (H2/H3) - P1

**Rules:**
- Use H2 (`##`) for main sections
- Use H3 (`###`) for subsections
- Separate major H2 sections with `---` horizontal rules
- Keep hierarchy shallow (avoid H4+ when possible)
- Use descriptive section names

**Example:**

```markdown
---

## Null Safety

### Null Conditional Operator

Use `?.` to safely access members...

### SerializeField Validation

Always validate SerializeFields in Start or Awake...

---

## Try-Catch Guidelines

### When to Use Try-Catch

Wrap operations that may throw exceptions...
```

---

### 5. References (H2) - P1

**Rules:**
- Always the last section
- Use simple markdown links without bold or emoji
- Link to related documents, external resources, or tools
- Separate with `---` horizontal rule before this section

**Example (✅ Good):**

```markdown
---

## References

- [Naming Conventions](naming-conventions.md)
- [Code Organization](code-organization.md)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
```

**Anti-Pattern (❌ Bad):**

```markdown
### References

- **Naming Conventions**: [naming-conventions.md](naming-conventions.md)
- 📘 **Code Organization**: [code-organization.md](code-organization.md)
```

---

## Section Separators - P1

**Rules:**
- Use `---` to separate major H2 sections
- Place exactly one blank line before and after `---`
- Do not overuse (only between major sections)

**Example:**

```markdown
## Section 1

Content...

---

## Section 2

Content...
```

---

## SpecKit Document Integration - P2

### Spec Document Structure

```markdown
# [Feature Name] Specification

## Purpose

Defines the requirements and behavior for [feature].

## Checklist

- [ ] Define user requirements
- [ ] Specify acceptance criteria
- [ ] Identify edge cases

---

## Requirements

### Functional Requirements

...

### Non-Functional Requirements

...

---

## References

- [Plan Document](../98_plans/[feature]-plan.md)
```

---

### Plan Document Structure

```markdown
# [Feature Name] Implementation Plan

## Purpose

Outlines the technical approach for implementing [feature].

## Checklist

- [ ] Define architecture approach
- [ ] Identify dependencies
- [ ] Estimate timeline

---

## Technical Approach

### Architecture

...

### Dependencies

...

---

## References

- [Spec Document](../00_spec/[feature]-spec.md)
- [Tasks Document](tasks/[feature]-tasks.md)
```

---

### Tasks Document Structure

```markdown
# [Feature Name] Tasks

## Purpose

Breaks down [feature] implementation into actionable tasks.

## Checklist

- [ ] Complete Task 1
- [ ] Complete Task 2
- [ ] Complete Task 3

---

## Task 1: [Task Name]

### Description

...

### Acceptance Criteria

- [ ] Criterion 1
- [ ] Criterion 2

---

## Task 2: [Task Name]

...

---

## References

- [Plan Document](../ [feature]-plan.md)
```

---

## YAML Frontmatter - P3

For advanced use cases, add YAML frontmatter at the document start:

```markdown
---
title: Error Handling Patterns
date: 2025-01-15
author: Development Team
status: Active
version: 1.0.0
---

# Error handling patterns

## Purpose

...
```

**Fields:**
- `title`: Document title
- `date`: Creation or last update date (YYYY-MM-DD)
- `author`: Author or team name
- `status`: Active, Draft, Deprecated, Archived
- `version`: Semantic version (optional)

**Note:** Git history is the primary source of truth for update dates. Only use frontmatter when metadata is required for tooling or publishing.

---

## Anti-Patterns to Avoid

### Unnecessary Decoration - P1

**❌ Bad:**
```markdown
### 🔹 Core Features

**Important:** Use PascalCase for all class names.
```

**✅ Good:**
```markdown
### Core Features

Use PascalCase for all class names.
```

---

### Checklist at End - P1

**❌ Bad:**
```markdown
## References

- [Doc 1](doc1.md)

---

## Summary

- [ ] Item 1
- [ ] Item 2
```

**✅ Good:**
```markdown
## Checklist

- [ ] Item 1
- [ ] Item 2

---

## Content Section

...

---

## References

- [Doc 1](doc1.md)
```

---

### Overuse of Bold - P1

**❌ Bad:**
```markdown
Use **PascalCase** for **classes** and **camelCase** for **private fields**.
```

**✅ Good:**
```markdown
Use PascalCase for classes and camelCase for private fields.
```

Use bold sparingly, only for critical emphasis.

---

## Verification Checklist

Before completing any document, verify:

- [ ] Title (H1) exists and uses sentence case
- [ ] Purpose section is second
- [ ] Checklist section is third (immediately after Purpose)
- [ ] Content sections use H2/H3 with `---` separators
- [ ] References section is last
- [ ] No "Summary" section exists at the end
- [ ] Bold and emojis used only with clear purpose
- [ ] All checklist items are actionable

---

## References

- [SpecKit Templates](https://github.com/github/spec-kit/tree/main/templates)
- [Google Documentation Structure Guide](https://developers.google.com/style/document-structure)
- [GitLab Documentation Structure](https://docs.gitlab.com/ee/development/documentation/structure.html)

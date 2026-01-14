---
name: documentation
description: Technical documentation writing standards. Covers writing style, document structure, markdown formatting, file organization, and code examples. Use when writing or editing markdown documentation.
---

# Documentation writing standards

Apply Tang3cko documentation standards when writing technical documentation.

## Decision tree

```
What are you doing?
│
├─▶ Writing new documentation
│   ├─ Voice and tone → [Writing principles](./references/writing-principles.md)
│   └─ Document layout → [Document structure](./references/document-structure.md)
│
├─▶ Formatting questions
│   ├─ Markdown syntax → [Markdown formatting](./references/markdown-formatting.md)
│   └─ File/folder naming → [File organization](./references/file-organization.md)
│
└─▶ Adding code examples
    └─ Code style in docs → [Code examples](./references/code-examples.md)
```

---

## Quick reference

### Language and style (P1)

- Write in English (American spelling)
- Use active voice, present tense
- Address reader as "you"
- Keep sentences short (one idea per sentence)
- Use serial comma (A, B, and C)

### Markdown formatting (P1)

- Headings: ATX-style (`#`, `##`), sentence case
- Lists: Use `-` (not `*` or `+`)
- Code blocks: Always specify language (```csharp)
- Inline code: Backticks for code, class names, methods
- Sections: Separate with `---` horizontal rules

### File naming (P1)

- Use kebab-case: `api-reference.md`
- Keep under 30 characters
- Max 3-4 directory levels

### Document structure (P1)

1. Purpose section (what the document covers)
2. Main content with H2/H3 hierarchy
3. References section at end

---

## Priority levels

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

---

## References

- [Writing principles](./references/writing-principles.md) - Voice, tone, grammar, style
- [Document structure](./references/document-structure.md) - Standard document organization
- [Markdown formatting](./references/markdown-formatting.md) - Markdown syntax rules
- [File organization](./references/file-organization.md) - File naming, directory structure
- [Code examples](./references/code-examples.md) - Writing clear code examples

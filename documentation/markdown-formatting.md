# Markdown Formatting

## Purpose

This document defines Markdown syntax and formatting standards for all technical documentation. Consistent Markdown usage ensures readability, maintainability, and proper rendering across different platforms.

## Checklist

- [ ] Use ATX-style headings (`#`, `##`, `###`)
- [ ] Apply sentence case to headings
- [ ] Specify language in code blocks
- [ ] Use backticks for inline code and technical terms
- [ ] Use serial comma in lists
- [ ] Use descriptive link text (avoid "click here")
- [ ] Add alt text to all images

---

## Headings - P1

### ATX-Style Headings

**Rule:**
- Use ATX-style (`#`) headings, not Setext-style (`===` or `---`)
- Add one space after `#` symbols
- Limit heading depth to H3 (`###`) when possible

**✅ Good:**

```markdown
# Top-Level Heading

## Second-Level Heading

### Third-Level Heading
```

**❌ Bad:**

```markdown
Top-Level Heading
=================

Second-Level Heading
--------------------

####Fourth-Level Heading (no space)
```

---

### Sentence Case

**Rule:**
- Use sentence case for headings (capitalize only the first word and proper nouns)
- Exception: Technical terms that require specific capitalization (e.g., "ScriptableObject", "EventChannel")

**✅ Good:**

```markdown
## Error handling patterns
## ScriptableObject design principles
## Using the Unity profiler
```

**❌ Bad:**

```markdown
## Error Handling Patterns
## SCRIPTABLEOBJECT DESIGN PRINCIPLES
## Using The Unity Profiler
```

---

### Heading Hierarchy

**Rule:**
- Don't skip heading levels (e.g., H1 → H3)
- Use H1 (`#`) only once per document (for title)
- Use H2 (`##`) for main sections
- Use H3 (`###`) for subsections

**✅ Good:**

```markdown
# Document Title

## Main Section

### Subsection

### Another Subsection

## Another Main Section
```

**❌ Bad:**

```markdown
# Document Title

### Subsection (skipped H2)

## Main Section (after H3)
```

---

## Code Formatting - P1

### Code Blocks

**Rule:**
- Use triple backticks (```) for code blocks
- Always specify language identifier in lowercase
- Add blank lines before and after code blocks

**Supported Languages:**
- `csharp` - C# code
- `json` - JSON data
- `yaml` - YAML configuration
- `xml` - XML markup
- `bash` - Shell commands
- `markdown` - Markdown examples

**✅ Good:**

````markdown
Use `GameObject.Find()` sparingly in production code:

```csharp
private GameObject player;

private void Start()
{
    // Cache reference once
    player = GameObject.Find("Player");
}
```
````

**❌ Bad:**

````markdown
Use GameObject.Find() sparingly:
```
private GameObject player;
private void Start() {
    player = GameObject.Find("Player");
}
```
````

---

### Inline Code

**Rule:**
- Use single backticks for inline code, class names, methods, variables
- Use for technical terms and Unity-specific concepts
- Do not use for emphasis

**✅ Good:**

```markdown
The `PlayerHealth` class uses the `TakeDamage()` method to reduce `currentHealth`. All `MonoBehaviour` components should cache `GameObject` references.
```

**❌ Bad:**

```markdown
The PlayerHealth class uses the TakeDamage() method. Use `emphasis` for important points.
```

---

## Lists - P1

### Unordered Lists

**Rule:**
- Use `-` (hyphen) for unordered lists, not `*` or `+`
- Add one space after the hyphen
- Use consistent indentation (2 spaces for nested lists)

**✅ Good:**

```markdown
- First item
- Second item
  - Nested item 1
  - Nested item 2
- Third item
```

**❌ Bad:**

```markdown
* First item
+ Second item
    * Nested (inconsistent indentation)
```

---

### Ordered Lists

**Rule:**
- Use `1.`, `2.`, `3.` for ordered lists
- Or use `1.` for all items (Markdown auto-numbers)
- Add one space after the period

**✅ Good:**

```markdown
1. First step
2. Second step
3. Third step
```

or

```markdown
1. First step
1. Second step
1. Third step
```

---

### Serial Comma

**Rule:**
- Use serial comma (Oxford comma) in lists within sentences
- Example: "A, B, and C" not "A, B and C"

**✅ Good:**

```markdown
This pattern uses ScriptableObjects, EventChannels, and RuntimeSets.
```

**❌ Bad:**

```markdown
This pattern uses ScriptableObjects, EventChannels and RuntimeSets.
```

---

### Task Lists

**Rule:**
- Use GitHub-style task lists for checklists
- Format: `- [ ]` for unchecked, `- [x]` for checked
- One space after brackets

**✅ Good:**

```markdown
- [ ] Implement health system
- [x] Create player controller
- [ ] Add enemy AI
```

---

## Links - P1

### Descriptive Link Text

**Rule:**
- Use descriptive text that explains the link destination
- Avoid generic phrases like "click here", "this link", "read more"

**✅ Good:**

```markdown
See the [Unity Scripting API](https://docs.unity3d.com/ScriptReference/) for details.

Read more about [event-driven architecture patterns](event-channels.md).
```

**❌ Bad:**

```markdown
Click [here](https://docs.unity3d.com/ScriptReference/) for details.

Read more [here](event-channels.md).
```

---

### Internal Links

**Rule:**
- Use relative paths for internal documentation links
- Include `.md` extension
- Use kebab-case for file names

**✅ Good:**

```markdown
- [Naming Conventions](../core/naming-conventions.md)
- [Error Handling](../core/error-handling.md)
```

**❌ Bad:**

```markdown
- [Naming Conventions](/docs/core/naming-conventions)
- [Error Handling](https://myproject.com/docs/error-handling)
```

---

### External Links

**Rule:**
- Use full URLs for external links
- Open external links in new tab when appropriate (not controlled by Markdown)

**✅ Good:**

```markdown
- [Unity Documentation](https://docs.unity3d.com/)
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/)
```

---

## Emphasis - P2

### Bold

**Rule:**
- Use `**text**` for bold (not `__text__`)
- Use sparingly for critical emphasis only
- Do not overuse bold for every technical term

**✅ Good:**

```markdown
**Warning:** This operation is irreversible.

The `Start()` method runs once before the first frame update.
```

**❌ Bad:**

```markdown
Use **PascalCase** for **classes** and **camelCase** for **fields**.
```

---

### Italic

**Rule:**
- Use `*text*` for italic (not `_text_`)
- Use for introducing new terms or subtle emphasis

**✅ Good:**

```markdown
The *singleton pattern* should be avoided in favor of ScriptableObjects.
```

---

### Strikethrough

**Rule:**
- Use `~~text~~` for strikethrough (when supported)
- Use to show deprecated or outdated information

**Example:**

```markdown
~~Use Singleton pattern~~ → Use ScriptableObject pattern instead.
```

---

## Images and Diagrams - P2

### Image Syntax

**Rule:**
- Use `![alt text](path/to/image.png)` format
- Always provide descriptive alt text
- Use relative paths for images
- Store images in `images/` or `assets/` directory

**✅ Good:**

```markdown
![ScriptableObject dependency graph showing event flow](images/scriptableobject-graph.png)
```

**❌ Bad:**

```markdown
![](image.png)
```

---

### Image Guidelines

**P2 Rules:**
- Optimize image file size (< 1MB recommended)
- Use PNG for diagrams and screenshots
- Use JPEG for photos
- Use SVG for vector graphics when possible

**P3 Rules:**
- Add captions below images using italic text
- Specify image dimensions when layout matters

**Example:**

```markdown
![EventChannel architecture diagram](images/event-channels.png)

*Figure 1: EventChannel decouples publishers from subscribers*
```

---

## Tables - P2

### Basic Table Format

**Rule:**
- Use pipe (`|`) for columns
- Use hyphens (`-`) for header separator
- Align columns for readability (optional but recommended)

**✅ Good:**

```markdown
| Component      | Responsibility          | Dependencies    |
|----------------|-------------------------|-----------------|
| GameManager    | Core game loop          | EventChannels   |
| PlayerHealth   | Health management       | IntEventChannel |
| ScoreUI        | Score display           | IntEventChannel |
```

---

### Table Alignment

**Rule:**
- Use `:` for column alignment
- `:---` (left), `:---:` (center), `---:` (right)

**Example:**

```markdown
| Left-aligned | Center-aligned | Right-aligned |
|:-------------|:--------------:|--------------:|
| Text         | Text           | 123           |
| Text         | Text           | 456           |
```

---

## Horizontal Rules - P1

**Rule:**
- Use `---` for horizontal rules (three hyphens)
- Add blank lines before and after
- Use to separate major sections

**✅ Good:**

```markdown
## Section 1

Content for section 1.

---

## Section 2

Content for section 2.
```

**❌ Bad:**

```markdown
## Section 1
Content.
***
## Section 2
```

---

## Blockquotes - P2

**Rule:**
- Use `>` for blockquotes
- Add one space after `>`
- Use for quotes, notes, or callouts

**Example:**

```markdown
> **Note:** ScriptableObjects persist across scene loads. Remember to reset runtime state in `OnEnable()`.
```

---

## Escaping Characters - P3

**Rule:**
- Use backslash (`\`) to escape Markdown syntax
- Common escapes: `\*`, `\_`, `\#`, `\[`, `\]`

**Example:**

```markdown
Use \*asterisks\* to create italic text.

File names should use kebab\-case.
```

---

## Line Length - P3

**Rule:**
- Soft limit: 80-100 characters per line
- Hard limit: 120 characters per line
- Break long sentences at natural points

**Benefits:**
- Easier to review in diff tools
- Better readability in side-by-side views
- Cleaner Git diffs

---

## Markdown Linters - P3

**Recommended Tools:**
- `markdownlint` - Linting and style checking
- `prettier` - Auto-formatting
- `remark` - Markdown processor

**Common Rules:**
- MD001: Heading levels should only increment by one
- MD003: Heading style (ATX)
- MD004: Unordered list style (dash)
- MD022: Headings should be surrounded by blank lines
- MD031: Fenced code blocks should be surrounded by blank lines

---

## Verification Checklist

Before publishing Markdown documentation:

- [ ] All headings use ATX-style with sentence case
- [ ] Code blocks specify language identifiers
- [ ] Inline code used for technical terms
- [ ] Lists use consistent formatting
- [ ] Links use descriptive text
- [ ] Images include alt text
- [ ] Horizontal rules separate major sections
- [ ] No unnecessary bold or emphasis
- [ ] Serial comma used in lists

---

## References

- [CommonMark Specification](https://commonmark.org/)
- [GitHub Flavored Markdown](https://github.github.com/gfm/)
- [Markdown Guide](https://www.markdownguide.org/)
- [Google Markdown Style Guide](https://google.github.io/styleguide/docguide/style.html)
- [GitLab Markdown Guide](https://docs.gitlab.com/ee/user/markdown.html)

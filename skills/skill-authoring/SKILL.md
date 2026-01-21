---
name: skill-authoring
description: Create Claude Code Skills following Anthropic official requirements and Tang3cko conventions. Use when creating, editing, or reviewing skills in this repository.
---

# Skill Authoring Guide

## Purpose

Create effective Claude Code Skills by following Anthropic's official requirements and Tang3cko project conventions. This skill distinguishes between mandatory technical requirements and project-specific style choices.

## Checklist

- [ ] Frontmatter has only `name` and `description` (Anthropic requirement)
- [ ] Description includes "what it does" + "when to use" (Anthropic requirement)
- [ ] SKILL.md body under 500 lines (Anthropic requirement)
- [ ] No README, CHANGELOG, or auxiliary files (Anthropic requirement)
- [ ] Follow Tang3cko documentation standards (project convention)

---

## Anthropic official requirements - P1

These requirements come from the official skill-creator and must be followed.

### Frontmatter

Only `name` and `description` are allowed:

```yaml
---
name: your-skill-name
description: What it does. When to use it.
---
```

**Do not include any other fields.** Fields like `allowed-tools`, `model`, `context` are implementation details, not part of the skill specification.

### Description is the trigger

The description is the **only** thing Claude reads to decide whether to use a skill. Include:

1. **What** the skill does
2. **When** to use it (specific triggers)

```yaml
# ✅ Good
description: Extract text and tables from PDF files. Use when working with PDF files or when the user mentions PDFs, forms, or document extraction.

# ❌ Bad
description: Helps with documents
```

### Context is a shared resource

From skill-creator: "Claude is already very smart. Only add context Claude doesn't already have."

```markdown
# ✅ Good (concise, actionable)
Use pdfplumber for text extraction:
` ` `python
import pdfplumber
with pdfplumber.open("file.pdf") as pdf:
    text = pdf.pages[0].extract_text()
` ` `

# ❌ Bad (explains what Claude already knows)
PDF files are a common format that contains text and images.
To extract text, you need a library...
```

### Progressive disclosure

Keep SKILL.md under 500 lines. Split content into separate files:

```
skill-name/
├── SKILL.md              # Core instructions (<500 lines)
├── references/           # Documentation for Claude to read
│   └── advanced.md
├── scripts/              # Executable code
│   └── helper.py
└── assets/               # Files for output (templates, images)
    └── template.docx
```

**Keep references one level deep.** Avoid: `SKILL.md → a.md → b.md → c.md`

### What NOT to include

From skill-creator: "Do NOT create extraneous documentation or auxiliary files":

- README.md
- INSTALLATION_GUIDE.md
- QUICK_REFERENCE.md
- CHANGELOG.md

The skill should only contain information for Claude to do the job.

### Degrees of freedom

Match specificity to task fragility:

| Freedom | Use when | Example |
|---------|----------|---------|
| **High** | Multiple approaches valid | Code review guidelines |
| **Medium** | Preferred pattern exists | Parameterized scripts |
| **Low** | Operations fragile | DB migrations, exact commands |

---

## Tang3cko project conventions - P1

These are project-specific style choices for consistency within this repository. See `repository-setup:documentation` skill for full details.

### Document structure

```markdown
# Title

## Purpose

Brief explanation of what this document covers.

## Checklist

- [ ] Key verification items

---

## Section name - P1

Content with priority label...
```

### Priority labels

Add priority labels to section headings:

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

### Code example format

Show bad examples before good examples:

````markdown
**❌ Bad:**

```csharp
// Problem code
```

**✅ Good:**

```csharp
// Correct code
```
````

### Namespace placeholder

Use `ProjectName` as namespace placeholder:

```csharp
namespace ProjectName.Core
{
    public class Example { }
}
```

---

## Common mistakes - P1

### Mistake 1: Extra frontmatter fields

```yaml
# ❌ Bad
---
name: my-skill
description: Does something
allowed-tools: Read, Write    # Not allowed
model: claude-sonnet          # Not allowed
---

# ✅ Good
---
name: my-skill
description: Does something. Use when X happens.
---
```

### Mistake 2: Description without trigger context

```yaml
# ❌ Bad - No "when to use"
description: Processes Unity code

# ✅ Good - Clear trigger
description: Unity C# naming conventions. PascalCase for classes, camelCase for fields. Use when naming classes, methods, or fields in .cs files.
```

### Mistake 3: Explaining what Claude knows

```markdown
# ❌ Bad
ScriptableObjects are Unity assets that store data independently of scenes.
They can be used to share data between components...

# ✅ Good
Use EventChannelSO for decoupled communication:
` ` `csharp
[CreateAssetMenu(menuName = "Events/Void Event Channel")]
public class VoidEventChannelSO : ScriptableObject { }
` ` `
```

### Mistake 4: Creating auxiliary files

```
# ❌ Bad
skill-name/
├── SKILL.md
├── README.md           # Don't create
├── CHANGELOG.md        # Don't create
└── QUICK_REFERENCE.md  # Don't create

# ✅ Good
skill-name/
├── SKILL.md
└── references/
    └── api.md          # OK: Reference for Claude
```

---

## Testing skills - P2

### Manual testing

1. Perform a task that should trigger the skill
2. Verify the skill actually triggers
3. Check if the content helps complete the task

### Test with different models

| Model | Check for |
|-------|-----------|
| **Haiku** | Does the skill provide enough guidance? |
| **Sonnet** | Is the skill clear and efficient? |
| **Opus** | Does the skill avoid over-explaining? |

### Iteration workflow

From skill-creator:

1. Use the skill on real tasks
2. Notice struggles or inefficiencies
3. Identify how SKILL.md should be updated
4. Implement changes and test again

---

## References

- **Anthropic skill-creator**: Official requirements (invoke with `/skill-creator`)
- **Tang3cko documentation**: Project conventions (invoke with `repository-setup:documentation`)

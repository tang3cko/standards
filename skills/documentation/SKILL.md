---
name: documentation
description: Technical documentation standards. Writing style, markdown formatting, file organization. Use when writing or editing markdown files.
---

# Documentation Writing Standards

## Purpose

Apply Tang3cko documentation standards when writing technical documentation. This skill covers voice, tone, markdown formatting, file organization, and code examples.

## Checklist

- [ ] Write in English (American spelling)
- [ ] Use active voice, present tense
- [ ] Address reader as "you"
- [ ] Use sentence case for headings
- [ ] Use `-` for unordered lists
- [ ] Always specify language in code blocks
- [ ] Use kebab-case for file names

---

## Writing principles - P1

### Voice and tone

- Write in a conversational but professional tone
- Be direct and to the point
- Avoid overly formal or academic language

```markdown
✅ Good:
Use EventChannels to decouple systems. This approach makes testing easier.

❌ Bad:
It is recommended that one should utilize EventChannels for the purpose of decoupling systems.
```

### Active voice

Use active voice instead of passive voice:

```markdown
✅ Good (Active):
The `GameManager` handles the game state transitions.
You can cache component references in `Awake()`.

❌ Bad (Passive):
Game state transitions are handled by the `GameManager`.
Component references can be cached in `Awake()`.
```

### Second person ("you")

Address the reader as "you":

```markdown
✅ Good:
You should validate SerializeFields in `OnValidate()`.

❌ Bad:
One should validate SerializeFields in `OnValidate()`.
```

### Serial comma

Use serial comma before "and" or "or" in lists:

```markdown
✅ Good:
This system uses ScriptableObjects, EventChannels, and RuntimeSets.

❌ Bad:
This system uses ScriptableObjects, EventChannels and RuntimeSets.
```

---

## Document structure - P1

### Standard structure

```markdown
# Title

## Purpose

Brief explanation of what this document covers.

---

## Section 1

Content...

---

## Section 2

Content...

---

## References

- [Related Document](path/to/doc.md)
```

### Section rules

- Single H1 heading at document start
- Use sentence case (e.g., "Error handling patterns" not "Error Handling Patterns")
- Use H2 (`##`) for main sections
- Use H3 (`###`) for subsections
- Separate major H2 sections with `---` horizontal rules
- Don't skip heading levels (H1 → H3)

---

## Markdown formatting - P1

### Headings

- Use ATX-style (`#`) headings
- Add one space after `#` symbols
- Use sentence case

```markdown
✅ Good:
## Error handling patterns

❌ Bad:
## Error Handling Patterns
```

### Code blocks

- Always specify language identifier in lowercase
- Add blank lines before and after code blocks

````markdown
✅ Good:
```csharp
public class Example : MonoBehaviour
{
    // Code
}
```

❌ Bad:
```
public class Example
```
````

Supported languages: `csharp`, `json`, `yaml`, `xml`, `bash`, `markdown`

### Inline code

Use single backticks for class names, methods, variables:

```markdown
✅ Good:
The `PlayerHealth` class uses the `TakeDamage()` method.

❌ Bad:
The PlayerHealth class uses the TakeDamage() method.
```

### Lists

Use `-` (hyphen) for unordered lists:

```markdown
✅ Good:
- First item
- Second item
  - Nested item

❌ Bad:
* First item
+ Second item
```

### Links

Use descriptive link text:

```markdown
✅ Good:
See the [Unity Scripting API](https://docs.unity3d.com/ScriptReference/) for details.

❌ Bad:
Click [here](https://docs.unity3d.com/ScriptReference/) for details.
```

---

## File organization - P1

### File naming

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

### Directory structure

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

### Directory naming

- Use kebab-case for directory names
- Keep names short and descriptive

---

## Code examples - P1

### Namespace rules

Always use `ProjectName` as the namespace placeholder:

```csharp
// ✅ Good
namespace ProjectName.Core
{
    public class GameManager : MonoBehaviour { }
}

// ❌ Bad
namespace MyGame.Core
{
    public class GameManager : MonoBehaviour { }
}
```

### Good vs bad format

Show bad examples before good examples with emojis:

````markdown
**❌ Bad:**

```csharp
// Hidden dependency
GameManager.Instance.UpdateScore();
```

**✅ Good:**

```csharp
// Dependencies visible in Inspector
[SerializeField] private IntEventChannelSO onScoreChanged;
```
````

### Provide context

Include class declaration and necessary imports:

```csharp
// ❌ Bad: Isolated snippet
currentHealth -= damage;

// ✅ Good: Full context
using UnityEngine;

namespace ProjectName.Core
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private IntEventChannelSO onHealthChanged;
        private int currentHealth;

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            onHealthChanged?.RaiseEvent(currentHealth);
        }
    }
}
```

---

## Inclusive language - P1

### Avoid ableist terms

| Avoid | Use Instead |
|-------|-------------|
| sanity check | validation check |
| crazy, insane | unexpected, complex |
| dummy | placeholder, mock |

### Inclusive technical terms

| Avoid | Use Instead |
|-------|-------------|
| whitelist/blacklist | allowlist/blocklist |
| master/slave | primary/replica |
| master branch | main branch |

### Gender-neutral language

Use "they/their" for singular indefinite pronouns:

```markdown
✅ Good:
When a developer creates a component, they should follow naming conventions.

❌ Bad:
When a developer creates a component, he should follow naming conventions.
```

---

## Priority levels - P1

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

---

## Quick reference - P1

| Category | Standard |
|----------|----------|
| Language | English (American spelling) |
| Voice | Active voice |
| Person | Second person ("you") |
| Comma | Serial comma (A, B, and C) |
| Headings | ATX-style, sentence case |
| Lists | Use `-` (hyphen) |
| File names | kebab-case, under 30 chars |
| Code blocks | Always specify language |

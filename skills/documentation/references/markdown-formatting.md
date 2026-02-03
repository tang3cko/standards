# Markdown Formatting

Markdown syntax rules and examples.

---

## Headings

- Use ATX-style (`#`) headings
- Add one space after `#` symbols
- Use sentence case

```markdown
✅ Good:
## Error handling patterns

❌ Bad:
## Error Handling Patterns
```

---

## Code Blocks

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

---

## Inline Code

Use single backticks for class names, methods, variables:

```markdown
✅ Good:
The `PlayerHealth` class uses the `TakeDamage()` method.

❌ Bad:
The PlayerHealth class uses the TakeDamage() method.
```

---

## Lists

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

---

## Links

Use descriptive link text:

```markdown
✅ Good:
See the [Unity Scripting API](https://docs.unity3d.com/ScriptReference/) for details.

❌ Bad:
Click [here](https://docs.unity3d.com/ScriptReference/) for details.
```

---

## Tables

Use tables for comparing options:

```markdown
| Option | Pros | Cons |
|--------|------|------|
| A | Fast | Complex |
| B | Simple | Slow |
```

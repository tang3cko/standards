# Writing Principles

Voice, tone, grammar, and inclusive language guidelines.

---

## Voice and Tone

Write in a conversational but professional tone. Be direct and to the point.

```markdown
✅ Good:
Use EventChannels to decouple systems. This approach makes testing easier.

❌ Bad:
It is recommended that one should utilize EventChannels for the purpose of decoupling systems.
```

---

## Active Voice

Use active voice instead of passive voice:

```markdown
✅ Good (Active):
The `GameManager` handles the game state transitions.
You can cache component references in `Awake()`.

❌ Bad (Passive):
Game state transitions are handled by the `GameManager`.
Component references can be cached in `Awake()`.
```

---

## Second Person ("you")

Address the reader as "you":

```markdown
✅ Good:
You should validate SerializeFields in `OnValidate()`.

❌ Bad:
One should validate SerializeFields in `OnValidate()`.
```

---

## Serial Comma

Use serial comma before "and" or "or" in lists:

```markdown
✅ Good:
This system uses ScriptableObjects, EventChannels, and RuntimeSets.

❌ Bad:
This system uses ScriptableObjects, EventChannels and RuntimeSets.
```

---

## Inclusive Language

### Avoid Ableist Terms

| Avoid | Use Instead |
|-------|-------------|
| sanity check | validation check |
| crazy, insane | unexpected, complex |
| dummy | placeholder, mock |

### Inclusive Technical Terms

| Avoid | Use Instead |
|-------|-------------|
| whitelist/blacklist | allowlist/blocklist |
| master/slave | primary/replica |
| master branch | main branch |

### Gender-Neutral Language

Use "they/their" for singular indefinite pronouns:

```markdown
✅ Good:
When a developer creates a component, they should follow naming conventions.

❌ Bad:
When a developer creates a component, he should follow naming conventions.
```

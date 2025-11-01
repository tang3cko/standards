# Writing Principles

## Purpose

This document defines voice, tone, grammar, and style guidelines for technical documentation. Following these principles ensures documentation is clear, accessible, and professional across all projects.

## Checklist

- [ ] Use active voice instead of passive voice
- [ ] Address the reader as "you" (second person)
- [ ] Use serial comma in lists
- [ ] Write in English with clear, concise language
- [ ] Follow accessibility best practices (headings, alt text, contrast)
- [ ] Use inclusive language (avoid ableist terms)
- [ ] Keep sentences short and simple

---

## Voice and Tone - P1

### Conversational and Clear

**Rule:**
- Write in a conversational but professional tone
- Avoid overly formal or academic language
- Be direct and to the point
- Use contractions sparingly in technical writing

**✅ Good:**

```markdown
Use EventChannels to decouple systems. This approach makes testing easier and reduces dependencies between components.
```

**❌ Bad:**

```markdown
It is recommended that one should utilize EventChannels for the purpose of decoupling systems. This approach facilitates the testing process and minimizes inter-component dependencies.
```

---

### Respectful and Professional

**Rule:**
- Be respectful of all readers
- Avoid condescending language
- Don't assume reader's skill level
- Provide context when needed

**✅ Good:**

```markdown
If you're new to ScriptableObjects, start with the [basic introduction](scriptableobject-intro.md) before diving into advanced patterns.
```

**❌ Bad:**

```markdown
Obviously, everyone knows ScriptableObjects. If you don't, you shouldn't be reading this.
```

---

## Grammar and Style - P1

### Active Voice

**Rule:**
- Use active voice instead of passive voice
- Subject performs the action
- Makes writing clearer and more direct

**✅ Good (Active):**

```markdown
The `GameManager` handles the game state transitions.

You can cache component references in `Awake()`.

EventChannels decouple publishers from subscribers.
```

**❌ Bad (Passive):**

```markdown
Game state transitions are handled by the `GameManager`.

Component references can be cached in `Awake()`.

Publishers are decoupled from subscribers by EventChannels.
```

---

### Second Person ("You")

**Rule:**
- Address the reader as "you" (second person)
- Avoid first person ("I", "we") in technical documentation
- Exception: Use "we" for collaborative context or team decisions

**✅ Good:**

```markdown
You should validate SerializeFields in `OnValidate()`.

Add the EventChannel to your scene's GameManager.

You can use RuntimeSets to avoid `FindObjectsOfType()` calls.
```

**❌ Bad:**

```markdown
One should validate SerializeFields in `OnValidate()`.

The developer must add the EventChannel to the scene.

We will use RuntimeSets to avoid `FindObjectsOfType()` calls.
```

---

### Serial Comma (Oxford Comma)

**Rule:**
- Use serial comma before "and" or "or" in lists
- Format: "A, B, and C" not "A, B and C"
- Reduces ambiguity

**✅ Good:**

```markdown
This system uses ScriptableObjects, EventChannels, and RuntimeSets.

You can implement health systems, inventory management, and quest tracking.
```

**❌ Bad:**

```markdown
This system uses ScriptableObjects, EventChannels and RuntimeSets.

You can implement health systems, inventory management and quest tracking.
```

---

### Present Tense

**Rule:**
- Use present tense for describing behavior
- Use imperative mood for instructions
- Use future tense sparingly

**✅ Good:**

```markdown
The `Start()` method initializes components.

Create a new EventChannel asset in the Project window.

The system will log errors when validation fails.
```

---

## Language and Clarity - P1

### English Language

**Rule:**
- Write all documentation in English
- Use American English spelling (e.g., "color" not "colour")
- Exception: Proper nouns and brand names

**Examples:**
- `color`, `center`, `optimize` (American)
- Unity, GameObject, MonoBehaviour (proper nouns)

---

### Clear and Concise

**Rule:**
- Avoid unnecessary words
- Break long sentences into shorter ones
- One main idea per sentence

**✅ Good:**

```markdown
Cache component references in `Awake()`. This avoids repeated `GetComponent()` calls and improves performance.
```

**❌ Bad:**

```markdown
It is generally considered a best practice to cache component references in the `Awake()` method, as doing so will help to avoid making repeated calls to `GetComponent()`, which can negatively impact performance.
```

---

### Avoid Jargon

**Rule:**
- Define technical terms on first use
- Link to glossary or detailed explanations
- Use common terminology when possible

**✅ Good:**

```markdown
Use EventChannels (observable ScriptableObjects that notify subscribers) to decouple systems.
```

**❌ Bad:**

```markdown
Leverage the pub-sub paradigm via SO-based event buses for orthogonal system decoupling.
```

---

## Accessibility - P2

### Heading Hierarchy

**Rule:**
- Use proper heading hierarchy (H1 → H2 → H3)
- Don't skip levels (H1 → H3)
- Use headings to structure content logically

**Benefits:**
- Screen readers can navigate by headings
- Improves document scannability
- Helps search engines understand structure

---

### Alt Text for Images

**Rule:**
- Provide descriptive alt text for all images
- Describe what the image shows, not just its title
- Keep alt text concise (1-2 sentences)

**✅ Good:**

```markdown
![Diagram showing EventChannel flow from publisher to multiple subscribers through a ScriptableObject](images/event-channel-flow.png)
```

**❌ Bad:**

```markdown
![EventChannel](images/event-channel-flow.png)
```

---

### Link Text Clarity

**Rule:**
- Use descriptive link text that explains the destination
- Avoid "click here" or "read more"
- Link text should make sense out of context

**✅ Good:**

```markdown
See the [Unity Profiler documentation](https://docs.unity3d.com/Manual/Profiler.html) for optimization tips.
```

**❌ Bad:**

```markdown
Click [here](https://docs.unity3d.com/Manual/Profiler.html) for more information.
```

---

### Color and Contrast

**Rule (P3):**
- Don't rely on color alone to convey meaning
- Use symbols or text in addition to color
- Ensure sufficient contrast for readability

**✅ Good:**

```markdown
✅ **Good:** Use EventChannels for decoupling.

❌ **Bad:** Use Singleton pattern for global state.
```

**❌ Bad:**

```markdown
<span style="color: green">Good:</span> Use EventChannels.

<span style="color: red">Bad:</span> Use Singleton pattern.
```

---

## Inclusive Language - P1

### Avoid Ableist Terms

**Rule:**
- Avoid terms that reference disabilities in a negative way
- Use neutral alternatives

**Examples:**

| ❌ Avoid          | ✅ Use Instead       |
|-------------------|----------------------|
| sanity check      | validation check     |
| crazy, insane     | unexpected, complex  |
| blind to          | unaware of           |
| crippled          | limited, restricted  |
| dummy             | placeholder, mock    |

---

### Inclusive Technical Terms

**Rule:**
- Use inclusive alternatives for loaded terms
- Industry is moving toward these standards

**Examples:**

| ❌ Avoid            | ✅ Use Instead         |
|---------------------|------------------------|
| whitelist/blacklist | allowlist/blocklist    |
| master/slave        | primary/replica        |
| master branch       | main branch            |
| grandfathered       | legacy, existing       |

---

### Gender-Neutral Language

**Rule:**
- Use "they/their" for singular indefinite pronouns
- Avoid gendered assumptions

**✅ Good:**

```markdown
When a developer creates a new component, they should follow the naming conventions.

The user can customize their settings in the preferences panel.
```

**❌ Bad:**

```markdown
When a developer creates a new component, he should follow the naming conventions.

The user can customize his settings in the preferences panel.
```

---

## Formatting for Readability - P2

### Short Paragraphs

**Rule:**
- Keep paragraphs short (3-5 sentences)
- Break up long blocks of text
- Use whitespace generously

**✅ Good:**

```markdown
EventChannels provide a decoupled communication pattern. Publishers raise events without knowing who subscribes.

Subscribers register callbacks to respond to events. This approach reduces dependencies and improves testability.
```

**❌ Bad:**

```markdown
EventChannels provide a decoupled communication pattern where publishers raise events without knowing who subscribes and subscribers register callbacks to respond to events and this approach reduces dependencies between systems and improves testability by allowing components to be tested in isolation without complex setup.
```

---

### Bullet Points and Lists

**Rule:**
- Use lists to present multiple items
- Each list item should be parallel in structure
- Introduce lists with a colon or complete sentence

**✅ Good:**

```markdown
EventChannels offer several benefits:

- Reduce coupling between systems
- Improve testability
- Enable parallel development
- Support multiple subscribers
```

**❌ Bad:**

```markdown
EventChannels are beneficial because they reduce coupling and you can test them easily and multiple subscribers can listen.
```

---

### Code Formatting

**Rule:**
- Use inline code formatting for technical terms
- Use code blocks for examples
- Always specify language in code blocks

See [Code Examples](code-examples.md) for detailed guidelines.

---

## Word Choice - P2

### Be Specific

**Rule:**
- Use precise technical terms
- Avoid vague words like "thing", "stuff", "very"

**✅ Good:**

```markdown
Cache the `Rigidbody` reference in `Awake()` to avoid repeated `GetComponent()` calls.
```

**❌ Bad:**

```markdown
Save the thing in `Awake()` to avoid calling the method a lot.
```

---

### Use Positive Language

**Rule:**
- Focus on what to do, not just what to avoid
- Provide alternatives when showing anti-patterns

**✅ Good:**

```markdown
Use EventChannels to decouple systems. Avoid Singleton pattern for global state.
```

**❌ Bad:**

```markdown
Don't use Singleton pattern. It's bad.
```

---

### Consistent Terminology

**Rule:**
- Use the same term for the same concept throughout documentation
- Create a glossary for project-specific terms
- Follow Unity's terminology for Unity-specific concepts

**Examples:**
- GameObject (not "game object" or "Game Object")
- ScriptableObject (not "Scriptable Object")
- MonoBehaviour (not "Monobehavior" or "MonoBehavior")

---

## Numbers and Units - P3

### Numbers in Text

**Rule:**
- Spell out numbers one through nine
- Use numerals for 10 and above
- Use numerals for technical values (e.g., versions, measurements)

**Examples:**

```markdown
Unity supports three rendering pipelines.

The pool holds 50 enemy instances.

Version 2.1.0 includes five new features.
```

---

### Units of Measurement

**Rule:**
- Include units with numerical values
- Use standard abbreviations (MB, KB, ms, FPS)
- Add space between number and unit

**Examples:**

```markdown
The texture is 2 MB in size.

The method executes in 5 ms.

Target frame rate is 60 FPS.
```

---

## Verification Checklist

Before publishing documentation:

- [ ] Used active voice throughout
- [ ] Addressed reader as "you" (second person)
- [ ] Used serial comma in lists
- [ ] Wrote in clear, concise English
- [ ] Proper heading hierarchy for accessibility
- [ ] Alt text provided for all images
- [ ] Avoided ableist and non-inclusive language
- [ ] Used blocklist/allowlist (not blacklist/whitelist)
- [ ] Short paragraphs and sentences
- [ ] Consistent terminology throughout

---

## References

- [Google Developer Documentation Style Guide](https://developers.google.com/style)
- [Microsoft Writing Style Guide](https://learn.microsoft.com/en-us/style-guide/)
- [Inclusive Language Guidelines](https://www.apa.org/about/apa/equity-diversity-inclusion/language-guidelines)
- [Write the Docs - Style Guides](https://www.writethedocs.org/guide/writing/style-guides/)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

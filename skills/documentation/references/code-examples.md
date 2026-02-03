# Code Examples

Guidelines for including code in documentation.

---

## Namespace Rules

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

---

## Good vs Bad Format

Show examples with emojis for clarity:

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

---

## Provide Context

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

## Keep Examples Minimal

- Show only what's necessary to illustrate the point
- Remove boilerplate unless it's the focus
- Add comments only when logic is non-obvious

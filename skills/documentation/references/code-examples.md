# Code examples

## Purpose

This document defines standards for writing code examples in documentation. Clear, consistent code examples improve understanding and reduce ambiguity across all projects.

---

## Namespace rules - P1

### Use ProjectName as placeholder

**Rule:**
- Always use `ProjectName` as the namespace in code examples
- Never use specific project names (e.g., `Rookie`, `Daifugo`, `MyGame`)
- This ensures documentation is project-agnostic and reusable

**Example (✅ Good):**

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class GameManager : MonoBehaviour
    {
        // Implementation
    }
}
```

**Anti-Pattern (❌ Bad):**

```csharp
namespace Rookie.Quest
{
    // Using specific project name
}

namespace MyGame.Player
{
    // Using specific project name
}
```

---

### Namespace hierarchy

Follow project-agnostic namespace structure:

```csharp
namespace ProjectName.Core          // Core game logic
namespace ProjectName.Data          // ScriptableObject definitions
namespace ProjectName.Events        // EventChannel definitions
namespace ProjectName.UI            // UI components
namespace ProjectName.Audio         // Audio system
namespace ProjectName.Utils         // Utility classes
```

---

## Code block formatting - P1

### Syntax highlighting

**Rule:**
- Always specify language in code blocks
- Use lowercase language identifiers
- Common languages: `csharp`, `json`, `yaml`, `xml`, `bash`, `markdown`

**Example:**

````markdown
```csharp
public class Example : MonoBehaviour
{
    // Code
}
```

```json
{
  "version": "1.0.0",
  "name": "example"
}
```
````

---

### Inline code formatting

**Rule:**
- Use backticks for class names, methods, variables, and technical terms
- Use inline code for Unity-specific terms (e.g., `MonoBehaviour`, `GameObject`)

**Example:**

Use `PascalCase` for public properties and `camelCase` for private fields. The `Start()` method initializes components, while `Update()` runs every frame.

---

## Good vs bad examples - P1

### Use comparison format

**Rule:**
- Always show bad examples before good examples
- Use ❌ emoji for bad examples
- Use ✅ emoji for good examples
- Explain why the bad example is problematic

**Example:**

````markdown
### Singleton Abuse

**❌ Bad:**

```csharp
// Hidden dependency - not visible in Inspector
public class ScoreUI : MonoBehaviour
{
    private void Update()
    {
        GameManager.Instance.UpdateScore();
    }
}
```

**Problem:** Hidden dependencies make testing difficult and create tight coupling.

**✅ Good:**

```csharp
// Dependencies visible in Inspector
public class ScoreUI : MonoBehaviour
{
    [SerializeField] private IntEventChannelSO onScoreChanged;

    private void OnEnable()
    {
        onScoreChanged.OnEventRaised += UpdateScore;
    }

    private void OnDisable()
    {
        onScoreChanged.OnEventRaised -= UpdateScore;
    }

    private void UpdateScore(int newScore)
    {
        // Update UI
    }
}
```

**Benefits:** EventChannel decoupling, easy to test, Inspector-visible dependencies.
````

---

## Code comments - P2

### When to add comments

**Add comments for:**
- Non-obvious logic or algorithms
- Unity-specific behavior (e.g., execution order)
- Design decisions or tradeoffs
- Performance optimizations

**Example:**

```csharp
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyRuntimeSetSO enemySet;

    private void Start()
    {
        // Pre-allocate enemy pool to avoid runtime GC allocations
        InitializeEnemyPool(50);
    }

    private void Update()
    {
        // Spawn check runs every frame, but actual spawn uses cooldown
        if (CanSpawn())
        {
            SpawnEnemy();
        }
    }
}
```

---

### Avoid obvious comments

**❌ Bad:**

```csharp
// Get the component
Rigidbody rb = GetComponent<Rigidbody>();

// Add force
rb.AddForce(Vector3.forward);
```

**✅ Good:**

```csharp
// Cache Rigidbody reference to avoid repeated GetComponent calls
Rigidbody rb = GetComponent<Rigidbody>();

rb.AddForce(Vector3.forward);
```

---

## Context provision - P1

### Provide surrounding context

**Rule:**
- Don't show isolated snippets without context
- Include class declaration and necessary imports
- Show full method signatures when relevant

**❌ Bad:**

```csharp
currentHealth -= damage;
onHealthChanged?.RaiseEvent(currentHealth);
```

**✅ Good:**

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private IntEventChannelSO onHealthChanged;
        [SerializeField] private int maxHealth = 100;

        private int currentHealth;

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // Notify subscribers of health change
            onHealthChanged?.RaiseEvent(currentHealth);
        }
    }
}
```

---

## Complete examples - P1

### Ensure examples work

**Rule:**
- All code examples must compile
- Include necessary using statements
- Follow coding standards defined in other documents
- Use `#if UNITY_EDITOR` for editor-only code

**Example:**

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Data
{
    [CreateAssetMenu(fileName = "CardData", menuName = "ProjectName/Data/Card")]
    public class CardSO : ScriptableObject
    {
        [Header("Card Properties")]
        public string cardID;
        public string cardName;
        public int cost;
        public Sprite artwork;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(cardID) && cost >= 0;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(cardID))
                Debug.LogWarning($"[CardSO] cardID is empty on {name}.", this);
        }
#endif
    }
}
```

---

## XML documentation - P2

### Include XML docs for public APIs

**Rule:**
- Use XML documentation comments (`///`) for public classes, methods, and properties
- Provide `<summary>`, `<param>`, and `<returns>` tags
- Keep descriptions concise (1-2 sentences)

**Example:**

```csharp
namespace ProjectName.Utils
{
    /// <summary>
    /// Provides extension methods for Vector3 calculations.
    /// </summary>
    public static class Vector3Extensions
    {
        /// <summary>
        /// Flattens a Vector3 to the XZ plane (sets Y to 0).
        /// </summary>
        /// <param name="vector">The vector to flatten.</param>
        /// <returns>A new Vector3 with Y set to 0.</returns>
        public static Vector3 Flatten(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }
    }
}
```

---

## Code example categories - P2

### Full class examples

Use for demonstrating complete patterns:

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Core
{
    /// <summary>
    /// Manages game state transitions and event coordination.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onGameStarted;
        [SerializeField] private VoidEventChannelSO onGamePaused;
        [SerializeField] private VoidEventChannelSO onGameEnded;

        private GameState currentState = GameState.Menu;

        public void StartGame()
        {
            currentState = GameState.Playing;
            onGameStarted?.RaiseEvent();
        }

        public void PauseGame()
        {
            if (currentState == GameState.Playing)
            {
                currentState = GameState.Paused;
                onGamePaused?.RaiseEvent();
            }
        }

        public void EndGame()
        {
            currentState = GameState.Ended;
            onGameEnded?.RaiseEvent();
        }
    }

    public enum GameState
    {
        Menu,
        Playing,
        Paused,
        Ended
    }
}
```

---

### Snippet examples

Use for demonstrating specific techniques:

```csharp
// Cache component references in Awake
private Rigidbody rb;
private Animator animator;

private void Awake()
{
    rb = GetComponent<Rigidbody>();
    animator = GetComponent<Animator>();
}
```

---

### Anti-pattern examples

Use for showing what NOT to do:

```csharp
// ❌ BAD: FindObjectOfType every frame (very expensive)
private void Update()
{
    PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
    if (playerHealth != null)
    {
        healthText.text = $"HP: {playerHealth.currentHealth}";
    }
}

// ✅ GOOD: Cache reference once, update via EventChannel
private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealthUI;
}

private void UpdateHealthUI(int health)
{
    healthText.text = $"HP: {health}";
}
```

---

## Unity-specific considerations - P1

### Show Unity lifecycle methods

**Rule:**
- Use correct Unity lifecycle method names
- Add comments explaining execution order when relevant

**Example:**

```csharp
using UnityEngine;

namespace ProjectName.Core
{
    public class ComponentExample : MonoBehaviour
    {
        // Awake runs before Start, use for internal setup
        private void Awake()
        {
            CacheComponents();
        }

        // Start runs after Awake, use for initialization with other components
        private void Start()
        {
            InitializeWithDependencies();
        }

        // Update runs every frame
        private void Update()
        {
            // Avoid heavy operations here
        }

        // FixedUpdate runs on physics timestep (default 50 FPS)
        private void FixedUpdate()
        {
            // Physics operations here
        }
    }
}
```

---

### Show SerializeField usage

**Example:**

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.UI
{
    public class HealthBar : MonoBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onHealthChanged;

        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Image fillImage;

        [Header("Settings")]
        [SerializeField, Range(0f, 1f)] private float maxHealth = 100f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onHealthChanged == null)
                Debug.LogWarning($"[HealthBar] onHealthChanged is not assigned.", this);

            if (fillImage == null)
                Debug.LogWarning($"[HealthBar] fillImage is not assigned.", this);
        }
#endif
    }
}
```

---

## References

- [Naming Conventions](../core/naming-conventions.md)
- [Code Organization](../core/code-organization.md)
- [Comments & Documentation](../core/comments-documentation.md)
- [Unity Scripting API](https://docs.unity3d.com/ScriptReference/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

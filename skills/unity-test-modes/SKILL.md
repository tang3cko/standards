---
name: unity-test-modes
description: Edit Mode vs Play Mode tests. Decision tree, performance comparison. Use when choosing test mode in Unity.
---

# Test Modes: Edit Mode vs Play Mode

## Purpose

Define differences between Unity Test Framework's two test modes and when to use each.

## Checklist

- [ ] Use Edit Mode for pure C# logic tests
- [ ] Use Play Mode only when Unity lifecycle is required
- [ ] Prefer Edit Mode when possible for fast execution
- [ ] Clean up GameObjects in Play Mode tests

---

## Decision tree - P1

```
Does your code depend on Unity lifecycle (Awake, Start, Update)?
├─ YES → Use Play Mode Tests
└─ NO → Does it require Physics/Animation/Scene?
    ├─ YES → Use Play Mode Tests
    └─ NO → Use Edit Mode Tests ← Prefer this
```

---

## Comparison table - P1

| Feature | Edit Mode | Play Mode |
|---------|-----------|-----------|
| Speed | Milliseconds | Seconds |
| Pure C# | Yes | Yes |
| MonoBehaviour | No | Yes |
| Lifecycle | No | Yes (Awake, Start, Update) |
| Physics | No | Yes |
| Animation | No | Yes |
| Scenes | No | Yes |

---

## Edit Mode tests - P1

### Characteristics

- Very fast (milliseconds)
- Best for pure C# logic
- Cannot use MonoBehaviour

### Example

```csharp
using NUnit.Framework;

public class CardValidatorTests
{
    [Test]
    public void CanPlayCard_ValidCard_ReturnsTrue()
    {
        // Arrange
        var validator = new CardValidator();
        var card = new Card(rank: 5);
        var fieldCard = new Card(rank: 3);

        // Act
        bool result = validator.CanPlayCard(card, fieldCard);

        // Assert
        Assert.That(result, Is.True);
    }
}
```

---

## Play Mode tests - P1

### Characteristics

- Slow (seconds)
- Supports MonoBehaviour
- Supports Unity lifecycle

### Example

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class GameManagerTests
{
    [UnityTest]
    public IEnumerator StartGame_InitializesPlayers()
    {
        // Arrange
        var go = new GameObject();
        var manager = go.AddComponent<GameManager>();

        // Act
        manager.StartGame();
        yield return null;  // Wait one frame

        // Assert
        Assert.That(manager.CurrentPlayer, Is.Not.Null);

        // Cleanup
        Object.Destroy(go);
    }
}
```

---

## When to use which - P1

### Use Edit Mode when

- Testing pure C# logic
- Testing business logic
- Testing data structures
- Speed is critical (CI/CD)

### Use Play Mode when

- Testing MonoBehaviour behavior
- Requires Unity lifecycle (Awake/Start/Update)
- Physics/Animation testing
- Scene-based tests

---

## Performance impact - P1

### Edit Mode performance

```
Test execution: ~0.5ms
100 tests: ~50ms
1000 tests: ~500ms
```

### Play Mode performance

```
Unity Player initialization: ~2-5 seconds
Test execution: ~100ms-1000ms per test
100 tests: ~30-60 seconds
```

---

## Best practices - P1

**Extract logic to pure C# for Edit Mode testing:**

```csharp
// ✅ GOOD: Pure C# logic (Edit Mode)
public class GameLogic
{
    public CardPlayResult PlayCard(Card card, Card fieldCard) { }
}

// ✅ GOOD: Thin MonoBehaviour wrapper
public class GameManager : MonoBehaviour
{
    private GameLogic logic;

    void Awake()
    {
        logic = new GameLogic();
    }
}
```

**Clean up Play Mode tests:**

```csharp
[UnityTest]
public IEnumerator TestGameObject()
{
    var go = new GameObject();
    // Test logic...

    Object.Destroy(go);  // Always cleanup
    yield return null;
}
```

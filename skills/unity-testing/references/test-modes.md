# Test Modes: Edit Mode vs Play Mode

## Decision tree

```
Does your code depend on Unity lifecycle (Awake, Start, Update)?
├─ YES → Use Play Mode Tests
└─ NO → Does it require Physics/Animation/Scene?
    ├─ YES → Use Play Mode Tests
    └─ NO → Use Edit Mode Tests ← Prefer this
```

## Comparison table

| Feature | Edit Mode | Play Mode |
|---------|-----------|-----------|
| Speed | Milliseconds | Seconds |
| Pure C# | Yes | Yes |
| MonoBehaviour | No | Yes |
| Lifecycle | No | Yes (Awake, Start, Update) |
| Physics | No | Yes |
| Animation | No | Yes |
| Scenes | No | Yes |

## Edit Mode tests

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

## Play Mode tests

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

## Performance impact

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

## Best practices

**Extract logic to pure C# for Edit Mode testing:**

```csharp
// Pure C# logic (Edit Mode testable)
public class GameLogic
{
    public CardPlayResult PlayCard(Card card, Card fieldCard) { }
}

// Thin MonoBehaviour wrapper
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

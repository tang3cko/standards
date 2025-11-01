# Test modes: Edit Mode vs Play Mode

## Purpose

This document defines the differences between Unity Test Framework's two test modes (Edit Mode and Play Mode) and when to use each one.

## Checklist

- [ ] Use Edit Mode for pure C# logic tests
- [ ] Use Play Mode only when Unity lifecycle, Physics, or Animation is required
- [ ] Prefer Edit Mode when possible for fast test execution
- [ ] Extract logic from MonoBehaviour to enable Edit Mode testing
- [ ] Clean up GameObjects in Play Mode tests using TearDown

---

## Decision tree

```
Does your code depend on Unity lifecycle (Awake, Start, Update)?
├─ YES → Use Play Mode Tests
└─ NO → Does it require Physics/Animation/Scene?
    ├─ YES → Use Play Mode Tests
    └─ NO → Use Edit Mode Tests ← Prefer this when possible
```

---

## Edit Mode tests

### Characteristics

- Very fast (milliseconds)
- Runs in Unity Editor only
- No Play Mode required
- Best for pure C# logic
- Cannot use Unity lifecycle
- Cannot use MonoBehaviour

### Execution environment

```
EditorApplication.update callback loop
→ No Unity Player initialization
→ Very fast execution
```

### Use cases

**Pure C# classes:**

```csharp
// GameLogic.cs (Pure C#)
public class GameLogic
{
    public bool CanPlayCard(int cardRank, int fieldRank)
    {
        if (cardRank == 8) return true;  // ✅ 8-cut
        return cardRank >= fieldRank;
    }
}

// GameLogicTests.cs (Edit Mode)
[Test]
public void CanPlayCard_With8_AlwaysReturnsTrue()
{
    var logic = new GameLogic();
    bool result = logic.CanPlayCard(8, 1);
    Assert.That(result, Is.True);
}
```

**Business logic:**

```csharp
[Test]
public void CalculateScore_WithBonus_ReturnsDoubledScore()
{
    int score = ScoreCalculator.Calculate(100, 2.0f);
    Assert.That(score, Is.EqualTo(200));
}
```

**Validators:**

```csharp
[Test]
public void IsValidRank_WithValidRank_ReturnsTrue()
{
    bool result = CardValidator.IsValidRank(5);
    Assert.That(result, Is.True);
}
```

**Data structures:**

```csharp
[Test]
public void Shuffle_Deck_RandomizesOrder()
{
    var deck = DeckShuffler.Shuffle(cards);
    Assert.That(deck, Is.Not.EqualTo(cards));
}
```

---

## Play Mode tests

### Characteristics

- Slow (seconds)
- Runs Unity runtime
- Supports MonoBehaviour
- Supports Unity lifecycle
- Supports Physics/Animation
- Supports multiple platforms

### Execution environment

```
Full Unity Player initialization
→ Awake, Start, Update can run
→ High initialization cost
```

### Use cases

**MonoBehaviour behavior:**

```csharp
[UnityTest]
public IEnumerator GameManager_Initialize_SetsUpPlayers()
{
    var go = new GameObject();
    var manager = go.AddComponent<GameManager>();

    yield return null;  // Wait for Start()

    Assert.That(manager.IsInitialized, Is.True);
    Assert.That(manager.Players.Count, Is.EqualTo(4));

    Object.Destroy(go);
}
```

**Unity lifecycle dependent:**

```csharp
[UnityTest]
public IEnumerator Player_OnAwake_InitializesInventory()
{
    var player = new GameObject().AddComponent<Player>();

    yield return null;  // Awake/Start executed

    Assert.That(player.Inventory, Is.Not.Null);
}
```

**Physics simulations:**

```csharp
[UnityTest]
public IEnumerator Ball_WhenDropped_FallsDown()
{
    var ball = CreateBallWithRigidbody();
    float startY = ball.transform.position.y;

    yield return new WaitForSeconds(1.0f);

    Assert.That(ball.transform.position.y, Is.LessThan(startY));
}
```

**Animation:**

```csharp
[UnityTest]
public IEnumerator Card_Animate_CompletesAfter300ms()
{
    var card = CreateCardWithAnimation();
    card.Animate();

    yield return new WaitForSeconds(0.3f);

    Assert.That(card.IsAnimationComplete, Is.True);
}
```

**Scene-based tests:**

```csharp
[UnityTest]
public IEnumerator LoadGameScene_InitializesCorrectly()
{
    SceneManager.LoadScene("GameScene");
    yield return null;

    var manager = GameObject.FindObjectOfType<GameManager>();
    Assert.That(manager, Is.Not.Null);
}
```

---

## Comparison table

| Feature | Edit Mode | Play Mode |
|---------|-----------|-----------|
| Speed | Milliseconds | Seconds |
| Execution | EditorApplication.update | Unity Player |
| Pure C# | Yes | Yes |
| MonoBehaviour | No | Yes |
| Lifecycle | No | Yes (Awake, Start, Update) |
| Physics | No | Yes |
| Animation | No | Yes |
| Scenes | No | Yes |
| Coroutines | No | Yes |
| Platforms | Editor only | Editor + Build targets |

---

## Examples

### Edit Mode example

```csharp
using NUnit.Framework;

public class CardValidatorTests
{
    [Test]  // ← Standard [Test] attribute
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

Characteristics:
- Returns `void`
- Synchronous
- No `yield` statements
- Fast execution (< 1ms)

### Play Mode example

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;

public class GameManagerTests
{
    [UnityTest]  // ← [UnityTest] attribute
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
        Assert.That(manager.Players.Count, Is.EqualTo(4));

        // Cleanup
        Object.Destroy(go);
    }
}
```

Characteristics:
- Returns `IEnumerator`
- Asynchronous (coroutine)
- Can use `yield return`
- Slower execution (seconds)

---

## When to use which

### Use Edit Mode when

**Testing pure C# logic:**

```csharp
public class GameRules  // ← No MonoBehaviour
{
    public bool IsValid(Card card) { }
}
```

**Testing business logic:**

```csharp
public class ScoreCalculator  // ← Static utility
{
    public static int Calculate(int baseScore, float multiplier) { }
}
```

**Testing data structures:**

```csharp
public class Deck  // ← Pure C# class
{
    public List<Card> Shuffle() { }
}
```

**Speed is critical:**
- CI/CD fast feedback
- Rapid development cycle

### Use Play Mode when

**Testing MonoBehaviour:**

```csharp
public class GameManager : MonoBehaviour  // ← MonoBehaviour
{
    void Start() { }
    void Update() { }
}
```

**Requires Unity lifecycle:**

```csharp
// Need Awake/Start/Update to run
[UnityTest]
public IEnumerator TestLifecycle()
{
    yield return null;  // Execute lifecycle
}
```

**Physics/Animation:**

```csharp
// Need Physics engine
[UnityTest]
public IEnumerator TestCollision()
{
    yield return new WaitForFixedUpdate();
}
```

**Scene testing:**

```csharp
// Need to load actual scene
[UnityTest]
public IEnumerator TestSceneLoading()
{
    SceneManager.LoadScene("GameScene");
    yield return null;
}
```

---

## Best practices

### Do

**Extract logic to pure C# for Edit Mode testing:**

```csharp
// ✅ GOOD: Pure C# logic (Edit Mode)
public class GameLogic
{
    public CardPlayResult PlayCard(Card card, Card fieldCard) { }
}

// ✅ GOOD: Thin MonoBehaviour wrapper (Play Mode only if needed)
public class GameManager : MonoBehaviour
{
    private GameLogic logic;

    void Awake()
    {
        logic = new GameLogic();
    }

    void OnCardPlayed(Card card)
    {
        var result = logic.PlayCard(card, fieldCard);
        // Unity integration only
    }
}
```

**Use SetUp for fresh state:**

```csharp
[SetUp]
public void Setup()
{
    gameLogic = new GameLogic();  // Fresh for each test
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

### Do not

**Do not use Play Mode when Edit Mode suffices:**

```csharp
// ❌ BAD: Unnecessary Play Mode
[UnityTest]
public IEnumerator SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
    yield return null;  // Why?
}

// ✅ GOOD: Use Edit Mode
[Test]
public void SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
}
```

**Do not test Unity framework:**

```csharp
// ❌ BAD
[Test]
public void Vector3_Addition_Works()
{
    var result = new Vector3(1, 2, 3) + new Vector3(4, 5, 6);
    Assert.That(result, Is.EqualTo(new Vector3(5, 7, 9)));
}

// ✅ GOOD: Test YOUR code
[Test]
public void CalculateDistance_ReturnsCorrectValue()
{
    var distance = PositionCalculator.Distance(pointA, pointB);
    Assert.That(distance, Is.EqualTo(10f).Within(0.01f));
}
```

---

## Performance impact

### Edit Mode performance

```
Test execution: ~0.5ms
100 tests: ~50ms
1000 tests: ~500ms
```

Fast feedback loop.

### Play Mode performance

```
Unity Player initialization: ~2-5 seconds
Test execution: ~100ms-1000ms per test
100 tests: ~30-60 seconds
```

Use sparingly.

---

## Summary

| Need | Mode | Speed |
|------|------|-------|
| Pure C# logic | Edit Mode | Fast |
| MonoBehaviour | Play Mode | Slow |
| Business logic | Edit Mode | Fast |
| Unity lifecycle | Play Mode | Slow |
| Validators | Edit Mode | Fast |
| Physics/Animation | Play Mode | Slow |

---

## References

- [Test Patterns](patterns.md)
- [NUnit Quick Reference](nunit-quick-reference.md)
- [Unity Test Framework Manual](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

# Common testing pitfalls

## Purpose

This document defines common testing mistakes and their solutions to help you write effective tests.

## Checklist

- [ ] Test public behavior, not implementation details
- [ ] Avoid over-mocking simple objects
- [ ] Use fresh state for each test with SetUp
- [ ] Clean up resources (GameObjects, ScriptableObjects) in TearDown
- [ ] Test your code, not Unity framework
- [ ] Extract logic from MonoBehaviour for Edit Mode testing
- [ ] Use Edit Mode when possible for fast execution
- [ ] Keep one logical assertion per test
- [ ] Fix failing tests immediately or document valid reasons
- [ ] Use SetUp to avoid repeated initialization code

---

## Pitfall 1: Testing implementation details

### Problem

You test private methods or internal implementation.

```csharp
// ❌ BAD: Testing private implementation
public class GameManager
{
    private void PrivateHelper() { }
}

[Test]
public void PrivateHelper_DoesX()
{
    // Trying to test private method
    // This is testing implementation, not behavior
}
```

### Solution

You test public behavior.

```csharp
// ✅ GOOD: Test public behavior
public class GameManager
{
    public void ProcessCard(Card card)
    {
        // Internally calls PrivateHelper()
        // But you test the public result
    }

    private void PrivateHelper() { }
}

[Test]
public void ProcessCard_ValidCard_UpdatesState()
{
    var manager = new GameManager();

    manager.ProcessCard(card);

    Assert.That(manager.State, Is.EqualTo(expectedState));
}
```

Why:
- Private methods are implementation details
- Tests break when you refactor
- Testing public behavior is sufficient

---

## Pitfall 2: Over-mocking

### Problem

You mock everything.

```csharp
// ❌ BAD: Mocking everything
var mockCard = Substitute.For<ICard>();
var mockRank = Substitute.For<IRank>();
var mockSuit = Substitute.For<ISuit>();

mockCard.Rank.Returns(mockRank);
mockRank.Value.Returns(5);
mockSuit.Name.Returns("Hearts");
// ... too complex
```

### Solution

You use real objects for simple things.

```csharp
// ✅ GOOD: Use real simple objects
var card = new Card(rank: 5, suit: Suit.Hearts);
var validator = Substitute.For<IRuleValidator>();  // Mock only complex dependency

validator.CanPlayCard(card, fieldCard).Returns(true);
```

Rule: "Mock what you write, use real for simple things"

---

## Pitfall 3: Shared mutable state

### Problem

You share static fields or state between tests.

```csharp
// ❌ BAD: Shared state
private static List<Card> deck = new List<Card>();

[Test]
public void Test1()
{
    deck.Add(new Card());  // Modifies shared state
    Assert.That(deck.Count, Is.EqualTo(1));
}

[Test]
public void Test2()
{
    // Depends on Test1's state
    Assert.That(deck.Count, Is.EqualTo(???));
}
```

### Solution

You create new objects for each test.

```csharp
// ✅ GOOD: Fresh state per test
private List<Card> deck;

[SetUp]
public void Setup()
{
    deck = new List<Card>();  // New for EACH test
}

[Test]
public void Test1()
{
    deck.Add(new Card());
    Assert.That(deck.Count, Is.EqualTo(1));
}

[Test]
public void Test2()
{
    // Fresh deck, independent
    Assert.That(deck.Count, Is.EqualTo(0));
}
```

---

## Pitfall 4: Not cleaning up

### Problem

You do not clean up GameObjects, ScriptableObjects, etc.

```csharp
// ❌ BAD: No cleanup
[UnityTest]
public IEnumerator TestGameObject()
{
    var go = new GameObject();
    var component = go.AddComponent<MyComponent>();

    yield return null;

    Assert.That(component.IsInitialized, Is.True);
    // GameObject leaks
}
```

### Solution

You always clean up.

```csharp
// ✅ GOOD: Cleanup in test
[UnityTest]
public IEnumerator TestGameObject()
{
    var go = new GameObject();
    var component = go.AddComponent<MyComponent>();

    yield return null;

    Assert.That(component.IsInitialized, Is.True);

    Object.Destroy(go);  // ✅ Cleanup
    yield return null;
}

// ✅ BETTER: Cleanup in TearDown
private GameObject testObject;

[SetUp]
public void Setup()
{
    testObject = new GameObject();
}

[TearDown]
public void Teardown()
{
    if (testObject != null)
    {
        Object.Destroy(testObject);
    }
}
```

---

## Pitfall 5: Testing Unity framework

### Problem

You test Unity standard functionality.

```csharp
// ❌ BAD: Testing Unity's Vector3
[Test]
public void Vector3_Addition_Works()
{
    var v1 = new Vector3(1, 2, 3);
    var v2 = new Vector3(4, 5, 6);
    var result = v1 + v2;

    Assert.That(result, Is.EqualTo(new Vector3(5, 7, 9)));
}
```

### Solution

You test YOUR code.

```csharp
// ✅ GOOD: Test YOUR code that uses Vector3
[Test]
public void CalculateDistance_ReturnsCorrectValue()
{
    var distance = PositionCalculator.Distance(pointA, pointB);

    Assert.That(distance, Is.EqualTo(10f).Within(0.01f));
}
```

Why:
- Unity is already tested
- Wastes time
- Focus on YOUR code

---

## Pitfall 6: Logic in MonoBehaviour

### Problem

You write business logic in MonoBehaviour.

```csharp
// ❌ BAD: Logic in MonoBehaviour
public class GameManager : MonoBehaviour
{
    void Update()
    {
        // Complex game logic here
        if (currentCard.Rank == 8)
        {
            ResetField();
            // ...
        }
    }
}
```

### Solution

You extract logic to pure C# classes (Humble Object Pattern).

```csharp
// ✅ GOOD: Pure C# logic (testable)
public class GameLogic
{
    public bool ShouldResetField(Card card)
    {
        return card.Rank == 8;
    }
}

// ✅ GOOD: Thin MonoBehaviour
public class GameManager : MonoBehaviour
{
    private GameLogic logic;

    void Awake()
    {
        logic = new GameLogic();
    }

    void Update()
    {
        if (logic.ShouldResetField(currentCard))
        {
            ResetField();  // Unity integration only
        }
    }
}
```

---

## Pitfall 7: Using Play Mode when Edit Mode suffices

### Problem

You use Play Mode unnecessarily.

```csharp
// ❌ BAD: Unnecessary Play Mode
[UnityTest]
public IEnumerator SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
    yield return null;  // Why?
}
```

### Solution

You use Edit Mode for pure C# logic.

```csharp
// ✅ GOOD: Use Edit Mode (< 1ms)
[Test]
public void SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
}
```

Performance impact:
- Edit Mode: < 1ms
- Play Mode: > 100ms

---

## Pitfall 8: Multiple unrelated assertions

### Problem

You test multiple unrelated things in one test.

```csharp
// ❌ BAD: Multiple unrelated assertions
[Test]
public void MultipleThings()
{
    // Test card
    var card = new Card(5);
    Assert.That(card.Rank, Is.EqualTo(5));

    // Test player (unrelated)
    var player = new Player();
    player.TakeDamage(10);
    Assert.That(player.Health, Is.EqualTo(90));

    // Test deck (unrelated)
    var deck = new Deck();
    Assert.That(deck.Count, Is.EqualTo(52));
}
```

### Solution

You keep one test per logical concept.

```csharp
// ✅ GOOD: One concept per test
[Test]
public void Card_CreatedWithRank5_HasRank5()
{
    var card = new Card(5);
    Assert.That(card.Rank, Is.EqualTo(5));
}

[Test]
public void Player_TakesDamage_ReducesHealth()
{
    var player = new Player();
    player.TakeDamage(10);
    Assert.That(player.Health, Is.EqualTo(90));
}

[Test]
public void Deck_Initialized_Has52Cards()
{
    var deck = new Deck();
    Assert.That(deck.Count, Is.EqualTo(52));
}
```

---

## Pitfall 9: Ignoring test failures

### Problem

You hide failing tests with `[Ignore]`.

```csharp
// ❌ BAD: Hiding failures
[Test]
[Ignore("Broken, will fix later")]
public void BrokenTest()
{
    // This test fails but you ignore it
}
```

### Solution

You fix immediately or provide valid reasons.

```csharp
// ✅ GOOD: Fix immediately
[Test]
public void FixedTest()
{
    // Fixed the issue
}

// ✅ ACCEPTABLE: Valid reason with ticket
[Test]
[Ignore("Waiting for Unity bug fix #12345")]
public void WaitingForUnityFix()
{
    // Valid reason with ticket number
}
```

---

## Pitfall 10: Not using SetUp

### Problem

You repeat the same setup code in each test.

```csharp
// ❌ BAD: Repeated setup
[Test]
public void Test1()
{
    var gameLogic = new GameLogic();
    var validator = new MockValidator();
    // ... test
}

[Test]
public void Test2()
{
    var gameLogic = new GameLogic();  // Duplicate
    var validator = new MockValidator();  // Duplicate
    // ... test
}
```

### Solution

You use `[SetUp]` for shared setup.

```csharp
// ✅ GOOD: Shared setup
private GameLogic gameLogic;
private MockValidator validator;

[SetUp]
public void Setup()
{
    gameLogic = new GameLogic();
    validator = new MockValidator();
}

[Test]
public void Test1()
{
    // Use shared objects
}

[Test]
public void Test2()
{
    // Use shared objects
}
```

---

## Quick checklist

Before committing tests:

- [ ] Testing public behavior, not implementation details
- [ ] Not over-mocking simple objects
- [ ] No shared mutable state between tests
- [ ] Cleaning up resources (GameObjects, ScriptableObjects)
- [ ] Testing YOUR code, not Unity framework
- [ ] Using Edit Mode when possible
- [ ] One logical assertion per test
- [ ] No ignored tests without valid reason
- [ ] Using `[SetUp]` for common initialization
- [ ] Tests are independent and can run in any order

---

## Summary

| Pitfall | Solution |
|---------|----------|
| Testing implementation | Test public behavior |
| Over-mocking | Use real simple objects |
| Shared state | Fresh objects in `[SetUp]` |
| No cleanup | Use `[TearDown]` |
| Testing Unity | Test YOUR code |
| Logic in MonoBehaviour | Extract to Pure C# |
| Unnecessary Play Mode | Use Edit Mode |
| Multiple assertions | One concept per test |
| Ignoring failures | Fix or provide reason |
| Repeated setup | Use `[SetUp]` |

---

## References

- [Test Patterns](patterns.md)
- [Test Modes](test-modes.md)
- [Testing Principles](principles.md)

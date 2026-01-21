# Common Testing Pitfalls

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

---

## Pitfall 1: Testing implementation details

### Problem

```csharp
// Bad: Testing private implementation
public class GameManager
{
    private void PrivateHelper() { }
}

[Test]
public void PrivateHelper_DoesX()
{
    // Testing implementation, not behavior
}
```

### Solution

```csharp
// Good: Test public behavior
[Test]
public void ProcessCard_ValidCard_UpdatesState()
{
    var manager = new GameManager();
    manager.ProcessCard(card);
    Assert.That(manager.State, Is.EqualTo(expectedState));
}
```

---

## Pitfall 2: Over-mocking

### Problem

```csharp
// Bad: Mocking everything
var mockCard = Substitute.For<ICard>();
var mockRank = Substitute.For<IRank>();
var mockSuit = Substitute.For<ISuit>();
// ... too complex
```

### Solution

```csharp
// Good: Use real simple objects
var card = new Card(rank: 5, suit: Suit.Hearts);
var validator = Substitute.For<IRuleValidator>();  // Mock only complex dependency
```

---

## Pitfall 3: Shared mutable state

### Problem

```csharp
// Bad: Shared state
private static List<Card> deck = new List<Card>();

[Test]
public void Test1()
{
    deck.Add(new Card());  // Modifies shared state
}

[Test]
public void Test2()
{
    // Depends on Test1's state
}
```

### Solution

```csharp
// Good: Fresh state per test
private List<Card> deck;

[SetUp]
public void Setup()
{
    deck = new List<Card>();  // New for EACH test
}
```

---

## Pitfall 4: Not cleaning up

### Problem

```csharp
// Bad: No cleanup
[UnityTest]
public IEnumerator TestGameObject()
{
    var go = new GameObject();
    yield return null;
    Assert.That(...);
    // GameObject leaks
}
```

### Solution

```csharp
// Good: Cleanup in test
[UnityTest]
public IEnumerator TestGameObject()
{
    var go = new GameObject();
    yield return null;
    Assert.That(...);

    Object.Destroy(go);
    yield return null;
}

// Better: Cleanup in TearDown
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
        Object.Destroy(testObject);
}
```

---

## Pitfall 5: Testing Unity framework

### Problem

```csharp
// Bad: Testing Unity's Vector3
[Test]
public void Vector3_Addition_Works()
{
    var result = new Vector3(1, 2, 3) + new Vector3(4, 5, 6);
    Assert.That(result, Is.EqualTo(new Vector3(5, 7, 9)));
}
```

### Solution

```csharp
// Good: Test YOUR code that uses Vector3
[Test]
public void CalculateDistance_ReturnsCorrectValue()
{
    var distance = PositionCalculator.Distance(pointA, pointB);
    Assert.That(distance, Is.EqualTo(10f).Within(0.01f));
}
```

---

## Pitfall 6: Logic in MonoBehaviour

### Problem

```csharp
// Bad: Logic in MonoBehaviour
public class GameManager : MonoBehaviour
{
    void Update()
    {
        if (currentCard.Rank == 8) ResetField();
    }
}
```

### Solution

```csharp
// Good: Pure C# logic (testable)
public class GameLogic
{
    public bool ShouldResetField(Card card) => card.Rank == 8;
}

// Thin MonoBehaviour
public class GameManager : MonoBehaviour
{
    private GameLogic logic;

    void Update()
    {
        if (logic.ShouldResetField(currentCard))
            ResetField();
    }
}
```

---

## Pitfall 7: Unnecessary Play Mode

### Problem

```csharp
// Bad: Unnecessary Play Mode
[UnityTest]
public IEnumerator SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
    yield return null;  // Why?
}
```

### Solution

```csharp
// Good: Use Edit Mode (< 1ms)
[Test]
public void SimpleCalculation()
{
    int result = 2 + 2;
    Assert.That(result, Is.EqualTo(4));
}
```

---

## Pitfall 8: Multiple unrelated assertions

### Problem

```csharp
// Bad: Multiple unrelated assertions
[Test]
public void MultipleThings()
{
    var card = new Card(5);
    Assert.That(card.Rank, Is.EqualTo(5));

    var player = new Player();
    player.TakeDamage(10);
    Assert.That(player.Health, Is.EqualTo(90));  // Unrelated
}
```

### Solution

```csharp
// Good: One concept per test
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
```

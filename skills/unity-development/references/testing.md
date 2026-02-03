# Unity Testing

Testing fundamentals for Unity projects using NUnit and Unity Test Framework.

---

## Test Mode Decision

```
Does your code depend on Unity lifecycle (Awake, Start, Update)?
├─ YES → Use Play Mode Tests
└─ NO → Does it require Physics/Animation/Scene?
    ├─ YES → Use Play Mode Tests
    └─ NO → Use Edit Mode Tests ← Prefer this
```

| Feature | Edit Mode | Play Mode |
|---------|-----------|-----------|
| Speed | Milliseconds | Seconds |
| MonoBehaviour | No | Yes |
| Lifecycle | No | Yes |
| Physics/Animation | No | Yes |

---

## AAA Pattern

Structure every test using Arrange-Act-Assert.

```csharp
[Test]
public void PlayCard_ValidCard_ReturnsSuccess()
{
    // Arrange
    var gameLogic = new GameLogic();
    var card = new Card(rank: 5);
    var fieldCard = new Card(rank: 3);

    // Act
    var result = gameLogic.PlayCard(card, fieldCard);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
}
```

---

## FIRST Principles

| Principle | Meaning |
|-----------|---------|
| **F**ast | Execute quickly (unit tests < 15ms) |
| **I**ndependent | Tests don't depend on each other |
| **R**epeatable | Same results every time |
| **S**elf-validating | Automatic pass/fail via assertions |
| **T**imely | Written before or with implementation |

---

## Humble Object Pattern

Extract logic from MonoBehaviour to make it testable.

```csharp
// Pure C# class (easily testable in Edit Mode)
public class GameLogic
{
    public CardPlayResult PlayCard(CardSO card, PlayerHandSO hand, IRuleValidator validator)
    {
        if (!validator.IsCardInHand(card, hand))
            return CardPlayResult.Fail("Card not in hand");

        hand.RemoveCard(card);
        return CardPlayResult.Success(card);
    }
}

// MonoBehaviour becomes thin wrapper
public class GameManager : MonoBehaviour
{
    private GameLogic gameLogic;

    private void Awake() => gameLogic = new GameLogic();

    private void HandleCardPlayed(CardSO card)
    {
        var result = gameLogic.PlayCard(card, hand, ruleValidator);
        // Unity integration only
    }
}
```

---

## Test Doubles Decision

```
Need to test with dependency?
├─ Dependency unused → Dummy
├─ Need specific return value → Stub
├─ Need to verify calls → Mock
├─ Need recording + state → Spy
└─ Need working implementation → Fake
```

See test-doubles.md for detailed examples.

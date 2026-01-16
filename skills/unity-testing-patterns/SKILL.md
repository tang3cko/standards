---
name: unity-testing-patterns
description: Humble Object, Test Data Builder, Factory patterns. Extract logic from MonoBehaviour. Use when writing testable Unity code.
---

# Test Patterns

## Purpose

Common design patterns for writing testable code and creating maintainable tests in Unity.

## Checklist

- [ ] Extract logic from MonoBehaviour using Humble Object Pattern
- [ ] Use Test Data Builder for creating complex test objects
- [ ] Apply Factory Pattern for consistent test data
- [ ] Inject dependencies through interfaces

---

## Humble Object Pattern - P1

The most important pattern for making MonoBehaviour code testable.

### The problem

MonoBehaviour is difficult to test directly:
- Cannot instantiate with `new`
- Depends on Unity lifecycle
- Requires Play Mode (slow)

### The solution

Extract business logic to pure C# classes.

**Before (Not testable):**

```csharp
// ❌ BAD: Logic mixed with MonoBehaviour
public class GameManager : MonoBehaviour
{
    private void OnCardPlayed(CardSO card)
    {
        // Hard to test validation
        if (!IsCardInHand(card)) return;

        // Hard to test business logic
        hand.RemoveCard(card);

        // Special rules hard to test
        if (card.Rank == 8) ResetField();
    }
}
```

**After (Testable):**

```csharp
// ✅ GOOD: Pure C# class (easily testable)
public class GameLogic
{
    public CardPlayResult PlayCard(
        CardSO card,
        PlayerHandSO hand,
        IRuleValidator validator)
    {
        if (!validator.IsCardInHand(card, hand))
            return CardPlayResult.Fail("Card not in hand");

        hand.RemoveCard(card);
        bool shouldResetField = (card.Rank == 8);

        return CardPlayResult.Success(card, shouldResetField);
    }
}

// MonoBehaviour becomes thin wrapper
public class GameManager : MonoBehaviour
{
    private GameLogic gameLogic;

    private void Awake()
    {
        gameLogic = new GameLogic();
    }

    private void HandleCardPlayed(CardSO card)
    {
        var result = gameLogic.PlayCard(card, hand, ruleValidator);
        // Unity integration only
    }
}
```

**Fast Edit Mode testing:**

```csharp
[Test]
public void PlayCard_ValidCard_ReturnsSuccess()
{
    // Arrange
    var gameLogic = new GameLogic();
    var mockValidator = new MockRuleValidator();
    mockValidator.SetValid(true);

    // Act
    var result = gameLogic.PlayCard(card, hand, mockValidator);

    // Assert
    Assert.That(result.IsSuccess, Is.True);
}
```

---

## Test Data Builder Pattern - P1

### The problem

Creating test objects with many properties is tedious.

### The solution

Use Builder Pattern with fluent API.

```csharp
public class CardBuilder
{
    private int rank = 5;  // Sensible defaults
    private Suit suit = Suit.Hearts;

    public CardBuilder WithRank(int rank)
    {
        this.rank = rank;
        return this;
    }

    public CardBuilder As8Cut()
    {
        this.rank = 8;
        return this;
    }

    public Card Build()
    {
        return new Card { Rank = rank, Suit = suit };
    }
}
```

### Usage

```csharp
[Test]
public void CanPlay_8Cut_AlwaysReturnsTrue()
{
    var card = new CardBuilder()
        .As8Cut()
        .Build();

    Assert.That(card.CanPlayOn(anyFieldCard), Is.True);
}
```

---

## Factory Pattern - P1

### Simple factory

```csharp
public class TestDataFactory
{
    public static Card CreateValidCard(int rank = 5, Suit suit = Suit.Hearts)
    {
        return new Card { Rank = rank, Suit = suit, IsValid = true };
    }

    public static List<Card> CreateFullDeck()
    {
        var cards = new List<Card>();
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                cards.Add(CreateValidCard(rank, suit));
            }
        }
        return cards;
    }
}
```

### Usage

```csharp
[Test]
public void Shuffle_FullDeck_Randomizes()
{
    var deck = TestDataFactory.CreateFullDeck();
    var shuffled = DeckShuffler.Shuffle(deck);
    Assert.That(shuffled, Is.Not.EqualTo(deck));
}
```

---

## Dependency Injection - P1

### Interface-based DI

```csharp
public interface ITimeProvider
{
    float DeltaTime { get; }
}

// Production
public class UnityTimeProvider : ITimeProvider
{
    public float DeltaTime => Time.deltaTime;
}

// Test
public class TestTimeProvider : ITimeProvider
{
    public float DeltaTime { get; set; } = 0.016f;
}

// Class with DI
public class ComboTracker
{
    private readonly ITimeProvider timeProvider;

    public ComboTracker(ITimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }
}
```

### Test with mock

```csharp
[Test]
public void Update_DecreasesRemainingTime()
{
    var testTime = new TestTimeProvider { DeltaTime = 1.0f };
    var tracker = new ComboTracker(testTime);

    tracker.Update();

    Assert.That(tracker.RemainingTime, Is.EqualTo(2.0f).Within(0.01f));
}
```

---

## Pattern comparison - P1

| Pattern | Purpose | Benefit |
|---------|---------|---------|
| Humble Object | Extract logic from MonoBehaviour | Fast Edit Mode tests |
| Test Data Builder | Create test objects | Reduce duplication |
| Factory | Consistent test data | DRY principle |
| Dependency Injection | Mock dependencies | Isolation |

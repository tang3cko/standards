# Test patterns

## Purpose

This document defines common design patterns for writing testable code and creating maintainable tests in Unity projects.

## Checklist

- [ ] Extract logic from MonoBehaviour using Humble Object Pattern
- [ ] Use Test Data Builder for creating complex test objects
- [ ] Apply Factory Pattern for consistent test data
- [ ] Inject dependencies through interfaces for testability
- [ ] Keep MonoBehaviours thin and focused on Unity integration

---

## Humble Object Pattern

The Humble Object Pattern is the most important pattern for making MonoBehaviour code testable.

### The problem

MonoBehaviour is difficult to test directly:
- Cannot instantiate with `new`
- Depends on Unity lifecycle (Awake, Start, Update)
- Requires Play Mode (slow)
- Difficult to mock

### The solution

Extract business logic to pure C# classes and make MonoBehaviour a thin wrapper.

```
Pure C# Logic (testable, fast)
    ↓
MonoBehaviour (humble object, thin wrapper)
    - Receives Unity events
    - Calls Pure C# logic
    - Handles Unity integration only
```

### Before: Not testable

```csharp
// ❌ BAD: Logic mixed with MonoBehaviour
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerHandSO hand;
    [SerializeField] private CardSO currentFieldCard;

    private void OnCardPlayed(CardSO card)
    {
        // Validation logic (hard to test)
        if (!IsCardInHand(card)) return;
        if (!CanPlayCard(card, currentFieldCard)) return;

        // Business logic (hard to test)
        hand.RemoveCard(card);
        currentFieldCard = card;

        // Special rules (hard to test)
        if (card.Rank == 8)
        {
            ResetField();  // 8-cut
        }

        // Unity integration
        OnCardPlayedEvent.RaiseEvent(card);
    }

    private bool IsCardInHand(CardSO card) { /* ... */ }
    private bool CanPlayCard(CardSO card, CardSO fieldCard) { /* ... */ }
    private void ResetField() { /* ... */ }
}
```

Problems:
- Play Mode required
- Logic cannot be separated
- Dependencies difficult to mock
- Test execution is slow

### After: Testable

#### Step 1: Extract pure C# logic

```csharp
// ✅ GOOD: Pure C# class (easily testable)
public class GameLogic
{
    public CardPlayResult PlayCard(
        CardSO card,
        PlayerHandSO hand,
        CardSO currentFieldCard,
        IRuleValidator validator)
    {
        // Validation
        if (!validator.IsCardInHand(card, hand))
            return CardPlayResult.Fail("Card not in hand");

        if (!validator.CanPlayCard(card, currentFieldCard))
            return CardPlayResult.Fail("Cannot play this card");

        // Business logic
        hand.RemoveCard(card);

        // Special rules
        bool shouldResetField = (card.Rank == 8);  // 8-cut

        return CardPlayResult.Success(
            newFieldCard: card,
            isWin: hand.IsEmpty,
            shouldResetField: shouldResetField
        );
    }
}

// Result object
public class CardPlayResult
{
    public bool IsSuccess { get; private set; }
    public string ErrorMessage { get; private set; }
    public CardSO NewFieldCard { get; private set; }
    public bool IsWin { get; private set; }
    public bool ShouldResetField { get; private set; }

    public static CardPlayResult Success(
        CardSO newFieldCard,
        bool isWin,
        bool shouldResetField)
    {
        return new CardPlayResult
        {
            IsSuccess = true,
            NewFieldCard = newFieldCard,
            IsWin = isWin,
            ShouldResetField = shouldResetField
        };
    }

    public static CardPlayResult Fail(string errorMessage)
    {
        return new CardPlayResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}
```

#### Step 2: MonoBehaviour becomes thin wrapper

```csharp
// MonoBehaviour is now a "humble object"
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerHandSO hand;
    [SerializeField] private GameEventChannelSO onCardPlayedEvent;
    [SerializeField] private GameEventChannelSO onFieldResetEvent;

    private GameLogic gameLogic;
    private IRuleValidator ruleValidator;
    private CardSO currentFieldCard;

    private void Awake()
    {
        gameLogic = new GameLogic();
        ruleValidator = GetComponent<RuleValidator>();
    }

    private void OnEnable()
    {
        onCardPlayedEvent.OnEventRaised += HandleCardPlayed;
    }

    private void OnDisable()
    {
        onCardPlayedEvent.OnEventRaised -= HandleCardPlayed;
    }

    private void HandleCardPlayed(CardSO card)
    {
        // Call pure logic
        var result = gameLogic.PlayCard(card, hand, currentFieldCard, ruleValidator);

        if (!result.IsSuccess)
        {
            Debug.LogWarning(result.ErrorMessage);
            return;
        }

        // Unity integration ONLY
        currentFieldCard = result.NewFieldCard;
        onCardPlayedEvent.RaiseEvent(card);

        if (result.ShouldResetField)
            onFieldResetEvent.RaiseEvent();

        if (result.IsWin)
            EndGame();
    }

    private void EndGame()
    {
        // Unity-specific logic
    }
}
```

#### Step 3: Easy Edit Mode testing

```csharp
// Fast Edit Mode test (< 1ms)
using NUnit.Framework;

public class GameLogicTests
{
    private GameLogic gameLogic;
    private MockRuleValidator mockValidator;

    [SetUp]
    public void Setup()
    {
        gameLogic = new GameLogic();
        mockValidator = new MockRuleValidator();
    }

    [Test]
    public void PlayCard_ValidCard_ReturnsSuccess()
    {
        // Arrange
        var card = CreateCard(rank: 5);
        var hand = CreateHand(card);
        mockValidator.SetValid(true);

        // Act
        var result = gameLogic.PlayCard(card, hand, null, mockValidator);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.NewFieldCard, Is.EqualTo(card));
    }

    [Test]
    public void PlayCard_With8_ShouldActivate8Cut()
    {
        // Arrange
        var card8 = CreateCard(rank: 8);
        var hand = CreateHand(card8);
        mockValidator.SetValid(true);

        // Act
        var result = gameLogic.PlayCard(card8, hand, null, mockValidator);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.ShouldResetField, Is.True);
    }

    [Test]
    public void PlayCard_CardNotInHand_ReturnsFail()
    {
        // Arrange
        var card = CreateCard(rank: 5);
        var hand = CreateHand();
        mockValidator.SetCardInHand(false);

        // Act
        var result = gameLogic.PlayCard(card, hand, null, mockValidator);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.ErrorMessage, Is.EqualTo("Card not in hand"));
    }

    // Helper methods
    private CardSO CreateCard(int rank) { /* ... */ }
    private PlayerHandSO CreateHand(params CardSO[] cards) { /* ... */ }
}

// Simple mock
public class MockRuleValidator : IRuleValidator
{
    private bool isCardInHand = true;
    private bool canPlayCard = true;

    public void SetCardInHand(bool value) => isCardInHand = value;
    public void SetCanPlayCard(bool value) => canPlayCard = value;
    public void SetValid(bool value)
    {
        isCardInHand = value;
        canPlayCard = value;
    }

    public bool IsCardInHand(CardSO card, PlayerHandSO hand) => isCardInHand;
    public bool CanPlayCard(CardSO card, CardSO fieldCard) => canPlayCard;
}
```

### Benefits

- Fast: Edit Mode (milliseconds)
- Testable: Pure C# logic is easy to test
- Mockable: Dependencies are interfaces
- Maintainable: Clear separation of concerns
- Reusable: Logic can be reused outside Unity

---

## Test Data Builder Pattern

### The problem

Creating test objects with many properties is tedious.

```csharp
// ❌ BAD: Repetitive setup
[Test]
public void Test1()
{
    var card = new Card();
    card.Rank = 5;
    card.Suit = Suit.Hearts;
    card.IsJoker = false;
    // ... many more properties
}

[Test]
public void Test2()
{
    var card = new Card();
    card.Rank = 8;  // Only this differs
    card.Suit = Suit.Hearts;
    card.IsJoker = false;
    // ... same setup repeated
}
```

### The solution

Use Builder Pattern with fluent API.

```csharp
// Builder
public class CardBuilder
{
    private int rank = 5;  // Sensible defaults
    private Suit suit = Suit.Hearts;
    private bool isJoker = false;

    public CardBuilder WithRank(int rank)
    {
        this.rank = rank;
        return this;
    }

    public CardBuilder WithSuit(Suit suit)
    {
        this.suit = suit;
        return this;
    }

    public CardBuilder AsJoker()
    {
        this.isJoker = true;
        return this;
    }

    public CardBuilder As8Cut()
    {
        this.rank = 8;
        return this;
    }

    public Card Build()
    {
        return new Card
        {
            Rank = rank,
            Suit = suit,
            IsJoker = isJoker
        };
    }
}
```

### Usage

```csharp
[Test]
public void CanPlay_ValidCard_ReturnsTrue()
{
    // Focus on what's important
    var card = new CardBuilder()
        .WithRank(5)
        .Build();

    Assert.That(card.CanPlayOn(fieldCard), Is.True);
}

[Test]
public void CanPlay_8Cut_AlwaysReturnsTrue()
{
    // Semantic method
    var card = new CardBuilder()
        .As8Cut()
        .Build();

    Assert.That(card.CanPlayOn(anyFieldCard), Is.True);
}

[Test]
public void CanPlay_Joker_AlwaysReturnsTrue()
{
    // All other properties use defaults
    var joker = new CardBuilder()
        .AsJoker()
        .Build();

    Assert.That(joker.CanPlayOn(anyFieldCard), Is.True);
}
```

### Benefits

- DRY: Shared defaults eliminate repetition
- Maintainable: Constructor changes only affect builder
- Readable: Fluent API shows intent
- Flexible: Easy to create variations

---

## Factory Pattern for tests

### When to use

- Creating similar objects repeatedly
- Object creation is complex
- Need consistent test data

### Simple factory

```csharp
public class TestDataFactory
{
    public static Card CreateValidCard(int rank = 5, Suit suit = Suit.Hearts)
    {
        return new Card
        {
            Rank = rank,
            Suit = suit,
            IsValid = true
        };
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

    public static PlayerHandSO CreateHand(params Card[] cards)
    {
        var hand = ScriptableObject.CreateInstance<PlayerHandSO>();
        foreach (var card in cards)
        {
            hand.AddCard(card);
        }
        return hand;
    }
}
```

### Usage

```csharp
[Test]
public void Shuffle_FullDeck_Randomizes()
{
    // Arrange
    var deck = TestDataFactory.CreateFullDeck();

    // Act
    var shuffled = DeckShuffler.Shuffle(deck);

    // Assert
    Assert.That(shuffled, Is.Not.EqualTo(deck));
}

[Test]
public void DrawCard_FromFullDeck_ReturnsCard()
{
    // Arrange
    var deck = TestDataFactory.CreateFullDeck();

    // Act
    var card = deck.DrawCard();

    // Assert
    Assert.That(card, Is.Not.Null);
    Assert.That(deck.Count, Is.EqualTo(51));
}
```

---

## Dependency injection for testability

### Interface-based DI

```csharp
// Define interface
public interface ITimeProvider
{
    float DeltaTime { get; }
}

// Production implementation
public class UnityTimeProvider : ITimeProvider
{
    public float DeltaTime => Time.deltaTime;
}

// Test implementation
public class TestTimeProvider : ITimeProvider
{
    public float DeltaTime { get; set; } = 0.016f;
}

// Class with DI
public class ComboTracker
{
    private readonly ITimeProvider timeProvider;
    private float remainingTime;

    public ComboTracker(ITimeProvider timeProvider)
    {
        this.timeProvider = timeProvider;
    }

    public void Update()
    {
        remainingTime -= timeProvider.DeltaTime;
    }
}
```

### Test with mock

```csharp
[Test]
public void Update_DecreasesRemainingTime()
{
    // Arrange
    var testTime = new TestTimeProvider { DeltaTime = 1.0f };
    var tracker = new ComboTracker(testTime);
    tracker.StartCombo();

    // Act
    tracker.Update();

    // Assert
    Assert.That(tracker.RemainingTime, Is.EqualTo(2.0f).Within(0.01f));
}
```

---

## Pattern comparison

| Pattern | Purpose | Benefit |
|---------|---------|---------|
| Humble Object | Extract logic from MonoBehaviour | Fast Edit Mode tests |
| Test Data Builder | Create test objects | Reduce duplication |
| Factory | Consistent test data | DRY principle |
| Dependency Injection | Mock dependencies | Isolation and control |

---

## Best practices

### Do

**Extract logic to pure C#:**

```csharp
// ✅ GOOD: Pure C# (Edit Mode testable)
public class GameLogic
{
    public bool CanPlay(Card card) { }
}

// ✅ GOOD: Thin MonoBehaviour
public class GameManager : MonoBehaviour
{
    private GameLogic logic;
}
```

**Use builders for complex objects:**

```csharp
var player = new PlayerBuilder()
    .WithName("TestPlayer")
    .WithLevel(5)
    .Build();
```

**Inject dependencies:**

```csharp
public class MyClass
{
    public MyClass(IDependency dep) { }  // ✅ Testable
}
```

### Do not

**Do not put logic in MonoBehaviour:**

```csharp
// ❌ BAD
public class GameManager : MonoBehaviour
{
    void Update()
    {
        // Complex game logic here
    }
}
```

**Do not create test objects manually:**

```csharp
// ❌ BAD: Repetitive
var card1 = new Card { Rank = 5, Suit = Suit.Hearts };
var card2 = new Card { Rank = 5, Suit = Suit.Hearts };
var card3 = new Card { Rank = 5, Suit = Suit.Hearts };

// ✅ GOOD: Use builder or factory
var cards = Enumerable.Range(0, 3)
    .Select(_ => new CardBuilder().WithRank(5).Build())
    .ToList();
```

---

## Summary

### Essential patterns

1. Humble Object Pattern - Make MonoBehaviour testable
2. Test Data Builder - Simplify test object creation
3. Dependency Injection - Make dependencies mockable

### Quick checklist

- [ ] Extract logic from MonoBehaviour (Humble Object)
- [ ] Use builders for complex test objects
- [ ] Inject dependencies via interfaces
- [ ] Keep MonoBehaviours thin

---

## References

- [Test Doubles Guide](test-doubles-guide.md)
- [NUnit Quick Reference](nunit-quick-reference.md)
- [Test Modes](test-modes.md)

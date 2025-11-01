# Test doubles guide

## Purpose

This document defines the types of test doubles and when to use each one for effective testing.

## Checklist

- [ ] Use Dummy when you need to fill parameters that are not used
- [ ] Use Stub for providing specific return values (state verification)
- [ ] Use Spy for recording method calls and arguments
- [ ] Use Fake for simplified working implementations
- [ ] Use Mock for verifying behavior (how methods were called)
- [ ] Prefer NSubstitute over manual mocks to reduce boilerplate

---

## What are test doubles

Test Double is a general term for any object you use in place of a production object for testing purposes.

Why use test doubles:
- Test isolation and independence
- Fast execution (avoid slow operations)
- Controllable conditions (simulate specific scenarios)
- Reproducible results (stable test outcomes)

---

## The five types

### Quick reference

| Type | Purpose | Returns Data | Records Calls | Verifies Behavior |
|------|---------|--------------|---------------|-------------------|
| Dummy | Fill parameters | No | No | No |
| Stub | Provide answers | Yes | No | No |
| Spy | Record calls | Yes | Yes | No |
| Fake | Working impl | Yes | No | No |
| Mock | Verify behavior | Yes | Yes | Yes |

---

## 1. Dummy

### Purpose

You use a Dummy to fill parameters that are never actually used.

```csharp
public interface ILogger
{
    void Log(string message);
}

// Dummy - does nothing
public class DummyLogger : ILogger
{
    public void Log(string message) { }
}

// Usage
[Test]
public void Calculate_ReturnsResult()
{
    var dummy = new DummyLogger();  // Required but not used
    var calculator = new Calculator(dummy);

    int result = calculator.Add(2, 3);

    Assert.That(result, Is.EqualTo(5));
    // Logger is never checked
}
```

When to use: You need a dependency but do not use it in the test.

---

## 2. Stub

### Purpose

You use a Stub to provide specific return values (state verification).

```csharp
public interface ICardRepository
{
    List<Card> GetAllCards();
}

// Stub - returns specific data
public class StubCardRepository : ICardRepository
{
    private List<Card> cards;

    public StubCardRepository(List<Card> cards)
    {
        this.cards = cards;
    }

    public List<Card> GetAllCards()
    {
        return cards;  // Always returns the same data
    }
}

// Usage
[Test]
public void ShuffleCards_Randomizes()
{
    // Arrange
    var testCards = new List<Card>
    {
        new Card(1),
        new Card(2),
        new Card(3)
    };
    var stub = new StubCardRepository(testCards);
    var shuffler = new CardShuffler(stub);

    // Act
    var shuffled = shuffler.Shuffle();

    // Assert (state verification)
    Assert.That(shuffled.Count, Is.EqualTo(3));
    // You do not care HOW GetAllCards() was called
}
```

When to use: You need specific return values for your test.

---

## 3. Spy

### Purpose

You use a Spy to record method calls.

```csharp
public interface IEventChannel
{
    void RaiseEvent(Card card);
}

// Spy - records calls
public class SpyEventChannel : IEventChannel
{
    public int CallCount { get; private set; }
    public List<Card> ReceivedCards { get; } = new List<Card>();

    public void RaiseEvent(Card card)
    {
        CallCount++;
        ReceivedCards.Add(card);
    }
}

// Usage
[Test]
public void PlayCard_RaisesEventOnce()
{
    // Arrange
    var spy = new SpyEventChannel();
    var gameLogic = new GameLogic(spy);

    // Act
    gameLogic.PlayCard(card);

    // Assert (call count verification)
    Assert.That(spy.CallCount, Is.EqualTo(1));
    Assert.That(spy.ReceivedCards, Does.Contain(card));
}
```

When to use: You want to verify method call count or arguments.

---

## 4. Fake

### Purpose

You use a Fake to provide a simplified working implementation.

```csharp
public interface IPlayerDatabase
{
    void SavePlayer(Player player);
    Player LoadPlayer(int id);
}

// Fake - in-memory database
public class FakePlayerDatabase : IPlayerDatabase
{
    private Dictionary<int, Player> players = new Dictionary<int, Player>();
    private int nextId = 1;

    public void SavePlayer(Player player)
    {
        if (player.Id == 0)
        {
            player.Id = nextId++;
        }
        players[player.Id] = player;
    }

    public Player LoadPlayer(int id)
    {
        return players.TryGetValue(id, out var player) ? player : null;
    }
}

// Usage
[Test]
public void SavePlayer_AssignsId()
{
    // Arrange
    var fakeDb = new FakePlayerDatabase();
    var player = new Player { Name = "Test" };

    // Act
    fakeDb.SavePlayer(player);

    // Assert
    Assert.That(player.Id, Is.Not.EqualTo(0));

    var loaded = fakeDb.LoadPlayer(player.Id);
    Assert.That(loaded.Name, Is.EqualTo("Test"));
}
```

When to use: The real implementation is slow or complex (database, file I/O, etc.).

---

## 5. Mock

### Purpose

You use a Mock to verify behavior.

```csharp
public interface IRuleValidator
{
    bool CanPlayCard(Card card, Card fieldCard);
}

// Manual Mock - tracks and verifies calls
public class MockRuleValidator : IRuleValidator
{
    private int callCount = 0;
    private Card lastCard;
    private Card lastFieldCard;
    private bool returnValue = true;

    // Configuration
    public void SetReturnValue(bool value)
    {
        returnValue = value;
    }

    // Implementation
    public bool CanPlayCard(Card card, Card fieldCard)
    {
        callCount++;
        lastCard = card;
        lastFieldCard = fieldCard;
        return returnValue;
    }

    // Verification
    public void VerifyCalledOnce()
    {
        if (callCount != 1)
            throw new Exception($"Expected 1 call, got {callCount}");
    }

    public void VerifyCalledWith(Card expectedCard, Card expectedFieldCard)
    {
        if (lastCard != expectedCard || lastFieldCard != expectedFieldCard)
            throw new Exception("Called with unexpected arguments");
    }
}

// Usage
[Test]
public void PlayCard_CallsValidator()
{
    // Arrange
    var mock = new MockRuleValidator();
    mock.SetReturnValue(true);
    var gameLogic = new GameLogic(mock);

    // Act
    gameLogic.PlayCard(card, fieldCard);

    // Assert (behavior verification)
    mock.VerifyCalledOnce();
    mock.VerifyCalledWith(card, fieldCard);
}
```

When to use: You need to verify method calls.

---

## Mock vs Stub: The key difference

### Critical distinction

- Stub: State verification (WHAT was returned)
- Mock: Behavior verification (HOW it was called)

### State verification (Stub)

```csharp
[Test]
public void GetScore_ReturnsValue()
{
    // Arrange
    var stub = new StubScoreService();
    stub.SetScore(100);  // Configure state

    // Act
    int score = stub.GetScore();

    // Assert (state verification)
    Assert.That(score, Is.EqualTo(100));  // Check WHAT was returned
}
```

### Behavior verification (Mock)

```csharp
[Test]
public void SaveScore_CallsService()
{
    // Arrange
    var mock = new MockScoreService();
    var player = new Player();

    // Act
    player.SaveScore(100, mock);

    // Assert (behavior verification)
    mock.VerifyCalledWith(100);  // Check HOW it was called
}
```

---

## Using NSubstitute

### Why NSubstitute

- Concise syntax
- Unity support
- Type safe
- Less code than manual mocks

### Installation

Import the NSubstitute Unity package.

### Basic usage

```csharp
using NSubstitute;

[Test]
public void PlayCard_CallsValidator()
{
    // Arrange - Create mock
    var mock = Substitute.For<IRuleValidator>();
    mock.CanPlayCard(Arg.Any<Card>(), Arg.Any<Card>())
        .Returns(true);

    var gameLogic = new GameLogic(mock);

    // Act
    gameLogic.PlayCard(card, fieldCard);

    // Assert - Verify calls
    mock.Received(1).CanPlayCard(card, fieldCard);
}
```

### Configuring return values

```csharp
[Test]
public void GetScore_ReturnsConfigured()
{
    // Arrange
    var stub = Substitute.For<IScoreService>();
    stub.GetScore().Returns(100);

    // Act
    int score = stub.GetScore();

    // Assert
    Assert.That(score, Is.EqualTo(100));
}
```

### Argument matching

```csharp
[Test]
public void TestArguments()
{
    var mock = Substitute.For<IValidator>();

    // Any argument
    mock.Validate(Arg.Any<Card>()).Returns(true);

    // Specific argument
    mock.Validate(Arg.Is<Card>(c => c.Rank == 8)).Returns(true);

    // Act & Assert
    Assert.That(mock.Validate(new Card(8)), Is.True);
}
```

---

## When to use which

### Use Stub when

- Testing state/output
- Need specific return values
- Do not care about calls

Example: Repository returning test data

### Use Mock when

- Testing interactions
- Verifying method calls
- Checking call order

Example: Event channel raising events

### Use Spy when

- Need both state and behavior
- Manual verification needed

Example: Logger recording messages

### Use Fake when

- Real implementation too slow
- Need actual logic
- In-memory alternative available

Example: In-memory database for tests

### Use Dummy when

- Parameter required but unused
- Simplest possible implementation

Example: Unused logger parameter

---

## Best practices

### Do

**Use interfaces for dependencies:**

```csharp
public class GameLogic
{
    private readonly IRuleValidator validator;  // ✅ Interface

    public GameLogic(IRuleValidator validator)
    {
        this.validator = validator;
    }
}
```

**Mock what you write, use real for simple objects:**

```csharp
// ✅ GOOD
var mockValidator = Substitute.For<IRuleValidator>();
var realCard = new Card(rank: 5);  // Simple object - use real

// ❌ BAD - over-mocking
var mockCard = Substitute.For<ICard>();
var mockRank = Substitute.For<IRank>();
```

**Keep interfaces focused:**

```csharp
// ✅ GOOD
public interface IRuleValidator
{
    bool CanPlayCard(Card card, Card fieldCard);
}

// ❌ BAD - too many responsibilities
public interface IEverything
{
    void DoThis();
    void DoThat();
    void DoEverything();
}
```

### Do not

**Do not test implementation details:**

```csharp
// ❌ BAD
mock.Received().PrivateHelperMethod();  // Do not test internals

// ✅ GOOD
Assert.That(result.IsValid, Is.True);  // Test public behavior
```

**Do not over-mock:**

```csharp
// ❌ BAD
var mockInt = Substitute.For<IInt>();  // Too much

// ✅ GOOD
int value = 5;  // Use real simple types
```

**Do not mock what you do not own:**

```csharp
// ❌ BAD
var mockTransform = Substitute.For<Transform>();  // Unity API

// ✅ GOOD - wrap it
public interface IPositionProvider
{
    Vector3 GetPosition();
}
```

---

## Quick decision tree

```
Need to test with dependency?
├─ Dependency unused → Dummy
├─ Need specific return value → Stub
├─ Need to verify calls → Mock
├─ Need recording + state → Spy
└─ Need working implementation → Fake
```

---

## Summary

| Type | Verification | Use Case |
|------|--------------|----------|
| Dummy | None | Fill parameters |
| Stub | State (WHAT) | Return values |
| Spy | State + Calls | Record info |
| Fake | State | Working impl |
| Mock | Behavior (HOW) | Verify calls |

---

## References

- [Test Patterns](patterns.md)
- [NUnit Quick Reference](nunit-quick-reference.md)
- [NSubstitute Documentation](https://nsubstitute.github.io/)

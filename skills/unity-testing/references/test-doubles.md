# Test Doubles Guide

## The five types

| Type | Purpose | Returns Data | Records Calls | Verifies Behavior |
|------|---------|--------------|---------------|-------------------|
| Dummy | Fill parameters | No | No | No |
| Stub | Provide answers | Yes | No | No |
| Spy | Record calls | Yes | Yes | No |
| Fake | Working impl | Yes | No | No |
| Mock | Verify behavior | Yes | Yes | Yes |

## 1. Dummy

Fill parameters that are never actually used.

```csharp
public class DummyLogger : ILogger
{
    public void Log(string message) { }  // Does nothing
}

[Test]
public void Calculate_ReturnsResult()
{
    var dummy = new DummyLogger();  // Required but not used
    var calculator = new Calculator(dummy);

    int result = calculator.Add(2, 3);

    Assert.That(result, Is.EqualTo(5));
}
```

## 2. Stub

Provide specific return values (state verification).

```csharp
public class StubCardRepository : ICardRepository
{
    private List<Card> cards;

    public StubCardRepository(List<Card> cards)
    {
        this.cards = cards;
    }

    public List<Card> GetAllCards() => cards;
}

[Test]
public void ShuffleCards_Randomizes()
{
    var stub = new StubCardRepository(testCards);
    var shuffler = new CardShuffler(stub);

    var shuffled = shuffler.Shuffle();

    Assert.That(shuffled.Count, Is.EqualTo(3));
}
```

## 3. Spy

Record method calls.

```csharp
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

[Test]
public void PlayCard_RaisesEventOnce()
{
    var spy = new SpyEventChannel();
    var gameLogic = new GameLogic(spy);

    gameLogic.PlayCard(card);

    Assert.That(spy.CallCount, Is.EqualTo(1));
    Assert.That(spy.ReceivedCards, Does.Contain(card));
}
```

## 4. Fake

Simplified working implementation.

```csharp
public class FakePlayerDatabase : IPlayerDatabase
{
    private Dictionary<int, Player> players = new();
    private int nextId = 1;

    public void SavePlayer(Player player)
    {
        if (player.Id == 0) player.Id = nextId++;
        players[player.Id] = player;
    }

    public Player LoadPlayer(int id)
    {
        return players.TryGetValue(id, out var player) ? player : null;
    }
}
```

## 5. Mock

Verify behavior (how methods were called).

```csharp
public class MockRuleValidator : IRuleValidator
{
    private int callCount = 0;
    private bool returnValue = true;

    public void SetReturnValue(bool value) => returnValue = value;

    public bool CanPlayCard(Card card, Card fieldCard)
    {
        callCount++;
        return returnValue;
    }

    public void VerifyCalledOnce()
    {
        if (callCount != 1)
            throw new Exception($"Expected 1 call, got {callCount}");
    }
}
```

## Mock vs Stub

- **Stub**: State verification (WHAT was returned)
- **Mock**: Behavior verification (HOW it was called)

---

## Using NSubstitute

### Basic usage

```csharp
using NSubstitute;

[Test]
public void PlayCard_CallsValidator()
{
    var mock = Substitute.For<IRuleValidator>();
    mock.CanPlayCard(Arg.Any<Card>(), Arg.Any<Card>())
        .Returns(true);

    var gameLogic = new GameLogic(mock);
    gameLogic.PlayCard(card, fieldCard);

    mock.Received(1).CanPlayCard(card, fieldCard);
}
```

### Configuring return values

```csharp
var stub = Substitute.For<IScoreService>();
stub.GetScore().Returns(100);

int score = stub.GetScore();
Assert.That(score, Is.EqualTo(100));
```

---

## Best practices

**Use interfaces for dependencies:**

```csharp
public class GameLogic
{
    private readonly IRuleValidator validator;  // Interface

    public GameLogic(IRuleValidator validator)
    {
        this.validator = validator;
    }
}
```

**Mock what you write, use real for simple objects:**

```csharp
var mockValidator = Substitute.For<IRuleValidator>();
var realCard = new Card(rank: 5);  // Simple object - use real
```

**Do not mock what you do not own:**

```csharp
// Bad
var mockTransform = Substitute.For<Transform>();  // Unity API

// Good - wrap it
public interface IPositionProvider
{
    Vector3 GetPosition();
}
```

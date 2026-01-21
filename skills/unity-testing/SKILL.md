---
name: unity-testing
description: Unity testing fundamentals. NUnit, Edit Mode vs Play Mode, FIRST principles, test doubles (Mock/Stub/Spy), Humble Object pattern, assembly definitions. Use when writing or reviewing unit tests in Unity.
---

# Unity Testing

Testing fundamentals for Unity projects using NUnit and Unity Test Framework.

---

## Test mode decision - P1

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

## AAA pattern - P1

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

## FIRST principles - P1

| Principle | Meaning |
|-----------|---------|
| **F**ast | Execute quickly (unit tests < 15ms) |
| **I**ndependent | Tests don't depend on each other |
| **R**epeatable | Same results every time |
| **S**elf-validating | Automatic pass/fail via assertions |
| **T**imely | Written before or with implementation |

---

## NUnit quick reference - P1

```csharp
// ATTRIBUTES
[Test]                    // Standard test
[UnityTest]               // Coroutine test (Play Mode)
[SetUp]                   // Before each test
[TearDown]                // After each test
[TestCase(1, 2, 3)]       // Parameterized (Edit Mode)

// ASSERTIONS (Constraint Model)
Assert.That(a, Is.EqualTo(b));           // Equal
Assert.That(a, Is.GreaterThan(5));       // Compare
Assert.That(a, Is.True);                 // Boolean
Assert.That(a, Is.Null);                 // Null
Assert.That(list, Has.Count.EqualTo(3)); // Collection
Assert.That(str, Does.Contain("sub"));   // String
Assert.That(() => f(), Throws.Nothing);  // Exception
Assert.That(3.14f, Is.EqualTo(3.14f).Within(0.01f)); // Float tolerance
```

See [references/nunit.md](references/nunit.md) for full reference.

---

## Test doubles decision - P1

```
Need to test with dependency?
├─ Dependency unused → Dummy
├─ Need specific return value → Stub
├─ Need to verify calls → Mock
├─ Need recording + state → Spy
└─ Need working implementation → Fake
```

See [references/test-doubles.md](references/test-doubles.md) for examples.

---

## Humble Object pattern - P1

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

See [references/patterns.md](references/patterns.md) for Builder and Factory patterns.

---

## References

| Topic | File | When to Read |
|-------|------|--------------|
| FIRST, AAA, TDD | [principles.md](references/principles.md) | Understanding test fundamentals |
| Edit vs Play Mode | [test-modes.md](references/test-modes.md) | Choosing test mode |
| Test patterns | [patterns.md](references/patterns.md) | Humble Object, Builder, Factory, DI |
| NUnit reference | [nunit.md](references/nunit.md) | Full attributes and assertions |
| Test doubles | [test-doubles.md](references/test-doubles.md) | Mock, Stub, Spy, Fake, NSubstitute |
| Assembly setup | [assemblies.md](references/assemblies.md) | Setting up .asmdef for tests |
| Common pitfalls | [pitfalls.md](references/pitfalls.md) | Debugging test issues |

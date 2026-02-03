# Testing Principles

## FIRST principles

### Fast

Tests must execute quickly to encourage frequent execution.

```csharp
// Good: Fast Edit Mode test (< 1ms)
[Test]
public void Calculate_TwoNumbers_ReturnsSum()
{
    int result = Calculator.Add(2, 3);
    Assert.That(result, Is.EqualTo(5));
}

// Bad: Slow Play Mode test with unnecessary delay
[UnityTest]
public IEnumerator SlowTest()
{
    yield return new WaitForSeconds(3.0f);  // Avoid waiting
}
```

### Independent

Tests must not depend on each other or share state.

```csharp
private Player player;

[SetUp]
public void Setup()
{
    player = new Player();  // Fresh instance for every test
}

[Test]
public void Test1_PlayerActions()
{
    player.DoSomething();
    Assert.That(player.ActionCount, Is.EqualTo(1));
}
```

### Repeatable

Tests must produce the same results every time.

```csharp
[Test]
public void Repeatable_FixedSeed()
{
    var random = new Random(12345);  // Fixed seed
    int value = random.Next(1, 10);
    Assert.That(value, Is.EqualTo(6));  // Always the same result
}
```

### Self-validating

Tests must automatically determine pass or fail without manual verification.

### Timely

Write tests before or immediately after implementation.

---

## AAA pattern

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
    Assert.That(result.NewFieldCard, Is.EqualTo(card));
}
```

---

## TDD workflow

Test-Driven Development follows Red-Green-Refactor.

### Phase 1: Red

Write a failing test that defines the desired behavior.

```csharp
[Test]
public void Add_TwoNumbers_ReturnsSum()
{
    var calculator = new Calculator();
    int result = calculator.Add(2, 3);  // Method doesn't exist yet
    Assert.That(result, Is.EqualTo(5));
}
```

### Phase 2: Green

Write minimum code to make the test pass.

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return 5;  // Hard-coding is OK for now
    }
}
```

### Phase 3: Refactor

Improve the code while keeping all tests green.

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;  // Now generalized
    }
}
```

---

## Core rules

### Rule 1: Test only YOUR code

```csharp
// Good: Test YOUR code
[Test]
public void YourMethod_ValidInput_ReturnsExpectedResult()
{
    var yourClass = new YourClass();
    var result = yourClass.YourMethod();
    Assert.That(result, Is.True);
}

// Bad: Test third-party library (already tested)
[Test]
public void ThirdPartyLibrary_Works() { }
```

### Rule 2: Keep unit tests under 15ms

### Rule 3: One logical assertion per test

### Rule 4: Prefer Edit Mode over Play Mode

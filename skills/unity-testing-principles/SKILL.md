---
name: unity-testing-principles
description: FIRST principles and AAA pattern for Unity tests. TDD workflow, test rules. Use when writing unit tests in Unity.
---

# Testing Principles

## Purpose

Fundamental principles for writing fast, reliable, maintainable tests in Unity projects.

## Checklist

- [ ] Follow FIRST principles (Fast, Independent, Repeatable, Self-validating, Timely)
- [ ] Structure tests using AAA pattern (Arrange-Act-Assert)
- [ ] Write tests for YOUR code only, not third-party libraries
- [ ] Keep unit tests under 15ms execution time
- [ ] Ensure one logical assertion per test
- [ ] Prefer Edit Mode over Play Mode for speed

---

## FIRST principles - P1

### Fast

Tests must execute quickly to encourage frequent execution.

```csharp
// ✅ GOOD: Fast Edit Mode test (< 1ms)
[Test]
public void Calculate_TwoNumbers_ReturnsSum()
{
    int result = Calculator.Add(2, 3);
    Assert.That(result, Is.EqualTo(5));
}

// ❌ BAD: Slow Play Mode test with unnecessary delay
[UnityTest]
public IEnumerator SlowTest()
{
    yield return new WaitForSeconds(3.0f);  // Avoid waiting
}
```

### Independent

Tests must not depend on each other or share state.

```csharp
// ✅ GOOD: Each test is independent
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

Tests must produce the same results every time they run.

```csharp
// ✅ GOOD: Repeatable with fixed seed
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

```csharp
// ✅ GOOD: Self-validating with assertion
[Test]
public void AutoCheck_UsesAssertion()
{
    var result = Calculate();
    Assert.That(result, Is.EqualTo(42));  // Automatic pass/fail
}
```

### Timely

Write tests before or immediately after implementation.

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
    Assert.That(result.NewFieldCard, Is.EqualTo(card));
}
```

---

## TDD workflow - P1

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

## Core rules - P1

### Rule 1: Test only YOUR code

```csharp
// ✅ GOOD: Test YOUR code
[Test]
public void YourMethod_ValidInput_ReturnsExpectedResult()
{
    var yourClass = new YourClass();
    var result = yourClass.YourMethod();
    Assert.That(result, Is.True);
}

// ❌ BAD: Test third-party library (already tested)
[Test]
public void ThirdPartyLibrary_Works()
{
    // Don't test code you don't own!
}
```

### Rule 2: Keep unit tests under 15ms

```csharp
// ✅ GOOD: Fast pure calculation (< 1ms)
[Test]
public void FastCalculation_ReturnsResult()
{
    int result = 2 + 3;
    Assert.That(result, Is.EqualTo(5));
}
```

### Rule 3: One logical assertion per test

```csharp
// ✅ GOOD: Single concept
[Test]
public void AddCard_IncreasesCount()
{
    hand.AddCard(card);
    Assert.That(hand.Count, Is.EqualTo(1));
}
```

### Rule 4: Prefer Edit Mode over Play Mode

```csharp
// ✅ GOOD: Edit Mode for pure logic (faster)
[Test]
public void CalculateScore_MultiplierApplied_ReturnsScaled()
{
    int score = ScoreCalculator.Calculate(100, 2);
    Assert.That(score, Is.EqualTo(200));
}
```

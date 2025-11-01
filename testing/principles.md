# Testing principles

## Purpose

This document defines fundamental principles you must follow when writing tests. These principles ensure your tests are fast, reliable, maintainable, and valuable across all Unity projects.

## Checklist

- [ ] Follow FIRST principles (Fast, Independent, Repeatable, Self-validating, Timely)
- [ ] Structure tests using AAA pattern (Arrange-Act-Assert)
- [ ] Apply TDD workflow (Red-Green-Refactor) when appropriate
- [ ] Write tests for YOUR code only, not third-party libraries
- [ ] Keep unit tests under 15ms execution time
- [ ] Ensure one logical assertion per test
- [ ] Prefer Edit Mode over Play Mode for speed

---

## FIRST principles

Every test you write should follow the FIRST principles. These five characteristics ensure your tests remain valuable over time.

### Fast

Tests must execute quickly to encourage frequent execution.

**Guidelines:**
- Unit tests should complete within 15ms
- Prefer Edit Mode (milliseconds) over Play Mode (seconds)
- Mock slow dependencies like file I/O, network calls, or databases
- Run tests frequently during development

**Good example:**

```csharp
// ✅ GOOD: Fast Edit Mode test (< 1ms)
[Test]
public void Calculate_TwoNumbers_ReturnsSum()
{
    int result = Calculator.Add(2, 3);
    Assert.That(result, Is.EqualTo(5));
}
```

**Bad example:**

```csharp
// ❌ BAD: Slow Play Mode test with unnecessary delay
[UnityTest]
public IEnumerator SlowTest()
{
    yield return new WaitForSeconds(3.0f);  // Avoid waiting
}
```

---

### Independent

Tests must not depend on each other or share state. Each test should run in isolation and produce the same result regardless of execution order.

**Guidelines:**
- Never share state between tests using static or instance variables
- Create fresh objects in `[SetUp]` for each test
- Tests should pass when run individually or in any order
- Enable parallel test execution

**Bad example:**

```csharp
// ❌ BAD: Tests depend on execution order
private static Player player;  // Shared state

[Test, Order(1)]
public void Test1_CreatesPlayer()
{
    player = new Player();
}

[Test, Order(2)]
public void Test2_UsesPlayer()
{
    player.DoSomething();  // Depends on Test1
}
```

**Good example:**

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

[Test]
public void Test2_PlayerState()
{
    Assert.That(player.IsActive, Is.True);
}
```

---

### Repeatable

Tests must produce the same results every time they run, regardless of environment or timing.

**Guidelines:**
- Avoid dependencies on current time, random values, or external state
- Use fixed seeds for random number generators
- Mock time-dependent functionality
- Tests should pass on any machine, at any time

**Bad example:**

```csharp
// ❌ BAD: Non-repeatable due to random values
[Test]
public void NonRepeatable_RandomValue()
{
    var random = new Random();  // Different every time
    int value = random.Next(1, 10);
    Assert.That(value, Is.GreaterThan(0));  // May fail randomly
}
```

**Good example:**

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

---

### Self-validating

Tests must automatically determine pass or fail without manual verification.

**Guidelines:**
- Use assertions to verify results
- Avoid manual inspection via `Debug.Log`
- Test runner should clearly report pass/fail
- No human interpretation required

**Bad example:**

```csharp
// ❌ BAD: Requires manual verification
[Test]
public void ManualCheck_LogsResult()
{
    var result = Calculate();
    Debug.Log(result);  // Requires manual inspection
}
```

**Good example:**

```csharp
// ✅ GOOD: Self-validating with assertion
[Test]
public void AutoCheck_UsesAssertion()
{
    var result = Calculate();
    Assert.That(result, Is.EqualTo(42));  // Automatic pass/fail
}
```

---

### Timely

Tests should be written at the right time to be most effective.

**Guidelines:**
- Write tests before or immediately after implementation
- Use TDD (Test-Driven Development) when appropriate
- Don't postpone writing tests
- Tests written later are often incomplete or missing

---

## AAA pattern

Structure every test using the Arrange-Act-Assert pattern. This three-phase structure makes tests easy to read and maintain.

### Structure

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange: Set up test conditions
    var sut = new SystemUnderTest();
    var input = CreateTestInput();

    // Act: Execute the method under test
    var result = sut.Method(input);

    // Assert: Verify the result
    Assert.That(result, Is.EqualTo(expectedValue));
}
```

---

### Benefits

- Clear separation of concerns
- Easy to read and understand
- Obvious where failures occur
- Consistent across the team

---

### Complete example

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

Test-Driven Development follows a three-step cycle: Red-Green-Refactor. This cycle ensures you write only necessary code and maintain high test coverage.

### The cycle

```
RED (Write failing test)
  ↓
GREEN (Make it pass)
  ↓
REFACTOR (Improve code)
  ↓
Repeat
```

---

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

**Rules:**
- Test must fail (compilation error counts)
- Failure reason should be clear
- Start small (1-5 lines of test code)

**Why:** Confirms the test actually detects the missing functionality.

---

### Phase 2: Green

Write the minimum code to make the test pass.

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return 5;  // ✅ Hard-coding is OK for now!
    }
}
```

**Rules:**
- Write only enough code to pass the test
- Hard-coding is acceptable
- Don't aim for perfect code yet

**Why:** Confirms the test can detect success.

---

### Phase 3: Refactor

Improve the code while keeping all tests green.

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;  // ✅ Now generalized
    }
}
```

**Rules:**
- All tests must remain passing
- Improve code quality
- Remove duplication
- Improve naming

**What to refactor:**
- Production code
- Test code (both need maintenance)

---

### Cycle timing

- Red to Green: 2-10 minutes
- Refactor: 1-5 minutes
- One complete cycle: 5-15 minutes

Keep cycles short to maintain focus and momentum.

---

## Core rules

### Rule 1: Test only YOUR code

Write tests for code you write, not for third-party libraries or frameworks.

```csharp
// ✅ GOOD: Test YOUR code
[Test]
public void YourMethod_ValidInput_ReturnsExpectedResult()
{
    var yourClass = new YourClass();
    var result = yourClass.YourMethod();
    Assert.That(result, Is.True);
}

// ❌ BAD: Test third-party library
[Test]
public void ThirdPartyLibrary_Works()
{
    var lib = new ThirdPartyLibrary();
    // Don't test code you don't own!
}
```

Unity framework is already tested. Don't test Unity's `Vector3` addition or `GameObject` lifecycle.

---

### Rule 2: Keep unit tests under 15ms

Fast tests enable frequent execution and rapid feedback.

```csharp
// ✅ GOOD: Fast pure calculation (< 1ms)
[Test]
public void FastCalculation_ReturnsResult()
{
    int result = 2 + 3;
    Assert.That(result, Is.EqualTo(5));
}

// ❌ RETHINK: Slow test (> 15ms)
[Test]
public void SlowTest_LoadsFile()
{
    // File I/O, network calls, heavy computation → consider mocking
}
```

If a test is slow, extract the logic to a separate class and mock slow dependencies.

---

### Rule 3: One logical assertion per test

Each test should verify one specific behavior or concept.

```csharp
// ✅ GOOD: Single concept
[Test]
public void AddCard_IncreasesCount()
{
    hand.AddCard(card);
    Assert.That(hand.Count, Is.EqualTo(1));
}

// ✅ ACCEPTABLE: Related assertions for same concept
[Test]
public void AddCard_AddsCardToCollection()
{
    hand.AddCard(card);

    Assert.That(hand.Count, Is.EqualTo(1));
    Assert.That(hand.Cards, Does.Contain(card));
}

// ❌ BAD: Multiple unrelated assertions
[Test]
public void MultipleUnrelatedThings()
{
    hand.AddCard(card);
    Assert.That(hand.Count, Is.EqualTo(1));

    player.TakeDamage(10);  // Unrelated to card logic
    Assert.That(player.Health, Is.EqualTo(90));
}
```

---

### Rule 4: Tests must be independent

Each test should create its own dependencies and not rely on other tests.

```csharp
// ✅ GOOD: Independent tests
[Test]
public void Test1_AddItem()
{
    var player = new Player();  // Fresh instance
    player.AddItem(item);
    Assert.That(player.Inventory.Count, Is.EqualTo(1));
}

[Test]
public void Test2_RemoveItem()
{
    var player = new Player();  // Fresh instance again
    player.RemoveItem(item);
    Assert.That(player.Inventory.Count, Is.EqualTo(0));
}
```

---

### Rule 5: Prefer Edit Mode over Play Mode

Use Edit Mode tests whenever possible for faster execution.

```csharp
// ✅ GOOD: Edit Mode for pure logic (faster)
[Test]
public void CalculateScore_MultiplierApplied_ReturnsScaled()
{
    int score = ScoreCalculator.Calculate(100, 2);
    Assert.That(score, Is.EqualTo(200));
}

// ✅ USE ONLY WHEN NECESSARY: Play Mode for Unity lifecycle
[UnityTest]
public IEnumerator Component_Awake_InitializesState()
{
    var go = new GameObject();
    var component = go.AddComponent<MyComponent>();

    yield return null;  // Wait for Unity lifecycle

    Assert.That(component.IsInitialized, Is.True);
}
```

---

## Quick reference

### Essential checklist

Before committing your tests:

- [ ] Follow FIRST principles
- [ ] Use AAA pattern (Arrange-Act-Assert)
- [ ] Apply TDD when appropriate (Red-Green-Refactor)
- [ ] Tests are independent
- [ ] Tests run fast (< 15ms for unit tests)
- [ ] One logical assertion per test
- [ ] Test YOUR code, not third-party libraries

---

### The golden rule

**"Write tests only for code you write and mock everything else"**

---

## References

- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development) - Kent Beck
- [FIRST Principles](https://github.com/tekguard/Principles-of-Unit-Testing)
- [AAA Pattern](https://docs.microsoft.com/en-us/visualstudio/test/unit-test-basics)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)

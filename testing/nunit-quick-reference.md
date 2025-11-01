# NUnit quick reference

## Purpose

This document provides a quick reference cheat sheet for commonly used NUnit attributes and assertions.

## Checklist

- [ ] Use `[Test]` for Edit Mode tests
- [ ] Use `[UnityTest]` for Play Mode tests with coroutines
- [ ] Use Constraint Model (`Assert.That`) instead of Classic Model
- [ ] Use `[ValueSource]` for parameterization in both Edit and Play Mode
- [ ] Use `[SetUp]` and `[TearDown]` for common initialization and cleanup

---

## Core attributes

### `[Test]` - Standard test

You can use this attribute in both Edit Mode and Play Mode for standard tests.

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var sut = new SystemUnderTest();

    // Act
    var result = sut.Method();

    // Assert
    Assert.That(result, Is.True);
}
```

- Returns: `void`
- Mode: Edit Mode, Play Mode
- Synchronous

### `[UnityTest]` - Coroutine test

You use this attribute in Play Mode for coroutine tests.

```csharp
[UnityTest]
public IEnumerator MethodName_Scenario_ExpectedBehavior()
{
    // Arrange
    var go = new GameObject();

    // Act
    yield return null;  // Wait one frame

    // Assert
    Assert.That(go, Is.Not.Null);

    // Cleanup
    Object.Destroy(go);
}
```

- Returns: `IEnumerator`
- Mode: Play Mode only
- Asynchronous (coroutine)

---

## Setup and teardown

### `[SetUp]` - Before each test

```csharp
private GameLogic gameLogic;

[SetUp]
public void Setup()
{
    gameLogic = new GameLogic();  // Runs before EACH test
}
```

### `[TearDown]` - After each test

```csharp
[TearDown]
public void Teardown()
{
    // Cleanup after EACH test
    gameLogic = null;
}
```

### `[OneTimeSetUp]` - Before all tests

```csharp
[OneTimeSetUp]
public void OneTimeSetup()
{
    // Runs ONCE before all tests in this fixture
    LoadTestData();
}
```

### `[OneTimeTearDown]` - After all tests

```csharp
[OneTimeTearDown]
public void OneTimeTeardown()
{
    // Runs ONCE after all tests in this fixture
    CleanupTestData();
}
```

---

## Parameterized tests

### `[TestCase]` - Edit Mode only

```csharp
[TestCase(1, 1, ExpectedResult = true)]
[TestCase(1, 2, ExpectedResult = false)]
[TestCase(8, 1, ExpectedResult = true)]  // 8-cut
public bool CanPlayCard(int cardRank, int fieldRank)
{
    return CardValidator.CanPlayCard(cardRank, fieldRank);
}
```

- Not supported with `[UnityTest]`
- Use with `[Test]` only

### `[ValueSource]` - Both modes

```csharp
[Test]
public void IsValidRank_WithVariousRanks(
    [ValueSource(nameof(ValidRanks))] int rank)
{
    bool result = CardValidator.IsValidRank(rank);
    Assert.That(result, Is.True);
}

private static int[] ValidRanks = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
```

- Works with `[Test]`
- Works with `[UnityTest]` (ONLY parameterization that works)

---

## Test organization

### `[Category]` - Categorize tests

```csharp
[Test]
[Category("Unit")]
[Category("Fast")]
public void FastUnitTest() { }

[Test]
[Category("Integration")]
[Category("Slow")]
public void SlowIntegrationTest() { }
```

Common categories:
- `Unit` - Fast unit tests
- `Integration` - Integration tests
- `Smoke` - Critical path only
- `Fast` - < 100ms
- `Slow` - > 1 second

### `[Ignore]` - Skip test

```csharp
[Test]
[Ignore("Bug #12345 - waiting for fix")]
public void BrokenTest() { }
```

Always provide a reason.

---

## Assertions (Constraint Model)

### Equality

```csharp
// Equal
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.EqualTo(wrong));

// Same reference
Assert.That(obj1, Is.SameAs(obj2));
```

### Comparison

```csharp
// Numeric comparison
Assert.That(value, Is.GreaterThan(5));
Assert.That(value, Is.LessThan(10));
Assert.That(value, Is.GreaterThanOrEqualTo(5));
Assert.That(value, Is.LessThanOrEqualTo(10));
Assert.That(value, Is.InRange(1, 10));

// Float comparison with tolerance
Assert.That(3.14f, Is.EqualTo(3.14f).Within(0.01f));
```

### Boolean

```csharp
Assert.That(condition, Is.True);
Assert.That(condition, Is.False);
```

### Null

```csharp
Assert.That(obj, Is.Null);
Assert.That(obj, Is.Not.Null);
```

### Type

```csharp
Assert.That(obj, Is.InstanceOf<string>());
Assert.That(obj, Is.TypeOf<string>());
Assert.That(obj, Is.AssignableTo<IEnumerable>());
```

### Strings

```csharp
Assert.That("Hello World", Does.Contain("World"));
Assert.That("Hello", Does.StartWith("Hel"));
Assert.That("World", Does.EndWith("rld"));
Assert.That("test", Does.Match(@"t.st"));  // Regex

// Case insensitive
Assert.That("HELLO", Is.EqualTo("hello").IgnoreCase);
```

### Collections

```csharp
var list = new List<int> { 1, 2, 3, 4, 5 };

// Count
Assert.That(list, Has.Count.EqualTo(5));
Assert.That(list, Is.Empty.Not);

// Contains
Assert.That(list, Does.Contain(3));
Assert.That(list, Has.Member(3));

// Order
Assert.That(list, Is.Ordered);
Assert.That(list, Is.Ordered.Descending);

// All/Some/None
Assert.That(list, Has.All.GreaterThan(0));
Assert.That(list, Has.Some.GreaterThan(3));
Assert.That(list, Has.None.LessThan(0));

// Exact count
Assert.That(list, Has.Exactly(1).EqualTo(3));
Assert.That(list, Has.Exactly(5).Items);
```

### Combining constraints

```csharp
// AND
Assert.That(value, Is.GreaterThan(3).And.LessThan(10));

// OR
Assert.That(value, Is.EqualTo(5).Or.EqualTo(10));

// NOT
Assert.That(value, Is.Not.Null);
```

### Exceptions

```csharp
// Throws specific exception
Assert.That(
    () => ThrowingMethod(),
    Throws.TypeOf<ArgumentException>()
);

// Throws with message
Assert.That(
    () => ThrowingMethod(),
    Throws.TypeOf<ArgumentException>()
        .With.Message.Contains("Invalid")
);

// Does not throw
Assert.That(() => SafeMethod(), Throws.Nothing);
```

---

## Unity-specific

### LogAssert

```csharp
using UnityEngine.TestTools;

[Test]
public void Method_LogsError_WhenInvalid()
{
    // Expect error log
    LogAssert.Expect(LogType.Error, "Invalid card");

    // Act - this logs an error
    CardValidator.Validate(invalidCard);

    // Test passes because you expected the error
}
```

---

## Classic Model (Legacy)

We do not recommend using the Classic Model. Use the Constraint Model instead.

```csharp
// ❌ Classic (Legacy)
Assert.AreEqual(expected, actual);
Assert.IsTrue(condition);
Assert.IsNull(obj);

// ✅ Constraint (Recommended)
Assert.That(actual, Is.EqualTo(expected));
Assert.That(condition, Is.True);
Assert.That(obj, Is.Null);
```

---

## Quick decision table

| Need | Attribute | Mode |
|------|-----------|------|
| Standard test | `[Test]` | Both |
| Coroutine test | `[UnityTest]` | Play Mode |
| Setup each test | `[SetUp]` | Both |
| Cleanup each test | `[TearDown]` | Both |
| Setup once | `[OneTimeSetUp]` | Both |
| Cleanup once | `[OneTimeTearDown]` | Both |
| Multiple inputs | `[TestCase]` | Edit Mode |
| Param (both modes) | `[ValueSource]` | Both |
| Categorize | `[Category]` | Both |
| Skip test | `[Ignore]` | Both |

---

## Common patterns

### Basic test

```csharp
[Test]
public void Method_Scenario_Behavior()
{
    // Arrange
    var sut = new MyClass();

    // Act
    var result = sut.Method();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
}
```

### Test with SetUp

```csharp
private MyClass sut;

[SetUp]
public void Setup()
{
    sut = new MyClass();
}

[Test]
public void Test1()
{
    var result = sut.Method();
    Assert.That(result, Is.True);
}

[Test]
public void Test2()
{
    var result = sut.OtherMethod();
    Assert.That(result, Is.False);
}
```

### Parameterized test

```csharp
[TestCase(1, 1, true)]
[TestCase(1, 2, false)]
[TestCase(8, 1, true)]
public void CanPlay_VariousCases(int cardRank, int fieldRank, bool expected)
{
    bool result = CardValidator.CanPlay(cardRank, fieldRank);
    Assert.That(result, Is.EqualTo(expected));
}
```

### Unity test with cleanup

```csharp
[UnityTest]
public IEnumerator TestGameObject()
{
    // Arrange
    var go = new GameObject();
    var component = go.AddComponent<MyComponent>();

    // Act
    yield return null;

    // Assert
    Assert.That(component.IsInitialized, Is.True);

    // Cleanup
    Object.Destroy(go);
}
```

---

## Best practices

### Do

```csharp
// Use Constraint Model
Assert.That(actual, Is.EqualTo(expected));

// Descriptive names
[Test]
public void PlayCard_ValidCard_ReturnsSuccess() { }

// One logical assertion
Assert.That(result.IsSuccess, Is.True);

// Cleanup in TearDown
[TearDown]
public void Teardown() { Object.Destroy(go); }
```

### Do not

```csharp
// Classic Model (Legacy)
Assert.AreEqual(expected, actual);  // ❌

// Vague names
[Test]
public void Test1() { }  // ❌

// Multiple unrelated assertions
Assert.That(a, Is.True);
Assert.That(b, Is.False);  // ❌ Unrelated

// No cleanup
// GameObject leaks  ❌
```

---

## Quick reference card

```csharp
// ATTRIBUTES
[Test]                    // Standard test
[UnityTest]               // Coroutine test
[SetUp]                   // Before each
[TearDown]                // After each
[TestCase(1, 2, 3)]       // Parameterized (Edit Mode)
[ValueSource("data")]     // Parameterized (Both modes)
[Category("Unit")]        // Categorize
[Ignore("reason")]        // Skip

// ASSERTIONS
Assert.That(a, Is.EqualTo(b));           // Equal
Assert.That(a, Is.GreaterThan(5));       // Compare
Assert.That(a, Is.True);                 // Boolean
Assert.That(a, Is.Null);                 // Null
Assert.That(list, Has.Count.EqualTo(3)); // Collection
Assert.That(str, Does.Contain("sub"));   // String
Assert.That(() => f(), Throws.Nothing);  // Exception
```

---

## References

- [Test Doubles Guide](test-doubles-guide.md)
- [Test Patterns](patterns.md)
- [NUnit Documentation](https://docs.nunit.org/)

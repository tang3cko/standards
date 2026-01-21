# NUnit Quick Reference

## Core attributes

### `[Test]` - Standard test

```csharp
[Test]
public void MethodName_Scenario_ExpectedBehavior()
{
    var sut = new SystemUnderTest();
    var result = sut.Method();
    Assert.That(result, Is.True);
}
```

- Returns: `void`
- Mode: Edit Mode, Play Mode
- Synchronous

### `[UnityTest]` - Coroutine test

```csharp
[UnityTest]
public IEnumerator MethodName_Scenario_ExpectedBehavior()
{
    var go = new GameObject();
    yield return null;  // Wait one frame
    Assert.That(go, Is.Not.Null);
    Object.Destroy(go);
}
```

- Returns: `IEnumerator`
- Mode: Play Mode only
- Asynchronous

## Setup and teardown

```csharp
private GameLogic gameLogic;

[SetUp]
public void Setup()
{
    gameLogic = new GameLogic();  // Before EACH test
}

[TearDown]
public void Teardown()
{
    gameLogic = null;  // After EACH test
}

[OneTimeSetUp]
public void OneTimeSetup()
{
    // ONCE before all tests
}

[OneTimeTearDown]
public void OneTimeTeardown()
{
    // ONCE after all tests
}
```

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

## Assertions (Constraint Model)

### Equality

```csharp
Assert.That(actual, Is.EqualTo(expected));
Assert.That(actual, Is.Not.EqualTo(wrong));
```

### Comparison

```csharp
Assert.That(value, Is.GreaterThan(5));
Assert.That(value, Is.LessThan(10));
Assert.That(value, Is.InRange(1, 10));

// Float comparison with tolerance
Assert.That(3.14f, Is.EqualTo(3.14f).Within(0.01f));
```

### Boolean and Null

```csharp
Assert.That(condition, Is.True);
Assert.That(condition, Is.False);
Assert.That(obj, Is.Null);
Assert.That(obj, Is.Not.Null);
```

### Collections

```csharp
var list = new List<int> { 1, 2, 3, 4, 5 };

Assert.That(list, Has.Count.EqualTo(5));
Assert.That(list, Does.Contain(3));
Assert.That(list, Is.Ordered);
Assert.That(list, Has.All.GreaterThan(0));
```

### Strings

```csharp
Assert.That("Hello World", Does.Contain("World"));
Assert.That("Hello", Does.StartWith("Hel"));
Assert.That("test", Does.Match(@"t.st"));  // Regex
```

### Exceptions

```csharp
Assert.That(
    () => ThrowingMethod(),
    Throws.TypeOf<ArgumentException>()
);

Assert.That(() => SafeMethod(), Throws.Nothing);
```

## Combining constraints

```csharp
// AND
Assert.That(value, Is.GreaterThan(3).And.LessThan(10));

// OR
Assert.That(value, Is.EqualTo(5).Or.EqualTo(10));
```

## Unity-specific

### LogAssert

```csharp
using UnityEngine.TestTools;

[Test]
public void Method_LogsError_WhenInvalid()
{
    LogAssert.Expect(LogType.Error, "Invalid card");
    CardValidator.Validate(invalidCard);
}
```

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

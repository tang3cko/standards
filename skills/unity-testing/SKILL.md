---
name: unity-testing
description: Unity testing standards using NUnit and Unity Test Framework. Covers FIRST principles, AAA pattern, Edit Mode vs Play Mode, test doubles, and common pitfalls. Use when writing or reviewing Unity tests.
---

# Unity testing standards

Apply Tang3cko testing standards when writing Unity tests.

## Quick reference

### FIRST principles

- **F**ast: Tests run quickly
- **I**ndependent: Tests don't depend on each other
- **R**epeatable: Same result every time
- **S**elf-validating: Pass or fail, no manual check
- **T**imely: Written with or before production code

### AAA pattern

```csharp
[Test]
public void MethodName_Condition_ExpectedResult()
{
    // Arrange
    var sut = new SystemUnderTest();

    // Act
    var result = sut.DoSomething();

    // Assert
    Assert.That(result, Is.EqualTo(expected));
}
```

### Test modes

| Mode | Use for |
|------|---------|
| Edit Mode | Pure logic, no MonoBehaviour lifecycle |
| Play Mode | MonoBehaviour, Physics, Coroutines |

## Detailed guides

Read these files for complete standards:

- [Principles](../../unity/testing/principles.md)
- [Test modes](../../unity/testing/test-modes.md)
- [Patterns](../../unity/testing/patterns.md)
- [NUnit quick reference](../../unity/testing/nunit-quick-reference.md)
- [Test doubles guide](../../unity/testing/test-doubles-guide.md)
- [Assembly definitions](../../unity/testing/assembly-definitions.md)
- [Common pitfalls](../../unity/testing/common-pitfalls.md)

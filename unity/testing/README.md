# Testing standards

## Purpose

This directory contains comprehensive testing standards for Unity projects using Unity Test Framework and NUnit. You will learn how to write fast, maintainable, and effective tests that follow industry best practices.

## Checklist

- [ ] Read [principles.md](principles.md) to understand core testing principles
- [ ] Choose the right test mode using [test-modes.md](test-modes.md)
- [ ] Apply the Humble Object Pattern from [patterns.md](patterns.md)
- [ ] Reference [nunit-quick-reference.md](nunit-quick-reference.md) for NUnit syntax
- [ ] Learn test doubles from [test-doubles-guide.md](test-doubles-guide.md)
- [ ] Configure assembly definitions using [assembly-definitions.md](assembly-definitions.md)
- [ ] Avoid mistakes described in [common-pitfalls.md](common-pitfalls.md)

---

## Getting started

### For beginners

If you are new to Unity testing, follow this learning path:

1. Read [principles.md](principles.md) - Core testing principles (FIRST, AAA, TDD)
2. Read [test-modes.md](test-modes.md) - Edit Mode vs Play Mode basics
3. Read [patterns.md](patterns.md) - Humble Object Pattern for MonoBehaviour testing
4. Reference [nunit-quick-reference.md](nunit-quick-reference.md) - NUnit cheat sheet

This foundation will enable you to write your first tests confidently.

---

### For intermediate developers

If you already write tests but want to improve quality:

1. Review [test-doubles-guide.md](test-doubles-guide.md) - Mock vs Stub distinctions
2. Review [common-pitfalls.md](common-pitfalls.md) - Common mistakes to avoid
3. Ensure your assembly definitions follow [assembly-definitions.md](assembly-definitions.md)

---

### For experienced developers

If you are refining your testing strategy:

1. Review [patterns.md](patterns.md) - Advanced patterns like Test Data Builder
2. Verify your tests against [principles.md](principles.md) - FIRST checklist
3. Audit your codebase using [common-pitfalls.md](common-pitfalls.md) - Quality audit

---

## Document structure

### Core principles

**[principles.md](principles.md)** - Foundational testing principles

You will learn:
- FIRST principles (Fast, Independent, Repeatable, Self-validating, Timely)
- AAA pattern (Arrange-Act-Assert)
- TDD workflow (Red-Green-Refactor)

Use this document to understand the "why" behind testing best practices.

---

### Test execution modes

**[test-modes.md](test-modes.md)** - Edit Mode vs Play Mode comparison

You will learn:
- When to use Edit Mode (fast, pure C#)
- When to use Play Mode (Unity integration)
- Performance implications
- Decision tree for choosing modes

Use this document when deciding which test mode to use for your tests.

---

### Design patterns

**[patterns.md](patterns.md)** - Testing design patterns for Unity

You will learn:
- Humble Object Pattern (extracting logic from MonoBehaviour)
- Test Data Builder Pattern (creating test objects)
- Dependency Injection (mockable dependencies)

Use this document when structuring your code for testability.

---

### NUnit reference

**[nunit-quick-reference.md](nunit-quick-reference.md)** - NUnit attributes and assertions

You will find:
- Quick reference for `[Test]`, `[UnityTest]`, `[SetUp]`, etc.
- Assertion syntax (`Assert.That` constraint model)
- Common patterns and examples

Use this document as a cheat sheet while writing tests.

---

### Test doubles

**[test-doubles-guide.md](test-doubles-guide.md)** - Mocking and stubbing guide

You will learn:
- Five types of test doubles (Dummy, Stub, Spy, Fake, Mock)
- Mock vs Stub distinction (behavior vs state verification)
- NSubstitute usage examples

Use this document when you need to isolate dependencies in tests.

---

### Assembly setup

**[assembly-definitions.md](assembly-definitions.md)** - Test assembly configuration

You will learn:
- How to create test assemblies
- Required assembly definition settings
- Edit Mode vs Play Mode configuration
- Common setup issues and solutions

Use this document when setting up new test assemblies or troubleshooting test visibility issues.

---

### Common mistakes

**[common-pitfalls.md](common-pitfalls.md)** - Common testing mistakes and solutions

You will find:
- 10 common testing pitfalls
- Anti-patterns with explanations
- Solutions with code examples

Use this document to audit your tests and improve quality.

---

## Quick reference card

### Test mode decision

```
Need Unity lifecycle (Awake, Start, Update)?
└─ YES → Play Mode [UnityTest]
└─ NO  → Edit Mode [Test] (faster)
```

### Test double decision

```
Need to verify method calls?
└─ YES → Mock
└─ NO  → Stub (for return values)
```

### Key principles

- Write tests for YOUR code, not Unity framework
- Extract logic from MonoBehaviour to Pure C# classes
- Use Edit Mode when possible (100x faster)
- Mock what you write, use real for simple objects
- One logical assertion per test

---

## References

- [Unity Test Framework Manual](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [NUnit Documentation](https://docs.nunit.org/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development)

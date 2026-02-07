# Unity Testing

Testing overview and navigation to specific testing reference files.

---

## Test Philosophy - P1

- **Prefer Edit Mode** tests over Play Mode for speed
- **Extract logic** from MonoBehaviour to pure C# for testability
- Follow **FIRST** principles (Fast, Independent, Repeatable, Self-validating, Timely)
- Structure tests with **AAA** pattern (Arrange-Act-Assert)

---

## Test Mode Decision - P1

```
Does your code depend on Unity lifecycle (Awake, Start, Update)?
├─ YES -> Use Play Mode Tests
└─ NO -> Does it require Physics/Animation/Scene?
    ├─ YES -> Use Play Mode Tests
    └─ NO -> Use Edit Mode Tests <- Prefer this
```

| Feature | Edit Mode | Play Mode |
|---------|-----------|-----------|
| Speed | Milliseconds | Seconds |
| MonoBehaviour | No | Yes |
| Lifecycle | No | Yes |
| Physics/Animation | No | Yes |

See [test-modes.md](test-modes.md) for detailed comparison and examples.

---

## Testing Reference Files - P2

| Topic | File | Summary |
|-------|------|---------|
| FIRST, AAA, TDD | [principles.md](principles.md) | Core testing principles and workflows |
| Edit vs Play Mode | [test-modes.md](test-modes.md) | Decision guide and mode comparison |
| Humble Object, Builder, DI | [patterns.md](patterns.md) | Testing patterns for testable code |
| Dummy, Stub, Spy, Mock | [test-doubles.md](test-doubles.md) | Test double types and NSubstitute |
| NUnit attributes, assertions | [nunit.md](nunit.md) | NUnit quick reference |
| Assembly definitions | [assemblies.md](assemblies.md) | Test assembly setup |
| Common mistakes | [pitfalls.md](pitfalls.md) | 8 common testing pitfalls |

---

## Test Doubles Quick Decision - P2

```
Need to test with dependency?
├─ Dependency unused -> Dummy
├─ Need specific return value -> Stub
├─ Need to verify calls -> Mock
├─ Need recording + state -> Spy
└─ Need working implementation -> Fake
```

See [test-doubles.md](test-doubles.md) for detailed examples.

---

## References

- [principles.md](principles.md) - FIRST principles, AAA, TDD
- [test-modes.md](test-modes.md) - Edit Mode vs Play Mode
- [patterns.md](patterns.md) - Testing patterns
- [test-doubles.md](test-doubles.md) - Test doubles guide
- [nunit.md](nunit.md) - NUnit reference
- [assemblies.md](assemblies.md) - Assembly definitions
- [pitfalls.md](pitfalls.md) - Common pitfalls

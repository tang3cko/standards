---
name: unity
description: Unity C# coding standards covering naming conventions, ScriptableObject architecture (Tang3cko.ReactiveSO), testing (NUnit/Unity Test Framework), and UI (UI Toolkit/uGUI). Use when writing, reviewing, or refactoring Unity C# code, implementing architecture patterns, writing tests, or creating UI components.
---

# Unity coding standards

Apply Tang3cko coding standards when working with Unity projects.

## Decision tree

```
What are you doing?
│
├─▶ Writing/reviewing C# code
│   ├─ Naming rules → [Naming conventions](./references/core/naming-conventions.md)
│   ├─ File/folder structure → [Code organization](./references/core/code-organization.md)
│   ├─ Error handling → [Error handling](./references/core/error-handling.md)
│   ├─ Performance issues → [Performance](./references/core/performance.md)
│   ├─ Async (Awaitable/UniTask) → [Unity specifics](./references/core/unity-specifics.md)
│   ├─ Input handling → [Input System](./references/core/input-system.md)
│   └─ Unity-specific patterns → [Unity specifics](./references/core/unity-specifics.md)
│
├─▶ Designing architecture/data flow
│   ├─ Design philosophy → [Design principles](./references/architecture/design-principles.md)
│   ├─ Pattern selection → [Architecture overview](./references/architecture/overview.md)
│   ├─ Event-driven communication → [Event channels](./references/architecture/event-channels.md)
│   ├─ Reactive state → [Variables](./references/architecture/variables.md)
│   ├─ Object tracking → [Runtime sets](./references/architecture/runtime-sets.md)
│   ├─ Per-entity state → [Reactive entity sets](./references/architecture/reactive-entity-sets.md)
│   ├─ RES + Job System → [RES Job System](./references/architecture/reactive-entity-sets-job-system.md)
│   ├─ RES persistence → [RES Persistence](./references/architecture/reactive-entity-sets-persistence.md)
│   └─ Command pattern → [Actions](./references/architecture/actions.md)
│
├─▶ Writing/reviewing tests
│   ├─ Testing principles → [Principles](./references/testing/principles.md)
│   ├─ Edit vs Play Mode → [Test modes](./references/testing/test-modes.md)
│   ├─ Test patterns → [Patterns](./references/testing/patterns.md)
│   ├─ NUnit syntax → [NUnit quick reference](./references/testing/nunit-quick-reference.md)
│   └─ Mock/Stub/Spy → [Test doubles guide](./references/testing/test-doubles-guide.md)
│
└─▶ Creating UI
    ├─ UI Toolkit naming → [BEM naming](./references/ui/ui-toolkit/bem-naming.md)
    ├─ USS variables → [Design tokens](./references/ui/ui-toolkit/design-tokens.md)
    ├─ UXML structure → [UXML structure](./references/ui/ui-toolkit/uxml-structure.md)
    ├─ uGUI patterns → [uGUI best practices](./references/ui/ugui/best-practices.md)
    ├─ World Space UI → [World space UI](./references/ui/ugui/world-space-ui.md)
    └─ Font sizes → [Font size guidelines](./references/ui/accessibility/font-size-guidelines.md)
```

---

## Quick reference

### Naming conventions (P1)

| Element | Convention | Example |
|---------|------------|---------|
| Class/Interface | PascalCase, `I` prefix for interfaces | `PlayerController`, `IInteractable` |
| Private field | camelCase | `playerHealth`, `moveSpeed` |
| Public field/Property | PascalCase | `MaxHealth`, `IsAlive` |
| Method | PascalCase, start with verb | `TakeDamage()`, `GetInventory()` |
| Boolean | `is`, `has`, `can`, `should` prefix | `isActive`, `hasKey`, `canJump` |
| Constant | UPPER_SNAKE_CASE | `MAX_HEALTH`, `DEFAULT_SPEED` |
| ScriptableObject | `SO` suffix | `EnemyDataSO`, `ItemConfigSO` |
| EventChannel | `EventChannelSO` suffix | `OnPlayerDeathEventChannelSO` |
| Namespace | `ProjectName.Category` | `ProjectName.Core`, `ProjectName.UI` |

### Architecture pattern selection (P1)

**Core principles:** Observability, Asset-based DI, DOD (see [Design principles](./references/architecture/design-principles.md))

```
Need communication between systems?
├─ Store current value? → Variable (IntVariableSO, FloatVariableSO)
└─ Fire and forget? → EventChannel (VoidEventChannelSO, IntEventChannelSO)

Track active objects in scene?
├─ Need per-object state? → ReactiveEntitySet
└─ Simple tracking? → RuntimeSet

Dependency priority:
1. EventChannel (decoupled)
2. SerializeField (explicit)
3. FindFirstObjectByType (fallback)
4. Singleton (last resort)
```

### Testing essentials (P1)

**FIRST principles:**
- **F**ast: Tests run quickly
- **I**ndependent: Tests don't depend on each other
- **R**epeatable: Same result every time
- **S**elf-validating: Pass or fail, no manual check
- **T**imely: Written with or before production code

**AAA pattern:**
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

**Test modes:**
| Mode | Use for |
|------|---------|
| Edit Mode | Pure logic, no MonoBehaviour lifecycle |
| Play Mode | MonoBehaviour, Physics, Coroutines/Async |

### UI quick reference (P1)

**BEM naming (UI Toolkit):**
```
block__element--modifier

Examples:
- inventory-panel
- inventory-panel__item-slot
- inventory-panel__item-slot--selected
```

**Font size minimums (accessibility):**
| Platform | Body text | UI elements |
|----------|-----------|-------------|
| Desktop  | 16px      | 14px        |
| Mobile   | 16px      | 12px        |
| VR       | 24px      | 20px        |

---

## Priority levels

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

---

## References

### Core

- [Naming conventions](./references/core/naming-conventions.md) - PascalCase, camelCase, naming rules
- [Code organization](./references/core/code-organization.md) - Namespace, folder structure
- [Error handling](./references/core/error-handling.md) - Null safety, try-catch patterns
- [Comments documentation](./references/core/comments-documentation.md) - XML docs, Tooltip, Header
- [Performance](./references/core/performance.md) - Caching, pooling, Update optimization
- [Unity specifics](./references/core/unity-specifics.md) - Async patterns, deltaTime, RequireComponent
- [Input System](./references/core/input-system.md) - InputReader pattern, Input Action Assets

### Architecture (Tang3cko.ReactiveSO)

- [Design principles](./references/architecture/design-principles.md) - Observability, Asset-based DI, DOD, Set Theory
- [Overview](./references/architecture/overview.md) - Pattern selection flowchart, comparison table
- [Event channels](./references/architecture/event-channels.md) - Decoupled communication
- [Variables](./references/architecture/variables.md) - Reactive state, auto-notification
- [Runtime sets](./references/architecture/runtime-sets.md) - Object tracking without Find
- [Reactive entity sets](./references/architecture/reactive-entity-sets.md) - Per-entity state, O(1) lookup
- [RES Job System](./references/architecture/reactive-entity-sets-job-system.md) - Orchestrator, Burst, parallel processing
- [RES Persistence](./references/architecture/reactive-entity-sets-persistence.md) - Snapshot, restore, save/load
- [Actions](./references/architecture/actions.md) - Command pattern, reusable behaviors
- [Dependency management](./references/architecture/dependency-management.md) - DI priorities
- [Extension patterns](./references/architecture/extension-patterns.md) - SpecKit workflow

### Testing

- [Principles](./references/testing/principles.md) - FIRST, AAA, TDD workflow
- [Test modes](./references/testing/test-modes.md) - Edit Mode vs Play Mode
- [Patterns](./references/testing/patterns.md) - Humble Object, Test Data Builder
- [NUnit quick reference](./references/testing/nunit-quick-reference.md) - Attributes, assertions
- [Test doubles guide](./references/testing/test-doubles-guide.md) - Dummy, Stub, Spy, Fake, Mock
- [Assembly definitions](./references/testing/assembly-definitions.md) - Test assembly setup
- [Common pitfalls](./references/testing/common-pitfalls.md) - 10 common mistakes and solutions

### UI Toolkit

- [BEM naming](./references/ui/ui-toolkit/bem-naming.md) - block__element--modifier convention
- [Design tokens](./references/ui/ui-toolkit/design-tokens.md) - USS variables, theming
- [UXML structure](./references/ui/ui-toolkit/uxml-structure.md) - Document organization
- [USS responsive](./references/ui/ui-toolkit/uss-responsive.md) - Flexbox, responsive layouts

### uGUI

- [Best practices](./references/ui/ugui/best-practices.md) - Performance tips
- [World space UI](./references/ui/ugui/world-space-ui.md) - World Space Canvas setup
- [Billboard](./references/ui/ugui/billboard.md) - Camera-facing UI

### Accessibility

- [Font size guidelines](./references/ui/accessibility/font-size-guidelines.md) - Platform-specific minimums

### Examples

- [Good examples](./references/examples/good/) - Recommended patterns
- [Anti-patterns](./references/examples/anti-patterns/) - What to avoid

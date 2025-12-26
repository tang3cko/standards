# Unity coding standards

## Purpose

This directory contains coding standards for Unity projects using C#. It covers architecture patterns, core conventions, UI implementation, networking, and testing.

---

## Guides

### Core guides

| Guide | Description |
|-------|-------------|
| [Naming conventions](core/naming-conventions.md) | PascalCase, camelCase, naming rules |
| [Code organization](core/code-organization.md) | Namespace rules, folder structure |
| [Error handling](core/error-handling.md) | Null safety, try-catch patterns |
| [Comments documentation](core/comments-documentation.md) | XML docs, Tooltip, Header attributes |
| [Performance](core/performance.md) | Caching, pooling, Update optimization |
| [Unity specifics](core/unity-specifics.md) | deltaTime, RequireComponent, Colliders |

### Architecture guides

| Guide | Description |
|-------|-------------|
| [Architecture overview](architecture/README.md) | Pattern selection guide, relationship diagram |
| [ScriptableObject](architecture/scriptableobject.md) | SO-driven architecture, data assets |
| [Event channels](architecture/event-channels.md) | Tang3cko.ReactiveSO, decoupling |
| [Variables](architecture/variables.md) | Reactive variables, auto-notification |
| [Runtime sets](architecture/runtime-sets.md) | Dynamic object collections |
| [Reactive entity sets](architecture/reactive-entity-sets.md) | Centralized entity state management |
| [Dependency management](architecture/dependency-management.md) | Dependency injection priorities |
| [Extension patterns](architecture/extension-patterns.md) | SpecKit workflow, feature additions |

### UI guides

| Guide | Description |
|-------|-------------|
| [Font size guidelines](ui/accessibility/font-size-guidelines.md) | Platform-specific minimum font sizes |
| [BEM naming](ui/ui-toolkit/bem-naming.md) | BEM naming for UI Toolkit |
| [Design tokens](ui/ui-toolkit/design-tokens.md) | USS variables, theming |
| [UXML structure](ui/ui-toolkit/uxml-structure.md) | UXML best practices |
| [USS responsive](ui/ui-toolkit/uss-responsive.md) | Flexbox, responsive layouts |
| [World space UI](ui/ugui/world-space-ui.md) | World Space Canvas setup |
| [Billboard](ui/ugui/billboard.md) | Camera-facing UI |
| [uGUI best practices](ui/ugui/best-practices.md) | uGUI performance tips |

### Testing guides

| Guide | Description |
|-------|-------------|
| [Principles](testing/principles.md) | FIRST, AAA, TDD workflow |
| [Test modes](testing/test-modes.md) | Edit Mode vs Play Mode |
| [Patterns](testing/patterns.md) | Humble Object, Test Data Builder |
| [NUnit quick reference](testing/nunit-quick-reference.md) | NUnit attributes, assertions |
| [Test doubles guide](testing/test-doubles-guide.md) | Dummy, Stub, Spy, Fake, Mock |
| [Assembly definitions](testing/assembly-definitions.md) | Test assembly configuration |
| [Common pitfalls](testing/common-pitfalls.md) | Mistakes and solutions |

### Networking guides

| Guide | Description |
|-------|-------------|
| [Mirror basics](networking/mirror/basics.md) | NetworkBehaviour, NetworkIdentity |
| [Server authority](networking/mirror/server-authority.md) | Server Authority pattern |
| [SyncVar and ClientRpc](networking/mirror/syncvar-clientrpc.md) | [SyncVar], [Command], [ClientRpc] |
| [Late join](networking/mirror/late-join.md) | Late join, disconnect handling |
| [Interactable pattern](networking/mirror/interactable-pattern.md) | InteractableObject base class |

---

## Priority levels

Each document uses priority levels to indicate importance:

- **P1 (Required)**: Must be followed
- **P2 (Recommended)**: Should be followed when possible
- **P3 (Optional)**: Nice-to-have

---

## Examples

### Best practices

| File | Description |
|------|-------------|
| [event-channel-example.cs](examples/good/event-channel-example.cs) | EventChannel implementation |
| [scriptableobject-example.cs](examples/good/scriptableobject-example.cs) | ScriptableObject implementation |
| [variable-example.cs](examples/good/variable-example.cs) | Variables implementation |

### Anti-patterns

| File | Description |
|------|-------------|
| [singleton-abuse.cs](examples/anti-patterns/singleton-abuse.cs) | Singleton abuse |
| [update-heavy.cs](examples/anti-patterns/update-heavy.cs) | Heavy processing in Update |

---

## References

- [Unity Manual](https://docs.unity3d.com/Manual/)
- [Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@latest)
- [Mirror Networking](https://mirror-networking.gitbook.io/)

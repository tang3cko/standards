# Unity Coding Standards - Quick Reference

This directory contains shared coding standards for Unity projects.

## 🚀 Quick Start Guide

### Common Scenarios

| When you need to... | Read this |
|---------------------|-----------|
| **Add a new feature** | `architecture/extension-patterns.md` → `architecture/scriptableobject.md` |
| **Implement UI** | `ui/accessibility/font-size-guidelines.md` → `ui/ui-toolkit/` (all files) → Start with `bem-naming.md` |
| **Write tests** | `testing/README.md` → `testing/principles.md` → `testing/patterns.md` |
| **Name classes/variables** | `core/naming-conventions.md` |
| **Handle errors/nulls** | `core/error-handling.md` |
| **Optimize performance** | `core/performance.md` |
| **Use events** | `architecture/event-channels.md` |
| **Write documentation** | `documentation/README.md` |
| **Setup multiplayer** | `networking/mirror/basics.md` → `networking/mirror/server-authority.md` |
| **Ensure accessibility** | `ui/accessibility/font-size-guidelines.md` |

### Priority Reading Order

1. **Start here (10 min)**: `core/naming-conventions.md`, `core/code-organization.md`
2. **Core patterns (20 min)**: `architecture/scriptableobject.md`, `architecture/event-channels.md`
3. **Your domain**: Choose ui/, testing/, or networking/ based on current task

---

## 📁 Directory Structure

### core/ - Fundamental Rules

Basic coding conventions applied to all Unity projects. **Read these first!**

| File | Key Topics | Priority |
|------|-----------|----------|
| [naming-conventions.md](core/naming-conventions.md) | PascalCase, camelCase, _private fields | 🔴 Must Read |
| [code-organization.md](core/code-organization.md) | Namespace rules, folder structure | 🔴 Must Read |
| [comments-documentation.md](core/comments-documentation.md) | XML docs, Tooltip, Header attributes | 🟡 Recommended |
| [error-handling.md](core/error-handling.md) | Null safety, try-catch patterns | 🔴 Must Read |
| [performance.md](core/performance.md) | Caching, pooling, Update optimization | 🟡 Recommended |
| [unity-specifics.md](core/unity-specifics.md) | deltaTime, RequireComponent, Colliders | 🟡 Recommended |

### architecture/ - Architecture Patterns

Architecture based on ScriptableObject and event-driven design. **The foundation of this project.**

| File | Key Topics | Priority |
|------|-----------|----------|
| [scriptableobject.md](architecture/scriptableobject.md) | SO-driven architecture, data assets | 🔴 Must Read |
| [event-channels.md](architecture/event-channels.md) | Tang3cko.ReactiveSO, decoupling | 🔴 Must Read |
| [variables.md](architecture/variables.md) | Reactive variables, auto-notification | 🟡 Recommended |
| [runtime-sets.md](architecture/runtime-sets.md) | Dynamic object collections | 🟡 Recommended |
| [dependency-management.md](architecture/dependency-management.md) | Dependency injection priorities | 🟢 Reference |
| [extension-patterns.md](architecture/extension-patterns.md) | SpecKit workflow, feature additions | 🔴 Must Read |

### ui/ - UI Implementation Standards

**accessibility/ - Accessibility Standards** (Cross-platform compliance)

| File | Key Topics | Priority |
|------|-----------|----------|
| [font-size-guidelines.md](ui/accessibility/font-size-guidelines.md) | Platform-specific minimum font sizes, XAG compliance | 🔴 Must Read |

**ui-toolkit/ - UI Toolkit Standards** (Primary UI system for this project)

| File | Key Topics | Priority |
|------|-----------|----------|
| [bem-naming.md](ui/ui-toolkit/bem-naming.md) | BEM naming (block__element--modifier) | 🔴 Must Read |
| [design-tokens.md](ui/ui-toolkit/design-tokens.md) | USS variables, theming | 🟡 Recommended |
| [uxml-structure.md](ui/ui-toolkit/uxml-structure.md) | UXML best practices | 🟡 Recommended |
| [uss-responsive.md](ui/ui-toolkit/uss-responsive.md) | Flexbox, responsive layouts | 🟡 Recommended |

**ugui/ - uGUI Standards** (For World Space UI only)

| File | Key Topics | Priority |
|------|-----------|----------|
| [world-space-ui.md](ui/ugui/world-space-ui.md) | World Space Canvas setup | 🟢 Reference |
| [billboard.md](ui/ugui/billboard.md) | Camera-facing UI | 🟢 Reference |
| [best-practices.md](ui/ugui/best-practices.md) | uGUI performance tips | 🟢 Reference |

### networking/ - Mirror Networking Standards

For multiplayer game development (applicable projects only)

**mirror/ - Mirror Networking Implementation**

| File | Key Topics | Priority |
|------|-----------|----------|
| [basics.md](networking/mirror/basics.md) | NetworkBehaviour, NetworkIdentity | 🔴 Must Read |
| [server-authority.md](networking/mirror/server-authority.md) | Server Authority pattern | 🔴 Must Read |
| [syncvar-clientrpc.md](networking/mirror/syncvar-clientrpc.md) | [SyncVar], [Command], [ClientRpc] | 🔴 Must Read |
| [late-join.md](networking/mirror/late-join.md) | Late join, disconnect handling | 🟡 Recommended |
| [interactable-pattern.md](networking/mirror/interactable-pattern.md) | InteractableObject base class | 🟡 Recommended |

### documentation/ - Documentation Writing Guide

Standards for writing technical documentation

| File | Key Topics | Priority |
|------|-----------|----------|
| [README.md](documentation/README.md) | Overview, quick reference | 🟡 Recommended |
| [document-structure.md](documentation/document-structure.md) | Document organization | 🟡 Recommended |
| [code-examples.md](documentation/code-examples.md) | Clear code examples | 🟡 Recommended |
| [markdown-formatting.md](documentation/markdown-formatting.md) | Markdown syntax | 🟢 Reference |
| [file-organization.md](documentation/file-organization.md) | File naming, directory structure | 🟢 Reference |
| [writing-principles.md](documentation/writing-principles.md) | Voice, tone, style | 🟢 Reference |

### testing/ - Testing Standards

Standards for Unity Test Framework and NUnit testing

| File | Key Topics | Priority |
|------|-----------|----------|
| [README.md](testing/README.md) | Navigation, learning paths | 🟡 Recommended |
| [principles.md](testing/principles.md) | FIRST, AAA, TDD workflow | 🔴 Must Read |
| [test-modes.md](testing/test-modes.md) | Edit Mode vs Play Mode | 🟡 Recommended |
| [patterns.md](testing/patterns.md) | Humble Object, Test Data Builder | 🟡 Recommended |
| [nunit-quick-reference.md](testing/nunit-quick-reference.md) | NUnit attributes, assertions | 🔴 Must Read |
| [test-doubles-guide.md](testing/test-doubles-guide.md) | Dummy, Stub, Spy, Fake, Mock | 🟡 Recommended |
| [assembly-definitions.md](testing/assembly-definitions.md) | Test assembly configuration | 🟢 Reference |
| [common-pitfalls.md](testing/common-pitfalls.md) | Mistakes and solutions | 🟡 Recommended |

### examples/ - Code Examples

**good/ - Best Practices**

| File | Description |
|------|-------------|
| [event-channel-example.cs](examples/good/event-channel-example.cs) | EventChannel implementation ✅ |
| [scriptableobject-example.cs](examples/good/scriptableobject-example.cs) | ScriptableObject implementation ✅ |
| [variable-example.cs](examples/good/variable-example.cs) | Variables implementation ✅ |

**anti-patterns/ - Anti-Patterns** (What NOT to do)

| File | Description |
|------|-------------|
| [singleton-abuse.cs](examples/anti-patterns/singleton-abuse.cs) | Singleton abuse ❌ |
| [update-heavy.cs](examples/anti-patterns/update-heavy.cs) | Heavy processing in Update ❌ |

---

## 📚 Project-Specific Reading Checklist

### ✅ For Single-Player Games (Beta)

**Phase 1: Absolute Essentials (30 min)**
- [ ] `core/naming-conventions.md`
- [ ] `core/code-organization.md`
- [ ] `core/error-handling.md`
- [ ] `architecture/scriptableobject.md`
- [ ] `architecture/event-channels.md`

**Phase 2: UI Implementation (20 min)**
- [ ] `ui/accessibility/font-size-guidelines.md`
- [ ] `ui/ui-toolkit/bem-naming.md`
- [ ] `ui/ui-toolkit/design-tokens.md`
- [ ] `ui/ui-toolkit/uxml-structure.md`

**Phase 3: Testing (when needed)**
- [ ] `testing/principles.md`
- [ ] `testing/nunit-quick-reference.md`

**Phase 4: Reference (as needed)**
- [ ] `core/performance.md` - When optimizing
- [ ] `architecture/variables.md` - When using reactive data
- [ ] `testing/patterns.md` - When structuring complex tests

### ✅ For Multiplayer Games

**All of Single-Player +**
- [ ] `networking/mirror/basics.md`
- [ ] `networking/mirror/server-authority.md`
- [ ] `networking/mirror/syncvar-clientrpc.md`
- [ ] `ui/ugui/world-space-ui.md` (if using World Space UI)

---

## 🎯 Priority Legend

| Icon | Meaning | When to Read |
|------|---------|--------------|
| 🔴 **Must Read** | Critical standards | Before ANY coding |
| 🟡 **Recommended** | Important patterns | When implementing relevant features |
| 🟢 **Reference** | Lookup material | When you encounter specific issues |

---

## 💡 Quick Tips

**New to the project?**
1. Start with "Priority Reading Order" (30 min)
2. Bookmark this page
3. Refer back when implementing specific features

**Starting a new feature?**
1. Check "Common Scenarios" table
2. Read the recommended files
3. Review code examples in `examples/good/`

**Stuck on a problem?**
1. Search this README for keywords
2. Check the relevant subsection
3. Read the 🔴 Must Read files in that category

---

## 📝 Change Log

| Date | Description |
|------|-------------|
| 2025-10-31 | Converted to Quick Reference format with scenario-based navigation |
| 2025-10-30 | Added testing/ directory with 8 comprehensive testing standards files |
| 2025-10-29 | Added documentation/ directory with 6 writing guide files |
| 2025-10-28 | Initial version created. Common rules extracted and standardized. |

---

## 📦 Usage Notes

These coding standards are designed to be shared across multiple Unity projects via Git submodule or similar mechanisms.

Apply the standards consistently across:
- Prototypes and production code
- Single-player and multiplayer projects
- 2D and 3D projects

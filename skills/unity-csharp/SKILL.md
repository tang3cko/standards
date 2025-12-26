---
name: unity-csharp
description: Unity C# coding standards including naming conventions, code organization, error handling, performance optimization, and ScriptableObject architecture. Use when writing, reviewing, or refactoring Unity C# code.
---

# Unity C# coding standards

Apply Tang3cko coding standards when working with Unity C# code.

## Quick reference

### Naming conventions

- Classes/Interfaces: PascalCase, interfaces use `I` prefix
- Private fields: camelCase
- Public fields/Properties: PascalCase
- Methods: PascalCase, start with verb
- Boolean: `is`, `has`, `can`, `should` prefix
- ScriptableObject: `SO` suffix
- EventChannel: `EventChannelSO` suffix
- Namespace: `ProjectName.Category`

### Priority levels

- **P1 (Required)**: Must follow
- **P2 (Recommended)**: Should follow
- **P3 (Optional)**: Nice-to-have

## Detailed guides

Read these files for complete standards:

### Core

- [Naming conventions](../../unity/core/naming-conventions.md)
- [Code organization](../../unity/core/code-organization.md)
- [Error handling](../../unity/core/error-handling.md)
- [Comments documentation](../../unity/core/comments-documentation.md)
- [Performance](../../unity/core/performance.md)
- [Unity specifics](../../unity/core/unity-specifics.md)

### Architecture

- [Architecture overview](../../unity/architecture/README.md) - Pattern selection guide
- [ScriptableObject](../../unity/architecture/scriptableobject.md)
- [Event channels](../../unity/architecture/event-channels.md)
- [Variables](../../unity/architecture/variables.md)
- [Runtime sets](../../unity/architecture/runtime-sets.md)
- [Reactive entity sets](../../unity/architecture/reactive-entity-sets.md)
- [Dependency management](../../unity/architecture/dependency-management.md)
- [Extension patterns](../../unity/architecture/extension-patterns.md)

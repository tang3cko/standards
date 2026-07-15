---
name: godot-core
description: Write Godot 4.x + .NET (C#) code following Tang3cko standards — naming, project structure, export/NodePath gotchas, Resource-based decoupling patterns, autoloads. Use when writing or reviewing C# code, scenes, or resources in a Godot project.
when_to_use: Triggers on "Godot", "GodotSharp", ".tscn", ".tres", "NodePath", "autoload", "Area3D", "QueueFree", "GlobalClass", "Camera3D", "Container", "MouseFilter", "ゴドー", "ゴドット", "シーン構成".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Core Standards

Help users write, review, and refactor Godot 4.x + .NET (C#) code following Tang3cko core standards.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references   | matching the code domain (naming-and-structure/gotchas/code-patterns)"
TaskCreate: "Phase 2: Write/review code | follow Core Rules"
TaskCreate: "Phase 3: Self-Check        | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Convention questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the references named by the workflow path | Always |
| 3 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

Apply these to every piece of Godot C# you write, edit, or approve. Detailed rationale lives in references/ — these tables are the contract.

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| Scene file (`.tscn`) | PascalCase | `Main.tscn`, `Player.tscn` |
| Script file (`.cs`) | PascalCase, no suffix | `PlayerInteractor.cs` |
| Class | PascalCase; Godot nodes are `partial` | `public partial class PlayerInteractor : Node` |
| Resource file (`.tres`) | snake_case | `key_brass.tres` |
| Folder | PascalCase | `Scripts/`, `Interaction/Core/` |
| Namespace | Project name (singular) | `namespace Smith;` |
| Interface | `I{Concept}` | `IInteractable` |
| Validator | `{Concept}Validator : Resource` | `RequiredItemValidator` |
| EventChannel | `{Concept}EventChannel : Resource` | `InteractionContextEventChannel` |

### Silent-Failure Guards

These fail without an error message — treat them as hard rules:

- `[Export] NodePath` is always initialized: `[Export] private NodePath _path = new();`
- Never `[Export]` a typed node reference (`Camera3D` etc.) — it stays null. Export a `NodePath` and resolve with `GetNodeOrNull<T>()` in `_Ready()`
- `[Export]` property casing in `.tscn` must match the C# spelling exactly (no snake_case auto-conversion)
- Never child an `Area3D` under a `StaticBody3D` — place them as siblings under a common `Pivot`
- `.tres` files are cached by load path (shared instance); set `resource_local_to_scene = true` when per-instance state is needed
- Never assign `Camera3D.Current = false` directly — call `ClearCurrent(enableNext: false)` (a bare assignment lets Godot silently auto-promote another live camera)
- A hand-authored `MeshInstance3D` under a `Skeleton3D` needs an explicit `skeleton = NodePath("..")` — without it the mesh freezes in bind pose with no warning
- Never set anchors/offsets from code on a `Container`-derived node (`SetAnchorsAndOffsetsPreset` etc.) — it collides with the layout pass (godot#88788)
- A full-screen overlay `Control` must use `MouseFilter.Ignore`, not `Pass` — `Pass` still eats clicks meant for the controls underneath

### Dependency Priority

```
1. Resource-based EventChannel (.tres)  <- Cross-system decoupling (ReactiveSO)
2. Node Signal                          <- Single emitter, guaranteed in scene; unsubscribe in _ExitTree()
3. [Export] NodePath injection          <- Explicit, visible in Inspector
4. Group lookup (AddToGroup/GetFirstNodeInGroup) <- Scene-local service, unique target
5. Autoload singleton                   <- Global-lifetime systems only
```

### Anti-Patterns

Don't: autoload-by-default, typed node `[Export]`s, group lookup of an ancestor inside a descendant's `_Ready()` (depth-first ordering returns null), leaving node Signal subscriptions dangling past `_ExitTree()`.
Do: split player controller by responsibility (movement vs interaction), compose behavior with `Resource` validators, keep logic in pure C# classes so it stays testable (see godot-testing).

## Workflow

Identify the task type and follow the matching path.

### Writing new code

1. Load the references matching the code domain (naming-and-structure, gotchas, code-patterns)
2. Generate code following the Core Rules above and the loaded standards
3. Run the Self-Check below before presenting

### Reviewing code

1. Check against the Core Rules above plus the matching references
2. Note specific issues with line numbers
3. Categorize issues by priority (must fix, should fix, nice to have)

### A convention question

1. Load the reference matching the question domain
2. Explain with Bad/Good code examples where applicable

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant code.

- [ ] All file/class/resource names match the Naming table (`.tres` snake_case, everything else PascalCase)
- [ ] Every Silent-Failure Guard holds (`NodePath = new()`, no typed node exports, exact export casing, no Area3D under StaticBody3D, `ClearCurrent(enableNext: false)` not `Current = false`, explicit `skeleton` NodePath on hand-authored MeshInstance3D, no code-driven Container anchors, overlay Control `MouseFilter.Ignore`)
- [ ] Dependencies use the highest viable rung of the Dependency Priority ladder
- [ ] Node Signal subscriptions are released in `_ExitTree()`
- [ ] Namespace matches the project's RootNamespace
- [ ] No Anti-Pattern present

## Reference Files

| File | Use When |
|------|----------|
| references/naming-and-structure.md | Naming conventions, directory structure, .NET configuration |
| references/gotchas.md | Export/NodePath/casing pitfalls, `_Ready()` ordering, QueueFree, joypad input, Label3D, Camera3D.Current, skeleton NodePath, Container anchors, MouseFilter, .gitignore case-folding, engine warnings, signal unsubscribe scope |
| references/code-patterns.md | Interaction system, ReactiveSO (EventChannel/Variable/RuntimeSet), autoloads, player split, group lookup, Signal vs EventChannel |

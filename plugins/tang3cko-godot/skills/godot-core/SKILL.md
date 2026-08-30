---
name: godot-core
description: Write Godot 4.x + .NET (C#) code following Tang3cko standards — naming, project structure, export/NodePath gotchas, Resource-based decoupling patterns, autoloads. Use when writing or reviewing C# code, scenes, or resources in a Godot project.
when_to_use: Triggers on "Godot", "GodotSharp", ".tscn", ".tres", "NodePath", "autoload", "Area3D", "QueueFree", "GlobalClass", "Camera3D", "Container", "MouseFilter", "ゴドー", "ゴドット", "シーン構成".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Core Standards

Tang3cko's Godot 4.x + .NET (C#) standards for naming, project structure, and Resource-based
decoupling. The Core Rules below are always in force; the acceptance criteria live in references/.

The Silent-Failure Guards are the part that bites. Every one of them fails with no error message
and no warning, so violating code reads as correct in review and ships broken.

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

### Platform Support

iOS/Android C#/.NET export is officially **experimental** as of Godot 4.7.1 (2026-07) — desktop only is stable, web is unsupported. iOS ships on NativeAOT: reflection-based `System.Text.Json` is disabled at runtime there, so shipped C# uses source-generated `JsonSerializerContext`, and "works on Android (Mono/JIT)" proves nothing for iOS. Details: `references/gotchas.md`.

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
- [ ] Every Silent-Failure Guard in the list above holds, checked item-by-item
- [ ] Dependencies use the highest viable rung of the Dependency Priority ladder
- [ ] Node Signal subscriptions to longer-lived emitters are released in `_ExitTree()`
- [ ] Namespace matches the project's RootNamespace
- [ ] No Anti-Pattern present; shipped code respects the Platform Support constraint (no new unchecked reflection)
- [ ] The references named by the chosen workflow path were actually Read this session — SKILL.md is only the index; the acceptance criteria live in references/, so skipping them means working against an incomplete standard
- [ ] The `## Checklist` at the end of every reference loaded this session was run item-by-item against the work — those checklists are the per-domain definition of done

## Reference Files

Each file below is the only place its listed content exists — SKILL.md deliberately does not restate it.

| File | Load When |
|------|-----------|
| references/naming-and-structure.md | Any naming or file-placement decision beyond the Naming table above. Only source for: directory layout (Scripts/Scenes mirroring, Resources split by data type), `Tools/` vs lowercase `scripts/` macOS case-collision, .NET baseline configuration (Sdk version, TargetFramework, Nullable, RootNamespace) |
| references/gotchas.md | Touching exports, scene-tree lifecycle, input handling, UI Controls, `.gitignore`, or code that ships to mobile. Only source for: the failure mechanics behind every Silent-Failure Guard, QueueFree timing, joypad idle-motion threshold, autoload ordering, `_Ready()` depth-first trap, Label3D mirroring, `.gitignore` case-folding, engine-warning triage, the signal-unsubscribe lifetime rule, iOS NativeAOT / experimental mobile C# constraints |
| references/code-patterns.md | Designing system communication or composing gameplay systems. Only source for: interaction-system layer layout and the `IInteractable` contract, ReactiveSO implementations (EventChannel/Variable/RuntimeSet code), autoload conventions, player-controller split, group-lookup applicability conditions, Signal-vs-EventChannel decision criteria |

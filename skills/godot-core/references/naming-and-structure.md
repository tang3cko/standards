# Naming and Structure

File naming conventions and directory structure for Godot 4.x + .NET projects.

## Naming Conventions

| Artifact | Convention | Example |
|----------|------------|---------|
| Scene files (`.tscn`) | PascalCase | `Main.tscn`, `Player.tscn`, `Pickup.tscn` |
| Script files (`.cs`) | PascalCase, no suffix | `PlayerInteractor.cs`, `InteractionValidator.cs` |
| Class names | PascalCase; Godot nodes are `partial` | `public partial class PlayerInteractor : Node` |
| Resource files (`.tres`) | snake_case | `key_brass.tres`, `on_interaction_context_changed.tres` |
| Folder names | PascalCase | `Scripts/`, `Scenes/`, `Player/`, `Interaction/Core/` |
| C# namespace | Project name (singular) | `namespace Smith;`, `namespace EscapeRoom;` |
| Interface names | `I{Concept}` | `IInteractable`, `IInteractor` |
| Validator abstract | `{Concept}Validator : Resource` | `InteractionValidator`, `RequiredItemValidator` |
| EventChannel | `{Concept}EventChannel : Resource` | `InteractionContextEventChannel` |

## Export Property Casing

`[Export]` property names are referenced in `.tscn` files with the **exact same spelling**. There is no automatic snake_case conversion (unlike GDScript exports).

```csharp
[Export] public string Verb { get; set; }
```

```ini
# In .tscn the name stays PascalCase
Verb = "Use"
```

A casing mismatch fails silently, so it can become a critical bug. See `gotchas.md` for details.

## Directory Structure

```
ProjectRoot/
├── Scripts/
│   ├── {Domain}/
│   │   ├── Core/           # Abstractions and interfaces
│   │   └── {Concrete}      # Implementations
│   ├── Player/
│   ├── UI/
│   ├── Inventory/
│   └── Util/
├── Scenes/
│   ├── {Domain}/           # Mirror the Scripts/ hierarchy
│   ├── Main.tscn           # Entry point
│   ├── Levels/
│   ├── UI/
│   └── Interactables/
├── Resources/
│   ├── {DataType}/         # Events/, Items/, Validators/, Variables/
│   └── {Name}.tres
├── addons/                 # Shared patterns such as ReactiveSO
└── Tests/                  # Separate .csproj, placed as a peer
```

### Structural principles

- **Mirror Scripts and Scenes hierarchies**: e.g. `Scripts/Interaction/Core/` pairs with `Scenes/Interactables/`
- **Split Resources by data type**: semantic folders (`Events/`, `Items/`, `Validators/`), not per-feature folders
- **No Unity-style `Assets/` folder**: Godot resolves relative paths inside `.tscn` / `.tres` on its own
- **Tests/ is a separate peer .csproj**: excluded from the main build via `<Compile Remove="Tests/**" />`

## .NET Configuration

Baseline settings for Godot 4.x + .NET projects.

| Item | Value |
|------|-------|
| Godot.NET.Sdk | 4.6.x |
| TargetFramework | net10.0 |
| Nullable | disable |
| ImplicitUsings | enable (Tests only) |
| RootNamespace | Project name (e.g. `Smith`) |

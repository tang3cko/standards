# Testing Strategy

xUnit-based testing strategy using the Humble Object pattern. Only pure C# logic that does not depend on Godot nodes is tested.

## Policy

- **Framework**: xUnit (v2 packages: `xunit` + `xunit.runner.visualstudio`). xUnit v3 ships as a separate `xunit.v3` package and turns test projects into standalone executables — the link-reference pattern below works with either, but existing projects stay on v2 (security-fix-only upstream) until a deliberate migration
- **In scope**: Godot-independent pure C# logic (calculation, data transformation, validation)
- **Out of scope**: Node-based implementations, UI, scene behavior (manual verification, or future integration tests)
- **Placement**: `Tests/` as a peer directory, excluded from the main `.csproj` via `<Compile Remove="Tests/**" />`

## Project Structure

```
ProjectRoot/
├── {Project}.csproj           # Main project (excludes Tests)
├── Scripts/
│   └── ...
└── Tests/
    ├── Tests.csproj           # Separate project
    └── {Feature}Test.cs       # PascalCase + "Test" suffix
```

### Tests.csproj pattern

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>MyGame.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.*" />
    <!-- Only needed when linked files use Godot value types (Vector2, Color, ...);
         a fully engine-free logic layer can drop this reference entirely -->
    <PackageReference Include="GodotSharp" Version="4.6.*" />
  </ItemGroup>
  <ItemGroup>
    <Compile Include="../Scripts/Inventory/InventoryStack.cs" Link="InventoryStack.cs" />
    <!-- Link only the pure-logic files that are needed -->
  </ItemGroup>
</Project>
```

`<Compile Include="../Scripts/.../*.cs" Link="..." />` **link-references** only the pure C# files from the main `Scripts/`. Files that depend on Godot nodes are never linked.

## Humble Object Pattern

Logic lives in pure C# classes separated from Godot nodes, which keeps it testable.

### Example: CraftingRecipe

```csharp
namespace MyGame;

public class CraftingRecipe
{
    public string Id { get; set; }
    public List<RecipeIngredient> Ingredients { get; set; }

    public bool Matches(IEnumerable<InventoryStack> available)
    {
        return Ingredients.All(ing =>
            available.Any(s => s.ItemId == ing.ItemId && s.Count >= ing.Count));
    }
}
```

```csharp
namespace MyGame.Tests;

public class RecipeApplierTest
{
    [Fact]
    public void Matches_ReturnsTrueWhenAllIngredientsPresent()
    {
        var recipe = new CraftingRecipe { /* ... */ };
        var inventory = new[] { new InventoryStack("iron", 2) };

        Assert.True(recipe.Matches(inventory));
    }
}
```

## Choosing What to Test

| Target | Test? |
|--------|-------|
| Pure data transformation / calculation (CraftingRecipe, InventoryLayoutCalculator) | ◎ Unit test |
| Data registry consistency (Items, Recipes) | ◎ Integrity test |
| Godot Node lifecycle | × Manual verification |
| Scene transitions / UI display | × Manual verification (future: integration test) |
| Input handling | × Manual verification |

## Running Tests

```bash
# In the Tests/ directory
dotnet test
```

## Testing Classes That Reference `internal` Helpers

When the pure logic under test depends on an `internal` class, that dependency must also be linked in `Tests.csproj` via `<Compile Include="..." Link="..." />`.

```xml
<ItemGroup>
  <Compile Include="../Scripts/Voxel/Core/MarchingCubesMesher.cs" Link="MarchingCubesMesher.cs" />
  <Compile Include="../Scripts/Voxel/Core/MarchingCubesTables.cs" Link="MarchingCubesTables.cs" />
</ItemGroup>
```

Example: `MarchingCubesMesher` references `internal MarchingCubesTables`, so linking only `MarchingCubesMesher.cs` fails to compile.

`InternalsVisibleTo` would also work, but the link approach is preferred because `Tests.csproj` then explicitly documents which pure-logic files depend on which supporting implementations.

## Checklist

- [ ] Every linked file is pure C# — no Godot node dependency anywhere in the link list (Godot value types are acceptable only with the `GodotSharp` package reference in place)
- [ ] Every `internal` dependency of a linked file is itself linked in `Tests.csproj`, or the build breaks
- [ ] The main `.csproj` still excludes the test tree via `<Compile Remove="Tests/**" />`
- [ ] Test files are `{Feature}Test.cs`, methods are `Method_Condition_ExpectedResult`, namespace is `{Project}.Tests`
- [ ] Each test reads as Arrange / Act / Assert and asserts one behavior
- [ ] `dotnet test Tests/Tests.csproj` was run (or the user was told it was not, and why)

# Testing Strategy

xUnit-based testing strategy using the Humble Object pattern. Only pure C# logic that does not depend on Godot nodes is tested.

## Policy

- **Framework**: xUnit
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
    <RootNamespace>Smith.Tests</RootNamespace>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="xunit" Version="2.*" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.*" />
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
namespace Smith;

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
namespace Smith.Tests;

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

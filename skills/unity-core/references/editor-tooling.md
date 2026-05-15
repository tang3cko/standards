# Editor Tooling

Patterns for one-click scene assembly, prefab generation, and developer-facing editor windows.

---

## Setup Editor Pattern

Most non-trivial features need scaffolding (prefabs, ScriptableObject assets, scene wiring). Manual setup is error-prone and not idempotent. Use a static class in an Editor asmdef with a `[MenuItem]` entry point.

### `BuildAssets()` / `WireScene()` separation

Split the work into two pure phases. Each phase is independently testable and can be re-run safely.

```csharp
public static class DepthMeterSetupEditor
{
    [MenuItem("Tools/Unburied/Setup Depth Meter")]
    public static void Run()
    {
        BuildAssets();
        WireScene();
    }

    public static void BuildAssets()
    {
        // Create or load SO assets, prefabs, materials.
        // No scene mutation here.
    }

    public static void WireScene()
    {
        // Open scene, find / create instances, set serialized references.
        // No asset creation here.
    }
}
```

Why separate:

- `BuildAssets` runs in tests via `OneTimeSetUp` to verify SO/prefab structure without touching scenes
- `WireScene` runs separately for scene-only re-wiring after manual asset edits
- Failures are isolated to a single phase

### Idempotence

Both phases must be safe to re-run. Each `Find` / `Load` / `CreateXxx` call must check existence first.

```csharp
private static ItemSO LoadOrCreateItem(string assetPath, string itemId, string displayName, Capability capability)
{
    var existing = AssetDatabase.LoadAssetAtPath<ItemSO>(assetPath);
    if (existing != null) return existing;

    var instance = ScriptableObject.CreateInstance<ItemSO>();
    // ... configure ...
    AssetDatabase.CreateAsset(instance, assetPath);
    return instance;
}
```

### Test coverage

A `PrefabStructureTest` verifies `BuildAssets()` produces the expected hierarchy. The test calls `BuildAssets()` in `OneTimeSetUp` and asserts on the resulting prefab tree (component presence, serialized references, child anchors). Idempotence is verified by calling it twice.

```csharp
[TestFixture]
public class DepthMeterPrefabStructureTest
{
    [OneTimeSetUp]
    public void Setup() => DepthMeterSetupEditor.BuildAssets();

    [Test]
    public void Prefab_HasPromptAnchor() { /* assert */ }

    [Test]
    public void BuildAssets_CalledTwice_DoesNotDuplicate()
    {
        DepthMeterSetupEditor.BuildAssets();  // second call
        var assets = AssetDatabase.FindAssets("DepthMeter_Pickup");
        Assert.That(assets.Length, Is.EqualTo(1));
    }
}
```

### Scoped log suppression

Setup editors often produce expected warnings (e.g. "X not assigned" before wiring completes). Suppress them with a scoped `ILogHandler` that filters by log prefix only — never with global `Debug.unityLogger.filterLogType`.

```csharp
public static void WireScene()
{
    var previous = Debug.unityLogger.logHandler;
    Debug.unityLogger.logHandler = new ScopedPrefixFilter(previous, "[WorldPickup]", "[DepthDisplayController]");
    try
    {
        // ... wiring ...
    }
    finally
    {
        Debug.unityLogger.logHandler = previous;
    }
}
```

---

## Constants

Asset paths and prefab paths live in `private const string` at the top of the editor class so reorganizing the asset tree is a single-place change.

```csharp
private const string MainScenePath = "Assets/_Project/Scenes/Main.unity";
private const string LanternBasePrefabPath = "Assets/_Project/Prefabs/Environment/Lantern_Base.prefab";
```

---

## References

- [code-organization.md](code-organization.md) - Runtime / Editor Boundary section
- See `unity-testing` skill for test patterns including `BuildAssets` `OneTimeSetUp` use

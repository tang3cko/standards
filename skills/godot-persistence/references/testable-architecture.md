# Testable Persistence Architecture

## Humble Object Split

Persistence code splits cleanly into a pure-C# half and a Godot-runtime half. Keep the boundary strict — it determines what a unit test can reach without spinning up the engine.

| Layer | Depends on Godot runtime | Typical contents |
|-------|--------------------------|-------------------|
| Format & logic (pure C#) | No | Binary serializer/deserializer, CRC32, version migrator, path resolver, typed save-data mapper (`Capture`/`Apply`), debug formatter/renderer |
| I/O orchestration (Godot-dependent) | Yes (`FileAccess`, `DirAccess`, `GD.Push*`) | The save/load coordinator (open/write/rename/delete), backup rotator |

Concretely, a project's `Scripts/Persistence/` folder typically looks like:

```
SaveSerializer.cs      <- pure C#: binary encode/decode + CRC32
Crc32.cs                <- pure C#
SaveDataMigrator.cs     <- pure C#: stepwise version migration
SavePaths.cs            <- pure C#: path resolution + sandbox override
SaveDataFormatter.cs    <- pure C#: renders a loaded dict for a debug viewer
SessionSnapshot.cs      <- pure C#: typed SaveData <-> Dictionary<string,object> mapper
SaveSystem.cs            <- Godot-dependent: FileAccess/DirAccess orchestration
BackupRotator.cs         <- Godot-dependent: DirAccess rename/remove for rotation
```

The pure-C# files carry no `using Godot;` for anything beyond value types that don't require the runtime (if even that). They link-reference into a unit test project the same way any other pure logic does — see the godot-testing skill's Layer 1 (xUnit, link-referenced pure C#) for the mechanics of `Tests.csproj` link references and `internal` visibility.

**Should fix**: when a type in this folder starts calling `Godot.FileAccess`/`DirAccess`/`GD.*`, that's a signal it has crossed into I/O orchestration — either accept it stays untested by unit tests (covered by E2E/manual play-test instead), or split the Godot-touching part out into its own smaller type so the logic around it stays pure.

## Typed Mapper Over the Raw Dictionary

Keep the wire-level `Dictionary<string, object>` payload separate from the game's actual save schema. A typed data class plus a static `Capture`/`Apply` mapper is pure C# and testable independent of the binary format:

```csharp
public sealed class SessionSaveData
{
    public int Day;
    public int Balance;
    public Dictionary<string, int> Flags = new();
    // ...
}

public static class SessionSnapshot
{
    public static Dictionary<string, object> Capture(SessionSaveData data) { /* ... */ }
    public static SessionSaveData Apply(Dictionary<string, object> dict) { /* ... */ }
}
```

This isolates schema changes (adding a field) to the mapper, keeps the binary format itself schema-agnostic, and gives unit tests a round-trip target (`Apply(Capture(x))` should reproduce `x`) that doesn't require touching a file at all.

## Session vs Persistent Store Separation

When "start a new run" should reset progress but *not* erase everything the game has ever learned about the player (unlocks, endings seen, meta-counters), split into two independently-lifecycled stores rather than one store with conditional reset rules baked into individual keys.

```
SessionStore (reset on "new run" / "start over")
├── DAY_3_STARTED
├── MAIL_RECEIVED_042
├── CURRENT_ARC_FLAG
└── ...

PersistentStore (cleared only by an explicit "erase all data" action)
├── GAME_CLEARED
├── ENDING_A_SEEN
├── TOTAL_PLAYTHROUGHS
└── ...
```

Why two stores instead of one with a "persistent" bit per key:

- **Bug prevention.** "Does this reset" becomes a property of *which store the key lives in*, not something a future author has to remember to set correctly on every new key. A key added to the wrong store fails loudly (wrong file, wrong test) rather than silently leaking state across runs.
- **Maintainability.** Anything reading `PersistentStore` is explicitly opting into "this must survive a reset" semantics — reviewers can spot a change's blast radius from the store name alone.
- **Design headroom.** Content that reacts to persistent state (`ENDING_B_SEEN == true` changing later dialogue, a playthrough counter unlocking bonus content) becomes a straightforward two-store condition rather than a special case bolted onto session data.

If a rule/condition system evaluates flags from both stores (e.g. "day 1 of a run AND the player has cleared the game before"), reference each store explicitly by name/tag in the rule data rather than merging both stores into one flat namespace — a flag name colliding across stores should be impossible to write by accident, not just unlikely.

Each store gets its own save file (its own `SaveSystem`/coordinator instance, its own path family) rather than being sections within one file, so a session reset can delete/reinitialize its file without touching the persistent one at all.

## Test Sandbox Boundary

The persistence logic's own responsibility here is narrow: the pure-C# `SavePaths`-style resolver must support an env-var override (see save-system.md), and any code path that performs a *destructive* sandbox operation (most notably a recursive delete of "the save directory" before a test run) must refuse to proceed unless that override is actively set, and must reject a sandbox path that resolves to (or looks unsafely close to) the real user-data directory.

```csharp
var sandboxDir = System.Environment.GetEnvironmentVariable(SavePaths.SandboxEnvVar);
if (string.IsNullOrEmpty(sandboxDir))
    throw new InvalidOperationException(
        $"{SavePaths.SandboxEnvVar} is not set - refusing to run against the real user:// profile.");

var realUserDir = OS.GetUserDataDir();
if (string.Equals(sandboxDir, realUserDir, StringComparison.OrdinalIgnoreCase)
    || string.IsNullOrWhiteSpace(sandboxDir.Trim('/')))
    throw new InvalidOperationException($"Sandbox dir '{sandboxDir}' looks unsafe to recursively delete.");
```

This check belongs conceptually next to the persistence code (it exists because of how `SavePaths` works), even though the harness that calls it — `.runsettings` env var wiring, per-test cleanup timing, engine-error-as-failure — is owned by the godot-testing skill. Treat this file's rule as: "persistence code must make the override safe to use"; treat godot-testing's rule as: "the test harness must actually use it correctly." See godot-testing's e2e-harness reference for the harness side.

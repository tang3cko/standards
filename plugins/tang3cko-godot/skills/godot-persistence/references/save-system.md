# Save System: Atomic Write, Backup Rotation, Load Fallback, Paths

## File Family

One save "family" is a base path plus three derived paths, all owned by a single coordinator instance:

```
user://save.dat        <- main (loaded first)
user://save.dat.tmp    <- write target before rename
user://save.dat.bak0   <- previous main
user://save.dat.bak1   <- one generation before that
```

Extend the `.bakN` chain for more generations if the project wants deeper history; two generations is a reasonable default (protects against "this save wrote garbage" without unbounded disk growth). Give each independent save family (e.g. a profile save vs. a per-run save) its own coordinator instance instead of multiplexing multiple logical saves through one file.

## Save Flow (Atomic Write + Rotation)

```csharp
public SaveError Save(IReadOnlyDictionary<string, object> dict)
{
    // 1. Serialize to an in-memory buffer FIRST. A failure here (e.g. an
    //    unsupported value type deep in the dict) must never touch disk.
    byte[] bytes;
    using (var ms = new MemoryStream())
    {
        _serializer.Serialize(ms, dict);
        bytes = ms.ToArray();
    }

    // 2. Write the full buffer to the temp path. Probe the file's error
    //    state before disposing it — a failed write doesn't always throw.
    using (var file = FileAccess.Open(TempPath, FileAccess.ModeFlags.Write))
    {
        if (file == null) return SaveError.OpenFailed;
        file.StoreBuffer(bytes);
        if (file.GetError() != Error.Ok) return SaveError.WriteFailed;
    }

    // 3. Rotate existing main/bak0/bak1 BEFORE renaming temp into place.
    var rotateErr = BackupRotator.Rotate(SavePath, Backup0Path, Backup1Path);
    if (rotateErr != Error.Ok) return SaveError.RenameFailed;

    // 4. Promote temp -> main. A single rename is atomic on the underlying
    //    filesystem, so a crash here loses at most this save attempt.
    var renameErr = PromoteTempToMain();
    if (renameErr != Error.Ok) return SaveError.RenameFailed;

    return SaveError.Ok;
}
```

**Must fix — step ordering.** Rotate happens *after* the tmp write succeeds and *before* the promote-rename. Getting this backwards creates real corruption windows:

- Rotating before the tmp write is confirmed good risks discarding a valid backup generation to make room for a save that then fails to write.
- Promoting before rotating means a crash between the two leaves both the old file (still at the main path) and the new file (already renamed over it, or not) in an ambiguous state, and the backup chain never advances.

**Must fix — never write directly to the main path.** Every write goes to `.tmp` and gets promoted by rename. Writing to `save.dat` directly and truncating it in place means any crash, power loss, or write error mid-save corrupts the only copy the game has.

## Backup Rotation

```csharp
public static class BackupRotator
{
    // Called AFTER a fresh tmp write succeeds, BEFORE promoting tmp -> main:
    //   1. Delete the oldest backup slot if present.
    //   2. Rename each remaining backup up one slot (bak0 -> bak1, ...).
    //   3. Rename current main -> the youngest backup slot.
    // Caller promotes temp -> main afterward.
    public static Error Rotate(string mainPath, string bak0Path, string bak1Path)
    {
        // Anchor DirAccess at the paths' own directory (see "Path Resolution"
        // below) — do not assume a "user://"-anchored handle works for all paths.
    }
}
```

This is inherently Godot-runtime-dependent (`DirAccess`/`FileAccess`) and stays out of unit test scope; cover it with an E2E or manual-play save/load pass instead (see godot-testing skill).

## Load Flow (N-Generation Fallback)

```csharp
public (Dictionary<string, object> Data, LoadError Error, LoadSource Source) Load()
{
    var sources = new (string Path, LoadSource Source)[]
    {
        (SavePath,    LoadSource.Primary),
        (Backup0Path, LoadSource.Backup0),
        (Backup1Path, LoadSource.Backup1),
    };

    bool anyExisted = false;
    foreach (var (path, source) in sources)
    {
        if (!FileAccess.FileExists(path)) continue;
        anyExisted = true;

        var (data, status) = TryLoadFrom(path); // deserialize + migrate
        if (status == SaveSerializer.DeserializeStatus.Ok && data != null)
        {
            if (source != LoadSource.Primary)
                GD.Print($"SaveSystem.Load: recovered from {source}"); // don't stay silent
            return (data, LoadError.Ok, source);
        }
        GD.PushWarning($"SaveSystem.Load: {path} unusable ({status})");
    }

    return anyExisted
        ? (null, LoadError.AllSourcesCorrupt, LoadSource.Default)
        : (null, LoadError.NoSaveFound, LoadSource.Default);
}
```

Rules:

- Walk generations in freshness order and stop at the first one that both deserializes (`DeserializeStatus.Ok`) and migrates cleanly (`MigrationStatus.UpToDate` or `Migrated`) — treat `FromFuture`/`Invalid` migration results the same as a deserialize failure for fallback purposes.
- **Distinguish "nothing ever existed" from "everything that existed is unusable."** These are different states for the caller: the first is a normal first-run/new-profile case, the second means the player likely lost progress and may deserve a message, not a silent blank save.
- A recovery from a non-primary source is worth logging even on success — it's a signal something upstream (a bad shutdown, a full disk) is corrupting the main file even though the player didn't lose data this time.

## Path Resolution (SavePaths pattern)

Centralize every save-path computation behind one resolver so no call site hand-builds a `user://` string, and so tests can redirect all of them at once via a single env var.

```csharp
public static class SavePaths
{
    public const string SandboxEnvVar = "MYGAME_SAVE_DIR";
    private const string UserPrefix = "user://";

    public static string Resolve(string fileName, Func<string, string> getEnvVar = null)
    {
        getEnvVar ??= Environment.GetEnvironmentVariable; // BCL, not OS.GetEnvironment —
                                                            // keeps this class Godot-runtime-free
        var sandboxDir = getEnvVar(SandboxEnvVar);
        if (string.IsNullOrEmpty(sandboxDir))
            return UserPrefix + fileName;

        // Godot's FileAccess/DirAccess accept absolute OS paths directly, not
        // only res:// / user:// schemes, so the sandbox dir is used verbatim.
        return sandboxDir.TrimEnd('/', '\\') + "/" + fileName;
    }

    // DirAccess.Rename/Remove need a handle anchored in the target's own
    // directory. A DirAccess opened at "user://" cannot reliably operate on
    // an out-of-scheme sandbox path once the env var override is active, so
    // every rename/remove call site must split like this instead of assuming
    // "user://" as the anchor.
    public static (string Dir, string FileName) SplitDirAndName(string path)
    {
        if (path != null && path.StartsWith(UserPrefix))
            return (UserPrefix, path[UserPrefix.Length..]);

        var lastSlash = path?.LastIndexOf('/') ?? -1;
        return lastSlash < 0 ? (".", path) : (path[..lastSlash], path[(lastSlash + 1)..]);
    }
}
```

**Must fix — sandbox override, not sandbox-only.** When the env var is unset, resolve `user://` exactly as before; only redirect when it's explicitly set. This keeps production behavior untouched while giving tests/CI a way to opt in.

**Must fix — pure C# env accessor.** Use `System.Environment.GetEnvironmentVariable`, not `Godot.OS.GetEnvironment`, so `SavePaths` has zero Godot runtime dependency and can link-reference into a unit test project (see testable-architecture.md). `Godot.Environment` also shadows `System.Environment` under `using Godot;` — qualify explicitly (`System.Environment`) at any call site that has both usings.

**Must fix — anchor DirAccess at the resolved directory, not a hardcoded scheme.** `BackupRotator` and the save coordinator's rename/remove calls must open their `DirAccess` at `SplitDirAndName(path).Dir`, never at a literal `"user://"`. Once a sandbox override points outside the `user://` scheme, a hardcoded anchor silently fails or operates on the wrong directory.

## Platform Notes (verified in a production Godot project, 2026-07)

**JSON serialization on iOS requires source generation.** If a project serializes save models with `System.Text.Json` (instead of, or alongside, the binary format), remember that Godot C# ships on .NET NativeAOT on iOS, where reflection-based `System.Text.Json` is disabled at runtime. Persistence models must use a source-generated `JsonSerializerContext`. The failure mode is nasty: every save call throws `InvalidOperationException` on iOS while Android (Mono/JIT) works fine — and because the throw happens inside the write path, code that streams JSON straight into the `.tmp` file (instead of serializing to memory first, as the Save Flow above requires) leaves zero-byte `.tmp` orphans behind as the only visible evidence.

**`user://` location on iOS.** The official docs only say mobile `user://` is "unique to the project and not accessible by other applications"; in practice on iOS it resolves inside the app container's `Documents/` directory — meaning save files can be user-visible via the Files app / iTunes file sharing if the corresponding Info.plist keys are enabled.

## Delete-All / Reset

A "delete all save data" operation (used by "erase save" UI and by test teardown) must:

- Enumerate the exact known file set (main, tmp, each backup generation) rather than doing a wildcard/glob delete — an unrelated file that happens to share a path prefix must never be swept up.
- Go through the same `SplitDirAndName` anchoring as rotation/promotion.
- Treat a missing file as success, not an error — deleting something already absent should be a no-op.

## Checklist

- [ ] Save flow order is exactly: serialize to memory → write `.tmp` (and probe `GetError()` before disposing) → rotate backups → rename `.tmp` over main; nothing ever writes directly to the main path
- [ ] Load walks main → bak0 → bak1 → … in freshness order, treats `FromFuture`/`Invalid` migration like a deserialize failure for fallback, and logs any recovery from a non-primary source
- [ ] `LoadError.NoSaveFound` (nothing ever existed) and `LoadError.AllSourcesCorrupt` (something existed, none usable) are distinct results
- [ ] Every path is built through the `SavePaths` resolver; the sandbox env var redirects only when set, via `System.Environment` (not `OS.GetEnvironment`)
- [ ] Every `DirAccess` used for rename/remove is anchored via `SplitDirAndName(path).Dir`, never a literal `"user://"`
- [ ] If `System.Text.Json` touches save models anywhere, a source-generated `JsonSerializerContext` is wired in (iOS NativeAOT disables the reflection path)

# Binary Save Format

A minimal, dependency-free binary format for save data: no third-party serializer, no reflection, no encryption (indie-scale save data has no attacker model that justifies it — omit encryption unless a concrete requirement says otherwise).

## File Layout

Little-endian throughout.

```
[Magic       4 bytes ASCII, e.g. "SAVX"]
[Version     4-byte int32]
[PayloadLen  4-byte uint32]
[Payload     N bytes]
[CRC32       4-byte uint32, computed over Payload only]
```

`Payload` encodes a `Dictionary<string, object>`, recursively:

```
[EntryCount  4-byte int32]
  For each entry:
    [KeyLen  2-byte uint16][Key UTF-8 bytes]
    [TypeTag 1 byte][Value]

TypeTag:
  0 = int32   (4 bytes)
  1 = int64   (8 bytes)
  2 = double  (8 bytes, IEEE754)
  3 = string  ([Len 4-byte int32][UTF-8 bytes])
  4 = nested-dict (recursive Payload, same shape)
```

A `Dictionary<string, object>` payload (rather than a typed DTO at the wire level) keeps the serializer and the game's save schema decoupled: adding a field to a save is a call-site change, not a format change. A typed mapper (`Capture`/`Apply` style, see testable-architecture.md) sits on top and is the layer that actually knows what the keys mean.

## Serializer

```csharp
public sealed class SaveSerializer
{
    public enum DeserializeStatus
    {
        Ok, Truncated, ChecksumMismatch, InvalidFormat, FromFutureVersion, InvalidVersion,
    }

    public void Serialize(Stream stream, IReadOnlyDictionary<string, object> data)
    {
        var payloadStream = new MemoryStream();
        using (var w = new BinaryWriter(payloadStream, Encoding.UTF8, leaveOpen: true))
            WriteDict(w, data);

        var payload = payloadStream.ToArray();
        uint crc = Crc32.Compute(payload);

        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        writer.Write(Magic);
        writer.Write(SaveDataMigrator.CurrentVersion);
        writer.Write((uint)payload.Length);
        writer.Write(payload);
        writer.Write(crc);
    }

    public (Dictionary<string, object> Data, DeserializeStatus Status) Deserialize(Stream stream)
    {
        // 1. Read the fixed header into a buffer first so truncation is
        //    detected cleanly instead of throwing mid-parse.
        // 2. Verify Magic byte-for-byte -> InvalidFormat on mismatch.
        // 3. Read PayloadLen bytes of payload -> Truncated if short.
        // 4. Read the trailing CRC32 and compare against Crc32.Compute(payload)
        //    -> ChecksumMismatch on mismatch, BEFORE attempting to parse the payload.
        // 5. Only after the checksum passes, parse Payload into a Dictionary.
        //    Wrap parsing in try/catch for EndOfStreamException (-> Truncated)
        //    and InvalidDataException (-> InvalidFormat); never let a malformed
        //    file throw out of Deserialize.
    }
}
```

**Must fix**: verify the checksum *before* parsing the payload into objects. Parsing a corrupted payload first (and only checksumming after) risks throwing from deep inside recursive parsing instead of failing cleanly with `ChecksumMismatch`.

**Must fix**: `Deserialize` never throws for malformed input. A corrupt or truncated save file is expected input (disk full mid-write, external corruption, an old build's stray bytes) — it must produce a status enum the caller can branch on, not an exception the caller must remember to catch.

## CRC32

A standard reflected CRC32 (polynomial `0xEDB88320`) over the payload bytes only — not over the header, so version/magic changes don't invalidate an otherwise-valid payload's checksum semantics.

```csharp
public static class Crc32
{
    private static readonly uint[] Table = BuildTable();

    public static uint Compute(ReadOnlySpan<byte> data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (var b in data)
            crc = (crc >> 8) ^ Table[(crc ^ b) & 0xFF];
        return crc ^ 0xFFFFFFFF;
    }

    private static uint[] BuildTable()
    {
        var table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint c = i;
            for (int j = 0; j < 8; j++)
                c = (c & 1) != 0 ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
            table[i] = c;
        }
        return table;
    }
}
```

CRC32 catches accidental corruption (partial write, disk error, manual tampering by a curious player) — it is not a security mechanism. Don't reach for a cryptographic hash here; it solves a problem this format doesn't have.

## Version Migration

One monotonically increasing version int; a stepwise migrator walks a loaded dict forward one version at a time rather than special-casing "convert from any old version directly to latest."

```csharp
public static class SaveDataMigrator
{
    public const int CurrentVersion = 3;
    public const string SaveVersionKey = "save_version";

    public enum MigrationStatus { UpToDate, Migrated, FromFuture, Invalid }

    // Steps[v] transforms a version-v dict into version v+1 in place.
    private static readonly Dictionary<int, Action<Dictionary<string, object>>> Steps = new()
    {
        [1] = dict => { /* e.g. rename a key, add a new field with a default */ },
        [2] = dict => { /* next step */ },
    };

    public static (Dictionary<string, object> Result, MigrationStatus Status) Migrate(
        IReadOnlyDictionary<string, object> input)
    {
        if (input == null) return (null, MigrationStatus.Invalid);
        if (!input.TryGetValue(SaveVersionKey, out var v) || v is not int version)
            return (null, MigrationStatus.Invalid);
        if (version < 0) return (null, MigrationStatus.Invalid);
        if (version > CurrentVersion) return (CopyOf(input), MigrationStatus.FromFuture);
        if (version == CurrentVersion) return (CopyOf(input), MigrationStatus.UpToDate);

        var working = CopyOf(input);
        for (var v2 = version; v2 < CurrentVersion; v2++)
        {
            if (Steps.TryGetValue(v2, out var step)) step(working);
            working[SaveVersionKey] = v2 + 1;
        }
        return (working, MigrationStatus.Migrated);
    }
}
```

Rules:

- **Register steps as the schema evolves, never rewrite old steps.** A step that already shipped is a historical fact about what old saves look like; changing it retroactively breaks migration for saves written between the old and new step.
- **`FromFuture` is not `Invalid`.** A save written by a newer build of the game (player rolled back, or is on a beta channel) is not corrupt — it's just ahead. Treat it as its own status so the caller can message the player correctly ("this save is from a newer version") instead of "corrupted."
- A project that starts at version 1 with no steps yet still defines `CurrentVersion` and the `Steps` dictionary shape up front — the first schema change only needs to add an entry, not restructure the migrator.

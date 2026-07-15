---
name: godot-persistence
description: Design and implement save/persistence systems for Godot 4.x + .NET following Tang3cko standards — binary save format with CRC32 checksums and version migration, atomic tmp-then-rename writes, N-generation backup rotation with fallback loading, session-vs-persistent data separation, and env-var-sandboxed save paths for tests. Use when writing or reviewing save/load code in a Godot project.
when_to_use: Triggers on "save", "セーブ", "persistence", "永続化", "CRC", "バックアップ", "backup", "user://", "SaveData", "atomic write", "アトミック書き込み", "ロールバック", "SavePaths", "SaveSystem".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Persistence Standards

Help users design, write, and review save/load and persistence code for Godot 4.x + .NET projects following Tang3cko standards.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references   | matching the code domain (binary-format/save-system/testable-architecture)"
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

Apply these to every piece of Godot save/persistence C# you write, edit, or approve. Detailed rationale lives in references/ — these tables are the contract.

### File Layout

Every save file is Magic + Version + Payload + CRC32, little-endian:

| Field | Size | Notes |
|-------|------|-------|
| Magic | 4 bytes ASCII | Format identity; reject on mismatch (`InvalidFormat`), never try to parse further |
| Version | 4-byte int32 | Schema version; drives migration on load |
| PayloadLen | 4-byte uint32 | Guards against reading past a truncated stream |
| Payload | N bytes | Recursive `Dictionary<string, object>` encoding (int32/int64/double/string/nested-dict, each behind a 1-byte type tag) |
| CRC32 | 4-byte uint32 | Computed over Payload only; mismatch is corruption, not a crash |

Deserialize returns a status enum (`Ok`, `Truncated`, `ChecksumMismatch`, `InvalidFormat`, `FromFutureVersion`, `InvalidVersion`), never throws for a bad file — a corrupt save is expected input, not an exceptional one.

### Version Migration

- One monotonically increasing `CurrentVersion` int, stored inside the payload as well as the header
- A stepwise migrator (`Steps[v]` transforms version `v` into `v+1`) walks a loaded dict forward to `CurrentVersion` one step at a time — never a single big "convert anything to latest" function
- `version > CurrentVersion` is `FromFuture`, not `Invalid` — a newer client wrote this save; do not silently downgrade or corrupt it
- `version < 0` or a missing version key is `Invalid` — refuse to guess

### Atomic Write + Backup Rotation

Save flow, in this exact order — reordering any step reopens a crash window:

1. Serialize to an in-memory buffer first; never stream a header to disk before the whole payload is known good (a mid-serialize crash must never leave a partial header on disk)
2. Write the full buffer to a `.tmp` path
3. Rotate backups (oldest generation deleted, each older generation renamed up one slot, current main renamed into the youngest backup slot) — **only after the tmp write succeeded**
4. Rename `.tmp` over the main path — a rename is a single filesystem operation, so a crash here loses at most the in-flight save, never corrupts the previous one

Rotation must run *before* the promote-rename, not after: a crash between "promote" and "rotate" would leave two files claiming to be the freshest generation.

### N-Generation Load Fallback

Load walks main → bak0 → bak1 → … → default, stopping at the first source that deserializes and migrates cleanly:

- A source that exists but fails (checksum, truncation, future-version) is a warning, not a fatal error — fall through to the next generation
- Only report "no save data" when a source never existed anywhere in the chain; report "all sources corrupt" when at least one existed but none was usable — callers need to distinguish first-run from data loss
- Successful recovery from a non-primary source should be logged (not silent) — the player got lucky, but something upstream still corrupted the main file

### Path Resolution (SavePaths pattern)

- All save-path construction goes through one static resolver, never ad hoc `user://` string-building at call sites
- The resolver checks a sandbox env var first; when set, every path resolves under it (absolute OS path) instead of `user://`; when unset, it resolves `user://<name>` as normal
- Path resolution must stay Godot-runtime-independent (use `System.Environment`, not `OS.GetEnvironment`) so it can link-reference into a pure C# test project without a Godot runtime
- Directory-scoped filesystem ops (rename, remove) must open their `DirAccess` anchored at the resolved path's *own* directory, never a hardcoded `user://` handle — a handle fixed at `user://` cannot operate on an out-of-scheme sandbox path

### Session vs Persistent Data Separation

When a game has "start new run" semantics alongside data that must survive across runs (clear flags, unlock counts, meta-progression), split into two independent stores rather than one store with ad hoc reset logic:

| Store | Resets on | Holds |
|-------|-----------|-------|
| Session-scoped | New run / "start over" | In-run flags, day/turn counters, delivered-but-not-yet-consumed content |
| Persistent | Explicit "erase all data" only | Cross-run unlock flags, ending-seen flags, playthrough counters |

A rule/condition system that reads both stores should reference them by an explicit store tag (not merge them into one namespace) so "does this flag reset on new run" is answered by which store it lives in, not by convention or naming.

### Test Sandbox Safety

Any test harness that redirects save I/O away from the real user profile must, before doing anything destructive (e.g. recursive delete of the sandbox directory):

- **Refuse to run** if the sandbox env var is unset — never silently fall back to writing into the developer's real save directory
- Compare the resolved sandbox path against the real user-data directory and reject if they match (case-insensitively) or if the sandbox path is empty/root-like, before a recursive delete runs

E2E test/CI details (env var wiring, engine-error-as-failure, harness setup) belong to the godot-testing skill — this skill only owns the safety checks the persistence code itself must guarantee.

### Humble Object Split

| Layer | Depends on Godot runtime | Examples |
|-------|--------------------------|----------|
| Format/logic | No — pure C# | Binary serializer/deserializer, CRC32, version migrator, path resolver, save-data formatter/mapper |
| I/O orchestration | Yes — `FileAccess`/`DirAccess`/`GD.Push*` | The save/load coordinator, backup rotator |

Keep the split strict: if a type touches `Godot.FileAccess`, `Godot.DirAccess`, or `GD.*`, it is I/O orchestration and stays untested by unit tests (covered by E2E/manual play-test instead); everything else should be pure C# and link-referenced into a test project (see godot-testing skill).

## Workflow

Identify the task type and follow the matching path.

### Writing new code

1. Load the references matching the code domain (binary-format for serializer/migrator work, save-system for write/rotate/load-fallback/path work, testable-architecture for layering and session/persistent split decisions)
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

- [ ] Save file layout is Magic + Version + Payload + CRC32; deserialize returns a status enum, never throws on a corrupt/truncated file
- [ ] Migration walks stepwise (`v -> v+1 -> ... -> Current`), and `FromFuture` is handled distinctly from `Invalid`
- [ ] Save flow order is: serialize to memory → write tmp → rotate backups → rename tmp over main (in that order)
- [ ] Load falls through main → bak0 → bak1 → … → default, and distinguishes "no save anywhere" from "all sources corrupt"
- [ ] All save paths are built through one resolver; that resolver honors a sandbox env var and stays Godot-runtime-independent
- [ ] Any `DirAccess` used for rename/remove is anchored at the target path's own directory, not a hardcoded `user://`
- [ ] Session-scoped and persistent data live in separate stores, not one store with conditional reset logic
- [ ] A test harness that recursively deletes a sandbox directory refuses to run when the sandbox env var is unset, and rejects a sandbox path that matches (or looks unsafely close to) the real user-data directory
- [ ] Format/migration/path-resolution logic is pure C# (no `Godot.FileAccess`/`DirAccess`/`GD.*`); only the I/O coordinator and backup rotator touch the Godot runtime

## Reference Files

| File | Use When |
|------|----------|
| references/binary-format.md | Serializer/deserializer layout, type tags, CRC32, DeserializeStatus, stepwise version migration |
| references/save-system.md | Atomic write ordering, N-generation backup rotation, load fallback chain, SavePaths resolver, sandbox env var, DirAccess anchoring pitfalls |
| references/testable-architecture.md | Humble Object split for persistence code, session-vs-persistent store separation, link-referencing pure C# persistence logic into tests |

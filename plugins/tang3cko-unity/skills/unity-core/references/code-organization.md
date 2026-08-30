# Code Organization

Directory structure, namespace conventions, and file organization.

---

## Directory Structure

### Scripts directory

```
Assets/_Project/Scripts/
├── Audio/                 # AudioManager, BGMManager
├── Camera/                # Camera control
├── Combat/                # IDamageable, etc.
├── Core/                  # GameManager, SaveSystem, GameState
├── Data/                  # DataSO definition scripts
├── Enemy/                 # Enemy-related
├── Events/                # EventChannelSO definition scripts
├── Player/                # Player-related
├── Shop/                  # Shop-related
├── UI/                    # UI-related
└── Weapon/                # Weapon-related
```

### ScriptableObjects directory

```
Assets/_Project/ScriptableObjects/
├── Data/                  # Data definition SOs
│   ├── Audio/
│   ├── Enemies/
│   ├── Weapons/
│   └── ...
├── Events/                # EventChannelSO instances
│   ├── Player/
│   ├── Enemy/
│   └── ...
├── RuntimeSets/           # RuntimeSetSO instances
└── Settings/              # GameSettingsSO, etc.
```

### Prefabs directory

```
Assets/_Project/Prefabs/
├── Enemies/
├── Environment/
├── Projectiles/
├── UI/
└── Weapons/
```

---

## Classification Criteria

**Feature Domain-Based:**
- `Player/` - PlayerHealth, PlayerWallet, PlayerController
- `Enemy/` - EnemyHealth, EnemyController
- `Weapon/` - WeaponManager, ProjectileController
- `UI/` - UIManager, MenuController

**Cross-Cutting Concerns:**
- `Core/` - GameManager, GameState, SaveSystem
- `Events/` - EventChannelSO definitions
- `Data/` - DataSO definitions
- `Combat/` - IDamageable, IPoolable

---

## One File One Class

- One public class per file
- Filename must match class name

```
// Good
PlayerController.cs
  └─ public class PlayerController { }

// Bad
GameLogic.cs
  ├─ public class GameManager { }
  ├─ public class TurnManager { }     // Should be separate
  └─ public class ScoreManager { }    // Should be separate
```

**Exception:** Closely related types in same file

```csharp
// QuestSO.cs - Main class with related types
public class QuestSO : ScriptableObject { }

[System.Serializable]
public class ObjectiveData { }

public enum QuestType { Gather, Deliver }
```

---

## File Structure Order

See the unity-core SKILL.md Core Rules for the standard file structure order.

Additional guidance for larger classes:

- Group `[Header]` sections logically: Dependencies first, then Settings, then Event Channels
- Unity lifecycle methods should follow execution order: `Awake -> OnEnable -> Start -> Update -> FixedUpdate -> LateUpdate -> OnDisable -> OnDestroy`
- Separate event handlers (e.g., `OnTriggerEnter`) from private helper methods

---

## Namespace Conventions

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Category
{
    // All code must be in a namespace
    // Match namespace to directory path
}
```

---

## Runtime / Editor Boundary

How much editor concern (gizmo, `EditorUtility`, editor-only `SerializeField`) may leak into runtime classes.

### Pattern selection

| Pattern | When to use |
|---------|-------------|
| **C** — `#if UNITY_EDITOR` direct in runtime | Tag/flush side-effects of runtime processing |
| **D** — Editor asmdef + official attribute | Editor feature itself (`[CustomEditor]`, `[DrawGizmo]`, `[InitializeOnLoad]`, `[MenuItem]`, `SettingsProvider`) |
| **B** — `InternalsVisibleTo` + Editor asmdef | Editor continuously reads runtime `internal` state |
| **A** — Interface DI | Need swappable runtime/editor implementations or test seam |

### Gizmos

Use **Pattern D exclusively**. `OnDrawGizmos*` in runtime classes is forbidden in committed code.

```csharp
// Editor asmdef only
[DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
static void Draw(MyComponent target, GizmoType _)
{
    Gizmos.DrawWireCube(target.transform.position, Vector3.one);
}
```

Rationale: Unity 6 official docs state `[DrawGizmo]` "allows you to keep your gizmo-drawing code out of your component scripts so that it is not included in builds." Zero exceptions keeps lint and review trivial.

### Pattern C — allowed conditions (all five must hold)

A `#if UNITY_EDITOR` direct call in a runtime class is acceptable **only if all five conditions hold**:

1. The editor API call is a **leaf** (return value not used to branch downstream)
2. **No state held** (no field added or mutated)
3. **Runtime contract not polluted** (public/internal API shape unchanged)
4. **No test seam required**
5. The call is **post-processing of runtime work**, not editor functionality itself

```csharp
// Allowed (Pattern C)
public bool SaveToAsset()
{
    if (worldDataAsset == null) return false;
    Serializer.Write(worldDataAsset, model);

#if UNITY_EDITOR
    UnityEditor.EditorUtility.SetDirty(worldDataAsset);
#endif
    return true;
}
```

### AssetDatabase usage

`AssetDatabase` use is not blanket-banned. The line is responsibility, not namespace.

| Allowed in Pattern C | Trigger D/B escalation |
|----------------------|------------------------|
| `EditorUtility.SetDirty(target)` | `AssetDatabase.Refresh()` |
| `AssetDatabase.SaveAssetIfDirty(target)` | `AssetDatabase.CreateAsset(...)` |
| Single-target dirty/flush only | `AssetDatabase.LoadAssetAtPath<T>(...)` |
| | `AssetDatabase.ImportAsset(...)` |
| | `AssetDatabase.MoveAsset(...)` |
| | `AssetDatabase.DeleteAsset(...)` |
| | Any asset discovery / creation / reload / reorganization |

Single-target flush of a runtime save = Pattern C. Asset-set orchestration = editor functionality, escalate.

### Escalation triggers (1+ → mandatory D/B promotion discussion)

When any of the following appears, escalate from C to D or B in the same PR or open a refactor ticket:

- A second Pattern C call appears in the same class
- The C call starts using the editor API's return value or callbacks
- An editor-only helper / utility extraction is introduced
- The C call reaches into editor workflow APIs (asset DB orchestration, `Selection`, `Undo`, `EditorApplication.delayCall`, `SerializedObject`)

### Editor state must not be held in runtime classes

Editor state means fields kept for editor workflow, visualization, tooling, selection state, foldout state, or caching that does not contribute to player-build runtime semantics.

- `[SerializeField]` of editor state in runtime asmdef is **forbidden**
- Project-shared settings → `ScriptableObject` + `SettingsProvider` in Editor asmdef
- Per-user editor state → `ScriptableSingleton<T>` + `[FilePath(..., FilePathAttribute.Location.ProjectFolder)]` or `EditorPrefs`

### `OnValidate`

Runtime asmdef `OnValidate` should be `#if UNITY_EDITOR` guarded, and runtime-required invariants should live in shared helpers reused by `Awake` / `Initialize` and `OnValidate`.

```csharp
#if UNITY_EDITOR
private void OnValidate()
{
    EnforceInvariants();
    if (target == null)
        Debug.LogWarning($"[{GetType().Name}] target not assigned on {gameObject.name}.", this);
}
#endif

private void EnforceInvariants()
{
    speed = Mathf.Max(0f, speed);
}
```

Rationale: Unity does not strip `OnValidate` from builds even with managed stripping high; the guard is a hygiene measure.

### `Debug.DrawLine` / `Debug.DrawRay`

`UnityEngine.Debug.Draw*` lives in the runtime namespace and is not a boundary violation. Out of scope for the lint. Permanent retention in production code requires PR-level justification of ongoing diagnostic value.

### Detection

| Layer | Required | Tool |
|-------|----------|------|
| CI | Yes (single source of truth) | `rg`-based tripwire |
| PR checklist | Yes (design judgment) | Reviewer |
| Pre-commit hook | Optional | Local DX |

The CI tripwire flags candidates for review; final classification happens in PR. Do not encode the full rule in a shell one-liner.

CI lint targets (under runtime-asmdef paths):

- `using UnityEditor` / `UnityEditor.`
- `OnDrawGizmos` / `OnDrawGizmosSelected`
- `[DrawGizmo]` attribute usage

A Roslyn analyzer is not required: the C# compiler already rejects `UnityEditor` references in runtime build configurations, and `Microsoft.Unity.Analyzers` (UNT0001–UNT0042) has no equivalent diagnostic for this concern.

---

## ScriptableObject Serializer Pattern

When a `ScriptableObject` asset persists data with versioning concerns, the SO must not own the format-dispatch logic. Extract the read/write into a pure C# static class.

```csharp
// SO holds raw fields and accessors only
public sealed class VoxelWorldData : ScriptableObject
{
    [SerializeField] private byte[] densityField;
    [SerializeField] private byte[] materialIdField;
    [SerializeField] private int version;

    public const int CurrentVersion = 1;

    public byte[] DensityField => densityField;
    public byte[] MaterialIdField => materialIdField;
    public int Version => version;
    public bool HasDensityField => densityField != null && densityField.Length > 0;

    public void SetDensityField(byte[] field, int version) { /* ... */ }
}

// Pure C# static class owns version dispatch
public static class VoxelWorldDataSerializer
{
    public static void Read(VoxelWorldData asset, out DensityField density, out byte[] materialIds)
    {
        switch (asset.Version)
        {
            case 1: /* v1 decode */ break;
            case 2: /* v2 decode */ break;
            default: throw new NotSupportedException($"Version {asset.Version}");
        }
    }

    public static void Write(VoxelWorldData asset, DensityField density, int chunkSize, byte[] materialIds = null)
    {
        // Encode + asset.SetDensityField(...) at CurrentVersion
    }
}
```

Why:

- The SO does not need to know how its own bytes are interpreted, so adding a new format version means editing one class, not two
- The serializer is pure C# and unit-testable in Edit Mode
- Migration paths are localized to one switch statement
- The SO stays a simple data container, matching the "data SO holds data, not behavior" intent

---

## UPM Package Consumption

### Private / org-internal packages: git submodule + `file:` reference

Private UPM packages must be consumed as a git submodule under `Packages/_vendor/<Name>` plus a `file:` reference in `manifest.json` — NOT as an SSH git URL dependency.

```jsonc
// Packages/manifest.json
{
  "dependencies": {
    // Private package: submodule at Packages/_vendor/ReactiveSO,
    // file: path points at the package root (where package.json lives)
    "com.tang3cko.reactiveso": "file:_vendor/ReactiveSO/Assets/ReactiveSO"
  }
}
```

```bash
git submodule add git@github.com:org/ReactiveSO.git Packages/_vendor/ReactiveSO
```

Why (verified in a production Unity project, 2026-07):

- GUI-launched Unity does not inherit shell environment variables (`OP_SSH_KEY`, `GIT_SSH_COMMAND`), so UPM's SSH resolution fails with `sign_and_send_pubkey` even when the same URL works from a terminal
- 1Password's SSH agent suppresses signing prompts from non-foreground processes by design, so there is no interactive recovery path
- The submodule + `file:` approach removes SSH traffic from Unity startup entirely — git auth happens in the terminal (submodule update), where the agent works

Caveat: the `file:` path must point at the **package root inside the submodule** — the directory containing `package.json` — which may be a subdirectory of the repo (e.g. `file:_vendor/ReactiveSO/Assets/ReactiveSO`).

### HTTPS git URL dependencies

Public OSS packages over HTTPS git URLs (e.g. LitMotion) are unaffected by the SSH problem and remain fine as direct `manifest.json` git URL dependencies.

---

## References


- [naming.md](naming.md) - Naming conventions
- [comments.md](comments.md) - Documentation standards

## Checklist

- [ ] One public class per file, filename matches the class; only closely related `[Serializable]` types/enums share a file
- [ ] Namespace is `ProjectName.Category` and matches the directory path
- [ ] No `OnDrawGizmos*` in runtime classes — gizmos use Pattern D (`[DrawGizmo]` in an Editor asmdef) exclusively
- [ ] Every `#if UNITY_EDITOR` block in a runtime class satisfies all five Pattern C conditions, and no escalation trigger (second C call, return value used, editor workflow APIs) is present
- [ ] No editor-state `[SerializeField]` in runtime asmdefs; versioned SO persistence delegates format dispatch to a pure C# serializer class
- [ ] Private UPM packages are consumed via `Packages/_vendor/` submodule + `file:` reference pointing at the package root, never an SSH git URL in `manifest.json`

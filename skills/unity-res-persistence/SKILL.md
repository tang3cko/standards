---
name: unity-res-persistence
description: ReactiveEntitySet snapshot and persistence. Time-travel debugging, save/load. Use when saving entity state in Unity.
---

# RES Persistence

## Purpose

Capture and restore ReactiveEntitySet state using the Snapshot API. Enables time-travel mechanics, save/load systems, and state rollback.

---

## Snapshot API - P1

### Creating snapshots

```csharp
// Capture current state
EntitySetSnapshot<UnitState> snapshot = unitSet.CreateSnapshot(Allocator.Persistent);

// snapshot.Data - Copy of all entity data
// snapshot.EntityIds - Copy of all entity IDs
// snapshot.Count - Entity count at capture time
```

### Restoring snapshots

```csharp
// Restore to captured state
unitSet.RestoreSnapshot(snapshot);

// This:
// 1. Clears current EntitySet
// 2. Registers all entities from snapshot
// 3. Fires OnSetChanged event
```

### Memory management

```csharp
// Persistent allocator: manual disposal required
var snapshot = unitSet.CreateSnapshot(Allocator.Persistent);
// ... use snapshot ...
snapshot.Dispose(); // Required!

// Temp allocator: auto-disposed after 4 frames
using (var snapshot = unitSet.CreateSnapshot(Allocator.Temp))
{
    // Use within this scope
}
```

---

## Time-travel pattern - P1

```csharp
public class HistoryManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;

    private List<EntitySetSnapshot<UnitState>> history = new();
    private int maxSnapshots = 300; // 5 seconds at 60fps

    private void LateUpdate()
    {
        // Capture every frame
        if (history.Count >= maxSnapshots)
        {
            history[0].Dispose();
            history.RemoveAt(0);
        }
        history.Add(unitSet.CreateSnapshot(Allocator.Persistent));
    }

    public void Rewind(int frames)
    {
        int targetIndex = history.Count - 1 - frames;
        if (targetIndex >= 0 && targetIndex < history.Count)
        {
            unitSet.RestoreSnapshot(history[targetIndex]);

            // Truncate future (optional: for timeline branching)
            for (int i = history.Count - 1; i > targetIndex; i--)
            {
                history[i].Dispose();
                history.RemoveAt(i);
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var snapshot in history)
        {
            snapshot.Dispose();
        }
        history.Clear();
    }
}
```

---

## ReactiveEntitySetHolder - P1

Prevent ScriptableObject unload during scene transitions.

### Problem

```csharp
// Scene A: Register entities
enemySet.Register(1, state);

// Scene A unloads, Scene B loads
// If nothing references enemySet, Unity may unload it
// All runtime data lost!
```

### Solution

```csharp
public class GameManager : MonoBehaviour
{
    [SerializeField] private ReactiveEntitySetHolder holder;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        // holder keeps references to all RES assets
    }
}
```

### Setup

1. Create `ReactiveEntitySetHolder` asset
2. Click "Find All in Project" in Inspector
3. Reference holder from a `DontDestroyOnLoad` object

---

## Save/Load pattern - P2

### Save

```csharp
public void SaveGame(string path)
{
    var snapshot = unitSet.CreateSnapshot(Allocator.Temp);

    var saveData = new SaveData
    {
        entityCount = snapshot.Count,
        states = new UnitState[snapshot.Count],
        ids = new int[snapshot.Count]
    };

    snapshot.Data.Slice(0, snapshot.Count).CopyTo(saveData.states);
    snapshot.EntityIds.Slice(0, snapshot.Count).CopyTo(saveData.ids);

    string json = JsonUtility.ToJson(saveData);
    File.WriteAllText(path, json);

    snapshot.Dispose();
}
```

### Load

```csharp
public void LoadGame(string path)
{
    string json = File.ReadAllText(path);
    var saveData = JsonUtility.FromJson<SaveData>(json);

    unitSet.Clear();

    for (int i = 0; i < saveData.entityCount; i++)
    {
        unitSet.Register(saveData.ids[i], saveData.states[i]);
    }
}

[Serializable]
public class SaveData
{
    public int entityCount;
    public UnitState[] states;
    public int[] ids;
}
```

---

## Common mistakes - P1

### Wrong allocator for long-lived snapshots

```csharp
// NG: Temp becomes invalid after 4 frames
var snapshot = unitSet.CreateSnapshot(Allocator.Temp);
history.Add(snapshot); // Will corrupt!

// OK: Use Persistent for storage
var snapshot = unitSet.CreateSnapshot(Allocator.Persistent);
history.Add(snapshot);
// Remember to Dispose!
```

### Forgetting to dispose

```csharp
// NG: Memory leak
void SaveState()
{
    var snapshot = unitSet.CreateSnapshot(Allocator.Persistent);
    // Forgot Dispose!
}

// OK
void SaveState()
{
    var snapshot = unitSet.CreateSnapshot(Allocator.Persistent);
    try
    {
        // ... use snapshot ...
    }
    finally
    {
        snapshot.Dispose();
    }
}
```

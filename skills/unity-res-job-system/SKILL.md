---
name: unity-res-job-system
description: ReactiveEntitySet Job System integration. Orchestrator, Burst compilation, double buffering. Use when processing 1000+ entities.
---

# RES Job System Integration

## Purpose

High-performance parallel processing for ReactiveEntitySet using Unity's Job System and Burst compiler.

---

## When to use - P1

### Use Orchestrator when

- 1,000+ entities updating every frame
- Need Burst compilation for performance
- Running physics simulations or AI calculations

### Use direct SetData when

- Fewer than 1,000 entities
- Updates are infrequent (not every frame)
- Need immediate event notifications

---

## Double buffering - P1

```
Frame N:
┌─────────────────┐     ┌─────────────────┐
│  Front Buffer   │     │  Back Buffer    │
│  (Current State)│     │  (Next State)   │
│                 │     │                 │
│  Read by:       │     │  Written by:    │
│  - Rendering    │     │  - Jobs         │
│  - UI           │     │  - Simulation   │
└─────────────────┘     └─────────────────┘

After CompleteAndApply():
Buffers swap instantly via pointer exchange (O(1))
```

Benefits:
- **Lock-free reads** - Main thread reads front while Jobs write to back
- **No copying** - Pointer swap, not data copy
- **Consistent state** - Readers always see complete frame

---

## Orchestrator lifecycle - P1

```csharp
public class SimulationManager : MonoBehaviour
{
    [SerializeField] private UnitStateSetSO unitSet;

    private ReactiveEntitySetOrchestrator<UnitState> orchestrator;

    private void Start()
    {
        orchestrator = new ReactiveEntitySetOrchestrator<UnitState>(unitSet);
    }

    private void Update()
    {
        if (unitSet.Count == 0) return;

        JobHandle handle = ScheduleSimulation();
        orchestrator.ScheduleUpdate(handle, unitSet.Count);
    }

    private void LateUpdate()
    {
        orchestrator?.CompleteAndApply();
    }

    private void OnDestroy()
    {
        orchestrator?.Dispose();
        orchestrator = null;
    }
}
```

---

## Burst-compiled Job - P1

```csharp
[BurstCompile]
public struct UnitSimulationJob : IJobParallelFor
{
    [ReadOnly] public NativeSlice<UnitState> input;
    public NativeArray<UnitState> output;
    public float deltaTime;

    public void Execute(int index)
    {
        UnitState unit = input[index];

        // Update position
        unit.position += unit.velocity * deltaTime;

        // Boundary wrapping
        unit.position = math.fmod(unit.position + 100f, 200f) - 100f;

        output[index] = unit;
    }
}
```

### Scheduling

```csharp
private JobHandle ScheduleSimulation()
{
    var srcData = unitSet.Data;
    var srcIds = unitSet.EntityIds;
    var dstData = orchestrator.GetBackBuffer();
    var dstIds = orchestrator.GetBackBufferIds();

    // Copy IDs
    new NativeSlice<int>(dstIds, 0, srcIds.Length).CopyFrom(srcIds);

    var job = new UnitSimulationJob
    {
        input = srcData,
        output = dstData,
        deltaTime = Time.deltaTime
    };

    return job.Schedule(srcData.Length, 64);
}
```

---

## Common mistakes - P1

### Forgetting to dispose

```csharp
// NG: Memory leak
void OnDestroy()
{
    // orchestrator.Dispose(); // Forgot!
}

// OK
void OnDestroy()
{
    orchestrator?.Dispose();
    orchestrator = null;
}
```

### Forgetting to copy IDs

```csharp
// NG: IDs not in back buffer
private JobHandle ScheduleSimulation()
{
    var dstData = orchestrator.GetBackBuffer();
    // Missing: copy IDs to dstIds!
}

// OK
private JobHandle ScheduleSimulation()
{
    var srcIds = unitSet.EntityIds;
    var dstIds = orchestrator.GetBackBufferIds();
    new NativeSlice<int>(dstIds, 0, srcIds.Length).CopyFrom(srcIds);
    // ...
}
```

### Modifying RES during Job execution

```csharp
// NG: Unpredictable behavior
void Update()
{
    orchestrator.ScheduleUpdate(handle, count);
    unitSet.SetData(someId, newState); // Avoid!
}

// OK: Modify after CompleteAndApply
void LateUpdate()
{
    orchestrator.CompleteAndApply();
    unitSet.SetData(someId, newState); // Safe
}
```

---

## Performance tips - P2

### Batch size tuning

```csharp
// Small, fast operations: larger batches
job.Schedule(count, 256);

// Complex operations: smaller batches
job.Schedule(count, 32);

// Default starting point
job.Schedule(count, 64);
```

### Always use Burst

```csharp
[BurstCompile] // 10-100x performance improvement
public struct MyJob : IJobParallelFor
{
}
```

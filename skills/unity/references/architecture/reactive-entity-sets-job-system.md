# RES Job System Integration

## Purpose

High-performance parallel processing for ReactiveEntitySet using Unity's Job System and Burst compiler. Use the Orchestrator pattern for thousands of entities updating every frame.

---

## When to use - P1

### Use Orchestrator when

- 1,000+ entities updating every frame
- Need Burst compilation for performance
- Running physics simulations or AI calculations
- Want to parallelize updates across CPU cores

### Use direct SetData when

- Fewer than 1,000 entities
- Updates are infrequent (not every frame)
- Need immediate event notifications
- Simplicity over performance

---

## Double buffering - P1

Orchestrator maintains two copies of data.

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

- **Lock-free reads** - Main thread reads front buffer while Jobs write to back
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
        // 1. Create Orchestrator
        orchestrator = new ReactiveEntitySetOrchestrator<UnitState>(unitSet);
    }

    private void Update()
    {
        if (unitSet.Count == 0) return;

        // 2. Schedule Jobs
        JobHandle handle = ScheduleSimulation();
        orchestrator.ScheduleUpdate(handle, unitSet.Count);
    }

    private void LateUpdate()
    {
        // 3. Complete and swap buffers
        orchestrator?.CompleteAndApply();
    }

    private void OnDestroy()
    {
        // 4. Always dispose
        orchestrator?.Dispose();
        orchestrator = null;
    }
}
```

---

## NativeArray access - P1

### Reading current state

```csharp
// Front buffer (read-only for Jobs)
NativeSlice<UnitState> currentData = unitSet.Data;
NativeSlice<int> currentIds = unitSet.EntityIds;
```

### Writing next state

```csharp
// Back buffer (write target for Jobs)
NativeArray<UnitState> nextData = orchestrator.GetBackBuffer();
NativeArray<int> nextIds = orchestrator.GetBackBufferIds();
```

---

## Job implementation - P1

### Data struct (unmanaged required)

```csharp
public struct UnitState
{
    public float2 position;
    public float2 velocity;
    public int health;
    public int teamId;
}
```

### Burst-compiled Job

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

    // Copy IDs (unchanged in this simulation)
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

## Chaining Jobs - P2

```csharp
private JobHandle ScheduleSimulation()
{
    var data = orchestrator.GetBackBuffer();

    // Job 1: Physics
    var physicsJob = new PhysicsJob { data = data };
    var physicsHandle = physicsJob.Schedule(count, 64);

    // Job 2: AI (depends on physics)
    var aiJob = new AIJob { data = data };
    var aiHandle = aiJob.Schedule(count, 64, physicsHandle);

    // Job 3: Collision (depends on AI)
    var collisionJob = new CollisionJob { data = data };
    return collisionJob.Schedule(count, 64, aiHandle);
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

### Minimize main thread blocking

```csharp
// NG: Blocks main thread
void Update()
{
    var handle = ScheduleJob();
    handle.Complete(); // Blocking!
}

// OK: Complete at end of frame
void Update()
{
    var handle = ScheduleJob();
    orchestrator.ScheduleUpdate(handle, count);
}

void LateUpdate()
{
    orchestrator.CompleteAndApply();
}
```

### Always use Burst

```csharp
[BurstCompile] // 10-100x performance improvement
public struct MyJob : IJobParallelFor
{
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
    return job.Schedule(count, 64);
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

## References

- [Reactive Entity Sets](reactive-entity-sets.md) - Core concepts
- [Persistence](reactive-entity-sets-persistence.md) - Snapshot for time-travel
- [Unity Job System Manual](https://docs.unity3d.com/Manual/JobSystem.html)
- [Burst Compiler Manual](https://docs.unity3d.com/Packages/com.unity.burst@latest)

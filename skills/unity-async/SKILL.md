---
name: unity-async
description: Unity async patterns. Awaitable (Unity 6+), UniTask, Coroutines. Use when implementing async operations in .cs files.
---

# Unity async patterns

## Purpose

Follow Unity-specific conventions and patterns for async operations, including Awaitable (Unity 6+), UniTask, and Coroutines.

## Checklist

- [ ] Choose async pattern: Awaitable (Unity 6+), UniTask, or Coroutines
- [ ] Use `Time.deltaTime` for framerate-independent movement
- [ ] Apply `[RequireComponent]` for hard dependencies
- [ ] Use `FixedUpdate` for physics operations
- [ ] Subscribe to events in `OnEnable`, unsubscribe in `OnDisable`
- [ ] Cache `GetComponent` calls in `Awake`

---

## Async patterns comparison - P1

Unity offers multiple approaches for asynchronous operations. Choose based on your project's needs.

| Approach | Unity Version | Pros | Cons |
|----------|---------------|------|------|
| **Coroutines** | All | Simple, no dependencies, familiar | No try-catch, harder to compose |
| **Awaitable** | Unity 6+ | Built-in, zero dependencies, try-catch | Limited features (no WhenAll) |
| **UniTask** | Any (package) | Full-featured, performant, allocation-free | External dependency |

### Recommendation

- **Libraries**: Use `Awaitable` (zero external dependencies)
- **Apps/Games**: Use `UniTask` for production (full-featured)
- **Legacy/Simple**: Use Coroutines (still valid)

---

## Coroutines - P2

### Basic pattern

```csharp
public class CoroutineExample : MonoBehaviour
{
    private WaitForSeconds oneSecond;

    private void Awake()
    {
        // Cache WaitForSeconds to avoid garbage
        oneSecond = new WaitForSeconds(1f);
    }

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (true)
        {
            SpawnEnemy();
            yield return oneSecond;  // Reuse cached WaitForSeconds
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("Enemy spawned");
    }
}
```

### Stopping coroutines safely

```csharp
public class CoroutineControl : MonoBehaviour
{
    private Coroutine spawnCoroutine;

    public void StartSpawning()
    {
        // Stop existing coroutine before starting new one
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(2f);
        }
    }
}
```

---

## Awaitable (Unity 6+) - P1

Unity 6 introduces `Awaitable` as a built-in async/await solution.

### Basic pattern

```csharp
using UnityEngine;

public class AwaitableExample : MonoBehaviour
{
    private async void Start()
    {
        await SpawnEnemiesAsync();
    }

    private async Awaitable SpawnEnemiesAsync()
    {
        while (true)
        {
            SpawnEnemy();
            await Awaitable.WaitForSecondsAsync(1f);
        }
    }

    private void SpawnEnemy()
    {
        Debug.Log("Enemy spawned");
    }
}
```

### Awaitable equivalents

| Coroutine | Awaitable |
|-----------|-----------|
| `yield return null` | `await Awaitable.NextFrameAsync()` |
| `yield return new WaitForSeconds(1f)` | `await Awaitable.WaitForSecondsAsync(1f)` |
| `yield return new WaitForEndOfFrame()` | `await Awaitable.EndOfFrameAsync()` |
| `yield return new WaitForFixedUpdate()` | `await Awaitable.FixedUpdateAsync()` |

### Error handling with try-catch

```csharp
private async Awaitable LoadDataAsync()
{
    try
    {
        await Awaitable.WaitForSecondsAsync(1f);
        // Load data...
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Load failed: {ex.Message}");
    }
}
```

### Cancellation

```csharp
using System.Threading;

public class CancellableAsync : MonoBehaviour
{
    private CancellationTokenSource cts;

    private async void Start()
    {
        cts = new CancellationTokenSource();
        await SpawnLoopAsync(cts.Token);
    }

    private async Awaitable SpawnLoopAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            SpawnEnemy();
            await Awaitable.WaitForSecondsAsync(1f, token);
        }
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
```

### Limitations - P2

Awaitable lacks some Task features:
- No `WhenAll` / `WhenAny` (wrap in Task if needed)
- Unity Test Framework doesn't recognize Awaitable return type

```csharp
// Workaround: Wrap in Task for WhenAll
using System.Threading.Tasks;

private async Task WaitAllExample()
{
    await Task.WhenAll(
        LoadDataAsync().AsTask(),
        LoadTexturesAsync().AsTask()
    );
}
```

---

## UniTask - P1

For production apps/games, [UniTask](https://github.com/Cysharp/UniTask) provides a complete async solution.

### Installation

```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

### Basic pattern

```csharp
using Cysharp.Threading.Tasks;

public class UniTaskExample : MonoBehaviour
{
    private async void Start()
    {
        await SpawnEnemiesAsync();
    }

    private async UniTask SpawnEnemiesAsync()
    {
        while (true)
        {
            SpawnEnemy();
            await UniTask.Delay(1000);  // milliseconds
        }
    }
}
```

### UniTask advantages

```csharp
using Cysharp.Threading.Tasks;
using System.Threading;

public class UniTaskAdvanced : MonoBehaviour
{
    private CancellationTokenSource cts;

    private async UniTaskVoid Start()
    {
        cts = new CancellationTokenSource();

        // WhenAll support
        await UniTask.WhenAll(
            LoadDataAsync(),
            LoadTexturesAsync()
        );

        // Frame-aware delays
        await UniTask.DelayFrame(10);
        await UniTask.Yield(PlayerLoopTiming.PreUpdate);
    }

    private async UniTask LoadDataAsync()
    {
        await UniTask.Delay(1000, cancellationToken: cts.Token);
    }

    private async UniTask LoadTexturesAsync()
    {
        await UniTask.Delay(500, cancellationToken: cts.Token);
    }

    private void OnDestroy()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
```

---

## Time.deltaTime usage - P1

### Framerate-independent movement

```csharp
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    // Bad: Framerate-dependent
    private void Update()
    {
        transform.position += Vector3.forward * moveSpeed;  // Faster on high FPS!
    }

    // Good: Framerate-independent
    private void Update()
    {
        transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
    }
}
```

### Countdown timers

```csharp
public class Timer : MonoBehaviour
{
    private float timeRemaining = 10f;

    // Good: Use Time.deltaTime for countdowns
    private void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0)
            {
                OnTimerExpired();
            }
        }
    }
}
```

### When NOT to use Time.deltaTime

```csharp
// Bad: Don't use Time.deltaTime for instant actions
private void HandleJump()
{
    jumpForce += 10f * Time.deltaTime;  // Wrong! Jump fires once, not per-frame
}

// Good: Instant actions don't need deltaTime
private void HandleJump()
{
    ApplyJumpForce(10f);  // Correct! Apply full force instantly
}
```

---

## Update vs FixedUpdate - P1

### Update: framerate-dependent

```csharp
public class InputHandler : MonoBehaviour
{
    [SerializeField] private InputReaderSO inputReader;

    private void OnEnable()
    {
        inputReader.OnJumpEvent += HandleJump;
    }

    private void OnDisable()
    {
        inputReader.OnJumpEvent -= HandleJump;
    }

    // Discrete input: handle in callback
    private void HandleJump()
    {
        Jump();
    }

    // Good: Camera movement in Update
    private void Update()
    {
        UpdateCameraPosition();
    }
}
```

### FixedUpdate: physics operations

```csharp
public class PlayerPhysics : MonoBehaviour
{
    [SerializeField] private InputReaderSO inputReader;
    [SerializeField] private float moveForce = 10f;

    private Rigidbody rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        inputReader.OnMoveEvent += HandleMove;
    }

    private void OnDisable()
    {
        inputReader.OnMoveEvent -= HandleMove;
    }

    // Continuous input: cache value from event
    private void HandleMove(Vector2 input)
    {
        moveInput = input;
    }

    // Good: Physics in FixedUpdate using cached input
    private void FixedUpdate()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        rb.AddForce(movement * moveForce);
    }
}
```

### When to use each

| Method | Use Cases | Fixed Timestep |
|--------|-----------|----------------|
| **Update** | Input handling, Camera movement, UI updates, Timers | No (varies with FPS) |
| **FixedUpdate** | Rigidbody movement, AddForce, Physics calculations | Yes (0.02s default) |

---

## Component lifecycle methods - P1

### Execution order

```csharp
namespace ProjectName.Core
{
    public class LifecycleExample : MonoBehaviour
    {
        // 1. Awake: Called when script instance is loaded
        private void Awake()
        {
            // Initialize internal references
            // Cache GetComponent calls
        }

        // 2. OnEnable: Called when object becomes enabled
        private void OnEnable()
        {
            // Subscribe to events
        }

        // 3. Start: Called before first frame update (after all Awake)
        private void Start()
        {
            // Initialize with other objects' references
        }

        // 4-5. Update/LateUpdate: Called every frame
        private void Update() { }
        private void LateUpdate() { }

        // 6. OnDisable: Called when object becomes disabled
        private void OnDisable()
        {
            // Unsubscribe from events
        }

        // 7. OnDestroy: Called when object is destroyed
        private void OnDestroy()
        {
            // Cleanup resources
        }
    }
}
```

### OnEnable vs OnDisable for events

```csharp
using Tang3cko.ReactiveSO;

namespace ProjectName.Player
{
    public class PlayerEventHandler : MonoBehaviour
    {
        [SerializeField] private VoidEventChannelSO onGameStart;
        [SerializeField] private VoidEventChannelSO onGameEnd;

        // OnEnable: Subscribe to events
        private void OnEnable()
        {
            onGameStart?.OnEventRaised += HandleGameStart;
            onGameEnd?.OnEventRaised += HandleGameEnd;
        }

        // OnDisable: Unsubscribe to prevent memory leaks
        private void OnDisable()
        {
            onGameStart?.OnEventRaised -= HandleGameStart;
            onGameEnd?.OnEventRaised -= HandleGameEnd;
        }

        private void HandleGameStart() { }
        private void HandleGameEnd() { }
    }
}
```

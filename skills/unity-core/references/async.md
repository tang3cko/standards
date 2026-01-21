# Async patterns

## Comparison

| Approach | Unity Version | Pros | Cons |
|----------|---------------|------|------|
| **Coroutines** | All | Simple, no dependencies | No try-catch, harder to compose |
| **Awaitable** | Unity 6+ | Built-in, zero dependencies | Limited features (no WhenAll) |
| **UniTask** | Any (package) | Full-featured, performant | External dependency |

**Recommendation:**
- Libraries: Use Awaitable (zero dependencies)
- Apps/Games: Use UniTask (full-featured)
- Legacy/Simple: Use Coroutines

## Coroutines

```csharp
public class CoroutineExample : MonoBehaviour
{
    private WaitForSeconds oneSecond;

    private void Awake()
    {
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
            yield return oneSecond;
        }
    }
}
```

### Stopping coroutines

```csharp
private Coroutine spawnCoroutine;

public void StartSpawning()
{
    if (spawnCoroutine != null)
        StopCoroutine(spawnCoroutine);

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
```

## Awaitable (Unity 6+)

```csharp
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
```

### Awaitable equivalents

| Coroutine | Awaitable |
|-----------|-----------|
| `yield return null` | `await Awaitable.NextFrameAsync()` |
| `yield return new WaitForSeconds(1f)` | `await Awaitable.WaitForSecondsAsync(1f)` |
| `yield return new WaitForEndOfFrame()` | `await Awaitable.EndOfFrameAsync()` |
| `yield return new WaitForFixedUpdate()` | `await Awaitable.FixedUpdateAsync()` |

### Error handling

```csharp
private async Awaitable LoadDataAsync()
{
    try
    {
        await Awaitable.WaitForSecondsAsync(1f);
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Load failed: {ex.Message}");
    }
}
```

### Cancellation

```csharp
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
```

## UniTask

Installation:
```
https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask
```

```csharp
using Cysharp.Threading.Tasks;

private async UniTask SpawnEnemiesAsync()
{
    while (true)
    {
        SpawnEnemy();
        await UniTask.Delay(1000);
    }
}

// WhenAll support
await UniTask.WhenAll(
    LoadDataAsync(),
    LoadTexturesAsync()
);

// Frame-aware delays
await UniTask.DelayFrame(10);
await UniTask.Yield(PlayerLoopTiming.PreUpdate);
```

## Time.deltaTime usage

```csharp
// Good: Framerate-independent movement
private void Update()
{
    transform.position += Vector3.forward * moveSpeed * Time.deltaTime;
}

// Don't use for instant actions
private void HandleJump()
{
    ApplyJumpForce(10f);  // Full force, no deltaTime
}
```

## Update vs FixedUpdate

| Method | Use Cases |
|--------|-----------|
| **Update** | Input, Camera, UI, Timers |
| **FixedUpdate** | Rigidbody, AddForce, Physics |

```csharp
// Cache input from events, apply in FixedUpdate
private Vector2 moveInput;

private void HandleMove(Vector2 input) => moveInput = input;

private void FixedUpdate()
{
    rb.AddForce(new Vector3(moveInput.x, 0, moveInput.y) * moveForce);
}
```

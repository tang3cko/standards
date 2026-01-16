---
name: unity-performance
description: Unity performance optimization. Object pooling, caching, Update optimization. Use when optimizing .cs performance.
---

# Performance optimization

## Purpose

Improve game performance through caching strategies, object pooling, and Update loop optimization.

## Checklist

- [ ] Use object pooling for frequently instantiated objects
- [ ] Cache components in Awake/Start
- [ ] Use RuntimeSet instead of FindGameObjectsWithTag
- [ ] Event-driven architecture instead of Update polling
- [ ] Avoid string concatenation in Update
- [ ] Pre-allocate collections with known capacity
- [ ] Use LayerMask for Physics operations
- [ ] Cache WaitForSeconds in coroutines

---

## Object pooling - P1

### Basic implementation

```csharp
public class ObjectPool<T> where T : Component
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    private Transform parent;

    public ObjectPool(T prefab, Transform parent, int initialSize = 10)
    {
        this.prefab = prefab;
        this.parent = parent;

        // Pre-populate pool
        for (int i = 0; i < initialSize; i++)
        {
            T obj = Object.Instantiate(prefab, parent);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public T Get()
    {
        if (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            obj.gameObject.SetActive(true);
            return obj;
        }

        // Create new if pool is empty
        return Object.Instantiate(prefab, parent);
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            T obj = pool.Dequeue();
            Object.Destroy(obj.gameObject);
        }
    }
}
```

### Usage example

```csharp
namespace ProjectName.Enemy
{
    public class ProjectileManager : MonoBehaviour
    {
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform poolParent;

        private ObjectPool<Projectile> projectilePool;

        private void Awake()
        {
            projectilePool = new ObjectPool<Projectile>(projectilePrefab, poolParent, 20);
        }

        public void SpawnProjectile(Vector3 position, Vector3 direction)
        {
            Projectile projectile = projectilePool.Get();
            projectile.transform.position = position;
            projectile.Initialize(direction);
        }

        public void ReturnProjectile(Projectile projectile)
        {
            projectilePool.Return(projectile);
        }
    }
}
```

---

## Caching strategy - P1

### Component caching

```csharp
public class EnemyManager : MonoBehaviour
{
    // Bad: GetComponent every frame
    private void Update()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.forward);  // Heavy!
    }

    // Good: Cache in Awake
    private Transform cachedTransform;
    private Rigidbody cachedRigidbody;

    private void Awake()
    {
        cachedTransform = transform;
        cachedRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        cachedRigidbody.AddForce(Vector3.forward);
    }
}
```

### GameObject caching

```csharp
public class PlayerController : MonoBehaviour
{
    // Cache frequently accessed GameObjects
    private GameObject player;
    private GameObject mainCamera;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Bad: Find every frame
    private void BadUpdate()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");  // Heavy!
    }

    // Good: Use cached reference
    private void Update()
    {
        if (player != null)
        {
            // Use cached player
        }
    }
}
```

---

## Update loop optimization - P1

### Avoid heavy operations in Update

```csharp
// Bad: Heavy processing every frame
void Update()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");  // Very Heavy!
    foreach (var enemy in enemies)
    {
        ProcessEnemy(enemy);
    }
}

// Good: Use RuntimeSet + Event-Driven
[SerializeField] private EnemyRuntimeSetSO activeEnemies;
[SerializeField] private VoidEventChannelSO onEnemyStateChanged;

private void OnEnable()
{
    onEnemyStateChanged.OnEventRaised += ProcessEnemies;
}

private void ProcessEnemies()
{
    // Only called when necessary
    foreach (var enemy in activeEnemies.Items)
    {
        ProcessEnemy(enemy);
    }
}
```

### Use event-driven architecture

```csharp
// Bad: Polling in Update
private void Update()
{
    if (interactKeyPressed)  // Checking flag every frame
    {
        Interact();
    }

    UpdateHealthUI();  // Every frame
    UpdateScoreUI();   // Every frame
}

// Good: Event-Driven (InputReader + EventChannel)
[SerializeField] private InputReaderSO inputReader;
[SerializeField] private FloatEventChannelSO onHealthChanged;
[SerializeField] private IntEventChannelSO onScoreChanged;

private void OnEnable()
{
    inputReader.OnInteractEvent += HandleInteract;
    onHealthChanged.OnEventRaised += UpdateHealthUI;
    onScoreChanged.OnEventRaised += UpdateScoreUI;
}

private void OnDisable()
{
    inputReader.OnInteractEvent -= HandleInteract;
    onHealthChanged.OnEventRaised -= UpdateHealthUI;
    onScoreChanged.OnEventRaised -= UpdateScoreUI;
}

// Input handled via event callback - no polling
private void HandleInteract()
{
    Interact();
}

// No Update() needed - fully event-driven
```

---

## Anti-pattern: Heavy Update loop

```csharp
/// <summary>
/// ANTI-PATTERN: Heavy Update loop
///
/// Problems:
/// - UI updated every frame (expensive Canvas rebuild)
/// - FindObjectsOfType called every frame (very expensive)
/// - GetComponent called every frame (expensive)
/// </summary>
public class HeavyUpdateBad : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private TMPro.TextMeshProUGUI enemyCountText;

    // BAD: Update every frame
    private void Update()
    {
        // BAD: FindObjectOfType every frame (very expensive)
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            healthText.text = $"HP: {playerHealth.currentHealth}";
        }

        // BAD: FindObjectsOfType every frame (extremely expensive)
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        enemyCountText.text = $"Enemies: {enemies.Length}";

        // BAD: GetComponent every frame (expensive)
        Rigidbody rb = GetComponent<Rigidbody>();
    }
}

/// <summary>
/// GOOD: Event-driven UI updates
/// </summary>
public class EventDrivenUIGood : MonoBehaviour
{
    [Header("Event Channels")]
    [SerializeField] private IntEventChannelSO onHealthChanged;
    [SerializeField] private IntEventChannelSO onEnemyCountChanged;

    [Header("UI References")]
    [SerializeField] private TMPro.TextMeshProUGUI healthText;
    [SerializeField] private TMPro.TextMeshProUGUI enemyCountText;

    private void OnEnable()
    {
        // GOOD: Subscribe to EventChannels
        onHealthChanged?.OnEventRaised += UpdateHealth;
        onEnemyCountChanged?.OnEventRaised += UpdateEnemyCount;
    }

    private void OnDisable()
    {
        onHealthChanged?.OnEventRaised -= UpdateHealth;
        onEnemyCountChanged?.OnEventRaised -= UpdateEnemyCount;
    }

    // GOOD: Update only when EventChannel fires
    private void UpdateHealth(int health)
    {
        healthText.text = $"HP: {health}";
    }

    private void UpdateEnemyCount(int count)
    {
        enemyCountText.text = $"Enemies: {count}";
    }
}
```

---

## String operations - P1

### Avoid string concatenation in Update

```csharp
// Bad: String concatenation every frame
private void Update()
{
    scoreText.text = "Score: " + currentScore;  // Creates garbage
}

// Good: Only when score changes
[SerializeField] private IntEventChannelSO onScoreChanged;

private void OnEnable()
{
    onScoreChanged.OnEventRaised += UpdateScoreUI;
}

private void UpdateScoreUI(int newScore)
{
    scoreText.text = $"Score: {newScore}";
}
```

### String Builder for complex operations

```csharp
// Bad: Multiple concatenations
string BuildReport()
{
    string report = "Player: " + playerName + "\n";
    report += "Health: " + health + "\n";
    report += "Score: " + score + "\n";
    return report;  // Creates multiple garbage objects
}

// Good: StringBuilder
string BuildReport()
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Player: {playerName}");
    sb.AppendLine($"Health: {health}");
    sb.AppendLine($"Score: {score}");
    return sb.ToString();
}
```

---

## Collection management - P1

### Pre-allocate collections

```csharp
// Bad: Reallocate every time
public List<Enemy> GetNearbyEnemies()
{
    List<Enemy> result = new List<Enemy>();  // Creates garbage
    // Fill list
    return result;
}

// Good: Reuse collection
private List<Enemy> nearbyEnemiesCache = new List<Enemy>(100);

public List<Enemy> GetNearbyEnemies()
{
    nearbyEnemiesCache.Clear();
    // Fill list
    return nearbyEnemiesCache;
}
```

### Use capacity for known sizes

```csharp
// Bad: Default capacity (causes resizing)
var enemies = new List<Enemy>();

// Good: Pre-allocate known capacity
var enemies = new List<Enemy>(50);
```

---

## Physics optimization - P1

### Use layers for raycasts

```csharp
// Bad: Raycast hits everything
RaycastHit hit;
if (Physics.Raycast(transform.position, transform.forward, out hit))
{
    // Process hit
}

// Good: Use layerMask
[SerializeField] private LayerMask enemyLayer;

private void CheckRaycast()
{
    RaycastHit hit;
    if (Physics.Raycast(transform.position, transform.forward, out hit, 100f, enemyLayer))
    {
        // Only hits enemies
    }
}
```

---

## Coroutine optimization - P1

### Cache WaitForSeconds

```csharp
// Bad: Create new every time
IEnumerator BadCoroutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1f);  // Creates garbage
    }
}

// Good: Cache WaitForSeconds
private WaitForSeconds oneSecondWait;

private void Awake()
{
    oneSecondWait = new WaitForSeconds(1f);
}

IEnumerator GoodCoroutine()
{
    while (true)
    {
        yield return oneSecondWait;
    }
}
```

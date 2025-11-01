# Performance Optimization

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

## Object Pooling

### Basic Implementation

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

### Usage Example

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

## Caching Strategy

### Component Caching

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

### GameObject Caching

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

## Update Loop Optimization

### Avoid Heavy Operations in Update

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

### Use Event-Driven Architecture

```csharp
// Bad: Polling in Update
private void Update()
{
    if (Input.GetKeyDown(KeyCode.E))
    {
        Interact();
    }

    UpdateHealthUI();  // Every frame
    UpdateScoreUI();   // Every frame
}

// Good: Event-Driven
[SerializeField] private FloatEventChannelSO onHealthChanged;
[SerializeField] private IntEventChannelSO onScoreChanged;

private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealthUI;
    onScoreChanged.OnEventRaised += UpdateScoreUI;
}

private void Update()
{
    // Only input handling
    if (Input.GetKeyDown(KeyCode.E))
    {
        Interact();
    }
}
```

---

## String Operations

### Avoid String Concatenation in Update

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

### String Builder for Complex Operations

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

## Collection Management

### Pre-Allocate Collections

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

### Use Capacity for Known Sizes

```csharp
// Bad: Default capacity (causes resizing)
var enemies = new List<Enemy>();

// Good: Pre-allocate known capacity
var enemies = new List<Enemy>(50);
```

---

## Physics Optimization

### Use Layers for Raycasts

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

### Cache Physics Materials

```csharp
public class CollisionManager : MonoBehaviour
{
    // Cache physics materials
    private PhysicMaterial bouncyMaterial;
    private PhysicMaterial frictionlessMaterial;

    private void Awake()
    {
        bouncyMaterial = Resources.Load<PhysicMaterial>("Physics/Bouncy");
        frictionlessMaterial = Resources.Load<PhysicMaterial>("Physics/Frictionless");
    }
}
```

---

## Coroutine Optimization

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

---

## References

- [Unity Specifics](unity-specifics.md)
- [RuntimeSet Pattern](../architecture/runtime-sets.md)
- [Event Channels](../architecture/event-channels.md)

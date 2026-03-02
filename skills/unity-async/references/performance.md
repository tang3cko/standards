# Performance Optimization

Performance optimization including caching, pooling, event-driven updates, and GC avoidance.

---

## Component Caching

```csharp
// Bad: GetComponent every frame
private void Update()
{
    GetComponent<Rigidbody>().AddForce(Vector3.forward);
}

// Good: Cache in Awake
private Rigidbody cachedRigidbody;

private void Awake()
{
    cachedRigidbody = GetComponent<Rigidbody>();
}

private void Update()
{
    cachedRigidbody.AddForce(Vector3.forward);
}
```

---

## Event-Driven Updates

```csharp
// Bad: Update every frame
private void Update()
{
    healthText.text = $"HP: {playerHealth.currentHealth}";
}

// Good: Event-driven
[SerializeField] private IntEventChannelSO onHealthChanged;

private void OnEnable()
{
    onHealthChanged.OnEventRaised += UpdateHealthUI;
}

private void OnDisable()
{
    onHealthChanged.OnEventRaised -= UpdateHealthUI;
}

private void UpdateHealthUI(int health)
{
    healthText.text = $"HP: {health}";
}
```

---

## Garbage Collection Avoidance

### Avoid boxing in hot paths

```csharp
// Bad: Boxing int to object
void Update()
{
    object score = currentScore; // Boxing allocation
    Debug.Log(string.Format("Score: {0}", currentScore)); // Boxing
}

// Good: Use generic overloads or string interpolation outside Update
private void OnScoreChanged(int score)
{
    scoreText.text = $"Score: {score}"; // Event-driven, not per-frame
}
```

### Avoid closures in hot paths

```csharp
// Bad: Lambda captures 'this', creating closure allocation each frame
void Update()
{
    enemies.Sort((a, b) => Vector3.Distance(a.position, transform.position)
        .CompareTo(Vector3.Distance(b.position, transform.position)));
}

// Good: Use IComparer or cache the delegate
private int CompareByDistance(Enemy a, Enemy b)
{
    float distA = (a.position - cachedPosition).sqrMagnitude;
    float distB = (b.position - cachedPosition).sqrMagnitude;
    return distA.CompareTo(distB);
}
```

### Avoid LINQ in Update

```csharp
// Bad: LINQ allocates iterators and delegates every frame
void Update()
{
    var alive = enemies.Where(e => e.IsAlive).ToList();
    var closest = enemies.OrderBy(e => e.Distance).FirstOrDefault();
}

// Good: Use manual loops
void ProcessEnemies()
{
    for (int i = 0; i < enemies.Count; i++)
    {
        if (!enemies[i].IsAlive) continue;
        ProcessEnemy(enemies[i]);
    }
}
```

---

## Avoid Find in Update

```csharp
// Bad: Find every frame
void Update()
{
    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
}

// Good: Use RuntimeSet
[SerializeField] private EnemyRuntimeSetSO activeEnemies;

private void ProcessEnemies()
{
    foreach (var enemy in activeEnemies.Items)
    {
        ProcessEnemy(enemy);
    }
}
```

---

## Object Pooling

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
        return Object.Instantiate(prefab, parent);
    }

    public void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        pool.Enqueue(obj);
    }
}
```

---

## String Operations

```csharp
// Bad: Concatenation every frame
private void Update()
{
    scoreText.text = "Score: " + currentScore;
}

// Good: Event-driven
private void UpdateScoreUI(int newScore)
{
    scoreText.text = $"Score: {newScore}";
}

// Good: StringBuilder for complex strings
string BuildReport()
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Player: {playerName}");
    sb.AppendLine($"Health: {health}");
    return sb.ToString();
}
```

---

## Collection Pre-Allocation

```csharp
// Bad: Default capacity
var enemies = new List<Enemy>();

// Good: Pre-allocate
var enemies = new List<Enemy>(50);

// Good: Reuse collection
private List<Enemy> nearbyEnemiesCache = new List<Enemy>(100);

public List<Enemy> GetNearbyEnemies()
{
    nearbyEnemiesCache.Clear();
    // Fill list
    return nearbyEnemiesCache;
}
```

---

## Physics Optimization

```csharp
// Bad: Raycast hits everything
Physics.Raycast(transform.position, transform.forward, out hit);

// Good: Use LayerMask
[SerializeField] private LayerMask enemyLayer;

Physics.Raycast(transform.position, transform.forward, out hit, 100f, enemyLayer);
```

---

## Coroutine Caching

```csharp
// Bad: Create new every time
IEnumerator BadCoroutine()
{
    while (true)
    {
        yield return new WaitForSeconds(1f);
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

- [async.md](async.md) - Coroutine and async patterns
- See `unity-core` skill for core anti-patterns and error handling

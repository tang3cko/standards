# Unity-Specific Best Practices

## Purpose

Follow Unity-specific conventions and patterns to ensure framerate-independent behavior, proper component dependencies, and efficient lifecycle management.

## Checklist

- [ ] Use `Time.deltaTime` for framerate-independent movement
- [ ] Apply `[RequireComponent]` for hard dependencies
- [ ] Use `FixedUpdate` for physics operations
- [ ] Use `Update` for input and camera updates
- [ ] Prefer `OnTriggerEnter` for detection zones
- [ ] Use `OnCollisionEnter` for physical impacts
- [ ] Cache `GetComponent` calls in `Awake`
- [ ] Subscribe to events in `OnEnable`, unsubscribe in `OnDisable`
- [ ] Cache `WaitForSeconds` in coroutines
- [ ] Use `OnValidate` for Inspector validation

---

## Time.deltaTime Usage

### Framerate-Independent Movement

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

### Countdown Timers

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

### When NOT to Use Time.deltaTime

```csharp
// Bad: Don't use Time.deltaTime for instant actions
if (Input.GetKeyDown(KeyCode.Space))
{
    jumpForce += 10f * Time.deltaTime;  // Wrong! This is instant action
}

// Good: Instant actions don't need deltaTime
if (Input.GetKeyDown(KeyCode.Space))
{
    ApplyJumpForce(10f);
}
```

---

## RequireComponent Attribute

### Basic Usage

```csharp
// Ensures Rigidbody is attached when adding this component
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        // Safe to call GetComponent - RequireComponent guarantees it exists
        rb = GetComponent<Rigidbody>();
    }
}
```

### Multiple Component Requirements

```csharp
// Require multiple components
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class PhysicsObject : MonoBehaviour
{
    private Collider col;
    private Rigidbody rb;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
    }
}
```

### When to Use RequireComponent

```csharp
// Good: Component has hard dependency
[RequireComponent(typeof(AudioSource))]
public class SoundEmitter : MonoBehaviour
{
    // This component cannot function without AudioSource
}

// Good: Networking requires NetworkIdentity
[RequireComponent(typeof(NetworkIdentity))]
public class NetworkedObject : NetworkBehaviour
{
    // Mirror Networking requires this
}

// Bad: Don't use for optional dependencies
[RequireComponent(typeof(ParticleSystem))]  // Wrong if particles are optional
public class VisualEffect : MonoBehaviour
{
    // Use SerializeField instead if optional
    [SerializeField] private ParticleSystem optionalParticles;
}
```

---

## Update vs FixedUpdate

### Update: Framerate-Dependent

```csharp
public class InputHandler : MonoBehaviour
{
    // Good: Input handling in Update
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Good: Camera movement in Update
        UpdateCameraPosition();
    }
}
```

### FixedUpdate: Physics Operations

```csharp
public class PlayerPhysics : MonoBehaviour
{
    [SerializeField] private float moveForce = 10f;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Good: Physics in FixedUpdate
    private void FixedUpdate()
    {
        Vector3 movement = GetMovementInput();
        rb.AddForce(movement * moveForce);
    }

    private Vector3 GetMovementInput()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        return new Vector3(horizontal, 0, vertical);
    }
}
```

### When to Use Each

| Method | Use Cases | Fixed Timestep |
|--------|-----------|----------------|
| **Update** | Input handling, Camera movement, UI updates, Timers | No (varies with FPS) |
| **FixedUpdate** | Rigidbody movement, AddForce, Physics calculations | Yes (0.02s default) |

---

## Collider Event Detection

### OnTriggerEnter vs OnCollisionEnter

```csharp
public class CollisionHandler : MonoBehaviour
{
    // OnTriggerEnter: Detects overlap (Collider must be Trigger)
    private void OnTriggerEnter(Collider other)
    {
        // Use for: Pickups, Trigger zones, Detection areas
        if (other.CompareTag("Coin"))
        {
            CollectCoin(other.gameObject);
        }
    }

    // OnCollisionEnter: Detects physical collision (Collider NOT Trigger)
    private void OnCollisionEnter(Collision collision)
    {
        // Use for: Damage on impact, Bounce effects, Physics reactions
        if (collision.gameObject.CompareTag("Enemy"))
        {
            TakeDamage(10);
        }
    }
}
```

### Tag vs Layer Comparison

```csharp
public class DetectionZone : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;

    private void OnTriggerEnter(Collider other)
    {
        // Bad: String comparison every frame (slow)
        if (other.tag == "Enemy")  // Avoid string comparison
        {
            ProcessEnemy(other.gameObject);
        }

        // Good: CompareTag (faster)
        if (other.CompareTag("Enemy"))
        {
            ProcessEnemy(other.gameObject);
        }

        // Best: Layer-based detection (fastest)
        if (((1 << other.gameObject.layer) & enemyLayer) != 0)
        {
            ProcessEnemy(other.gameObject);
        }
    }
}
```

### 2D Collider Events

```csharp
public class Collider2DHandler : MonoBehaviour
{
    // 2D Trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Collectible"))
        {
            Collect(other.gameObject);
        }
    }

    // 2D Collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            OnLanded();
        }
    }
}
```

---

## Component Lifecycle Methods

### Execution Order

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
            Debug.Log("1. Awake");
        }

        // 2. OnEnable: Called when object becomes enabled
        private void OnEnable()
        {
            // Subscribe to events
            Debug.Log("2. OnEnable");
        }

        // 3. Start: Called before first frame update (after all Awake)
        private void Start()
        {
            // Initialize with other objects' references
            Debug.Log("3. Start");
        }

        // 4. Update: Called every frame
        private void Update()
        {
            // Per-frame logic
        }

        // 5. LateUpdate: Called after all Updates
        private void LateUpdate()
        {
            // Camera following (after character moved in Update)
        }

        // 6. OnDisable: Called when object becomes disabled
        private void OnDisable()
        {
            // Unsubscribe from events
            Debug.Log("6. OnDisable");
        }

        // 7. OnDestroy: Called when object is destroyed
        private void OnDestroy()
        {
            // Cleanup resources
            Debug.Log("7. OnDestroy");
        }
    }
}
```

### Awake vs Start

```csharp
public class InitializationExample : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;

    private Transform cachedTransform;
    private Rigidbody cachedRigidbody;

    // Awake: Initialize own components
    private void Awake()
    {
        // Good: Cache own components
        cachedTransform = transform;
        cachedRigidbody = GetComponent<Rigidbody>();

        // Good: Initialize internal state
        ResetState();
    }

    // Start: Initialize with external references
    private void Start()
    {
        // Good: Access other objects (their Awake already called)
        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        // Good: Register with managers
        GameManager.Instance?.RegisterPlayer(this);
    }
}
```

### OnEnable vs OnDisable for Events

```csharp
using Tang3cko.EventChannels;

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

        private void HandleGameStart()
        {
            Debug.Log("Game started");
        }

        private void HandleGameEnd()
        {
            Debug.Log("Game ended");
        }
    }
}
```

---

## GameObject and Component Access

### Cache GetComponent Calls

```csharp
public class ComponentCaching : MonoBehaviour
{
    // Bad: GetComponent every frame (expensive)
    private void Update()
    {
        GetComponent<Rigidbody>().AddForce(Vector3.forward);  // Heavy!
    }

    // Good: Cache in Awake
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        rb.AddForce(Vector3.forward);
    }
}
```

### Find Methods Performance

```csharp
public class FindMethods : MonoBehaviour
{
    // Bad: Find in Update (very expensive)
    private void Update()
    {
        GameObject player = GameObject.Find("Player");  // Very slow!
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy");  // Slow!
    }

    // Good: Find once and cache
    private GameObject player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    // Best: Use EventChannels + RuntimeSet (no Find needed)
    [SerializeField] private PlayerRuntimeSetSO activePlayers;

    private void ProcessPlayers()
    {
        foreach (var player in activePlayers.Items)
        {
            // Process each player
        }
    }
}
```

---

## Coroutines

### Basic Pattern

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

### Stopping Coroutines Safely

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

## Unity Editor Attributes

### Common Attributes

```csharp
public class AttributeExamples : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second")]
    [Range(1f, 10f)]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat Settings")]
    [Tooltip("Maximum health points")]
    [Min(1)]
    [SerializeField] private int maxHealth = 100;

    [Space(10)]
    [SerializeField] private bool enableDebug;

    [HideInInspector]
    public int hiddenValue;  // Not shown in Inspector

    [TextArea(3, 5)]
    [SerializeField] private string description;  // Multi-line text
}
```

### OnValidate for Inspector Validation

```csharp
public class ValidatedComponent : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private int maxEnemies = 10;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Clamp values when changed in Inspector
        speed = Mathf.Max(0, speed);
        maxEnemies = Mathf.Clamp(maxEnemies, 1, 100);

        // Warn about missing references
        if (GetComponent<Rigidbody>() == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Rigidbody component missing on {gameObject.name}", this);
        }
    }
#endif
}
```

---

## References

- [Performance Optimization](performance.md)
- [Naming Conventions](naming-conventions.md)
- [Error Handling](error-handling.md)

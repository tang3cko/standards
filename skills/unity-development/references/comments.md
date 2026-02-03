# Comments and documentation

## XML documentation comments

### Class documentation

```csharp
/// <summary>
/// Manages experience orbs and spawns them when enemies die
/// </summary>
public class ExperienceOrbManager : MonoBehaviour
{
}
```

### Method documentation

```csharp
/// <summary>
/// Spawns experience orbs at the specified position
/// </summary>
/// <param name="position">Spawn position</param>
/// <param name="experienceValue">Experience amount</param>
/// <returns>List of spawned orbs</returns>
public List<GameObject> SpawnOrbs(Vector3 position, int experienceValue)
{
}
```

### Property documentation

```csharp
/// <summary>
/// Player's current health
/// </summary>
public int CurrentHealth { get; private set; }

/// <summary>
/// Whether the player is alive
/// </summary>
public bool IsAlive => CurrentHealth > 0;
```

## Header attribute

```csharp
public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Combat Stats")]
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float defense = 5f;

    [Header("Event Channels")]
    [SerializeField] private VoidEventChannelSO onDeath;
    [SerializeField] private FloatEventChannelSO onHealthChanged;
}
```

## Tooltip attribute

```csharp
[Header("Base Stats")]

[Tooltip("Player's maximum health")]
[SerializeField] private float maxHealth = 100f;

[Tooltip("Movement speed per second (meters)")]
[Range(1f, 10f)]
[SerializeField] private float moveSpeed = 5f;
```

## Inline comments

**Good: Explaining complex logic**

```csharp
public int GetCardStrength()
{
    // Card game rule: 2 is strongest, Ace is second strongest
    if (rank == 2) return 15;
    if (rank == 1) return 14;
    return rank;
}
```

**Good: Explaining intent**

```csharp
// Server-only state updates for network synchronization
if (!NetworkServer.active) return;

// UI update deferred to next frame (avoid layout recalculation)
StartCoroutine(UpdateUINextFrame());
```

**Bad: Obvious code**

```csharp
// Bad: Unnecessary comment
// Increase player health
health += 10;

// Good: No comment needed
playerHealth.Heal(10);
```

## TODO comments

```csharp
// TODO: Add DOTween animation in Phase 2
public void PlayCard(CardSO card) { }

// FIXME: Incomplete cleanup handling on host disconnect
public override void OnStopServer() { }

// NOTE: This process only runs on server side
[ServerCallback]
private void Update() { }
```

## When NOT to document

**Self-explanatory code:**

```csharp
// Bad: Unnecessary
public Vector3 GetPosition() => transform.position;

// Good: Method name is descriptive
public Vector3 GetPlayerPosition() => transform.position;
```

**Unity standard methods:**

```csharp
// Bad: Unnecessary
/// <summary>
/// Awake method
/// </summary>
private void Awake() { }

// Good: No comment needed
private void Awake()
{
    // Only comment initialization logic if needed
}
```

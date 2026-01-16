---
name: unity-design-principles
description: Tang3cko.ReactiveSO design philosophy. Observability, Asset-based DI, Data-Oriented Design. Use when designing Unity architecture.
---

# Design Principles

## Purpose

Define the foundational design philosophy behind Tang3cko.ReactiveSO architecture.

---

## Core principles - P1

### The three pillars

| Principle | Goal | Enables |
|-----------|------|---------|
| **Observability** | Capture and restore complete state | Time Travel, Flight Recorder, Golden Snapshots |
| **Asset-based DI** | Decouple systems via Inspector | Flexible testing without DI containers |
| **Data-Oriented Design** | Separate data from logic | Contiguous memory, deterministic behavior |

---

## True Observability - P1

### The model

```
S_n(T_i) ⊆ T

T   = Total Set (all data in the system)
S_n = Observer (a filter or view)
i   = Time index
```

> **True Observability** means the ability to capture T_i (total state at moment i) deterministically.

### The RES solution: Data as "Truth"

```
Data (T)
├── Contiguous memory layout (unmanaged struct)
├── Instant snapshots (MemCpy/Blit, not serialization)
└── Deterministic (pure functions produce same results)

Behavior (S_n)
├── Pure functions (no side effects)
├── Applied to data externally
└── Can be tested independently
```

### Practical applications

| Pattern | Description |
|---------|-------------|
| **Time Travel Debugging** | Store history of T_i, seek to any frame |
| **Flight Recorder** | Ring buffer of last 60s snapshots |
| **Hot Swapping** | Save T_now to binary, edit externally, inject back |
| **Golden Snapshots** | Save complex states as test data |

---

## Asset-based Dependency Injection - P1

### Pure DI via Inspector

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private IntVariableSO health;  // Dependency
}
```

Dragging an asset into the Inspector field is **Pure DI via Field Injection**. The Unity Inspector acts as the "assembler."

### Traditional vs Asset-based

**Traditional (Tight Coupling):**

```csharp
void Spawn()
{
    GameManager.Instance.RegisterEnemy(newEnemy);
}
```

**Asset-based DI (Loose Coupling):**

```csharp
[SerializeField] private GameObjectRuntimeSetSO enemySet;

void Spawn()
{
    enemySet.Add(newEnemy);
}
```

### ScriptableObjects as stable anchors

Scenes load and unload. MonoBehaviours are created and destroyed. But **ScriptableObject assets remain constant** as stable connection points.

---

## Data-Oriented Design - P1

### The pattern

```
State (struct)        : Data only, no methods
Calculation logic     : Pure functions in separate classes
RES                   : Storage and notification
GameObject            : Visualization and reaction
```

### State structs: Fields only

```csharp
[Serializable]
public struct EnemyState
{
    public int health;
    public int maxHealth;
    public bool isStunned;
    public float stunEndTime;

    // NO methods that modify state
}
```

### Calculation logic: Pure functions

```csharp
public static class EnemyStateCalculator
{
    public static EnemyState ApplyDamage(EnemyState state, int damage)
    {
        state.health = Mathf.Max(0, state.health - damage);
        return state;
    }

    public static bool IsDead(EnemyState state)
    {
        return state.health <= 0;
    }
}
```

### Usage ties them together

```csharp
entitySet.UpdateData(enemyId, state =>
    EnemyStateCalculator.ApplyDamage(state, damage));
```

---

## Entity vs Object vs View - P1

| Concept | Description | Lifecycle |
|---------|-------------|-----------|
| **Entity** | Logical unit with ID and state | Defined by RES registration |
| **Object** | Runtime representation (GameObject) | Defined by Unity instantiation |
| **View** | GameObject that displays entity data | Doesn't own the data |

### The key insight

Entity existence is determined by presence in ReactiveEntitySet, **not** by existence of Unity object:

```
Entity exists in RES         -> Entity is "alive"
GameObject exists in scene   -> Object is "visible"
```

---

## Design checklist - P1

### 1. Observability

- Can the complete state T_i be captured at any moment?
- Can state be restored deterministically?
- Can any Observer S_n be applied to captured state?

### 2. Dependency

- Is the dependency provided externally (DI)?
- Is ScriptableObject used as stable anchor?
- Can dependencies be swapped for testing?

### 3. Data/Logic separation

- Is state stored in struct (no methods)?
- Is logic in pure functions (no side effects)?
- Can logic be tested without Unity?

### 4. Set membership

- Does entity exist in ReactiveEntitySet?
- Is state managed by RES, not GameObject?
- Can entity exist without visual representation?

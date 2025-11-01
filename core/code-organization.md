# Code Organization & File Structure

## Purpose

Consistent file structure and namespace design improve project maintainability and scalability.

## Checklist

- [ ] Namespace follows `ProjectName.Category` pattern
- [ ] Scripts/ organized by domain
- [ ] ScriptableObjects/ organized into Data/Events/RuntimeSets
- [ ] One public class per file
- [ ] Filename matches class name
- [ ] Follow file structure order

---

## Namespace Conventions

### Project Structure-Based Namespace

```csharp
namespace ProjectName.Category

// Examples
namespace ProjectName.Player      // Player-related
namespace ProjectName.Quest       // Quest system
namespace ProjectName.Network     // Network-related
namespace ProjectName.UI          // UI-related
namespace ProjectName.Interaction // Interaction system
namespace ProjectName.Core        // Core systems (FPS Counter, etc.)
namespace ProjectName.Data        // ScriptableObject definitions
```

### Implementation Example

```csharp
using UnityEngine;
using Mirror;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    public class QuestManager : NetworkBehaviour
    {
        // ...
    }
}

namespace ProjectName.Player
{
    public class PlayerInteract : MonoBehaviour
    {
        // ...
    }
}
```

---

## Directory Structure

### Scripts Directory

```
Assets/_Project/Scripts/
├── Audio/                 # AudioManager, BGMManager
├── Camera/                # Camera control
├── Combat/                # IDamageable, etc.
├── Core/                  # GameManager, SaveSystem, GameState, etc.
├── Data/                  # DataSO definition scripts
├── Enemy/                 # Enemy-related
├── Events/                # EventChannelSO definition scripts
├── Player/                # Player-related
├── Shop/                  # Shop-related
├── UI/                    # UI-related
└── Weapon/                # Weapon-related
```

### Scripts/ Classification Criteria

**Feature Domain-Based**

- `Player/` - Directly player-related (PlayerHealth, PlayerWallet, PlayerBuffManager)
- `Enemy/` - Directly enemy-related (EnemyHealth, EnemyController, EnemyPoolManager)
- `Weapon/` - Weapon systems (FloatingWeaponManager, ProjectileController)
- `UI/` - UI display control (PauseMenuController, UIManager, AudioSettingsManager)
- `Shop/` - Shop functionality (ShopManager, ShopItemSO)
- `Audio/` - Audio systems (AudioManager, BGMManager)

**Cross-Cutting Concerns**

- `Core/` - Game-wide (GameManager, GameState, SaveSystem, GameSettingsSO, GameStatsManager)
- `Events/` - EventChannelSO definitions
- `Data/` - DataSO definitions (base classes for various ScriptableObjects)
- `Combat/` - Combat commons (IDamageable, IPoolable)

---

## ScriptableObject Placement

### ScriptableObjects Directory

```
Assets/_Project/ScriptableObjects/
├── Data/                  # Data definition SOs
│   ├── Audio/            # AudioCategorySO, etc.
│   ├── Enemies/          # EnemyDataSO
│   ├── Pool/             # EnemyPoolConfigSO
│   ├── Projectiles/      # ProjectileDataSO
│   ├── ShopItems/        # ShopItemSO
│   ├── Wave/             # WaveConfigSO
│   └── Weapons/          # WeaponDataSO
│
├── Events/                # EventChannelSO
│   ├── Audio/            # Volume events
│   ├── Buff/             # Buff events
│   ├── Coin/             # Coin events
│   ├── Enemy/            # Enemy events
│   ├── Player/           # Player events
│   ├── Settings/         # Settings events
│   └── Weapon/           # Weapon events
│
├── RuntimeSets/           # RuntimeSetSO
│
└── Settings/              # GameSettingsSO, etc.
```

### ScriptableObjects/ Classification Criteria

- `Data/` - Static game data (enemy stats, weapon performance, audio categories)
- `Events/` - Communication EventChannels (subdirectories by feature)
- `RuntimeSets/` - Runtime dynamic collections (all enemies list, etc.)
- `Settings/` - Game settings (GameSettingsSO)

---

## Prefabs Placement

```
Assets/_Project/Prefabs/
├── Enemies/               # Enemy prefabs
├── Environment/           # Environment objects
├── Projectiles/           # Projectile prefabs
├── UI/                    # UI prefabs
└── Weapons/               # Weapon prefabs
```

---

## One File One Class Principle

### Basic Rule

- One public class per file
- Filename must match class name
- Internal classes (private/internal) are exceptions

### Implementation Example

```
// Good
PlayerController.cs
  └─ public class PlayerController { }

EnemyHealth.cs
  └─ public class EnemyHealth { }

// Bad
GameLogic.cs
  ├─ public class GameManager { }
  ├─ public class TurnManager { }     // Should be separate file
  └─ public class ScoreManager { }    // Should be separate file
```

### Exception: Closely Related Classes

```csharp
// QuestSO.cs
public class QuestSO : ScriptableObject
{
    // Main class
}

// Internal class (allowed)
[System.Serializable]
public class ObjectiveData
{
    public ObjectiveType type;
    public int requiredCount;
}

public enum QuestType
{
    Gather,
    Deliver
}
```

---

## File Structure Order

### Recommended Structure

```csharp
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.EventChannels;

namespace ProjectName.Category
{
    /// <summary>
    /// Class description
    /// </summary>
    public class ExampleClass : MonoBehaviour
    {
        // 1. Fields
        [Header("Dependencies")]
        [SerializeField] private OtherComponent dependency;

        [Header("Settings")]
        [SerializeField] private float speed = 5f;

        private Transform cachedTransform;
        private bool isInitialized;

        // 2. Properties
        public bool IsInitialized => isInitialized;
        public float CurrentSpeed { get; private set; }

        // 3. Unity Lifecycle Methods (recommended order)
        private void Awake() { }
        private void OnEnable() { }
        private void Start() { }
        private void Update() { }
        private void FixedUpdate() { }
        private void LateUpdate() { }
        private void OnDisable() { }
        private void OnDestroy() { }

        // 4. Public Methods
        public void Initialize() { }
        public void DoSomething() { }

        // 5. Private Methods
        private void UpdatePosition() { }
        private void HandleEvent() { }

        // 6. Event Handlers
        private void OnTriggerEnter(Collider other) { }
        private void OnCollisionEnter(Collision collision) { }

        // 7. Editor Only
#if UNITY_EDITOR
        private void OnValidate() { }
        private void OnDrawGizmos() { }
#endif
    }
}
```

---

## References

- [Naming Conventions](naming-conventions.md)
- [ScriptableObject Patterns](../architecture/scriptableobject.md)
- [EventChannel Patterns](../architecture/event-channels.md)

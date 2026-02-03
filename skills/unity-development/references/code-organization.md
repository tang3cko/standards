# Code organization

## Directory structure

### Scripts directory

```
Assets/_Project/Scripts/
├── Audio/                 # AudioManager, BGMManager
├── Camera/                # Camera control
├── Combat/                # IDamageable, etc.
├── Core/                  # GameManager, SaveSystem, GameState
├── Data/                  # DataSO definition scripts
├── Enemy/                 # Enemy-related
├── Events/                # EventChannelSO definition scripts
├── Player/                # Player-related
├── Shop/                  # Shop-related
├── UI/                    # UI-related
└── Weapon/                # Weapon-related
```

### ScriptableObjects directory

```
Assets/_Project/ScriptableObjects/
├── Data/                  # Data definition SOs
│   ├── Audio/
│   ├── Enemies/
│   ├── Weapons/
│   └── ...
├── Events/                # EventChannelSO instances
│   ├── Player/
│   ├── Enemy/
│   └── ...
├── RuntimeSets/           # RuntimeSetSO instances
└── Settings/              # GameSettingsSO, etc.
```

### Prefabs directory

```
Assets/_Project/Prefabs/
├── Enemies/
├── Environment/
├── Projectiles/
├── UI/
└── Weapons/
```

## Classification criteria

**Feature Domain-Based:**
- `Player/` - PlayerHealth, PlayerWallet, PlayerController
- `Enemy/` - EnemyHealth, EnemyController
- `Weapon/` - WeaponManager, ProjectileController
- `UI/` - UIManager, MenuController

**Cross-Cutting Concerns:**
- `Core/` - GameManager, GameState, SaveSystem
- `Events/` - EventChannelSO definitions
- `Data/` - DataSO definitions
- `Combat/` - IDamageable, IPoolable

## One file one class

- One public class per file
- Filename must match class name

```
// Good
PlayerController.cs
  └─ public class PlayerController { }

// Bad
GameLogic.cs
  ├─ public class GameManager { }
  ├─ public class TurnManager { }     // Should be separate
  └─ public class ScoreManager { }    // Should be separate
```

**Exception:** Closely related types in same file

```csharp
// QuestSO.cs - Main class with related types
public class QuestSO : ScriptableObject { }

[System.Serializable]
public class ObjectiveData { }

public enum QuestType { Gather, Deliver }
```

## File structure order

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Category
{
    public class ExampleClass : MonoBehaviour
    {
        // 1. Fields
        [Header("Dependencies")]
        [SerializeField] private OtherComponent dependency;

        [Header("Settings")]
        [SerializeField] private float speed = 5f;

        private bool isInitialized;

        // 2. Properties
        public bool IsInitialized => isInitialized;

        // 3. Unity Lifecycle Methods
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

        // 5. Private Methods
        private void UpdatePosition() { }

        // 6. Event Handlers
        private void OnTriggerEnter(Collider other) { }

        // 7. Editor Only
#if UNITY_EDITOR
        private void OnValidate() { }
#endif
    }
}
```

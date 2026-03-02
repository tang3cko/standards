# Code Organization

Directory structure, namespace conventions, and file organization.

---

## Directory Structure

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

---

## Classification Criteria

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

---

## One File One Class

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

---

## File Structure Order

See [_core-rules.md](_core-rules.md) for the standard file structure order.

Additional guidance for larger classes:

- Group `[Header]` sections logically: Dependencies first, then Settings, then Event Channels
- Unity lifecycle methods should follow execution order: `Awake -> OnEnable -> Start -> Update -> FixedUpdate -> LateUpdate -> OnDisable -> OnDestroy`
- Separate event handlers (e.g., `OnTriggerEnter`) from private helper methods

---

## Namespace Conventions

```csharp
using UnityEngine;
using Tang3cko.ReactiveSO;

namespace ProjectName.Category
{
    // All code must be in a namespace
    // Match namespace to directory path
}
```

---

## References

- [_core-rules.md](_core-rules.md) - File structure order quick reference
- [naming.md](naming.md) - Naming conventions
- [comments.md](comments.md) - Documentation standards

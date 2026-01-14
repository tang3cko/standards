# Actions

## Purpose

Implement the Command pattern using ScriptableObjects for data-driven, reusable commands. Actions encapsulate executable behaviors as Inspector-configurable assets.

---

## Design philosophy - P1

### Command pattern in Unity

Actions separate "what to do" from "when to do it."

**Traditional Unity:**

```
Button.onClick → MonoBehaviour.DoSomething()
                      ↓
               Hardcoded logic
```

**Action pattern:**

```
Button.onClick → ActionSO.Execute()
                      ↓
               Inspector-configured behavior
                      ↓
               Reusable across systems
```

### Key insight

Actions are **data-driven commands**. The same action asset can be triggered from:
- UI buttons
- Quest completion
- Event tables
- Dialogue systems
- Any other caller

---

## When to use - P1

### Use Actions when

- Data-driven systems (quest rewards, event tables, dialogue responses)
- Configurable behaviors (spawn effects, play sounds, show UI)
- Reusable commands (triggered from multiple places)
- Designer-configurable logic (tweak behavior without code)

### Use EventChannels when

- Simple notifications (no configurable behavior)
- One-way communication
- No parameters or simple parameters

### Comparison

| Feature | Actions | EventChannels |
|---------|---------|---------------|
| Purpose | Execute command | Broadcast notification |
| Configuration | Per-asset | Per-asset |
| Reusability | High (same action, many callers) | High (same event, many subscribers) |
| Parameters | Configure in Inspector + optional runtime | Runtime only |
| Return value | None | None |

---

## Basic usage - P1

### Creating an Action class

```csharp
using Tang3cko.ReactiveSO;
using UnityEngine;

namespace ProjectName.Actions
{
    [CreateAssetMenu(
        fileName = "SpawnEffect",
        menuName = "ProjectName/Actions/Spawn Effect")]
    public class SpawnEffectAction : ActionSO
    {
        [Header("Settings")]
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float duration = 2f;

        public override void Execute(
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            var instance = Object.Instantiate(effectPrefab);
            instance.transform.position += offset;
            Object.Destroy(instance, duration);

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Spawned {effectPrefab.name}");
#endif
        }
    }
}
```

### Creating an Action asset

Right-click in Project window:

```
Create > ProjectName > Actions > Spawn Effect
```

### Executing an Action

```csharp
using Tang3cko.ReactiveSO;
using UnityEngine;

namespace ProjectName.Core
{
    public class RewardSystem : MonoBehaviour
    {
        [SerializeField] private ActionSO rewardAction;

        public void GiveReward()
        {
            // Always use null-conditional operator
            rewardAction?.Execute();
        }
    }
}
```

---

## Generic Actions - P1

Use `ActionSO<T>` when the action needs a parameter at execution time.

### Creating a generic Action

```csharp
using Tang3cko.ReactiveSO;
using UnityEngine;

namespace ProjectName.Actions
{
    [CreateAssetMenu(
        fileName = "DamageAction",
        menuName = "ProjectName/Actions/Damage")]
    public class DamageAction : ActionSO<int>
    {
        [Header("Settings")]
        [SerializeField] private GameObject damageVFX;
        [SerializeField] private AudioClip damageSound;

        public override void Execute(int damage,
            string callerMember = "",
            string callerFile = "",
            int callerLine = 0)
        {
            Debug.Log($"Dealing {damage} damage");
            // Apply damage logic

#if UNITY_EDITOR
            var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
            NotifyActionExecuted(callerInfo);
            LogAction($"Damage: {damage}");
#endif
        }
    }
}
```

### Executing with parameter

```csharp
[SerializeField] private ActionSO<int> damageAction;

public void Attack()
{
    int damage = CalculateDamage();
    damageAction?.Execute(damage);
}
```

---

## Debugging features - P1

### Monitor Window

Access via `Window → Reactive SO → Monitor` to track action execution in real-time.

Features:
- Real-time execution logging
- Caller information (file, method, line number)
- Execution count
- Filter by action name

### Inspector settings

| Setting | Description |
|---------|-------------|
| `showInMonitor` | Show in Monitor Window during Play Mode |
| `showInConsole` | Log executions to Console |
| `description` | User-defined description for documentation |

### CallerInfo tracking

Actions automatically track where they were called from:

```csharp
// CallerInfo contains:
// - MemberName: Name of calling method
// - FilePath: Full path to calling file
// - LineNumber: Line number of call

// Formatted output: "FileName.cs:MethodName:42"
```

### OnAnyActionExecuted event

Static event for global action monitoring (Editor only):

```csharp
#if UNITY_EDITOR
private void OnEnable()
{
    ActionSO.OnAnyActionExecuted += HandleAnyActionExecuted;
}

private void OnDisable()
{
    ActionSO.OnAnyActionExecuted -= HandleAnyActionExecuted;
}

private void HandleAnyActionExecuted(ActionSO action, CallerInfo callerInfo)
{
    Debug.Log($"Action executed: {action.name} from {callerInfo}");
}
#endif
```

---

## Common patterns - P2

### Sequence Action

Execute multiple actions in order:

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Sequence")]
public class SequenceAction : ActionSO
{
    [SerializeField] private ActionSO[] actions;

    public override void Execute(
        string callerMember = "",
        string callerFile = "",
        int callerLine = 0)
    {
        foreach (var action in actions)
        {
            action?.Execute();
        }

#if UNITY_EDITOR
        NotifyActionExecuted(new CallerInfo(callerMember, callerFile, callerLine));
#endif
    }
}
```

### Conditional Action

Execute based on condition:

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Conditional")]
public class ConditionalAction : ActionSO
{
    [SerializeField] private BoolVariableSO condition;
    [SerializeField] private ActionSO trueAction;
    [SerializeField] private ActionSO falseAction;

    public override void Execute(
        string callerMember = "",
        string callerFile = "",
        int callerLine = 0)
    {
        if (condition != null && condition.Value)
        {
            trueAction?.Execute();
        }
        else
        {
            falseAction?.Execute();
        }

#if UNITY_EDITOR
        NotifyActionExecuted(new CallerInfo(callerMember, callerFile, callerLine));
#endif
    }
}
```

### Random Action

Execute one random action from list:

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Random")]
public class RandomAction : ActionSO
{
    [SerializeField] private ActionSO[] actions;

    public override void Execute(
        string callerMember = "",
        string callerFile = "",
        int callerLine = 0)
    {
        if (actions.Length > 0)
        {
            int index = Random.Range(0, actions.Length);
            actions[index]?.Execute();
        }

#if UNITY_EDITOR
        NotifyActionExecuted(new CallerInfo(callerMember, callerFile, callerLine));
#endif
    }
}
```

---

## Use cases - P1

### Quest reward system

```csharp
// Action definition
[CreateAssetMenu(menuName = "ProjectName/Actions/Give Item")]
public class GiveItemAction : ActionSO
{
    [SerializeField] private ItemDataSO item;
    [SerializeField] private int quantity = 1;

    public override void Execute(
        string callerMember = "",
        string callerFile = "",
        int callerLine = 0)
    {
        InventoryManager.Instance.AddItem(item, quantity);

#if UNITY_EDITOR
        NotifyActionExecuted(new CallerInfo(callerMember, callerFile, callerLine));
#endif
    }
}

// Quest definition with configurable rewards
[CreateAssetMenu(menuName = "ProjectName/Data/Quest")]
public class QuestSO : ScriptableObject
{
    public string questName;
    public ActionSO[] rewardActions;

    public void Complete()
    {
        foreach (var action in rewardActions)
        {
            action?.Execute();
        }
    }
}
```

### Event table system

```csharp
[CreateAssetMenu(menuName = "ProjectName/Data/Event Table")]
public class EventTableSO : ScriptableObject
{
    [System.Serializable]
    public class EventEntry
    {
        public string eventId;
        public ActionSO[] actions;
    }

    [SerializeField] private EventEntry[] entries;

    public void TriggerEvent(string eventId)
    {
        var entry = System.Array.Find(entries, e => e.eventId == eventId);
        if (entry != null)
        {
            foreach (var action in entry.actions)
            {
                action?.Execute();
            }
        }
    }
}
```

### Dialogue response system

```csharp
[System.Serializable]
public class DialogueChoice
{
    public string text;
    public ActionSO[] onChosenActions;
}

public class DialogueUI : MonoBehaviour
{
    public void OnChoiceSelected(DialogueChoice choice)
    {
        foreach (var action in choice.onChosenActions)
        {
            action?.Execute();
        }
    }
}
```

---

## Anti-patterns - P1

### Missing null-conditional operator

```csharp
// NG: Throws NullReferenceException if not assigned
rewardAction.Execute();

// OK: Safe if not assigned
rewardAction?.Execute();
```

### Passing explicit caller info

```csharp
// NG: Loses automatic caller tracking
action.Execute("", "", 0);

// OK: Let compiler fill caller info
action.Execute();
```

### Action doing too many things

```csharp
// NG: Violates single responsibility
[CreateAssetMenu(menuName = "Game/Actions/Do Everything")]
public class DoEverythingAction : ActionSO
{
    // Plays sound AND spawns VFX AND updates score AND ...
}

// OK: Single responsibility, compose with SequenceAction
[CreateAssetMenu(menuName = "Game/Actions/Play Sound")]
public class PlaySoundAction : ActionSO { }

[CreateAssetMenu(menuName = "Game/Actions/Spawn VFX")]
public class SpawnVFXAction : ActionSO { }
```

### Missing #if UNITY_EDITOR for monitoring

```csharp
// NG: Monitoring code in release build
public override void Execute(...)
{
    DoSomething();
    NotifyActionExecuted(callerInfo); // Error in build!
}

// OK: Wrap editor-only code
public override void Execute(...)
{
    DoSomething();

#if UNITY_EDITOR
    NotifyActionExecuted(new CallerInfo(callerMember, callerFile, callerLine));
#endif
}
```

---

## Asset organization - P2

```
Assets/_Project/ScriptableObjects/Actions/
├── Rewards/
│   ├── GiveGold.asset
│   └── GiveItem.asset
├── Effects/
│   ├── SpawnExplosion.asset
│   └── PlayFanfare.asset
└── UI/
    └── ShowNotification.asset
```

---

## References

- [Event Channels](event-channels.md) - For notification-style communication
- [Variables System](variables.md) - Reactive state
- [Design Principles](design-principles.md)

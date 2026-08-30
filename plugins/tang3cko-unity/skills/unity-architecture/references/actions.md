# Actions

ActionSO pattern for data-driven, configurable commands.

---

## Basic Usage

### Creating an Action class

```csharp
using Tang3cko.ReactiveSO;
using UnityEngine;

namespace ProjectName.Actions
{
    [CreateAssetMenu(fileName = "SpawnEffect", menuName = "ProjectName/Actions/Spawn Effect")]
    public class SpawnEffectAction : ActionSO
    {
        [Header("Settings")]
        [SerializeField] private GameObject effectPrefab;
        [SerializeField] private Vector3 offset;
        [SerializeField] private float duration = 2f;

        // Execute() is non-virtual on the base class so caller tracking and Monitor
        // notification stay centralized. Derived classes implement OnExecute().
        protected override void OnExecute()
        {
            var instance = Object.Instantiate(effectPrefab);
            instance.transform.position += offset;
            Object.Destroy(instance, duration);
        }
    }
}
```

### Executing an Action

```csharp
[SerializeField] private ActionSO rewardAction;

public void GiveReward()
{
    rewardAction?.Execute();  // Always use null-conditional
}
```

---

## Generic Actions

Use `ActionSO<T>` when the action needs a parameter at execution time.

```csharp
[CreateAssetMenu(fileName = "DamageAction", menuName = "ProjectName/Actions/Damage")]
public class DamageAction : ActionSO<int>
{
    [Header("Settings")]
    [SerializeField] private GameObject damageVFX;

    protected override void OnExecute(int damage)
    {
        Debug.Log($"Dealing {damage} damage");
    }
}
```

---

## Common Patterns

### Sequence Action

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Sequence")]
public class SequenceAction : ActionSO
{
    [SerializeField] private ActionSO[] actions;

    protected override void OnExecute()
    {
        foreach (var action in actions)
        {
            action?.Execute();
        }
    }
}
```

### Conditional Action

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Conditional")]
public class ConditionalAction : ActionSO
{
    [SerializeField] private BoolVariableSO condition;
    [SerializeField] private ActionSO trueAction;
    [SerializeField] private ActionSO falseAction;

    protected override void OnExecute()
    {
        if (condition != null && condition.Value)
            trueAction?.Execute();
        else
            falseAction?.Execute();
    }
}
```

---

## Use Cases

### Quest reward system

```csharp
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

---

## When to Use

| Use Actions when | Use EventChannels when |
|-----------------|------------------------|
| Data-driven systems (quest rewards, event tables) | Simple notifications |
| Configurable behaviors (spawn effects, play sounds) | One-way communication |
| Reusable commands (triggered from multiple places) | No configurable behavior |
| Designer-configurable logic | Simple parameters |

---

## Anti-Patterns

### Missing null-conditional operator

```csharp
// Bad: Throws NullReferenceException if not assigned
rewardAction.Execute();

// Good
rewardAction?.Execute();
```

### Trying to override `Execute` instead of `OnExecute`

`ActionSO.Execute()` is non-virtual. It captures caller info, invokes `OnExecute()`, and notifies
the Monitor Window in one place, so no derived class can forget to. The caller parameters exist
only under `#if UNITY_EDITOR`, because `CallerFilePath` would otherwise bake absolute source paths
into player builds.

```csharp
// Bad: does not compile — Execute is not virtual
public override void Execute(...) { }

// Good: implement the hook the base class calls
protected override void OnExecute()
{
    DoSomething();
}
```

Derived classes therefore never call `NotifyActionExecuted` or `LogAction` themselves, and need no
`#if UNITY_EDITOR` guard for monitoring — the base class already handles both.

---

## References

- [event-channels.md](event-channels.md) - EventChannel vs Actions comparison
- [architecture.md](architecture.md) - ScriptableObject patterns overview
- [variables.md](variables.md) - Variable pattern for shared state

---

## Checklist

- [ ] Every `Execute` call site uses the null-conditional operator (`action?.Execute()`)
- [ ] Derived classes implement `protected override void OnExecute()` — `Execute()` is non-virtual and must not be overridden
- [ ] No derived class calls `NotifyActionExecuted`/`LogAction`; the base class already does it
- [ ] ActionSO was chosen over EventChannel per the When to Use table (configurable/data-driven behavior, not a simple notification)
- [ ] `ActionSO<T>` is used when the caller supplies data at execution time; parameterless `ActionSO` otherwise
- [ ] Composite behavior uses SequenceAction/ConditionalAction assets rather than hardcoded orchestration in a consumer

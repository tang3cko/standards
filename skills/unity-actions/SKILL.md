---
name: unity-actions
description: ActionSO command pattern for reusable commands. Quest rewards, event tables, dialogue responses. Use when implementing commands in Unity.
---

# Actions

## Purpose

Implement the Command pattern using ScriptableObjects for data-driven, reusable commands.

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

### Executing an Action

```csharp
[SerializeField] private ActionSO rewardAction;

public void GiveReward()
{
    // Always use null-conditional operator
    rewardAction?.Execute();
}
```

---

## Generic Actions - P1

Use `ActionSO<T>` when the action needs a parameter at execution time.

```csharp
[CreateAssetMenu(
    fileName = "DamageAction",
    menuName = "ProjectName/Actions/Damage")]
public class DamageAction : ActionSO<int>
{
    [Header("Settings")]
    [SerializeField] private GameObject damageVFX;

    public override void Execute(int damage,
        string callerMember = "",
        string callerFile = "",
        int callerLine = 0)
    {
        Debug.Log($"Dealing {damage} damage");

#if UNITY_EDITOR
        var callerInfo = new CallerInfo(callerMember, callerFile, callerLine);
        NotifyActionExecuted(callerInfo);
#endif
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

## Common patterns - P2

### Sequence Action

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

```csharp
[CreateAssetMenu(menuName = "ProjectName/Actions/Conditional")]
public class ConditionalAction : ActionSO
{
    [SerializeField] private BoolVariableSO condition;
    [SerializeField] private ActionSO trueAction;
    [SerializeField] private ActionSO falseAction;

    public override void Execute(...)
    {
        if (condition != null && condition.Value)
            trueAction?.Execute();
        else
            falseAction?.Execute();
    }
}
```

---

## Use cases - P1

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

## Anti-patterns - P1

### Missing null-conditional operator

```csharp
// NG: Throws NullReferenceException if not assigned
rewardAction.Execute();

// OK: Safe if not assigned
rewardAction?.Execute();
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

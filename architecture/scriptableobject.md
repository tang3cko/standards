# ScriptableObject-Driven Architecture

## Purpose

Use ScriptableObjects as the foundation for data management and game state, separating data from logic and enabling Inspector-based configuration.

## Checklist

- [ ] Use ScriptableObjects for immutable game data
- [ ] Follow CreateAssetMenu naming conventions
- [ ] Separate runtime state from static data
- [ ] Include Header and Tooltip attributes
- [ ] Provide XML documentation for public classes
- [ ] Use appropriate CreateAssetMenu paths

---

## Data Container Pattern

### Immutable Data Definition

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Quest
{
    /// <summary>
    /// Quest data definition (immutable)
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "ProjectName/Data/Quest/QuestData")]
    public class QuestSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique quest identifier")]
        public string questID;

        [Tooltip("Quest display name")]
        public string questName;

        [Tooltip("Quest description")]
        [TextArea(3, 5)]
        public string questDescription;

        [Header("Objectives")]
        [Tooltip("Quest type")]
        public QuestType questType;

        [Tooltip("Quest objectives")]
        public List<ObjectiveData> objectives;

        [Tooltip("Time limit in seconds")]
        public float timeLimit = 300f;
    }

    [System.Serializable]
    public class ObjectiveData
    {
        public ObjectiveType type;
        public int requiredCount;
    }

    public enum QuestType
    {
        Gather,
        Deliver,
        Hunt
    }

    public enum ObjectiveType
    {
        CollectItem,
        DefeatEnemy,
        ReachLocation
    }
}
```

### Runtime State (When Needed)

```csharp
namespace ProjectName.Quest
{
    /// <summary>
    /// Quest progress state (runtime)
    /// </summary>
    [CreateAssetMenu(fileName = "QuestProgress", menuName = "ProjectName/Data/Quest/QuestProgress")]
    public class QuestProgressSO : ScriptableObject
    {
        [Header("Current Quest")]
        public QuestSO currentQuest;

        [Header("Progress")]
        public int currentProgress;
        public float timeRemaining;

        public QuestState State { get; private set; }

        public void Initialize(QuestSO quest)
        {
            currentQuest = quest;
            currentProgress = 0;
            timeRemaining = quest.timeLimit;
            State = QuestState.InProgress;
        }

        public bool AddProgress(int amount = 1)
        {
            currentProgress += amount;

            if (currentProgress >= currentQuest.requiredCount)
            {
                State = QuestState.Success;
                return true;
            }

            return false;
        }

        public void Clear()
        {
            currentQuest = null;
            currentProgress = 0;
            timeRemaining = 0f;
            State = QuestState.Inactive;
        }
    }

    public enum QuestState
    {
        Inactive,
        InProgress,
        Success,
        Failed
    }
}
```

---

## CreateAssetMenu Naming Conventions

Follow these patterns for CreateAssetMenu menuName:

### Data ScriptableObjects

Pattern: `"ProjectName/Data/Category/Specific"`

```csharp
[CreateAssetMenu(fileName = "Quest", menuName = "ProjectName/Data/Quest/QuestData")]
[CreateAssetMenu(fileName = "Enemy", menuName = "ProjectName/Data/Enemy/EnemyData")]
[CreateAssetMenu(fileName = "Weapon", menuName = "ProjectName/Data/Weapon/WeaponData")]
[CreateAssetMenu(fileName = "Character", menuName = "ProjectName/Data/Character")]
```

### RuntimeSet ScriptableObjects

Pattern: `"ProjectName/RuntimeSet/TypeName"`

```csharp
[CreateAssetMenu(fileName = "EnemyRuntimeSet", menuName = "ProjectName/RuntimeSet/Enemy")]
[CreateAssetMenu(fileName = "PlayerRuntimeSet", menuName = "ProjectName/RuntimeSet/Player")]
```

### EventChannel ScriptableObjects

Pattern: `"ProjectName/Events/TypeName Event Channel"`

```csharp
[CreateAssetMenu(fileName = "QuestEventChannel", menuName = "ProjectName/Events/Quest Event Channel")]
[CreateAssetMenu(fileName = "IntEventChannel", menuName = "ProjectName/Events/Int Event Channel")]
```

---

## Benefits

### Data-Logic Separation

```csharp
// Data: EnemyDataSO (ScriptableObject)
public class EnemyDataSO : ScriptableObject
{
    public int maxHealth = 100;
    public float moveSpeed = 3f;
    public int attackDamage = 10;
}

// Logic: EnemyController (MonoBehaviour)
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyDataSO enemyData;

    private int currentHealth;

    private void Start()
    {
        currentHealth = enemyData.maxHealth;
    }

    private void Update()
    {
        Move(enemyData.moveSpeed * Time.deltaTime);
    }
}
```

### Inspector-Based Configuration

- Edit values in Inspector without code changes
- Create multiple variants (Fast Enemy, Slow Enemy, Boss)
- Designer-friendly workflow
- Hot reload during Play mode

### Memory Efficiency

```csharp
// 100 enemies share the same EnemyDataSO instance
// Memory usage: 1 ScriptableObject + 100 small references
// vs. 100 separate copies of all data
```

---

## Practical Example

### Complete Quest System

```csharp
using UnityEngine;
using System.Collections.Generic;

namespace ProjectName.Quest
{
    /// <summary>
    /// Quest data definition
    /// </summary>
    [CreateAssetMenu(fileName = "Quest", menuName = "ProjectName/Data/Quest/QuestData")]
    public class QuestSO : ScriptableObject
    {
        [Header("Basic Info")]
        [Tooltip("Unique quest identifier")]
        public string questID;

        [Tooltip("Quest display name")]
        public string questName;

        [Tooltip("Quest description")]
        [TextArea(3, 5)]
        public string questDescription;

        [Header("Objectives")]
        public QuestType questType;
        public List<ObjectiveData> objectives;

        [Header("Rewards")]
        public int coinReward = 100;
        public int experienceReward = 50;

        [Header("Time Limit")]
        [Tooltip("Time limit in seconds (0 = no limit)")]
        public float timeLimit = 300f;

        public bool HasTimeLimit => timeLimit > 0;
    }

    [System.Serializable]
    public class ObjectiveData
    {
        public ObjectiveType type;
        public string targetID;
        public int requiredCount;
        public bool isOptional;
    }

    public enum QuestType
    {
        Story,
        Side,
        Daily,
        Challenge
    }

    public enum ObjectiveType
    {
        CollectItem,
        DefeatEnemy,
        ReachLocation,
        TalkToNPC
    }
}
```

---

## References

- [Event Channels](event-channels.md)
- [RuntimeSet Pattern](runtime-sets.md)
- [Extension Patterns](extension-patterns.md)

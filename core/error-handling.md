# Error Handling

## Purpose

Proper error handling prevents unexpected behavior and facilitates debugging.

## Checklist

- [ ] Use null conditional operator (?.)
- [ ] Null check SerializeFields in Start/Awake
- [ ] Protect file/network I/O with Try-Catch
- [ ] Include [ClassName] and context in logs
- [ ] Validate Inspector settings with OnValidate
- [ ] Use early return to avoid nesting
- [ ] Validate preconditions with Assert in development

---

## Null Safety

### Null Conditional Operator Usage

```csharp
// Good: Use null conditional operator (?.)
private void RaiseEvent()
{
    eventChannel?.RaiseEvent();
    onQuestCompleted?.RaiseEvent();
}

// Bad: No null check (NullReferenceException risk)
private void RaiseEvent()
{
    eventChannel.RaiseEvent();  // Crashes if eventChannel is null
}
```

### Explicit Null Checks

```csharp
private void ProcessTarget()
{
    // Null check with logging
    if (targetComponent == null)
    {
        Debug.LogError($"Target component is null in {gameObject.name}");
        return;
    }

    // Normal processing
    targetComponent.Execute();
}
```

### SerializeField Null Checks (Start/Awake)

```csharp
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameStatsManager gameStatsManager;

    private void Start()
    {
        // Fallback for unassigned Inspector references
        if (gameStatsManager == null)
        {
            gameStatsManager = FindFirstObjectByType<GameStatsManager>();

            if (gameStatsManager == null)
            {
                Debug.LogWarning("[UIManager] GameStatsManager not found. Timer display will not work properly.", this);
            }
        }
    }
}
```

---

## Try-Catch Patterns

### Resources.Load Error Handling

```csharp
public bool LoadData(string path)
{
    try
    {
        var data = Resources.Load<DataSO>(path);

        if (data == null)
        {
            Debug.LogWarning($"Data not found at path: {path}");
            return false;
        }

        ProcessData(data);
        return true;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Failed to load data: {e.Message}");
        return false;
    }
}
```

### File I/O Error Handling

```csharp
public bool SaveGame(string filePath)
{
    try
    {
        string jsonData = JsonUtility.ToJson(gameData);
        System.IO.File.WriteAllText(filePath, jsonData);

        Debug.Log($"Game saved successfully to {filePath}");
        return true;
    }
    catch (System.IO.IOException e)
    {
        Debug.LogError($"Failed to save game: {e.Message}");
        return false;
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Unexpected error during save: {e.Message}");
        return false;
    }
}
```

---

## Logging Guidelines

### Log Level Usage

```csharp
// Debug.Log: Normal information
Debug.Log("Game started successfully");

// Debug.LogWarning: Warning (execution continues)
if (playerCount == 0)
{
    Debug.LogWarning("No players found. Starting with default player.");
}

// Debug.LogError: Error (serious problem)
if (essentialData == null)
{
    Debug.LogError("Essential data is missing. Game cannot continue.");
}

// Debug.LogException: Exception information
try
{
    // Processing
}
catch (System.Exception e)
{
    Debug.LogException(e);
}
```

### Include Context in Logs

```csharp
// Bad: Insufficient information
Debug.LogError("Component is null");

// Good: Detailed context
Debug.LogError($"[{GetType().Name}] targetComponent is null on GameObject: {gameObject.name}", this);
```

### Log Format Rules

```csharp
// [ClassName] Message
Debug.Log($"[GameManager] Game started with {playerCount} players");

// Pass this to enable Inspector navigation
Debug.LogWarning($"[UIManager] PlayerHealth not found.", this);
```

---

## OnValidate Validation

### Inspector Setting Validation

```csharp
public class QuestManager : MonoBehaviour
{
    [SerializeField] private QuestEventChannelSO onQuestSelected;
    [SerializeField] private QuestProgressSO questProgress;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // EventChannel validation
        if (onQuestSelected == null)
        {
            Debug.LogWarning($"[{GetType().Name}] onQuestSelected is not assigned on {gameObject.name}.", this);
        }

        // ScriptableObject validation
        if (questProgress == null)
        {
            Debug.LogWarning($"[{GetType().Name}] questProgress is not assigned on {gameObject.name}.", this);
        }
    }
#endif
}
```

---

## Early Return Pattern

### Guard Clause Usage

```csharp
// Good: Early return for readability
public void ProcessQuest(QuestSO quest)
{
    if (quest == null)
    {
        Debug.LogError("Quest is null");
        return;
    }

    if (!quest.IsActive)
    {
        Debug.LogWarning("Quest is not active");
        return;
    }

    // Normal processing (no deep nesting)
    StartQuest(quest);
    UpdateUI();
}

// Bad: Deep nesting
public void ProcessQuest(QuestSO quest)
{
    if (quest != null)
    {
        if (quest.IsActive)
        {
            // Normal processing (deep nesting)
            StartQuest(quest);
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("Quest is not active");
        }
    }
    else
    {
        Debug.LogError("Quest is null");
    }
}
```

---

## Assert Usage

### Precondition Validation

```csharp
using UnityEngine.Assertions;

public void DealDamage(int damage)
{
    // Development-only checks (disabled in Release builds)
    Assert.IsTrue(damage > 0, "Damage must be positive");
    Assert.IsNotNull(targetEnemy, "Target enemy is null");

    // Damage processing
    targetEnemy.TakeDamage(damage);
}
```

---

## Practical Example

### Complete Error Handling

```csharp
using UnityEngine;
using UnityEngine.Assertions;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestEventChannelSO onQuestSelected;
        [SerializeField] private QuestProgressSO questProgress;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (onQuestSelected == null)
            {
                Debug.LogWarning($"[QuestManager] onQuestSelected is not assigned on {gameObject.name}.", this);
            }

            if (questProgress == null)
            {
                Debug.LogWarning($"[QuestManager] questProgress is not assigned on {gameObject.name}.", this);
            }
        }
#endif

        private void OnEnable()
        {
            // Null conditional operator
            onQuestSelected?.OnEventRaised += HandleQuestSelected;
        }

        private void OnDisable()
        {
            onQuestSelected?.OnEventRaised -= HandleQuestSelected;
        }

        private void HandleQuestSelected(QuestSO quest)
        {
            // Early return
            if (quest == null)
            {
                Debug.LogError("[QuestManager] Quest is null", this);
                return;
            }

            // Assert (development only)
            Assert.IsNotNull(questProgress, "questProgress is null");

            // Try-Catch
            try
            {
                questProgress.Initialize(quest);
                Debug.Log($"[QuestManager] Quest started: {quest.questName}");
            }
            catch (System.Exception e)
            {
                Debug.LogException(e, this);
                Debug.LogError($"[QuestManager] Failed to start quest: {quest.questName}", this);
            }
        }
    }
}
```

---

## References

- [Naming Conventions](naming-conventions.md)
- [Comments Documentation](comments-documentation.md)

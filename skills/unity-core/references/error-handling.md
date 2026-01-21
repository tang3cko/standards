# Error handling

## Null safety

### Null conditional operator

```csharp
// Good
eventChannel?.RaiseEvent();
onQuestCompleted?.RaiseEvent();

// Bad (NullReferenceException risk)
eventChannel.RaiseEvent();
```

### Explicit null checks

```csharp
private void ProcessTarget()
{
    if (targetComponent == null)
    {
        Debug.LogError($"Target component is null in {gameObject.name}");
        return;
    }
    targetComponent.Execute();
}
```

### SerializeField null checks

```csharp
private void Start()
{
    if (gameStatsManager == null)
    {
        gameStatsManager = FindFirstObjectByType<GameStatsManager>();
        if (gameStatsManager == null)
        {
            Debug.LogWarning("[UIManager] GameStatsManager not found.", this);
        }
    }
}
```

## Try-catch patterns

### File I/O

```csharp
public bool SaveGame(string filePath)
{
    try
    {
        string jsonData = JsonUtility.ToJson(gameData);
        System.IO.File.WriteAllText(filePath, jsonData);
        return true;
    }
    catch (System.IO.IOException e)
    {
        Debug.LogError($"Failed to save game: {e.Message}");
        return false;
    }
}
```

### Resources.Load

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

## Logging guidelines

### Log levels

```csharp
// Debug.Log: Normal information
Debug.Log("Game started successfully");

// Debug.LogWarning: Warning (execution continues)
Debug.LogWarning("No players found. Starting with default.");

// Debug.LogError: Error (serious problem)
Debug.LogError("Essential data is missing.");

// Debug.LogException: Exception information
Debug.LogException(e);
```

### Include context

```csharp
// Bad
Debug.LogError("Component is null");

// Good
Debug.LogError($"[{GetType().Name}] targetComponent is null on {gameObject.name}", this);
```

## OnValidate validation

```csharp
#if UNITY_EDITOR
private void OnValidate()
{
    if (onQuestSelected == null)
        Debug.LogWarning($"[{GetType().Name}] onQuestSelected not assigned on {gameObject.name}.", this);

    if (questProgress == null)
        Debug.LogWarning($"[{GetType().Name}] questProgress not assigned on {gameObject.name}.", this);
}
#endif
```

## Early return pattern

```csharp
// Good: Early return
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
}

// Bad: Deep nesting
public void ProcessQuest(QuestSO quest)
{
    if (quest != null)
    {
        if (quest.IsActive)
        {
            StartQuest(quest);
        }
    }
}
```

## Assert usage

```csharp
using UnityEngine.Assertions;

public void DealDamage(int damage)
{
    // Development-only checks
    Assert.IsTrue(damage > 0, "Damage must be positive");
    Assert.IsNotNull(targetEnemy, "Target enemy is null");

    targetEnemy.TakeDamage(damage);
}
```

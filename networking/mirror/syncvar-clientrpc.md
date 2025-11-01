# SyncVar and ClientRpc

## Purpose

Define proper usage of SyncVar for automatic state synchronization and ClientRpc for event notifications in Mirror Networking, ensuring efficient network communication and late-join support.

## Checklist

- [ ] Use SyncVar for persistent game state (health, score, quest status)
- [ ] Use ClientRpc for temporary events (effects, sounds, notifications)
- [ ] Add hook functions to SyncVar for client-side updates
- [ ] Never sync ScriptableObjects directly (use ID/string instead)
- [ ] Use ClientRpc for EventChannel integration
- [ ] Understand SyncVar hooks don't fire on server
- [ ] Keep SyncVar types simple (primitives, structs, strings)

---

## SyncVar Overview

SyncVar automatically synchronizes values from server to clients, including late-joining clients.

### Basic Pattern

```csharp
using Mirror;
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Player
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PlayerStats : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onHealthChanged;

        // Auto-sync from server to clients
        [SyncVar(hook = nameof(OnHealthChanged))]
        private int currentHealth = 100;

        [SyncVar(hook = nameof(OnScoreChanged))]
        private int score = 0;

        // Server updates SyncVar
        [Server]
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);
        }

        [Server]
        public void AddScore(int points)
        {
            score += points;
        }

        // Hook: Called on clients when SyncVar changes
        private void OnHealthChanged(int oldValue, int newValue)
        {
            // Update UI on clients
            onHealthChanged?.RaiseEvent(newValue);
        }

        private void OnScoreChanged(int oldValue, int newValue)
        {
            // Update score display
        }
    }
}
```

---

## SyncVar vs ClientRpc

| Feature | SyncVar | ClientRpc |
|---------|---------|-----------|
| **Purpose** | Persistent state | Temporary events |
| **Late Join** | ✅ Auto-synced | ❌ Not re-sent |
| **Use Cases** | Health, score, quest status | Effects, sounds, notifications |
| **Bandwidth** | Efficient (only changes) | Per-call overhead |
| **Hook** | Yes | No |

### When to Use SyncVar

```csharp
// ✅ Good: Persistent state
[SyncVar(hook = nameof(OnQuestSelectedChanged))]
private bool isQuestSelected;

[SyncVar(hook = nameof(OnPlayerHealthChanged))]
private int currentHealth;

[SyncVar(hook = nameof(OnGameStateChanged))]
private GameState currentState;
```

### When to Use ClientRpc

```csharp
// ✅ Good: Temporary events
[ClientRpc]
private void RpcPlayExplosionEffect(Vector3 position)
{
    // Spawn effect at position
}

[ClientRpc]
private void RpcPlaySound(AudioClip clip)
{
    // Play sound
}

[ClientRpc]
private void RpcShowNotification(string message)
{
    // Show temporary UI message
}
```

---

## ScriptableObject Synchronization

### Problem: Cannot Sync ScriptableObjects

```csharp
// ❌ Bad: ScriptableObjects cannot be synced
[SyncVar]
private QuestSO selectedQuest; // Won't work!
```

### Solution: Sync ID, Convert in Hook

```csharp
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestStateSync : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        [Header("Available Quests")]
        [SerializeField] private List<QuestSO> availableQuests;

        // Sync questID instead of QuestSO
        [SyncVar(hook = nameof(OnQuestIDChanged))]
        private string selectedQuestID = "";

        private void OnEnable()
        {
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised += HandleQuestSelected;
        }

        private void OnDisable()
        {
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised -= HandleQuestSelected;
        }

        private void HandleQuestSelected(QuestSO quest)
        {
            // Server only
            if (!NetworkServer.active) return;

            // Update SyncVar
            selectedQuestID = quest.questID;

            // Server doesn't receive hook, so fire EventChannel manually
            BroadcastQuestSelection(quest);
        }

        // Hook: Called on clients (NOT on server)
        private void OnQuestIDChanged(string oldID, string newID)
        {
            if (string.IsNullOrEmpty(newID)) return;

            // Find QuestSO by ID
            QuestSO quest = availableQuests.Find(q => q.questID == newID);
            if (quest == null)
            {
                Debug.LogError($"Quest not found: {newID}");
                return;
            }

            // Fire EventChannel on clients
            BroadcastQuestSelection(quest);
        }

        private void BroadcastQuestSelection(QuestSO quest)
        {
            // EventChannel fires (decoupled)
            onQuestSelected?.RaiseEvent(quest);
        }
    }
}
```

---

## SyncVar Hook Behavior

### Important: Hooks Don't Fire on Server

```csharp
[SyncVar(hook = nameof(OnValueChanged))]
private int value;

[Server]
private void UpdateValue(int newValue)
{
    value = newValue; // Hook does NOT fire on server

    // Must manually handle on server
    OnValueChanged(value, newValue);
}

// Hook fires on clients automatically
private void OnValueChanged(int oldValue, int newValue)
{
    // Clients receive this
    UpdateUI(newValue);
}
```

### Complete Pattern

```csharp
using Mirror;
using Tang3cko.EventChannels;

namespace ProjectName.Game
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class GameTimer : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private FloatEventChannelSO onTimeChanged;

        [SyncVar(hook = nameof(OnTimeRemainingChanged))]
        private float timeRemaining;

        [ServerCallback]
        private void Update()
        {
            if (timeRemaining > 0)
            {
                float oldTime = timeRemaining;
                timeRemaining -= Time.deltaTime;

                // Server: Hook doesn't fire, so call manually
                OnTimeRemainingChanged(oldTime, timeRemaining);
            }
        }

        // Hook: Fires on clients automatically, NOT on server
        private void OnTimeRemainingChanged(float oldValue, float newValue)
        {
            // Both server and clients execute this
            onTimeChanged?.RaiseEvent(newValue);
        }
    }
}
```

---

## ClientRpc Patterns

### Basic ClientRpc

```csharp
[ClientRpc]
private void RpcShowEffect(Vector3 position, EffectType effectType)
{
    // All clients see effect
    SpawnEffect(effectType, position);
}
```

### ClientRpc with EventChannel

```csharp
using Mirror;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestManager : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onQuestCompleted;
        [SerializeField] private VoidEventChannelSO onQuestFailed;

        [Server]
        private void CompleteQuest()
        {
            // Server logic
            SaveQuestResult(true);

            // Notify all clients
            RpcQuestCompleted();
        }

        [ClientRpc]
        private void RpcQuestCompleted()
        {
            // Fire EventChannel on all clients
            onQuestCompleted?.RaiseEvent();
        }

        [ClientRpc]
        private void RpcQuestFailed()
        {
            onQuestFailed?.RaiseEvent();
        }
    }
}
```

---

## Late Join Support

### SyncVar Automatically Handles Late Join

```csharp
// Scenario:
// T0: Host selects quest → selectedQuestID = "quest_001"
// T1: Client2 joins (late)
// → Client2 automatically receives selectedQuestID = "quest_001"
// → OnQuestIDChanged hook fires on Client2
// → Client2 has correct quest state ✅

[SyncVar(hook = nameof(OnQuestIDChanged))]
private string selectedQuestID = "";
```

### ClientRpc Does NOT Support Late Join

```csharp
// Scenario:
// T0: Host plays explosion effect → RpcPlayExplosion()
// T1: Client2 joins (late)
// → Client2 does NOT see explosion ❌
// → Past ClientRpc calls are not re-sent

[ClientRpc]
private void RpcPlayExplosion(Vector3 position)
{
    // Temporary event - not saved for late joiners
}
```

---

## Performance Considerations

### Minimize SyncVar Changes

```csharp
// ✅ Good: Update SyncVar only when necessary
[Server]
private void UpdateHealth(int newHealth)
{
    if (currentHealth != newHealth)
    {
        currentHealth = newHealth; // SyncVar update
    }
}

// ❌ Bad: Constant SyncVar updates
[ServerCallback]
private void Update()
{
    currentHealth = CalculateHealth(); // Updates every frame!
}
```

### Batch Updates

```csharp
// ✅ Good: Batch related updates
[Server]
private void UpdatePlayerStats(int health, int mana, int stamina)
{
    // Single network message with struct
    PlayerStats stats = new PlayerStats
    {
        health = health,
        mana = mana,
        stamina = stamina
    };

    RpcUpdateStats(stats);
}

// ❌ Bad: Multiple separate calls
[Server]
private void UpdatePlayerStats(int health, int mana, int stamina)
{
    RpcUpdateHealth(health);
    RpcUpdateMana(mana);
    RpcUpdateStamina(stamina); // 3 separate network messages
}
```

---

## References

- [Mirror Basics](basics.md)
- [Server Authority](server-authority.md)
- [Late Join Handling](late-join.md)
- [Interactable Pattern](interactable-pattern.md)

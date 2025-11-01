# Server Authority Pattern

## Purpose

Implement secure Server Authority pattern in Mirror Networking to prevent cheating, ensure state consistency, and maintain a single source of truth on the server.

## Checklist

- [ ] Server manages all critical game state
- [ ] Clients send requests via [Command]
- [ ] Server validates all client requests
- [ ] Never trust client input
- [ ] Use [Server] for server-only logic
- [ ] Check NetworkServer.active before server operations
- [ ] Update ScriptableObjects only on server
- [ ] Notify clients via [ClientRpc] or SyncVar

---

## Core Principle

**The server is the single source of truth.** Clients request actions, the server validates and executes them, then notifies all clients of the result.

```
Client Request → Server Validation → Server Execution → Client Notification
```

---

## Basic Pattern

```csharp
using Mirror;
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Combat
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PlayerHealth : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onHealthChanged;

        [SyncVar(hook = nameof(OnHealthChanged))]
        private int currentHealth = 100;

        // Client requests damage
        [Command(requiresAuthority = false)]
        public void CmdTakeDamage(int damage, uint attackerNetId)
        {
            // SERVER VALIDATES
            if (!ValidateDamage(damage, attackerNetId))
                return;

            // SERVER EXECUTES
            currentHealth -= damage;
            currentHealth = Mathf.Max(0, currentHealth);

            // Notify all clients
            RpcShowDamageEffect(damage);

            if (currentHealth <= 0)
            {
                RpcPlayerDied();
            }
        }

        [Server]
        private bool ValidateDamage(int damage, uint attackerNetId)
        {
            // Validate damage range
            if (damage < 0 || damage > 100)
            {
                Debug.LogWarning($"Invalid damage amount: {damage}");
                return false;
            }

            // Validate attacker exists
            if (NetworkServer.spawned.TryGetValue(attackerNetId, out NetworkIdentity attacker))
            {
                return true;
            }

            Debug.LogWarning($"Attacker not found: {attackerNetId}");
            return false;
        }

        [ClientRpc]
        private void RpcShowDamageEffect(int damage)
        {
            // Visual effect on all clients
            onHealthChanged?.RaiseEvent(currentHealth);
        }

        [ClientRpc]
        private void RpcPlayerDied()
        {
            // Death effect on all clients
        }

        private void OnHealthChanged(int oldHealth, int newHealth)
        {
            // SyncVar hook - update UI
            onHealthChanged?.RaiseEvent(newHealth);
        }
    }
}
```

---

## Quest System Example

```csharp
using Mirror;
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestManager : NetworkBehaviour
    {
        [Header("State")]
        [SerializeField] private QuestProgressSO questProgress;

        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onQuestCompleted;
        [SerializeField] private CollectableEventChannelSO onItemCollected;

        [SyncVar(hook = nameof(OnProgressChanged))]
        private int currentProgress;

        private void OnEnable()
        {
            if (onItemCollected != null)
                onItemCollected.OnEventRaised += HandleItemCollected;
        }

        private void OnDisable()
        {
            if (onItemCollected != null)
                onItemCollected.OnEventRaised -= HandleItemCollected;
        }

        private void HandleItemCollected(CollectableType item)
        {
            // SERVER ONLY: EventChannel handler
            if (!NetworkServer.active) return;

            // Validate and update progress
            if (ValidateCollection(item))
            {
                currentProgress++;

                // Update ScriptableObject (server only)
                questProgress.currentProgress = currentProgress;

                if (currentProgress >= questProgress.requiredCount)
                {
                    CompleteQuest();
                }
            }
        }

        [Server]
        private bool ValidateCollection(CollectableType item)
        {
            // Validate correct item type
            return item == questProgress.targetType;
        }

        [Server]
        private void CompleteQuest()
        {
            questProgress.state = QuestState.Success;
            RpcQuestCompleted();
        }

        [ClientRpc]
        private void RpcQuestCompleted()
        {
            // Notify all clients
            onQuestCompleted?.RaiseEvent();
        }

        private void OnProgressChanged(int oldValue, int newValue)
        {
            // Update UI on all clients
        }
    }
}
```

---

## Validation Patterns

### Range Validation

```csharp
[Command(requiresAuthority = false)]
private void CmdPurchaseItem(int itemPrice, uint buyerNetId)
{
    // Validate price range
    if (itemPrice < 0 || itemPrice > 10000)
    {
        Debug.LogWarning($"Invalid item price: {itemPrice}");
        return;
    }

    // Validate buyer has enough money
    if (!PlayerHasEnoughMoney(buyerNetId, itemPrice))
    {
        RpcShowError(buyerNetId, "Insufficient funds");
        return;
    }

    // Process purchase
    ProcessPurchase(buyerNetId, itemPrice);
}
```

### Cooldown Validation

```csharp
private Dictionary<uint, float> lastAbilityUse = new Dictionary<uint, float>();

[Command(requiresAuthority = false)]
private void CmdUseAbility(uint playerNetId)
{
    // Validate cooldown
    if (lastAbilityUse.TryGetValue(playerNetId, out float lastUse))
    {
        if (Time.time - lastUse < abilityCooldown)
        {
            RpcShowError(playerNetId, "Ability on cooldown");
            return;
        }
    }

    // Update last use time
    lastAbilityUse[playerNetId] = Time.time;

    // Execute ability
    ExecuteAbility(playerNetId);
}
```

### Distance Validation

```csharp
[Command(requiresAuthority = false)]
private void CmdInteract(uint interactorNetId, uint targetNetId)
{
    // Get interactor and target positions
    if (!NetworkServer.spawned.TryGetValue(interactorNetId, out NetworkIdentity interactor))
        return;

    if (!NetworkServer.spawned.TryGetValue(targetNetId, out NetworkIdentity target))
        return;

    // Validate distance
    float distance = Vector3.Distance(interactor.transform.position, target.transform.position);
    if (distance > maxInteractDistance)
    {
        Debug.LogWarning($"Interact distance too far: {distance}");
        return;
    }

    // Process interaction
    ProcessInteraction(interactorNetId, targetNetId);
}
```

---

## ScriptableObject Management

### Server-Only Updates

```csharp
using Mirror;
using UnityEngine;

namespace ProjectName.Game
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class GameStateManager : NetworkBehaviour
    {
        [SerializeField] private GameStateSO gameState;

        [ServerCallback]
        private void Update()
        {
            // Server-only ScriptableObject update
            if (gameState.isActive)
            {
                gameState.timeRemaining -= Time.deltaTime;

                if (gameState.timeRemaining <= 0)
                {
                    EndGame();
                }
            }
        }

        [Server]
        private void EndGame()
        {
            // Update ScriptableObject
            gameState.isActive = false;

            // Notify all clients
            RpcGameEnded();
        }

        [ClientRpc]
        private void RpcGameEnded()
        {
            // Clients handle game end
        }
    }
}
```

---

## Anti-Patterns

### Pattern 1: Trusting Client Input

```csharp
// ❌ Bad: No validation
[Command(requiresAuthority = false)]
private void CmdAddScore(int points)
{
    score += points; // Client can cheat!
}

// ✅ Good: Server validates
[Command(requiresAuthority = false)]
private void CmdEnemyKilled(uint enemyNetId, uint killerNetId)
{
    // Validate enemy exists and is dead
    if (ValidateKill(enemyNetId, killerNetId))
    {
        AddScore(killerNetId, enemyScoreValue);
    }
}
```

### Pattern 2: Client-Side State Updates

```csharp
// ❌ Bad: Client updates directly
private void Update()
{
    if (isLocalPlayer)
    {
        questProgress.timeRemaining -= Time.deltaTime; // Not synced!
    }
}

// ✅ Good: Server updates, clients receive via SyncVar
[ServerCallback]
private void Update()
{
    if (questProgress.state == QuestState.InProgress)
    {
        timeRemaining -= Time.deltaTime;
        // SyncVar automatically syncs to clients
    }
}
```

### Pattern 3: Missing NetworkServer.active Check

```csharp
// ❌ Bad: Runs on all clients
private void HandleEventChannel()
{
    UpdateGameState(); // Executes on everyone!
}

// ✅ Good: Server-only execution
private void HandleEventChannel()
{
    if (!NetworkServer.active) return;

    UpdateGameState(); // Server only
}
```

---

## References

- [Mirror Basics](basics.md)
- [SyncVar and ClientRpc](syncvar-clientrpc.md)
- [Late Join Handling](late-join.md)
- [Interactable Pattern](interactable-pattern.md)

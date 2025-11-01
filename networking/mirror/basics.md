# Mirror Networking Basics

## Purpose

Define fundamental patterns and best practices for implementing multiplayer systems using Mirror Networking, focusing on Server Authority pattern, NetworkBehaviour usage, and proper component setup.

## Checklist

- [ ] Use Server Authority pattern for all game-critical logic
- [ ] Add RequireComponent(typeof(NetworkIdentity)) to all NetworkBehaviours
- [ ] Follow Cmd/Rpc naming conventions (CmdMethodName, RpcMethodName)
- [ ] Check NetworkServer.active before server-side logic
- [ ] Use [Server], [Client], [Command], [ClientRpc] attributes correctly
- [ ] Integrate EventChannels with network events
- [ ] Never trust client input - validate on server

---

## Prerequisites

- **Mirror Networking**: Open-source networking library for Unity
- **Server Authority**: Server manages all critical state, clients handle display only
- **NetworkBehaviour**: Mirror's component base class
- **NetworkIdentity**: Network synchronization identifier (required component)

**Mirror Documentation**: https://mirror-networking.gitbook.io/

---

## Server Authority Pattern

### Basic Principle

The server is the single source of truth. Clients request actions, server validates and executes them.

```csharp
using Mirror;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    /// <summary>
    /// Quest manager with Server Authority
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestManager : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onQuestCompleted;

        // SyncVar: Auto-sync from server to clients
        [SyncVar(hook = nameof(OnProgressChanged))]
        private int currentProgress;

        // Server-only execution
        [Server]
        public void SelectQuest(QuestSO quest)
        {
            if (!NetworkServer.active) return;

            // Server authority processing
            currentProgress = 0;
        }

        // Client request to server
        [Command(requiresAuthority = false)]
        public void CmdCollectItem(string targetID, uint collectorNetId)
        {
            // Server validates
            if (ValidateCollection(targetID, collectorNetId))
            {
                currentProgress++;

                if (currentProgress >= 5)
                {
                    RpcQuestSuccess();
                }
            }
        }

        // Notify all clients
        [ClientRpc]
        private void RpcQuestSuccess()
        {
            // Raise EventChannel (UI subscribes to this)
            onQuestCompleted?.RaiseEvent();
        }

        // SyncVar hook (client-side UI update)
        private void OnProgressChanged(int oldValue, int newValue)
        {
            // Update UI on clients
            UpdateProgressUI(newValue);
        }

        private bool ValidateCollection(string targetID, uint collectorNetId)
        {
            // Server-side validation logic
            return true;
        }

        private void UpdateProgressUI(int progress)
        {
            // UI update logic
        }
    }
}
```

### Server Authority Benefits

- **Cheat Prevention**: Clients cannot modify progress arbitrarily
- **Consistency Guarantee**: Server is the single source of truth
- **Simplified Synchronization**: All clients share the same state

---

## Mirror Attributes

### [Server] / [ServerCallback]

Methods that execute only on the server.

```csharp
// [Server]: Warns if called on non-server
[Server]
private void QuestCompleted()
{
    questProgress.state = QuestState.Success;
    RpcQuestCompleted();
}

// [ServerCallback]: Does nothing on non-server (no warning)
[ServerCallback]
private void Update()
{
    if (questProgress.state != QuestState.InProgress) return;
    questProgress.timeRemaining -= Time.deltaTime;
}
```

### [Command]

Client-to-server requests.

```csharp
// requiresAuthority = false: Anyone can call
[Command(requiresAuthority = false)]
private void CmdRequestInteract(uint interactorNetId)
{
    // Server validates
    OnInteractServer(interactorNetId);
    RpcOnInteracted(interactorNetId);
}
```

**Naming Convention**: `Cmd` prefix required (e.g., `CmdRequestInteract`)

### [ClientRpc]

Server-to-all-clients notifications.

```csharp
[ClientRpc]
private void RpcQuestCompleted()
{
    // Raise EventChannel for UI
    onQuestCompleted?.RaiseEvent();
}
```

**Naming Convention**: `Rpc` prefix required (e.g., `RpcQuestCompleted`)

### [Client] / [ClientCallback]

Methods that execute only on clients.

```csharp
// [Client]: Warns if called on server
[Client]
private void PlayLocalEffect()
{
    particleSystem.Play();
}

// [ClientCallback]: Does nothing on server (no warning)
[ClientCallback]
private void OnTriggerEnter(Collider other)
{
    // Client-side collision handling
}
```

---

## NetworkIdentity and RequireComponent

### Required Pattern

All classes inheriting `NetworkBehaviour` require `NetworkIdentity`.

```csharp
using Mirror;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class MainQuestManager : NetworkBehaviour
    {
        // NetworkIdentity automatically added by Unity
    }
}
```

**Why Required:**
- Prevents forgetting to add NetworkIdentity in Inspector
- Unity automatically adds the component
- Simplifies setup process

---

## EventChannel Integration

### Server to Client Notifications

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
        [SerializeField] private IntEventChannelSO onProgressUpdated;

        [ClientRpc]
        private void RpcQuestCompleted()
        {
            // EventChannel fires (UI subscribes)
            onQuestCompleted?.RaiseEvent();
        }

        [ClientRpc]
        private void RpcProgressUpdated(int progress)
        {
            // EventChannel fires with data
            onProgressUpdated?.RaiseEvent(progress);
        }
    }
}
```

### EventChannel Handler with Server Check

```csharp
using Mirror;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestProgressTracker : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private CollectableEventChannelSO onItemCollected;

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
            // IMPORTANT: Server-only processing
            if (!NetworkServer.active) return;

            // Server-side logic
            UpdateProgress(item);
        }

        [Server]
        private void UpdateProgress(CollectableType item)
        {
            // Update quest progress
        }
    }
}
```

---

## Best Practices

### 1. Always Validate on Server

```csharp
// ✅ Good: Server validates
[Command(requiresAuthority = false)]
private void CmdPickupItem(uint itemNetId, uint playerNetId)
{
    // Validate on server
    if (!CanPickup(itemNetId, playerNetId))
        return;

    // Process pickup
    ProcessPickup(itemNetId, playerNetId);
}

// ❌ Bad: Trust client
[Command(requiresAuthority = false)]
private void CmdPickupItem(uint itemNetId)
{
    // No validation - client can cheat
    ProcessPickup(itemNetId);
}
```

### 2. Use NetworkServer.active Check

```csharp
// ✅ Good: Check before server logic
private void HandleQuestEvent()
{
    if (!NetworkServer.active) return;

    // Server-only logic
    UpdateQuestState();
}

// ❌ Bad: No check (runs on all clients)
private void HandleQuestEvent()
{
    UpdateQuestState(); // Executes on everyone!
}
```

### 3. Follow Naming Conventions

```csharp
// ✅ Good: Proper naming
[Command(requiresAuthority = false)]
private void CmdRequestInteract(uint netId) { }

[ClientRpc]
private void RpcShowEffect() { }

// ❌ Bad: Missing prefix
[Command(requiresAuthority = false)]
private void RequestInteract(uint netId) { } // Won't work!

[ClientRpc]
private void ShowEffect() { } // Won't work!
```

### 4. RequireComponent on All NetworkBehaviours

```csharp
// ✅ Good: RequireComponent
[RequireComponent(typeof(NetworkIdentity))]
public class PlayerController : NetworkBehaviour
{
    // NetworkIdentity automatically added
}

// ❌ Bad: Missing RequireComponent
public class PlayerController : NetworkBehaviour
{
    // Must manually add NetworkIdentity (error-prone)
}
```

---

## Common Patterns

### Player Input Pattern

```csharp
using Mirror;
using UnityEngine;

namespace ProjectName.Player
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class PlayerMovement : NetworkBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;

        private void Update()
        {
            // Only process input for local player
            if (!isLocalPlayer) return;

            HandleInput();
        }

        private void HandleInput()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;

            // Send movement to server
            CmdMove(movement);
        }

        [Command]
        private void CmdMove(Vector3 movement)
        {
            // Server processes movement
            transform.position += movement;
        }
    }
}
```

---

## References

- [Server Authority](server-authority.md)
- [SyncVar and ClientRpc](syncvar-clientrpc.md)
- [Late Join Handling](late-join.md)
- [Interactable Pattern](interactable-pattern.md)
- [Event Channels](../../architecture/event-channels.md)

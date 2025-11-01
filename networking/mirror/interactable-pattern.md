# Interactable Object Pattern

## Purpose

Define a unified base class pattern for interactable objects in Mirror Networking that supports both local-only interactions and Server Authority interactions through a configurable flag.

## Checklist

- [ ] Inherit from InteractableObject base class
- [ ] Set useServerAuthority flag appropriately
- [ ] Implement OnInteractServer for server validation
- [ ] Implement OnInteractClient for client effects
- [ ] Implement OnInteractLocal for local-only interactions
- [ ] Use RequireComponent(typeof(NetworkIdentity))
- [ ] Provide GetInteractText for UI prompts
- [ ] Raise EventChannels for decoupled communication

---

## Base Class Design

### Design Philosophy

The `useServerAuthority` flag allows switching between local processing and Server Authority in a unified base class.

```csharp
using Mirror;
using UnityEngine;

namespace ProjectName.Interaction
{
    /// <summary>
    /// Base class for interactable objects
    /// useServerAuthority flag switches between local/Server Authority modes
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class InteractableObject : NetworkBehaviour, IInteractable
    {
        [Header("Interactable Settings")]
        [Tooltip("Use Server Authority for validation")]
        [SerializeField] private bool useServerAuthority = true;

        public void Interact(Transform interactor)
        {
            if (useServerAuthority)
            {
                // Server Authority pattern
                HandleServerAuthorityInteract(interactor);
            }
            else
            {
                // Local processing only
                OnInteractLocal(interactor);
            }
        }

        private void HandleServerAuthorityInteract(Transform interactor)
        {
            // Validate interactor has NetworkIdentity
            if (!interactor.TryGetComponent<NetworkIdentity>(out var identity)) return;

            // Only local player can interact
            if (!identity.isLocalPlayer) return;

            // Send request to server
            CmdRequestInteract(identity.netId);
        }

        [Command(requiresAuthority = false)]
        private void CmdRequestInteract(uint interactorNetId)
        {
            // Server validates and processes
            OnInteractServer(interactorNetId);

            // Notify all clients
            RpcOnInteracted(interactorNetId);
        }

        [ClientRpc]
        private void RpcOnInteracted(uint interactorNetId)
        {
            // Client-side effects (all clients)
            OnInteractClient(interactorNetId);
        }

        // Override in derived classes
        protected virtual void OnInteractLocal(Transform interactor) { }
        protected virtual void OnInteractServer(uint interactorNetId) { }
        protected virtual void OnInteractClient(uint interactorNetId) { }

        public virtual string GetInteractText() => "[E] Interact";
        public Transform GetTransform() => transform;
    }

    public interface IInteractable
    {
        void Interact(Transform interactor);
        string GetInteractText();
        Transform GetTransform();
    }
}
```

---

## Server Authority Example

### Collectible Item (Mushroom)

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    /// <summary>
    /// Collectible mushroom with Server Authority
    /// </summary>
    public class MushroomInteractable : InteractableObject
    {
        [Header("Collectable Settings")]
        [SerializeField] private CollectableType collectableType = CollectableType.Mushroom;

        [Header("Event Channels")]
        [SerializeField] private CollectableEventChannelSO onItemCollected;

        // useServerAuthority = true (default)

        protected override void OnInteractServer(uint interactorNetId)
        {
            // SERVER: Validate and raise EventChannel
            Debug.Log($"[MushroomInteractable] Collected by {interactorNetId}");

            // Raise EventChannel (server only)
            // QuestManager subscribes to this
            onItemCollected?.RaiseEvent(collectableType);

            // Hide object on all clients (via RpcOnInteracted → OnInteractClient)
        }

        protected override void OnInteractClient(uint interactorNetId)
        {
            // ALL CLIENTS: Hide object
            gameObject.SetActive(false);

            // Play collection effect
            PlayCollectionEffect();
        }

        private void PlayCollectionEffect()
        {
            // Particle effect, sound, etc.
        }

        public override string GetInteractText()
        {
            return $"[E] Collect {collectableType}";
        }
    }
}
```

---

## Local-Only Example

### Receptionist NPC (UI Trigger)

```csharp
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Lobby
{
    /// <summary>
    /// Receptionist NPC that opens quest board (local UI only)
    /// </summary>
    public class ReceptionistInteractable : InteractableObject
    {
        [Header("Event Channels")]
        [SerializeField] private VoidEventChannelSO onQuestBoardOpened;

        // Set useServerAuthority = false in Inspector

        protected override void OnInteractLocal(Transform interactor)
        {
            // LOCAL PLAYER ONLY: Open UI
            Debug.Log($"[ReceptionistInteractable] Opening quest board for local player");

            // Raise EventChannel (local only)
            // QuestBoardUI subscribes to this
            onQuestBoardOpened?.RaiseEvent();
        }

        public override string GetInteractText()
        {
            return "[E] Talk to Receptionist";
        }
    }
}
```

---

## Networked Door Example

```csharp
using Mirror;
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Environment
{
    /// <summary>
    /// Door that can be opened by any player (Server Authority)
    /// </summary>
    public class DoorInteractable : InteractableObject
    {
        [Header("Door State")]
        [SyncVar(hook = nameof(OnDoorStateChanged))]
        private bool isOpen = false;

        [Header("Event Channels")]
        [SerializeField] private BoolEventChannelSO onDoorStateChanged;

        [Header("Settings")]
        [SerializeField] private Animator doorAnimator;

        // useServerAuthority = true

        protected override void OnInteractServer(uint interactorNetId)
        {
            // SERVER: Toggle door state
            isOpen = !isOpen;

            Debug.Log($"[DoorInteractable] Door {(isOpen ? "opened" : "closed")} by {interactorNetId}");
        }

        protected override void OnInteractClient(uint interactorNetId)
        {
            // Play door sound
            PlayDoorSound();
        }

        // SyncVar hook: Update visual state
        private void OnDoorStateChanged(bool oldState, bool newState)
        {
            // Update animator
            if (doorAnimator != null)
            {
                doorAnimator.SetBool("IsOpen", newState);
            }

            // Raise EventChannel
            onDoorStateChanged?.RaiseEvent(newState);
        }

        private void PlayDoorSound()
        {
            // Play sound effect
        }

        public override string GetInteractText()
        {
            return isOpen ? "[E] Close Door" : "[E] Open Door";
        }
    }
}
```

---

## Networked Chest Example

```csharp
using Mirror;
using UnityEngine;
using Tang3cko.EventChannels;

namespace ProjectName.Loot
{
    /// <summary>
    /// Loot chest with Server Authority (one-time interaction)
    /// </summary>
    public class LootChestInteractable : InteractableObject
    {
        [Header("Chest State")]
        [SyncVar(hook = nameof(OnChestOpenedChanged))]
        private bool isOpened = false;

        [Header("Loot Settings")]
        [SerializeField] private int goldAmount = 100;

        [Header("Event Channels")]
        [SerializeField] private IntEventChannelSO onGoldCollected;

        // useServerAuthority = true

        protected override void OnInteractServer(uint interactorNetId)
        {
            // SERVER: Check if already opened
            if (isOpened)
            {
                Debug.Log("[LootChestInteractable] Chest already opened");
                return;
            }

            // Open chest
            isOpened = true;

            // Give gold to interactor
            GiveGoldToPlayer(interactorNetId);

            Debug.Log($"[LootChestInteractable] Chest opened by {interactorNetId}");
        }

        protected override void OnInteractClient(uint interactorNetId)
        {
            // Play opening animation
            PlayOpenAnimation();
        }

        [Server]
        private void GiveGoldToPlayer(uint playerNetId)
        {
            // Find player and give gold
            if (NetworkServer.spawned.TryGetValue(playerNetId, out NetworkIdentity player))
            {
                // Implementation depends on your player system
                Debug.Log($"Giving {goldAmount} gold to player {playerNetId}");
            }
        }

        private void OnChestOpenedChanged(bool oldState, bool newState)
        {
            if (newState)
            {
                // Visual state: chest opened
                UpdateChestVisuals(true);
            }
        }

        private void PlayOpenAnimation()
        {
            // Play chest opening animation
        }

        private void UpdateChestVisuals(bool opened)
        {
            // Update chest model/texture
        }

        public override string GetInteractText()
        {
            return isOpened ? "Empty" : "[E] Open Chest";
        }
    }
}
```

---

## When to Use Each Mode

### Use Server Authority (useServerAuthority = true)

- Collectible items (mushrooms, coins)
- Doors, switches, levers
- Loot chests
- Quest objectives
- Any interaction affecting game state

**Why:** Server validates, prevents cheating, ensures consistency

### Use Local-Only (useServerAuthority = false)

- UI triggers (NPC dialogue, shop, quest board)
- Local player settings
- Camera triggers
- Tutorial prompts
- Any interaction affecting only local player

**Why:** No network synchronization needed, better performance

---

## Best Practices

### Validate on Server

```csharp
protected override void OnInteractServer(uint interactorNetId)
{
    // ✅ Good: Server validates
    if (!CanInteract(interactorNetId))
    {
        Debug.LogWarning($"Invalid interaction from {interactorNetId}");
        return;
    }

    // Process interaction
    ProcessInteraction(interactorNetId);
}
```

### Use EventChannels for Decoupling

```csharp
// ✅ Good: Raise EventChannel instead of direct calls
protected override void OnInteractServer(uint interactorNetId)
{
    onItemCollected?.RaiseEvent(collectableType);
    // QuestManager subscribes to this EventChannel
}

// ❌ Bad: Direct coupling
protected override void OnInteractServer(uint interactorNetId)
{
    questManager.CollectItem(collectableType); // Tight coupling
}
```

### Separate Server and Client Logic

```csharp
// ✅ Good: Clear separation
protected override void OnInteractServer(uint interactorNetId)
{
    // Server: Validation and state changes
    UpdateGameState();
}

protected override void OnInteractClient(uint interactorNetId)
{
    // Client: Visual/audio effects only
    PlayEffects();
}
```

---

## References

- [Mirror Basics](basics.md)
- [Server Authority](server-authority.md)
- [SyncVar and ClientRpc](syncvar-clientrpc.md)
- [Late Join Handling](late-join.md)
- [Event Channels](../../architecture/event-channels.md)

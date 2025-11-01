# Late Join Handling

## Purpose

Ensure proper state synchronization for players who join mid-game (late join) and handle player disconnections gracefully in Mirror Networking multiplayer games.

## Checklist

- [ ] Use SyncVar for persistent state (supports late join)
- [ ] Use ClientRpc for temporary events (no late join support)
- [ ] Sync ScriptableObject state via SyncVar + hook pattern
- [ ] Register/unregister in RuntimeSets (OnStartClient/OnStopClient)
- [ ] Handle host disconnection (saves partial state)
- [ ] Test late join scenarios thoroughly
- [ ] Validate all clients have same reference data (quest lists, etc.)

---

## Late Join Scenarios

### What is Late Join?

Mirror allows players to connect while the game is already in progress. This is called "late join."

**Typical Scenario:**
1. Host + Client1 start quest, waiting at exit zone
2. Client2 connects (late)
3. **Problem**: Is Client2's game state synchronized?

**Concrete Example:**
```
Time T0: Host selects quest
  → onQuestSelected.RaiseEvent() [EventChannel fires]
  → ExitZone.isQuestSelected = true

Time T1: Client2 joins (late)
  → ExitZone.isQuestSelected = false ❌
  → Countdown won't start even when everyone enters zone
```

---

## State Synchronization Methods

| Method | Late Join Support | Use Case | Example |
|--------|-------------------|----------|---------|
| **SyncVar** | ✅ Auto-sync | Persistent game state | Quest status, player HP, score |
| **ClientRpc** | ❌ Past calls not re-sent | Temporary events | Effects, sounds, temporary UI |
| **EventChannel alone** | ❌ Past events not re-fired | Local communication | Same-client component communication |
| **EventChannel + SyncVar** | ✅ Auto-sync | EventChannel decoupling + state management | **Quest selection (recommended)** |

**Important**: EventChannelSO (ScriptableObjects) are NOT network-synced. To share state over network, combine with SyncVar or ClientRpc.

---

## Recommended Pattern: EventChannel + SyncVar

Maintain EventChannel decoupling while supporting late join.

```csharp
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using Tang3cko.EventChannels;

namespace ProjectName.Quest
{
    /// <summary>
    /// Quest state network synchronization (late join support)
    /// </summary>
    [RequireComponent(typeof(NetworkIdentity))]
    public class QuestStateNetworkController : NetworkBehaviour
    {
        [Header("Event Channels")]
        [SerializeField] private QuestEventChannelSO onQuestSelected;

        [Header("Available Quests")]
        [SerializeField] private List<QuestSO> availableQuests;

        /// <summary>
        /// SyncVar: Late-joining clients automatically receive current value
        /// Hook: Fires EventChannel on clients
        ///
        /// Important: Cannot sync ScriptableObject directly, so sync questID
        /// </summary>
        [SyncVar(hook = nameof(OnQuestIDChanged))]
        private string selectedQuestID = "";

        private void OnEnable()
        {
            // Subscribe to local EventChannel
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised += HandleQuestSelectedLocal;
        }

        private void OnDisable()
        {
            if (onQuestSelected != null)
                onQuestSelected.OnEventRaised -= HandleQuestSelectedLocal;
        }

        /// <summary>
        /// Local EventChannel handler
        /// Only server updates SyncVar
        /// </summary>
        private void HandleQuestSelectedLocal(QuestSO quest)
        {
            // Server only
            if (!NetworkServer.active) return;

            Debug.Log($"[QuestStateNetworkController] Server updating selectedQuestID: {quest.questID}");

            // Update SyncVar (auto-syncs to all clients)
            selectedQuestID = quest.questID;

            // Server: Hook doesn't fire on server, so fire EventChannel manually
            BroadcastQuestSelection(quest);
        }

        /// <summary>
        /// SyncVar hook: Called on clients when quest changes
        ///
        /// Note: This hook does NOT fire on server
        /// (Server directly writes selectedQuestID)
        ///
        /// This hook fires when:
        /// 1. Existing clients: When SyncVar changes
        /// 2. Late-joining clients: When receiving initial value
        /// </summary>
        private void OnQuestIDChanged(string oldID, string newID)
        {
            Debug.Log($"[QuestStateNetworkController] Client received quest selection: {oldID} → {newID}");

            if (string.IsNullOrEmpty(newID)) return;

            // Find QuestSO by questID
            QuestSO quest = availableQuests.Find(q => q.questID == newID);
            if (quest == null)
            {
                Debug.LogError($"[QuestStateNetworkController] Quest not found: {newID}");
                return;
            }

            // Fire EventChannel on client
            BroadcastQuestSelection(quest);
        }

        /// <summary>
        /// Fire EventChannel (common for server and clients)
        /// </summary>
        private void BroadcastQuestSelection(QuestSO quest)
        {
            Debug.Log($"[QuestStateNetworkController] Broadcasting quest selection: {quest.questName}");

            // Fire EventChannel (decoupled)
            // All components subscribing to this EventChannel are notified
            // (e.g., ExitZoneTrigger, UI, etc.)
            onQuestSelected?.RaiseEvent(quest);
        }
    }
}
```

---

## Flow Diagram

### Existing Clients

```
1. Host: QuestBoardUI.AcceptQuest()
   → onQuestSelected.RaiseEvent() [Local fire]

2. Host: HandleQuestSelectedLocal()
   → selectedQuestID = "quest_001" [SyncVar update]

3. All Clients: OnQuestIDChanged("", "quest_001")
   → onQuestSelected.RaiseEvent() [Fire on all clients]
```

### Late-Joining Client (Joins at T1)

```
1. Connects to server
2. Automatically receives SyncVar current value "quest_001"
3. OnQuestIDChanged("", "quest_001") hook fires
4. onQuestSelected.RaiseEvent()
   → Same state as existing clients ✅
```

---

## Implementation Notes

### Cannot Sync ScriptableObjects Directly

```csharp
// ❌ Bad: ScriptableObject cannot be synced
[SyncVar]
private QuestSO selectedQuest;

// ✅ Good: Sync questID, convert in hook
[SyncVar(hook = nameof(OnQuestIDChanged))]
private string selectedQuestID;

private void OnQuestIDChanged(string oldID, string newID)
{
    QuestSO quest = availableQuests.Find(q => q.questID == newID);
    // Use quest
}
```

### Server Hook Doesn't Fire

```csharp
private void HandleQuestSelectedLocal(QuestSO quest)
{
    if (!NetworkServer.active) return;

    // Update SyncVar
    selectedQuestID = quest.questID;

    // Server: Hook doesn't fire, so fire EventChannel manually
    BroadcastQuestSelection(quest);
}
```

### availableQuests Consistency

All clients must have the same questID → QuestSO mapping.

```csharp
// QuestBoardUI and QuestStateNetworkController must reference same list
[SerializeField] private List<QuestSO> availableQuests;
```

---

## Disconnect Handling

### Player Disconnect Detection

```csharp
using Mirror;

namespace ProjectName.Network
{
    public class RoomPlayerController : NetworkRoomPlayer
    {
        [SerializeField] private RoomPlayerRuntimeSetSO roomPlayerSet;

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Add to RuntimeSet on connect
            roomPlayerSet?.Add(this);
        }

        public override void OnStopClient()
        {
            // Remove from RuntimeSet on disconnect
            roomPlayerSet?.Remove(this);
            base.OnStopClient();
        }
    }
}
```

### Host Disconnect Handling

In Mirror, when the host (server) disconnects, **all clients are forcibly disconnected**.

```csharp
using Mirror;

namespace ProjectName.Quest
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class MainQuestManager : NetworkBehaviour
    {
        [SerializeField] private QuestProgressSO questProgress;
        [SerializeField] private QuestResultSO questResult;

        public override void OnStopServer()
        {
            // Host disconnect handling
            // Save partial result if quest is in progress
            if (questProgress.state == QuestState.InProgress)
            {
                Debug.LogWarning("[MainQuestManager] Host disconnected during quest. Saving partial result.");
                questResult.SetResult(questProgress.currentQuest, false, questProgress.currentCount);
            }

            base.OnStopServer();
        }
    }
}
```

---

## Late Join Checklist

### States Requiring Late Join Support

- [ ] Quest selected flag → SyncVar + EventChannel
- [ ] Player Ready state → SyncVar (NetworkRoomPlayer standard)
- [ ] Game state (preparing/in-progress/completed) → SyncVar
- [ ] Player HP, score, etc. → SyncVar

### Events NOT Requiring Late Join Support

- [ ] Effect playback → ClientRpc
- [ ] Sound playback → ClientRpc
- [ ] Temporary notifications/messages → ClientRpc
- [ ] Animation triggers → ClientRpc

### Disconnect Handling

- [ ] RuntimeSet Add/Remove implementation (OnStartClient/OnStopClient)
- [ ] Proper cleanup in OnStopServer (host disconnect)
- [ ] Determine if game can continue after disconnect (all required vs some optional)

---

## Test Scenarios

| Test Case | Steps | Expected Result |
|-----------|-------|-----------------|
| LJ-001: Late join (before quest selection) | 1. Host starts<br>2. Client1 connects<br>3. Client2 connects | All see same lobby, quest not selected |
| LJ-002: Late join (after quest selection) | 1. Host selects quest<br>2. Client2 connects | Client2 receives quest selection, Exit Zone enabled |
| LJ-003: Late join (during quest) | 1. Quest starts<br>2. Client2 connects | Client2 sees quest progress (requires QuestProgress SyncVar) |
| DC-001: Client disconnect | 1. During quest, Client1 disconnects | Removed from RuntimeSet, remaining players continue |
| DC-002: Host disconnect | 1. During quest, Host disconnects | All players forced to lobby, partial result saved |

---

## References

- [Mirror Basics](basics.md)
- [Server Authority](server-authority.md)
- [SyncVar and ClientRpc](syncvar-clientrpc.md)
- [Interactable Pattern](interactable-pattern.md)
- [RuntimeSet Pattern](../../architecture/runtime-sets.md)

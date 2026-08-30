# NGO Sync Mechanisms

Full comparison of every official sync mechanism NGO provides, plus when to use each.

**Topology scope**: everything below is written for the classic client-server (host) topology. NGO 2.x also ships a distributed authority topology (Unity Multiplayer Services session, one client is session owner, authority follows `NetworkObject` ownership). The mechanisms and size classes are the same there, but "server-authoritative" reads as "authority-client-authoritative": write permissions default to owner, and validation must happen on whichever peer holds authority — there is no central server to trust. Tang3cko projects default to client-server; adopting distributed authority is an architecture decision, not a mechanism swap.

---

## Decision Tree

```
What are you syncing?
|
+-- One-shot event (no replay on join)
|   +-- Frequency < 5 Hz, size < 1 KB    -> Reliable Rpc
|   +-- Frequency >= 5 Hz                 -> reconsider, you probably want a NetworkVariable or snapshot stream
|
+-- Persistent state (late-joiners need current value)
|   +-- Single value / small struct        -> NetworkVariable<T>
|   +-- Collection                         -> NetworkList<T> or NetworkVariable<NetworkSerializable>
|   +-- Transform                          -> NetworkTransform (with quantization)
|
+-- High-frequency entity stream (>10 Hz, many entities, loss-tolerant)
|   +-- always                             -> Chunked Unreliable Rpc + sequence reassembly
|                                            (see snapshot-streaming.md)
|
+-- Bulk one-shot transfer (config, save, large struct)
|   +-- < 64 KB                            -> CustomMessagingManager Reliable
|   +-- >= 1 MB                            -> Out-of-band (HTTP / CDN), trigger via Rpc
|
+-- Late-joiner state that NetworkVariable can't express
    +-- always                             -> NetworkBehaviour.OnSynchronize (NGO 2.x)
```

---

## Mechanism Table

| Mechanism | Delivery | Fragments? | Size class | Late-join | Use for |
|---|---|---|---|---|---|
| `NetworkVariable<T>` | Reliable | Yes | up to transport limit | Yes (auto) | Persistent state with change events |
| `NetworkList<T>` | Reliable | Yes | up to transport limit | Yes (auto) | Persistent collections |
| `NetworkTransform` | Reliable (built-in delta + quantize) | Yes | small (internally optimized) | Yes (auto) | Transform replication |
| `Rpc` / `ClientRpc` / `ServerRpc` (Reliable) | Reliable | Yes | <= ~64 KB initial buffer | No | One-shot events, triggers |
| `Rpc` (Unreliable) | Unreliable | **No** — throws `OverflowException` past 1296 B (`MaximumTransmissionUnitSize & ~7`); `MaxPayloadSize` is never reached | **Design budget <= 1200 B** (one MTU) | No | High-frequency loss-tolerant signals |
| `CustomMessagingManager.SendNamedMessage` (Reliable) | Reliable | Yes | up to transport limit | No | Bulk transfer, custom protocols |
| `CustomMessagingManager.SendUnnamedMessage` (Reliable) | Reliable | Yes | up to transport limit | No | Minimal-overhead custom protocols |
| `NetworkBehaviour.OnSynchronize` (NGO 2.x) | Reliable | Yes | up to transport limit | Yes (spawn) | Custom late-join serialization |
| `NetworkSceneManager` | Reliable | Yes | internally chunked | Yes | Scene-level sync |
| Out-of-band (HTTP / CDN) | TCP / HTTPS | N/A | unlimited | N/A | Replays, saves, user content |

---

## NetworkVariable

**When**: persistent state that late-joiners must see. Single value or small struct.

```csharp
// Good: small, infrequently changed state
public NetworkVariable<int> Score = new(writePerm: NetworkVariableWritePermission.Server);
public NetworkVariable<MatchPhase> Phase = new(writePerm: NetworkVariableWritePermission.Server);

// Bad: high-frequency churn
public NetworkVariable<Vector3> Position = new(); // use NetworkTransform instead
public NetworkVariable<EnemyState[]> AllEnemies = new(); // use snapshot stream instead
```

Subscribe via `OnValueChanged`. Initial value is replicated automatically on spawn (late-join).

---

## NetworkList

**When**: collection that must be late-join replicated and changes are observable.

```csharp
public NetworkList<int> ConnectedClientIds;

void Awake() {
    ConnectedClientIds = new NetworkList<int>(
        writePerm: NetworkVariableWritePermission.Server);
}
```

Adding 1 element sends a single delta event, not the whole list. But replacing the whole list at 20 Hz still creates a Reliable storm.

---

## NetworkTransform

**When**: GameObject transform.

Always tune for what you actually need:

```csharp
// Boss Room precedent: half precision + Y-axis disable = 5 B saved per packet
[SerializeField] bool useHalfFloatPrecision = true;
[SerializeField] bool syncPositionY = false;
```

Disable axes that don't change. Disable rotation axes you don't use.

---

## Reliable RPC

**When**: one-shot events. UI triggers, hit confirmations, ability activations, score increments.

```csharp
[Rpc(SendTo.NotServer)]
private void PlayHitVisualRpc(Vector3 impactPosition, int hitPlayerId) { ... }
```

Default is Reliable. Auto-fragments. Don't use for high-frequency streams - reliable retransmit will form a storm under loss.

---

## Unreliable RPC

**When**: high-frequency loss-tolerant signals. Snapshot streams, heartbeats.

```csharp
[Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
private void SyncSnapshotChunkRpc(int seq, byte idx, byte total, ...) { ... }
```

**Design constraint**: one MTU per message (Rule 1 in SKILL.md; derivation and loss math in `rpc-payload-rules.md`). For variable-size payloads, see `snapshot-streaming.md` for the chunking pattern.

---

## CustomMessagingManager (Named / Unnamed)

**When**: bulk transfer that doesn't fit the RPC code-gen contract. Custom binary protocols. Pre-allocated `FastBufferWriter` reuse.

```csharp
// Send
var writer = new FastBufferWriter(payloadSize, Allocator.Temp);
writer.WriteValueSafe(myStruct);
NetworkManager.CustomMessagingManager.SendNamedMessage(
    "MyMessageName", clientId, writer, NetworkDelivery.ReliableFragmentedSequenced);

// Receive
NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(
    "MyMessageName", (senderId, reader) => {
        reader.ReadValueSafe(out MyStruct value);
        // ...
    });
```

Unnamed is faster (no string hash). Use named when human-readable channel naming aids debugging.

---

## OnSynchronize (NGO 2.x)

**When**: late-joiners need custom-serialized state that NetworkVariable cannot express.

```csharp
protected override void OnSynchronize<T>(ref BufferSerializer<T> serializer) {
    serializer.SerializeValue(ref customState);
}
```

Runs on spawn for each connecting client. Reliable, fragmented. Avoid heavy work here.

---

## Out-of-band Transfer

**When**: megabyte-scale content. Replays, saves, user-generated assets, audio clips.

Pattern:

1. Client A uploads to CDN / object storage
2. A sends URL via Reliable Rpc
3. Client B downloads via HTTP
4. B confirms via Reliable Rpc

Never push this through NGO's pipeline.

---

## Checklist

- [ ] The chosen mechanism came out of the decision tree above — if you picked one first and rationalized after, re-run the tree
- [ ] Late-join requirement checked explicitly: RPCs and Custom Messages do NOT replay for late joiners; persistent state uses NetworkVariable/NetworkList/OnSynchronize
- [ ] Nothing high-frequency (>10 Hz) rides on a Reliable mechanism (NetworkVariable churn, Reliable RPC stream)
- [ ] Transform sync uses NetworkTransform with unused axes disabled and half precision considered, not a hand-rolled NetworkVariable<Vector3>
- [ ] Anything approaching megabyte scale goes out-of-band (HTTP/CDN), with NGO used only for coordination
- [ ] If the project uses distributed authority topology, every "server validates" assumption was re-checked against ownership-based authority

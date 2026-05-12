# NGO Sync Mechanisms

Full comparison of every official sync mechanism NGO provides, plus when to use each.

---

## Mechanism Table

| Mechanism | Delivery | Fragments? | Size class | Late-join | Use for |
|---|---|---|---|---|---|
| `NetworkVariable<T>` | Reliable | Yes | up to transport limit | Yes (auto) | Persistent state with change events |
| `NetworkList<T>` | Reliable | Yes | up to transport limit | Yes (auto) | Persistent collections |
| `NetworkTransform` | Reliable (built-in delta + quantize) | Yes | small (internally optimized) | Yes (auto) | Transform replication |
| `Rpc` / `ClientRpc` / `ServerRpc` (Reliable) | Reliable | Yes | <= ~64 KB initial buffer | No | One-shot events, triggers |
| `Rpc` (Unreliable) | Unreliable | **No** | **<= 1200 B** | No | High-frequency loss-tolerant signals |
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

**Hard constraint**: payload must fit MTU (~1200 B safe). NGO does NOT fragment Unreliable RPCs. Overflow throws `OverflowException` at send time.

For variable-size payloads, see `snapshot-streaming.md` for chunking pattern.

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

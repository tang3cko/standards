# Anti-Patterns

Failure modes seen in real Tang3cko projects. Each entry: symptom -> root cause -> fix.

---

## 1. Unreliable RPC with Unbounded Array

**Symptom**: works in early playtests, throws `OverflowException: RPC parameters are too large for unreliable delivery` once entity count crosses some threshold.

```csharp
// BAD
[Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
private void SyncStatesRpc(EnemyState[] states, int[] ids, int count) { ... }
```

**Root cause**: array grows beyond MTU; Unreliable does not fragment.

**Fix**: chunk into MTU-safe slices with sequence reassembly. See `snapshot-streaming.md`.

---

## 2. NetworkVariable Churn at High Frequency

**Symptom**: bandwidth balloons, client appears to hang under any packet loss, NetworkManager auto-disconnects after a few seconds.

```csharp
// BAD: rewriting a big variable every tick
public NetworkVariable<EnemyState[]> AllEnemies = new();

void Update() {
    if (IsServer) AllEnemies.Value = BuildSnapshot(); // 20 Hz
}
```

**Root cause**: NetworkVariable is Reliable. Each write retransmits until acked. At 20 Hz the queue accumulates faster than it drains.

**Fix**: NetworkVariable is for state that changes occasionally. For high-frequency entity arrays, use a chunked snapshot stream and let NetworkVariable / OnSynchronize handle initial state only.

---

## 3. Reliable RPC Stream

**Symptom**: same as #2 - hangs under loss.

```csharp
// BAD
[Rpc(SendTo.NotServer)] // Reliable by default
private void SyncSnapshotRpc(EnemyState[] states) { ... }

void Update() {
    if (TickFinished()) SyncSnapshotRpc(allEnemies); // 20 Hz, Reliable
}
```

**Root cause**: same retransmit-storm mechanic as #2.

**Fix**: use Unreliable + chunked snapshot stream.

---

## 4. Forgetting Worst Case at Design Time

**Symptom**: code passes review, ships, breaks at content scale-up.

**Root cause**: nobody computed payload size at `maxN`.

**Fix**: for every RPC with array parameters, compute and document worst-case payload in a comment near the declaration:

```csharp
// Worst case: 200 entities * 24 B + 200 * 4 B + framing ~= 5.7 KB
// EXCEEDS unreliable MTU - must chunk
[Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
private void SyncStatesRpc(...) { ... }
```

If your worst-case calc shows overflow, the design is wrong. Fix it before merging.

---

## 5. Caching Network-Authoritative State Locally

**Symptom**: state divergence after disconnect / scene transition / late-join. Stale data drives subsequent decisions.

```csharp
// BAD: tracker holds its own copy
private string loadedNetworkSceneName;
private bool isNetworkSceneLoaded;

public void OnSceneLoaded(string name) {
    loadedNetworkSceneName = name; // duplicated source of truth
    isNetworkSceneLoaded = true;
}
```

**Root cause**: two sources of truth (local cache + actual NGO / SceneManager state). They diverge on edge cases.

**Fix**: query the authoritative system on demand. Inject as a delegate for testability.

```csharp
// GOOD
private readonly Func<string> getCurrentSceneName;
public string CurrentSceneName => getCurrentSceneName();
```

---

## 6. Megabyte Payload Through NGO

**Symptom**: connection times out, transport errors, late joiners disconnect.

**Root cause**: NGO's pipeline is built for game state, not bulk transfer.

**Fix**: out-of-band transfer via HTTP / CDN. Use NGO only to coordinate (send URL, ack receipt).

---

## 7. Quantization Leaking Into Gameplay Code

**Symptom**: gameplay code is full of `* 100`, `/ 100`, `(short)`, hard to read, easy to forget conversion.

**Root cause**: quantized wire types used as gameplay types.

**Fix**: quantize only at the network boundary. Gameplay uses `Vector3` / `float`. Wire format uses packed structs. Conversion lives in a dedicated codec class.

---

## 8. Trusting Client-Provided Sizes / Indices

**Symptom**: server crashes when malicious client sends garbage RPC.

```csharp
// BAD
[Rpc(SendTo.Server)]
private void ApplyDamageRpc(int requestedDamage, int targetId) {
    enemies[targetId].Health -= requestedDamage; // unbounded
}
```

**Root cause**: trusting client input.

**Fix**: server reads damage from its own config (AttackConfigSO), validates target exists, ignores client-provided magnitudes.

---

## 9. Mixed Reliability Through Same Code Path

**Symptom**: snapshot updates arrive out of order with their corresponding event RPCs. Visual glitches.

**Root cause**: separate Reliable and Unreliable channels don't preserve relative ordering.

**Fix**: keep cause and effect on the same delivery, or include sequence numbers and reconcile on the client.

# RPC Payload Rules

Hard rules for RPC payload sizing. The cost of getting this wrong is `OverflowException` at runtime or Reliable retransmit storms.

---

## MTU Reality

Unity Transport's UDP MTU is **1400 B** (`NetworkParameterConstants.MTU`). After NGO framing (RPC method hash, NetworkObjectId, NetworkBehaviourIndex, length prefixes) the **single-packet payload budget is ~1200 B per Unreliable message**.

NGO 2.x builds its Unreliable pipelines with a fragmentation stage: messages larger than one MTU are split across UDP packets, up to `UnityTransport.MaxPayloadSize` (**default 6144 B**, configurable). Past that cap the send errors out.

| Delivery | Limit | What happens past the design budget |
|---|---|---|
| Unreliable RPC | Design budget ~1200 B (one MTU); hard cap `MaxPayloadSize` (6144 B default) | Fragments across packets — one lost fragment drops the whole message. Send error above the hard cap |
| Unreliable Custom Message | Same | Same |
| Reliable RPC | ~64 KB initial buffer, auto-fragments to transport limit | Slower under loss, retransmit queue grows |
| Reliable Custom Message | Same as Reliable RPC | Same |

---

## Why You Still Budget One MTU

NGO 2.x *can* fragment Unreliable, but fragmentation on an unreliable channel compounds loss instead of hiding it:

- A message split into `k` fragments survives only if ALL `k` packets arrive: at 5% packet loss, a 5-fragment message loses **~23%** of sends
- Reassembly adds receive-side latency (must wait for the last fragment) and buffering on lossy links
- Loss becomes bursty and entity-count-dependent — exactly the failure mode that only appears at scale

So treat ~1200 B as the design budget for anything high-frequency; use the 1200 B–6 KB fragmented range only for low-rate loss-tolerant messages where a dropped message is genuinely free.

**NGO 1.x note**: 1.x did not fragment Unreliable at all — exceeding one MTU threw `OverflowException` at send time. On 1.x projects the 1200 B budget is a hard crash boundary.

---

## Computing Worst-Case Payload

For any RPC with array parameters, before merging:

```
worst_case = max_array_length * sizeof(element)
           + 4 (array length prefix)
           + sum(other_params)
           + ~40 (NGO framing)
```

Examples:

```
SyncSnapshotRpc(EnemyState[] states, int[] ids, int count)
  where EnemyState = 24 B, max count = 200

  payload = 200 * 24 + 200 * 4 + 4 + 4 + 4 + 4 + 40
          = 4800 + 800 + 16 + 40
          = 5656 B  -> 5 fragments on Unreliable; ~23% snapshot loss at 5% packet loss
```

If this is Unreliable, the design is broken at maxN. Fix at design time, not after shipping.

---

## When You're Over Budget

Decision order:

1. **Can the payload shrink?** Quantize floats, drop fields the receiver can derive, pack flags into bits.
2. **Can the channel change?** If the data must arrive in order and persists, use NetworkVariable. If it's a one-shot event, use Reliable RPC.
3. **Can you chunk it?** If it's a snapshot stream that's loss-tolerant, chunk it across Unreliable RPCs with sequence reassembly. See `snapshot-streaming.md`.

Do not "just switch to Reliable" for 20 Hz streams - reliable retransmit at that frequency under packet loss causes head-of-line blocking and client hangs.

---

## Quantization Cheatsheet

| Original | Range | Quantized | Bytes | Precision |
|---|---|---|---|---|
| `float` position component | +/- 327 m | `short / 100` | 2 | 0.01 m |
| `float` position component | +/- 32 m | `short / 1000` | 2 | 0.001 m |
| `float` rotation (deg) | 0-360 | `byte` | 1 | 1.4 deg |
| `float` health | 0-65535 | `ushort` | 2 | 1 HP |
| `float` health | 0-255 | `byte` | 1 | 1 HP |
| `float` velocity component | +/- 32 m/s | `short / 1000` | 2 | 0.001 m/s |
| Quaternion | unit length | smallest-three: largest index + 3 components | 4 | sub-degree |
| Boolean flags (up to 8) | true/false | packed `byte` bit mask | 1 | exact |

Convert at the network boundary, keep gameplay code in `float` / `Vector3`.

---

## Reliable Stream Anti-pattern

This pattern looks innocent but causes hangs at scale:

```csharp
// BAD: Reliable RPC at high frequency
[Rpc(SendTo.NotServer)] // Reliable by default
private void SyncEntitiesRpc(EnemyState[] states) { ... }

void Update() {
    if (TickFinished()) SyncEntitiesRpc(allEnemies); // 20 Hz
}
```

Under 5% packet loss:
- Each lost packet triggers retransmit
- The retransmit queue grows
- New ticks pile up behind un-acked packets
- Effective latency climbs to seconds
- Client appears hung

Use Unreliable + chunked snapshot stream for this profile.

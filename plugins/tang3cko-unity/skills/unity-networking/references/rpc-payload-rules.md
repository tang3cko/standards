# RPC Payload Rules

Hard rules for RPC payload sizing. The cost of getting this wrong is a send-time `OverflowException` on Unreliable, silent burst loss where fragmentation *is* in play, or Reliable retransmit storms.

---

## MTU Reality

Unity Transport's UDP MTU is **1400 B** (`NetworkParameterConstants.MTU`). After NGO framing (RPC method hash, NetworkObjectId, NetworkBehaviourIndex, length prefixes) the **single-packet payload budget is ~1200 B per Unreliable message**.

**An Unreliable RPC that exceeds one MTU does not fragment — it throws.** NGO derives its own non-fragmented ceiling from `NetworkManager.MaximumTransmissionUnitSize` (default **1300**), rounded down to an 8-byte boundary: `NetworkMessageManager.DefaultNonFragmentedMessageMaxSize` = `1300 & ~7` = **1296 B**. Past that, `NetworkBehaviour` throws `OverflowException("RPC parameters are too large for unreliable delivery.")` at send time.

`UnityTransport.MaxPayloadSize` (default 6144 B) is a **different and higher** limit that Unreliable RPCs never reach: NGO's own 1296 B ceiling stops the send first. Sizing an Unreliable payload against 6144 B is sizing against the wrong number.

Custom Messages follow the same limits as RPCs of the same delivery. Per-delivery budgets are Rule 1 in SKILL.md; this file carries the derivation and how to compute against it.

> **Measured, not inferred** (2026-08-10): NGO 2.11.0 + `com.unity.transport` 2.6.0 over Relay, verified against the package source in `Library/PackageCache`. Surfaced in Multiplayer_009 as a real bug — 6 of 10 snapshot packets dropping at 77 entities. This is a version- and transport-specific measurement: re-verify `DefaultNonFragmentedMessageMaxSize` and the `NetworkBehaviour` send path before trusting these exact bytes on another NGO version.
>
> This corrects an earlier version of this standard, which claimed NGO 2.x fragments Unreliable up to `MaxPayloadSize` (6144 B). It does not.

---

## Why You Still Budget One MTU

The 1296 B ceiling is the hard boundary, but the **design** budget is ~1200 B, for two separate reasons.

**Below the ceiling, the margin is your framing.** 1296 B is what NGO will accept, not what your payload may occupy — the difference absorbs NGO framing and leaves slack for a field added later. A design that sits at 1290 B breaks the next time anyone adds an `int`.

**Above the ceiling, you don't get graceful degradation.** There is no fragmented middle ground to fall back on: the send throws and the message is simply gone. Under Relay this surfaces as a *partial* failure — some ticks fit, some don't — which reads as packet loss rather than as a size bug.

Where fragmentation *does* apply (Reliable delivery, or Unreliable after raising `MaximumTransmissionUnitSize`), it compounds loss instead of hiding it:

- A message split into `k` fragments survives only if ALL `k` packets arrive: at 5% packet loss, a 5-fragment message loses **~23%** of sends
- Reassembly adds receive-side latency (must wait for the last fragment) and buffering on lossy links
- Loss becomes bursty and entity-count-dependent — exactly the failure mode that only appears at scale

So treat ~1200 B as the design budget for anything high-frequency, and treat 1296 B as a wall rather than a soft limit.

**NGO 1.x note**: 1.x also did not fragment Unreliable — exceeding one MTU threw `OverflowException` at send time. On this point 1.x and 2.11 behave the same way.

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
          = 5656 B  -> 4.4x over NGO's 1296 B Unreliable ceiling
                       -> OverflowException at send time, every tick, at maxN
```

If this is Unreliable, the design is broken at maxN — and it breaks by **throwing**, not by degrading. Note where it starts breaking: this payload is `28N + 56` B, so at 44 entities it is 1288 B and ships fine; at 45 it is 1316 B and throws. A sync site that passes every test at `N=10` can be one entity away from failing entirely. Fix at design time, not after shipping.

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

---

## Checklist

- [ ] Worst-case payload computed with the formula above (array at max length + prefixes + ~40 B framing) for every RPC with array or variable-size parameters
- [ ] The computed worst case is documented in a comment at the RPC declaration, including the verdict (fits / throws) and the `maxN` it was computed at
- [ ] Every Unreliable send site fits ~1200 B at `maxN` — **not** 6144 B. `UnityTransport.MaxPayloadSize` is not the Unreliable ceiling; NGO throws at `MaximumTransmissionUnitSize & ~7` (1296 B by default) first
- [ ] The `maxN` at which the payload crosses 1296 B is known and is comfortably above the design maximum — the failure is a thrown send, not degraded delivery
- [ ] Over-budget sites were resolved in the decision order (shrink → change channel → chunk), not by silently switching a 20 Hz stream to Reliable
- [ ] All quantization uses the cheatsheet ranges (value range fits the chosen integer type at the chosen scale) and converts only at the network boundary

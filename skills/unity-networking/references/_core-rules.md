# Core Rules (Auto-loaded)

Essential NGO networking rules. Apply these to every sync site.

---

## Rule 1: Payload Size Budget

| Delivery | Per-message budget | Enforcement |
|---|---|---|
| **Unreliable** RPC / Named / Unnamed Message | **<= 1200 B** | Runtime `OverflowException` if exceeded |
| **Reliable** RPC / Named / Unnamed Message | <= ~64 KB initial buffer, fragments to transport limit | Auto-fragmented, but retransmit storms above ~4 KB at high frequency |
| NetworkVariable / NetworkList | Reliable-fragmented, no hard ceiling | Same retransmit concern when churning |
| NetworkTransform | Internally quantized + delta | Use half-precision + axis-disable to shrink |

**Hard rule**: any sync site that exceeds 1200 B on an Unreliable channel is a bug. Either shrink (quantize / split) or change channel.

---

## Rule 2: Mechanism Decision Tree

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

## Rule 3: Frequency vs Persistence Matrix

| Frequency | Persistent (needs late-join) | Transient |
|---|---|---|
| Low (<= 1 Hz) | NetworkVariable | Reliable Rpc |
| Medium (1-10 Hz) | NetworkVariable | Reliable Rpc (watch retransmit) |
| **High (>10 Hz)** | **Avoid NetworkVariable. Use snapshot stream + initial sync via NetworkVariable / OnSynchronize** | **Unreliable chunked snapshot stream** |

NetworkVariable at high frequency causes Reliable retransmit storms when packet loss occurs - the loop appears as client hang.

---

## Rule 4: Forbidden Patterns

- Unreliable RPC with dynamically-sized array of unbounded length
- NetworkVariable<LargeStruct> rewritten every frame
- Reliable RPC stream at 20+ Hz with payload > 1 KB
- Sending megabytes through NGO (use HTTP/CDN)
- Quantization done by callers (use a typed wrapper, not raw shorts everywhere)

---

## Rule 5: Always Test Worst Case

A sync site that works at `N=10` entities can throw `OverflowException` at `N=50`. When designing:

1. Define `maxN` upfront
2. Compute worst-case payload at `maxN`
3. If `> 1200 B` and channel is Unreliable, fix the design now

---
name: unity-networking
description: Design and review Unity multiplayer networking on NGO (Netcode for GameObjects) 2.x following Tang3cko standards — payload sizing, reliable vs unreliable delivery, snapshot streaming, NetworkVariable vs RPC vs CustomMessage selection. Use when designing network sync, debugging dropped or oversized messages, or reviewing bandwidth.
when_to_use: Triggers on "NGO", "Netcode", "RPC", "NetworkVariable", "ClientRpc", "ServerRpc", "snapshot", "同期", "ネットワーク", "MTU", "OverflowException", "fragmentation", "bandwidth", "帯域".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Networking Standards

Tang3cko's standards for Unity multiplayer networking on NGO (Netcode for GameObjects) 2.x —
payload sizing, delivery selection, snapshot streaming. The Core Rules below are always in force;
the derivations, code patterns, and incident write-ups live in references/.

The rule skipped most often is Rule 4. A sync site that works at `N=10` starts throwing at
`N=50`, so the worst case at `maxN` gets computed and written at the RPC declaration — before the
code ships, not after a player reports a hang.

## Phase Tracking

**At workflow start (designing, debugging, or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Pick mechanism      | Mechanism Selection Guide + load its reference"
TaskCreate: "Phase 2: Verify payload      | worst-case size at maxN vs Rule 1 budget"
TaskCreate: "Phase 3: Write/review code   | follow Core Rules"
TaskCreate: "Phase 4: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Mechanism-choice questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 2 | Compute worst-case payload at `maxN` | Any new/changed sync site |
| 4 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

These rules apply to every sync site. They are the standard — do not generate or approve network code that violates them.

### Rule 1: Payload Size Budget

| Delivery | Per-message budget | Enforcement |
|---|---|---|
| **Unreliable** RPC / Named / Unnamed Message | **Design budget <= 1200 B** (one MTU). Hard cap = `NetworkManager.MaximumTransmissionUnitSize & ~7` (**1296 B** by default) | `OverflowException` at send time above the cap. **No fragmentation** — the message is not split, it is refused |
| **Reliable** RPC / Named / Unnamed Message | <= ~64 KB initial buffer, fragments to transport limit | Auto-fragmented, but retransmit storms above ~4 KB at high frequency |
| NetworkVariable / NetworkList | Reliable-fragmented, no hard ceiling | Same retransmit concern when churning |
| NetworkTransform | Internally quantized + delta | Use half-precision + axis-disable to shrink |

Any Unreliable sync site should fit one MTU (~1200 B payload; UTP MTU is 1400 B). Either shrink (quantize / split) or change channel.

**`UnityTransport.MaxPayloadSize` (default 6144 B) is not the Unreliable ceiling.** NGO's own non-fragmented limit — `NetworkMessageManager.DefaultNonFragmentedMessageMaxSize`, `1300 & ~7` = 1296 B — stops the send first, with `OverflowException("RPC parameters are too large for unreliable delivery.")`. Verified against the NGO 2.11.0 / `com.unity.transport` 2.6.0 package source on 2026-08-10; re-verify on other versions. Earlier revisions of this standard described 2.x as fragmenting Unreliable up to 6144 B — that was wrong, and it cost Multiplayer_009 a real bug (6 of 10 snapshot packets lost at 77 entities over Relay). See `references/rpc-payload-rules.md`.

### Rule 2: Frequency × Persistence Matrix

| Frequency | Persistent (needs late-join) | Transient |
|---|---|---|
| Low (<= 1 Hz) | NetworkVariable | Reliable Rpc |
| Medium (1-10 Hz) | NetworkVariable | Reliable Rpc (watch retransmit) |
| **High (>10 Hz)** | **Avoid NetworkVariable. Snapshot stream + initial sync via NetworkVariable / OnSynchronize** | **Unreliable chunked snapshot stream** |

NetworkVariable at high frequency causes Reliable retransmit storms under packet loss — visible as client hang.

### Rule 3: Forbidden Patterns

- Unreliable RPC with dynamically-sized array of unbounded length
- `NetworkVariable<LargeStruct>` rewritten every frame
- Reliable RPC stream at 20+ Hz with payload > 1 KB
- Sending megabytes through NGO (use HTTP/CDN)
- Quantization done by callers (use a typed codec at the network boundary)

### Rule 4: Always Compute Worst Case

A sync site that works at `N=10` entities throws at `N=50`, once the payload passes NGO's 1296 B Unreliable ceiling. Define `maxN` upfront, compute worst-case payload at `maxN`, and document it in a comment at the RPC declaration. If it exceeds 1200 B on Unreliable, fix the design now.

## Workflow

Identify the task type and follow the matching path.

### Designing a new sync system

1. Pick the mechanism from the Mechanism Selection Guide below and load its reference
2. Verify the payload fits the size class for the chosen delivery (Rule 1)
3. Generate code following the loaded standards
4. Run the Self-Check below before presenting

### Debugging dropped messages / bandwidth issue

1. Check `anti-patterns.md` for the matching failure mode
2. Classify the issue: size budget violation, frequency × persistence mismatch, or reliable retransmission storm
3. Recommend the correct mechanism

### Reviewing network code

1. Verify each sync site against the payload size budget (Rule 1) and the frequency × persistence matrix (Rule 2)
2. Check for Forbidden Patterns (Rule 3) and missing worst-case comments (Rule 4)
3. Categorize issues by priority (must fix, should fix, nice to have)

### Choosing a mechanism

1. Load `sync-mechanisms.md` for the full comparison table and decision tree
2. Show trade-offs with code examples

## Mechanism Selection Guide

| Need | Mechanism | Reference |
|------|-----------|-----------|
| Persistent state with late-join sync | NetworkVariable / NetworkList | references/sync-mechanisms.md |
| Transform sync | NetworkTransform | references/sync-mechanisms.md |
| One-shot event (no late-join) | Reliable Rpc (`[Rpc(SendTo…)]`) | references/sync-mechanisms.md |
| High-frequency snapshot stream | Chunked Unreliable Rpc + sequence reassembly | references/snapshot-streaming.md |
| Bulk binary transfer | CustomMessagingManager (Reliable) | references/sync-mechanisms.md |
| Megabyte-scale blob | Out-of-band (HTTP/CDN) | references/sync-mechanisms.md |
| Late-join custom state | NetworkBehaviour.OnSynchronize | references/sync-mechanisms.md |

Scope: these rules assume the classic client-server (host) topology. NGO 2.x also offers a distributed authority topology (Multiplayer Services session owner, authority follows object ownership) — the size/frequency rules still apply per authority, but "server validates" becomes "authority validates"; see the scope note in `references/sync-mechanisms.md`.

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] Every Unreliable sync site has a worst-case payload comment and fits 1200 B at `maxN`
- [ ] Mechanism matches the frequency × persistence matrix (Rule 2)
- [ ] No Forbidden Pattern (Rule 3) present
- [ ] Quantization lives in a codec class at the network boundary, not in gameplay code
- [ ] Server validates client-provided values (no trusted sizes/indices/magnitudes)
- [ ] The reference files named by the workflow path you followed were actually Read — the rules above are the compressed contract; the derivations, code patterns, and failure stories live only in references/ — and each loaded reference's closing `## Checklist` was run against your output

## Reference Files

| File | Load When |
|------|-----------|
| references/sync-mechanisms.md | Choosing or comparing sync mechanisms. Only place with the full decision tree, per-mechanism code examples (NetworkVariable, NetworkList, NetworkTransform, RPC, CustomMessagingManager, OnSynchronize, out-of-band), and the distributed-authority scope note |
| references/rpc-payload-rules.md | Sizing any RPC payload or deciding delivery channel. Only place with the worst-case payload formula, the derivation of NGO's 1296 B Unreliable ceiling and why the design budget sits below it, the over-budget decision order, and the quantization cheatsheet |
| references/snapshot-streaming.md | Building or reviewing a >10 Hz entity stream. Only place with the chunk wire format, client reassembly implementation, bandwidth worked example, tuning-knob table, and the late-joiner path split |
| references/anti-patterns.md | Debugging a live failure or reviewing existing network code. Only place with symptom → root cause → fix write-ups from real Tang3cko incidents, including non-payload traps (local state caching, mixed reliability ordering, trusting client input) |

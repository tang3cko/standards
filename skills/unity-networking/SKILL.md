---
name: unity-networking
description: Design and review Unity multiplayer networking on NGO (Netcode for GameObjects) following Tang3cko standards — payload sizing, reliable vs unreliable delivery, snapshot streaming, NetworkVariable vs RPC vs CustomMessage selection. Use when designing network sync, debugging RPC overflow, or reviewing bandwidth.
when_to_use: Triggers on "NGO", "Netcode", "RPC", "NetworkVariable", "ClientRpc", "ServerRpc", "snapshot", "同期", "ネットワーク", "MTU", "OverflowException", "fragmentation", "bandwidth", "帯域".
model: sonnet
allowed-tools: Read Glob Grep Edit Write
---

# Unity Networking Standards

Help users design, implement, and review Unity multiplayer networking on NGO (Netcode for GameObjects) following Tang3cko standards.

## Core Principles

1. **Payload size is part of the design contract** - Every NGO message must fit a predictable size class
2. **Unreliable means MTU-bound** - Unreliable delivery cannot fragment, so payload must stay under ~1200B
3. **Pick the mechanism by frequency × persistence** - RPC for events, NetworkVariable for state, CustomMessage for streams

## Workflow

`references/_core-rules.md` is auto-loaded with the payload budget, delivery rules, and mechanism decision tree. Identify the task type and follow the matching path.

### Designing a new sync system

1. Pick the mechanism from the Mechanism Selection Guide below and load its reference
2. Verify the payload fits the size class for the chosen delivery
3. Generate code following the loaded standards

### Debugging overflow / bandwidth issue

1. Check `anti-patterns.md` for the matching failure mode
2. Classify the issue: size class violation, frequency × persistence mismatch, or reliable retransmission storm
3. Recommend the correct mechanism

### Reviewing network code

1. Verify each sync site against the payload size budget
2. Verify each sync site picks the right mechanism for its frequency × persistence profile
3. Categorize issues by priority (must fix, should fix, nice to have)

### Choosing a mechanism

1. Load `sync-mechanisms.md` for the full comparison table
2. Apply the decision tree from `_core-rules.md`, then show trade-offs with code examples

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

## Reference Files

| File | Use When |
|------|----------|
| references/_core-rules.md | Auto-loaded: payload budget, delivery rules, mechanism decision tree |
| references/sync-mechanisms.md | Full mechanism comparison: NetworkVariable, RPC, CustomMessage, OnSynchronize |
| references/rpc-payload-rules.md | RPC size limits, Reliable vs Unreliable, MTU details |
| references/snapshot-streaming.md | High-frequency entity snapshot: chunking, quantization, sequence reassembly |
| references/anti-patterns.md | Common failure modes: Unreliable overflow, NetworkVariable churn, reliable storm |

---
name: unity-networking
description: Design and review Unity multiplayer networking following Tang3cko standards. NGO (Netcode for GameObjects) payload sizing, reliable vs unreliable delivery, snapshot streaming, NetworkVariable vs RPC vs CustomMessage selection. Use when designing network sync, debugging RPC overflow, or reviewing bandwidth. Triggers on "NGO", "Netcode", "RPC", "NetworkVariable", "ClientRpc", "ServerRpc", "snapshot", "同期", "ネットワーク", "MTU", "OverflowException", "fragmentation", "bandwidth", "帯域".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Networking Standards

Help users design, implement, and review Unity multiplayer networking on NGO (Netcode for GameObjects) following Tang3cko standards.

## Core Principles

1. **Payload size is part of the design contract** - Every NGO message must fit a predictable size class
2. **Unreliable means MTU-bound** - Unreliable delivery cannot fragment, so payload must stay under ~1200B
3. **Pick the mechanism by frequency × persistence** - RPC for events, NetworkVariable for state, CustomMessage for streams

## When Invoked

### Step 1: Determine Task Type

- **Designing a new sync system?** -> Go to Step 2a
- **Debugging RPC overflow / bandwidth issue?** -> Go to Step 2b
- **Reviewing existing network code?** -> Go to Step 2c
- **Choosing between RPC / NetworkVariable / CustomMessage?** -> Go to Step 2d

### Step 2a: Designing a New Sync System

1. `_core-rules.md` is auto-loaded with payload size budget and mechanism rules
2. Identify the sync pattern:
   - Persistent state -> `sync-mechanisms.md` (NetworkVariable section)
   - One-shot event -> `sync-mechanisms.md` (RPC section)
   - High-frequency snapshot stream -> `snapshot-streaming.md`
   - Bulk one-shot transfer -> `sync-mechanisms.md` (CustomMessage section)
3. Verify payload fits the size class for the chosen delivery
4. Generate code following loaded standards

### Step 2b: Debugging Overflow / Bandwidth Issue

1. `_core-rules.md` is auto-loaded
2. Check `anti-patterns.md` for the matching failure mode
3. Identify whether the issue is size class violation, frequency × persistence mismatch, or reliable retransmission storm
4. Recommend the correct mechanism

### Step 2c: Reviewing Network Code

1. `_core-rules.md` is auto-loaded
2. Verify each sync site against the payload size budget
3. Verify each sync site picks the right mechanism for its frequency × persistence profile
4. Categorize issues by priority (must fix, should fix, nice to have)

### Step 2d: Choosing a Mechanism

1. Load `sync-mechanisms.md` for the full comparison table
2. Apply the decision tree from `_core-rules.md`
3. Show trade-offs with code examples

## Mechanism Selection Guide

| Need | Mechanism | Reference |
|------|-----------|-----------|
| Persistent state with late-join sync | NetworkVariable / NetworkList | references/sync-mechanisms.md |
| Transform sync | NetworkTransform | references/sync-mechanisms.md |
| One-shot event (no late-join) | Reliable Rpc / ClientRpc / ServerRpc | references/sync-mechanisms.md |
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

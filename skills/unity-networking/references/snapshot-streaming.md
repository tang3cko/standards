# Snapshot Streaming

The standard pattern for high-frequency server-authoritative entity state sync over NGO. Mirrors what DOTS Netcode for Entities does internally.

---

## When to Use

- > 10 Hz update rate
- Many entities (10s to 100s)
- Server is authoritative
- Loss is acceptable (next snapshot supersedes)
- Examples: enemy positions, projectile streams, particle attributes synced server-side

If any of these does not apply, pick another mechanism from `sync-mechanisms.md`.

---

## Architecture

```
[Server, every tick]
1. Quantize all entity state to compact form (typically 8-16 B / entity)
2. Sort or take entities in some stable order
3. Pack as many as fit a single MTU-safe packet (~30-40 entities at 16 B each)
4. Send each chunk as a separate Unreliable RPC, tagged with:
   - snapshotSeq: monotonically increasing tick counter
   - chunkIdx: 0..totalChunks-1
   - totalChunks: chunks in this snapshot
5. Continue until all entities sent, or carry leftover to next tick

[Client]
1. Receive chunks indexed by snapshotSeq
2. Accumulate chunks in a per-seq buffer
3. When all chunks of seq arrived, atomically apply the assembled snapshot
4. Discard old seq buffers (drop late chunks - next snapshot already coming)
5. Apply via single state-set restore (full replace) or via per-entity upsert
```

---

## Wire Format

```csharp
[Rpc(SendTo.NotServer, Delivery = RpcDelivery.Unreliable)]
private void SyncSnapshotChunkRpc(
    int snapshotSeq,
    byte chunkIdx,
    byte totalChunks,
    int[] entityIds,
    TState[] states,
    int count
) { /* client-side handler */ }
```

`byte` for chunk indices supports up to 8 chunks per snapshot (with a bitmask `receivedMask`), which covers ~320 entities at 40-per-chunk. If you need more, widen to `ushort`.

---

## Quantization Layer

Quantize at the boundary, not in gameplay code:

```csharp
// Compact wire form
public struct EntityStateWire : INetworkSerializeByMemcpy {
    public short PositionX;  // * 0.01 m
    public short PositionY;
    public short PositionZ;
    public byte RotationY;   // 0..255 covers 360 deg
    public byte Health;
    public byte Flags;
    public byte TypeId;
}

// Conversion helpers (in a *Calculator class, pure functions, testable)
public static class EntityStateCodec {
    public static EntityStateWire Encode(in EntityStateRuntime r) => new() {
        PositionX = (short)Mathf.Clamp(r.Position.x * 100f, short.MinValue, short.MaxValue),
        // ...
    };

    public static EntityStateRuntime Decode(in EntityStateWire w) => new() {
        Position = new Vector3(w.PositionX * 0.01f, w.PositionY * 0.01f, w.PositionZ * 0.01f),
        // ...
    };
}
```

Worst case at `maxN = 200` entities, 12 B wire state, 4 B id:
```
chunkSize = 40
chunkPayload = 40 * 12 + 40 * 4 + ~16 (header) + ~40 (framing) = 720 B  -> OK
totalChunks = ceil(200 / 40) = 5
totalBandwidth = 5 chunks * 720 B * 20 Hz = 72 KB/s/client
```

Tune `chunkSize` so worst-case payload <= 1200 B.

---

## Client Reassembly

```csharp
private struct PendingSnapshot {
    public byte totalChunks;
    public byte receivedMask;       // bit per chunk, up to 8 chunks
    public int[] assembledIds;
    public TState[] assembledStates;
    public int assembledCount;
}

private readonly Dictionary<int, PendingSnapshot> pending = new();
private int lastAppliedSeq = -1;

private void OnChunkReceived(int seq, byte idx, byte total, int[] ids, TState[] states, int count) {
    if (seq <= lastAppliedSeq) return; // stale, ignore

    if (!pending.TryGetValue(seq, out var p)) {
        p = new PendingSnapshot {
            totalChunks = total,
            assembledIds = new int[total * CHUNK_SIZE],
            assembledStates = new TState[total * CHUNK_SIZE],
        };
    }
    if ((p.receivedMask & (1 << idx)) != 0) return; // duplicate

    Array.Copy(ids, 0, p.assembledIds, idx * CHUNK_SIZE, count);
    Array.Copy(states, 0, p.assembledStates, idx * CHUNK_SIZE, count);
    p.assembledCount += count;
    p.receivedMask |= (byte)(1 << idx);
    pending[seq] = p;

    byte completeMask = (byte)((1 << total) - 1);
    if (p.receivedMask == completeMask) {
        ApplyCompleteSnapshot(p);
        lastAppliedSeq = seq;
        CleanupOldSeqs(seq);
    }
}

private void CleanupOldSeqs(int currentSeq) {
    // anything < currentSeq is dead - next snapshot already in flight
    foreach (var seq in pending.Keys.Where(k => k <= currentSeq).ToList()) {
        pending.Remove(seq);
    }
}
```

If chunks of seq N are lost, that whole snapshot is dropped. At 20 Hz the next snapshot is 50 ms away - imperceptible.

---

## Late-Joiner Sync

Periodic snapshot streams cover steady-state. Late joiners need full state immediately:

- Send a one-shot **full snapshot** via Reliable Custom Message on connect (or NetworkBehaviour.OnSynchronize)
- After that, periodic Unreliable chunks take over

Don't conflate the two paths.

---

## Tuning Knobs

| Knob | Range | Default | Effect |
|---|---|---|---|
| `CHUNK_SIZE` | 20-50 entities | 40 | Smaller = more chunks/packets, larger = risk MTU overflow |
| Tick rate | 10-30 Hz | 20 | Higher = smoother but more bandwidth |
| Quantization precision | 0.001-0.1 m | 0.01 m | Tighter precision = wider value range, fewer bits per axis |
| Interpolation buffer | 2-3 ticks | 2 ticks | More = smoother but more visual delay |

Always measure under simulated 5% packet loss.

---

## Reference Implementations

- **DOTS Netcode for Entities Ghost System** - the canonical example, uses chunk filling + importance scaling
- **Glenn Fiedler / Gaffer On Games** - "Snapshot Compression" and "State Synchronization" articles
- **Quake III** - the original snapshot interpolation pattern
- **Overwatch** - delta snapshots + replay reconciliation (GDC 2017)

---

## Checklist

- [ ] "When to Use" criteria all hold (>10 Hz, many entities, authoritative sender, loss-tolerant) — if not, this is the wrong pattern; go back to `sync-mechanisms.md`
- [ ] `CHUNK_SIZE` tuned so worst-case chunk payload (states + ids + header + ~40 B framing) is <= 1200 B, shown with the arithmetic
- [ ] Chunk RPC carries `snapshotSeq` + `chunkIdx` + `totalChunks`, and the chunk-index width (`byte` = 8 chunks max via bitmask) covers `maxN / CHUNK_SIZE`
- [ ] Client reassembly ignores stale seqs, dedupes chunks, applies snapshots atomically only when complete, and cleans up superseded buffers
- [ ] Wire structs are quantized via a dedicated codec (gameplay stays `float`/`Vector3`), and total bandwidth (chunks × payload × Hz × clients) is computed and acceptable
- [ ] Late-joiner path is separate: one-shot Reliable full state on connect, periodic Unreliable chunks after — never conflated
- [ ] Behavior verified (or at least reasoned through) under simulated ~5% packet loss

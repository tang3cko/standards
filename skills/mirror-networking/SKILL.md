---
name: mirror-networking
description: Mirror networking standards for Unity multiplayer games. Covers NetworkBehaviour, SyncVar, Command, ClientRpc, server authority patterns, and late join handling. Use when implementing multiplayer features with Mirror.
---

# Mirror networking standards

Apply Tang3cko Mirror networking standards when working with Unity multiplayer code.

## Quick reference

### Key concepts

- **Server Authority**: Server owns game state, clients request changes via `[Command]`
- **SyncVar**: Server-to-client state synchronization
- **ClientRpc**: Server-to-all-clients method calls
- **TargetRpc**: Server-to-specific-client method calls
- **Command**: Client-to-server method calls

### Common patterns

```csharp
// Server authority pattern
[Command]
private void CmdRequestAction(ActionData data)
{
    if (!ValidateAction(data)) return;
    PerformAction(data);
    RpcNotifyAction(data);
}

[ClientRpc]
private void RpcNotifyAction(ActionData data)
{
    // Update visuals on all clients
}
```

## Detailed guides

Read these files for complete standards:

- [Mirror basics](../../unity/networking/mirror/basics.md)
- [Server authority](../../unity/networking/mirror/server-authority.md)
- [SyncVar and ClientRpc](../../unity/networking/mirror/syncvar-clientrpc.md)
- [Late join handling](../../unity/networking/mirror/late-join.md)
- [Interactable pattern](../../unity/networking/mirror/interactable-pattern.md)

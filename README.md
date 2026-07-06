# Tang3cko Standards

## Purpose

Shared coding standards and Claude Code skills for Tang3cko projects.

---

## Contents

### AI Agent Skills

| Skill | Description |
|-------|-------------|
| [unity-core](skills/unity-core/) | Naming, code organization, error handling, Input System, editor tooling |
| [unity-architecture](skills/unity-architecture/) | ScriptableObject architecture: EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO |
| [unity-async](skills/unity-async/) | UniTask / Awaitable / coroutines, performance optimization |
| [unity-testing](skills/unity-testing/) | Humble Object, Edit Mode vs Play Mode, NUnit, test doubles, asmdef |
| [unity-ui](skills/unity-ui/) | UI Toolkit (UXML/USS, BEM, world-space on 6.2+), uGUI, accessibility |
| [unity-networking](skills/unity-networking/) | NGO 2.x: payload sizing, delivery selection, snapshot streaming |
| [unity-mcp](skills/unity-mcp/) | Compile verification via the official Unity MCP bridge |
| [godot-core](skills/godot-core/) | Godot 4.x + .NET: naming, structure, export/NodePath gotchas, Resource-based decoupling |
| [godot-testing](skills/godot-testing/) | Godot 4.x + .NET: xUnit + Humble Object, link-referenced pure C# tests |
| [game-design](skills/game-design/) | Sakurai / verb theory / Miyazaki design philosophies, concept organization |

### Skill structure

Every code-oriented skill follows the same contract:

- **Phase Tracking** — at workflow start the agent registers each phase as a task (TaskCreate) and updates status as it progresses, with an execution checklist of MUST items verified at the end
- **Core Rules (always apply)** — inlined in SKILL.md so they are always in context when the skill triggers
- **Workflow** — task-type paths that name which references/ to load on demand
- **Self-Check** — a tracked phase: a checklist the agent must pass before presenting generated or edited code
- **Reference Files** — detailed docs loaded only when the task matches

---

## Installation

In Claude Code, use the `/plugin` command:

```
/plugin marketplace add tang3cko/standards
```

---

## Priority levels

Each standard uses priority levels to indicate importance:

- **Must fix**: Must be followed
- **Should fix**: Should be followed when possible
- **Nice to have**: Optional improvement

---

## References

- [Tang3cko.ReactiveSO](https://github.com/tang3cko/ReactiveSO) - ScriptableObject-based reactive architecture
- [Unity Manual](https://docs.unity3d.com/Manual/)
- [C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [Claude Code Skills](https://code.claude.com/docs/en/skills)

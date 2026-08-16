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
| [unity-mcp](skills/unity-mcp/) | Compile verification via the official Unity MCP bridge (Editor open) |
| [unity-headless](skills/unity-headless/) | Headless test/build execution via batchmode (Editor closed) |
| [godot-core](skills/godot-core/) | Godot 4.x + .NET: naming, structure, export/NodePath gotchas, Resource-based decoupling |
| [godot-architecture](skills/godot-architecture/) | Godot-free simulation core: SimEvent pull feed, notify APIs, dt substep |
| [godot-testing](skills/godot-testing/) | Godot 4.x + .NET: 3 layers — link-referenced xUnit, GdUnit4Net E2E, numeric/visual gates |
| [godot-animation](skills/godot-animation/) | Retargeting via both-sides BoneMap/rest_fixer normalization, pose verification |
| [godot-headless](skills/godot-headless/) | godot-mono CLI: import/build/test, offscreen PNG rendering (the --headless trap) |
| [godot-persistence](skills/godot-persistence/) | Save systems: versioned binary + CRC32, atomic writes, backup rotation, sandboxing |
| [project-conventions](skills/project-conventions/) | Engine-agnostic ops: commit messages, docs structure, decision records, task-tracker integration, backlog granularity |
| [game-design](skills/game-design/) | Sakurai / verb theory / Miyazaki design philosophies, concept organization |

### Skill structure

Every code-oriented skill follows the same contract:

- **Phase Tracking** — at workflow start the agent registers each phase as a task (TaskCreate) and updates status as it progresses, with an execution checklist of MUST items verified at the end
- **Core Rules (always apply)** — inlined in SKILL.md so they are always in context when the skill triggers. SKILL.md is the index and the always-apply contract only — the acceptance criteria live in references/
- **Workflow** — task-type paths that name exactly which references/ files to Read for the task at hand
- **Self-Check** — a tracked phase: a checklist the agent must pass before presenting generated or edited code. It requires that the workflow-named references were actually Read and that each loaded reference's end-of-file Checklist was run — the Self-Check cannot be completed from SKILL.md alone
- **Reference Files** — a "Load When" table: each row states the load trigger and what information exists only in that file. Every reference ends with a domain-specific `## Checklist` that the Self-Check delegates to

(The reference-checklist contract applies to all code-oriented skills — unity-*, godot-*, and project-conventions. game-design is a knowledge skill and exempt.)

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

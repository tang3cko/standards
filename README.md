# Tang3cko Standards

## Purpose

Shared coding standards and Claude Code skills for Tang3cko projects, distributed as a Claude Code plugin marketplace. Each plugin lives under `plugins/` — browse its `skills/` to see what it carries.

---

## Plugins

| Plugin | Contents |
|--------|----------|
| `tang3cko-unity` | Unity: naming, ScriptableObject architecture, async/performance, testing, UI, NGO networking, MCP compile checks, headless test/build |
| `tang3cko-godot` | Godot 4.x + .NET: naming/structure, Godot-free simulation core, testing, animation retargeting, headless CLI, save/persistence |
| `tang3cko-unreal` | Unreal Engine C++: naming and the reflection contract, pointers and validity, class selection, placement by authority and lifetime, delegates and replication, CQTest-first testing with headless runs |
| `tang3cko-core` | Engine-agnostic foundation: Sakurai / verb theory / Miyazaki design philosophies, concept organization |
| `unity-official-ui`, `unity-official-ui-uitk`, `unity-official-ui-ugui`, `unity-official-cli`, `unity-official-multiplayer-services` | Unity's official skills, pulled straight from [Unity-Technologies/skills](https://github.com/Unity-Technologies/skills): UI router, UI Toolkit, uGUI, Unity CLI, multiplayer services |

`tang3cko-unity`, `tang3cko-godot`, and `tang3cko-unreal` declare `tang3cko-core` as a dependency, so installing any of them pulls it in.

`tang3cko-unity` assumes the `unity-official-*` plugins are installed too: `unity-official-ui-uitk` / `unity-official-ui-ugui` carry the UI mechanics, `unity-official-cli` the editor and project management, `unity-official-multiplayer-services` the sessions layer. The Tang3cko skills hold only the conventions that differ from or add to those, and say so where they override an official default. The official repository ships no `marketplace.json`, which is why each skill is listed here as its own plugin of this marketplace rather than as a marketplace of its own.

Each `unity-official-*` plugin is pinned to a commit `sha` in `.claude-plugin/marketplace.json` rather than tracking `Unity-Technologies/skills` at `HEAD`. When upstream moves, Renovate opens a PR bumping the sha with a link to the upstream compare view; merging it is a manual review step where someone reads that diff before accepting it. Skills load namespaced under their plugin, e.g. `/unity-official-ui-uitk:ui-uitk`, and the bare `/ui-uitk` also works as long as no other installed skill uses that same name.

### Skills

| Skill | Description |
|-------|-------------|
| [unity-core](plugins/tang3cko-unity/skills/unity-core/) | Naming, code organization, error handling, Input System, editor tooling |
| [unity-architecture](plugins/tang3cko-unity/skills/unity-architecture/) | ScriptableObject architecture: EventChannel, RuntimeSet, Variable, ActionSO, ReactiveSO |
| [unity-async](plugins/tang3cko-unity/skills/unity-async/) | UniTask / Awaitable / coroutines, performance optimization |
| [unity-testing](plugins/tang3cko-unity/skills/unity-testing/) | Humble Object, Edit Mode vs Play Mode, NUnit, test doubles, asmdef |
| [unity-ui](plugins/tang3cko-unity/skills/unity-ui/) | UI Toolkit (UXML/USS, BEM, world-space on 6.2+), uGUI, accessibility |
| [unity-networking](plugins/tang3cko-unity/skills/unity-networking/) | NGO 2.x: payload sizing, delivery selection, snapshot streaming |
| [unity-mcp](plugins/tang3cko-unity/skills/unity-mcp/) | Compile verification via the official Unity MCP bridge (Editor open) |
| [unity-headless](plugins/tang3cko-unity/skills/unity-headless/) | Headless test/build execution via batchmode (Editor closed) |
| [godot-core](plugins/tang3cko-godot/skills/godot-core/) | Godot 4.x + .NET: naming, structure, export/NodePath gotchas, Resource-based decoupling |
| [godot-architecture](plugins/tang3cko-godot/skills/godot-architecture/) | Godot-free simulation core: SimEvent pull feed, notify APIs, dt substep |
| [godot-testing](plugins/tang3cko-godot/skills/godot-testing/) | Godot 4.x + .NET: 3 layers — link-referenced xUnit, GdUnit4Net E2E, numeric/visual gates |
| [godot-animation](plugins/tang3cko-godot/skills/godot-animation/) | Retargeting via both-sides BoneMap/rest_fixer normalization, pose verification |
| [godot-headless](plugins/tang3cko-godot/skills/godot-headless/) | godot-mono CLI: import/build/test, offscreen PNG rendering (the --headless trap) |
| [godot-persistence](plugins/tang3cko-godot/skills/godot-persistence/) | Save systems: versioned binary + CRC32, atomic writes, backup rotation, sandboxing |
| [unreal-core](plugins/tang3cko-unreal/skills/unreal-core/) | Naming, the reflection contract, pointer and validity rules, class selection ladder, module dependencies, logging and asserts |
| [unreal-architecture](plugins/tang3cko-unreal/skills/unreal-architecture/) | Placement by authority and lifetime, subsystems, multicast delegates, replication, Core/Shell boundary, Game Feature plugins |
| [unreal-testing](plugins/tang3cko-unreal/skills/unreal-testing/) | CQTest by default, the five-framework decision table, Humble Object, headless runs that fail on zero matches |
| [game-design](plugins/tang3cko-core/skills/game-design/) | Sakurai / verb theory / Miyazaki design philosophies, concept organization |

### Skill structure

Every code-oriented skill follows the same contract:

- **Phase Tracking** — at workflow start the agent registers each phase as a task (TaskCreate) and updates status as it progresses, with an execution checklist of MUST items verified at the end
- **Core Rules (always apply)** — inlined in SKILL.md so they are always in context when the skill triggers. SKILL.md is the index and the always-apply contract only — the acceptance criteria live in references/
- **Workflow** — task-type paths that name exactly which references/ files to Read for the task at hand
- **Self-Check** — a tracked phase: a checklist the agent must pass before presenting generated or edited code. It requires that the workflow-named references were actually Read and that each loaded reference's end-of-file Checklist was run — the Self-Check cannot be completed from SKILL.md alone
- **Reference Files** — a "Load When" table: each row states the load trigger and what information exists only in that file. Every reference ends with a domain-specific `## Checklist` that the Self-Check delegates to

(The reference-checklist contract applies to all code-oriented skills — unity-*, godot-*, and unreal-*. game-design is a knowledge skill and exempt.)

---

## Installation

In Claude Code, use the `/plugin` command:

```bash
# Add marketplace
/plugin marketplace add tang3cko/standards

# Install what the project needs — tang3cko-core comes along with any of them
/plugin install tang3cko-unity@tang3cko-standards
/plugin install tang3cko-godot@tang3cko-standards
/plugin install tang3cko-unreal@tang3cko-standards

# Or install the foundation on its own
/plugin install tang3cko-core@tang3cko-standards

# Unity projects also want Unity's official skills, served through this marketplace
/plugin install unity-official-ui@tang3cko-standards
/plugin install unity-official-ui-uitk@tang3cko-standards
/plugin install unity-official-ui-ugui@tang3cko-standards
/plugin install unity-official-cli@tang3cko-standards
/plugin install unity-official-multiplayer-services@tang3cko-standards
```

A plugin activates immediately when that is safe (Claude Code 2.1.221 and later); otherwise run `/reload-plugins`.

---

## Updates

Re-run the install command — since Claude Code 2.1.232 it refreshes the marketplace first:

```bash
/plugin install tang3cko-unity@tang3cko-standards
```

On earlier versions, refresh the marketplace first, then install again:

```bash
/plugin marketplace update tang3cko-standards
/plugin install tang3cko-unity@tang3cko-standards
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

# Modules, Plugins and Game Features

Load this when deciding what becomes a module, a plugin, or a Game Feature.

Three units, three different reasons. Picking by size is the common mistake; pick by what the unit must be able to do.

| Unit | Choose it when |
|---|---|
| **Module** | Code needs a compile-time boundary — editor code separated from runtime, or compile times have become the bottleneck |
| **Plugin** | The unit must carry **assets**, or ship independently. Modules cannot contain assets |
| **Game Feature** | The unit must **load and unload at runtime**, injecting itself into existing actors |

## Modules

Start with **one runtime module per project**. Split for a concrete reason: editor-only code mixed with runtime code, a system genuinely reused across projects, compile times, or a subsystem with no dependencies on the rest. Not preemptively — a module holding one or two classes is not worth its overhead.

Dependencies flow one way. Circular module dependencies are an error by default and the engine says support for them is being phased out. If two modules need each other's types, extract the shared types into a third both depend on.

Editor modules may depend on runtime modules. **Never the reverse.**

The directory/dependency mechanics — `Public`/`Private`/`Internal`, public versus private dependency choice, `LoadingPhase` ordering — are covered in full in `unreal-core/references/module-and-includes.md`. Load it before writing a `.Build.cs`.

## Plugins

The thing that actually forces a plugin is assets: plugins can contain them, modules cannot. Most plugins need only one module.

A plugin may depend only on plugins or modules at the same level or higher.

`LoadingPhase` orders coarsely, and within one phase the order is **not deterministic** — call `LoadModule` explicitly if one must precede another. Persistent "cannot find C++ class in plugin" errors from the editor usually mean the plugin needs `PreDefault`.

Adding a plugin to the `.uproject` requires rebuilding the editor target, or the editor dies during startup with no error.

## Game Features

A Game Feature plugin lives under `Plugins/GameFeatures` and can be activated and deactivated at runtime. Its mechanism is component injection: the feature adds Components to actors that already exist, rather than those actors knowing about the feature.

The consequence for design is direct, and it is the strongest practical argument for the Component rule in `placement.md`: **behaviour already living in a Component can become a Game Feature later; behaviour on the Actor cannot.** The Game Features page (`dev.epicgames.com/documentation/en-us/unreal-engine/game-features-and-modular-gameplay-in-unreal-engine`) says a feature's components "should handle all program logic and data storage".

This is the axis Lyra is modular on: one game module split into feature folders, with the modular parts pushed out into a handful of Game Feature plugins. **Lyra is horizontally modular, not layered.** It is not evidence for a pure-logic core module — see `core-shell.md`. Lyra is not bundled with an installed engine, so do not cite its file counts or plugin names as if they were locally verifiable; the property worth keeping is the shape (horizontal, not layered), not the numbers.

## For a small team

One primary game module is the correct starting point. A module without dependents does not even need the `Public`/`Private` split — code outside those folders behaves as private.

Reach for a plugin when something needs to carry assets or move to the next iteration wholesale. Reach for a Game Feature only when runtime activation is genuinely the requirement, not as a structuring device.

## Checklist

- [ ] The unit was chosen by capability — compile boundary, assets, runtime activation — not by size
- [ ] Splitting was justified concretely, not preemptively
- [ ] No circular module dependency; shared types extracted rather than cross-referenced
- [ ] No runtime module depends on an editor module
- [ ] Plugin dependencies respect the same-level-or-higher rule
- [ ] No load-order assumption within a single `LoadingPhase`
- [ ] Feature behaviour lives in Components, so it remains eligible to become a Game Feature
- [ ] Lyra is not cited as evidence for layered or engine-free modules, and no unverifiable Lyra numbers are quoted

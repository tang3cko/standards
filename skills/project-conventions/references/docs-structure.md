# docs/ Structure

## Numbered namespaces

```
docs/
  00_overview/       concept, design philosophy, one-time cross-cutting decisions
  01_architecture/   system design docs, cross-system communication
  02_game-design/    game concept, core loop, characters, story, reference works
  03_development/    process docs — testing strategy, asset/audio requirements
  05_standards/      project-local standards deltas (UI layout rules, etc.)
  99_ideas/          unimplemented, speculative design — quarantined
```

Numbering is deliberately non-contiguous (there is no `04_` in this list) — it leaves room to insert a namespace later without renumbering everything that follows. Each namespace typically has its own `README.md` acting as an index for that section.

Placement heuristic: ask "does this doc describe something that exists, or something someone is still deciding?" The former goes in `00`-`05`; the latter goes in `99_ideas/`.

## 99_ideas/ as a quarantine bucket

`99_ideas/` holds anything not yet decided or not yet built: alternate designs under consideration, features sketched but not committed to, redesign proposals for an already-shipped system. Keeping this separate from the numbered namespaces protects the property that "everything outside `99_ideas/` describes the current, real state of the project" — a reader (or agent) never has to guess whether a doc in `01_architecture/` is aspirational or accurate.

Typical contents: sound-design brainstorms, alternate core-verb proposals, font/typography options under evaluation, UI transition system sketches not yet implemented, save-system design alternatives. When an idea from `99_ideas/` gets built, its content graduates into the matching numbered namespace (and the speculative doc is retired or trimmed to just the decision record, not left duplicated).

## No progress content in docs/

Never place any of the following in a numbered namespace (or anywhere in `docs/`):

- Implementation-status tables ("Feature X: 80% done")
- Percentage-complete counters
- TODO / done checklists tracking work-in-progress

This content belongs in the task tracker (see references/task-tracker-integration.md). The reason is not aesthetic: a status table goes stale the moment work continues, and a stale doc is worse than no doc because it actively misleads. `docs/` should describe things that stay true regardless of how much is implemented — architecture, concept, standards — so it never needs a "last updated" caveat to be trusted.

When reviewing an existing docs/ tree, flag any status/progress content found and recommend moving it to the tracker rather than deleting the surrounding doc — the doc's non-progress content is usually still valid reference material.

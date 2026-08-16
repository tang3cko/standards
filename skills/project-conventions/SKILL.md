---
name: project-conventions
description: Apply Tang3cko engine-agnostic project operating conventions — commit messages with verification and known gaps, numbered docs/ namespaces, decision records (ADR) and their propagation, keeping progress tracking out of docs, task tracker integration, backlog granularity (Story-sized tickets and landing verification), and resuming interrupted work. Use when writing a commit message, structuring or reviewing docs/, filing or superseding an ADR, wiring up a task tracker, filing or auditing a backlog, or resuming work after an interruption, in any Unity/Godot project.
when_to_use: Triggers on "commit message", "コミットメッセージ", "docs構造", "docs structure", "運用規約", "known gaps", "既知の制約", "進捗管理", "task tracker", "再開", "resume", "resumption", "backlog", "バックログ", "起票", "粒度", "granularity", "Story", "Epic", "milestone", "マイルストーン", "ロードマップ", "roadmap", "ADR", "決定記録", "設計決定", "decision record", "波及先", "propagation", "superseded", "撤回", "withdrawn".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Project Conventions

Tang3cko's engine-agnostic operating conventions: commit messages, `docs/` structure, task-tracker
integration, backlog granularity, and resuming interrupted work. This skill is about *process*, not
engine code — pair it with the engine-specific core skill (unity-core / godot-core) when the task
also touches source code.

The part omitted most often is **Known gaps**. A commit message without it reads as complete work,
so the next session — or the next agent — re-derives state from the diff instead of reading the one
sentence that would have told it what was deliberately left undone.

## Phase Tracking

**At workflow start (commit message, docs structuring, tracker setup, or resumption path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load reference     | matching the path (commit-messages/docs-structure/adr-conventions/task-tracker-integration/backlog-granularity/resumption-workflow)"
TaskCreate: "Phase 2: Draft/review output | follow Core Rules"
TaskCreate: "Phase 3: Pre-Commit Check    | MUST pass every item before presenting a commit message"
```

Update status as you progress: `in_progress` when starting, `completed` when done. A convention question skips Phase Tracking. Phase 3 applies only to the commit-message path — docs/tracker/resumption paths end at Phase 2.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the reference named by the workflow path | Always |
| 3 | Pre-Commit Check on the drafted message | Drafting or reviewing a commit message |

## Core Rules (always apply)

Apply these to every commit, docs edit, or tracker interaction. Detailed rationale and examples live in references/ — these are the contract.

### Commit Message Shape

```
<summary line: verb + effect, one line>

- <change detail 1>
- <change detail 2>
- <how it was verified: what was run, and what it reported>
- <known gaps: what's deliberately left undone, and why>

🤖 Generated with Claude Code
```

| Part | Rule |
|------|------|
| Summary line | One line, names the effect not just the mechanism ("harden X" not "edit Y.cs") |
| Change bullets | One bullet per logical change; note *why* when the choice isn't obvious |
| Verification | Name what was run and what it reported |
| Known gaps | Explicit: what's left, and the decision behind leaving it (a deferred-scope note is a decision record, not an apology) |
| Trailer | Standard Claude Code trailer |

A commit is a resumption point: someone (including a future agent) must be able to read the message alone and know what remains.

### docs/ Namespace

| Namespace | Contents |
|-----------|----------|
| `00_overview/` | Concept, design philosophy, one-time cross-cutting decisions |
| `01_architecture/` | System design docs, cross-system communication |
| `02_game-design/` | Game concept, core loop, narrative, characters |
| `03_development/` | Process docs: testing strategy, asset/audio requirements |
| `05_standards/` | Project-local standards deltas (numbering leaves room to insert a namespace later) |
| `06_decisions/` | ADRs — decision records and their propagation |
| `99_ideas/` | Unimplemented, speculative design — isolated from authoritative docs |

`99_ideas/` is a quarantine bucket: anything not yet decided or not yet built goes here, never mixed into the numbered namespaces that describe what exists.

### Decision Records (ADR)

A design decision that is **made, narrowed, reversed or withdrawn** gets an ADR in `06_decisions/`,
and so does answering an earlier ADR's open question. The accident this prevents is a decision
changing in code while the old version survives in a doc, a comment, `CLAUDE.md` or a tracker
ticket — so a later session implements from a stale spec. That is why **the Propagation table is the
body of an ADR**, not an appendix to it.

Three rules break most often and are worth stating here rather than only in the reference:

- **Append-only.** A filed ADR is never rewritten. Changes are a *new* ADR; the only edits permitted
  on the old one are its Status line and one link line beneath it
- **Consent, not authorship.** A decision belongs in an ADR *once the user has agreed to it*. Having
  shown it to them is not agreement. Recording an unagreed decision as the user's is the worst
  failure the namespace has, because it destroys the ledger's ability to say who is accountable
- **No implementation status.** Built / in progress / assigned lives in the tracker, never here

The numbering slot, the three permitted `Decided:` header forms, the required sections, and the
tracker-search procedure behind the Propagation table live in `references/adr-conventions.md`. Each
project also restates the rules in its own `06_decisions/README.md`.

### No Progress in docs/

Never write implementation-status tables, percentage-complete, or "TODO/done" checklists into `docs/`. That content goes to the task tracker the project designates. `docs/` holds reference material that stays true regardless of what's implemented yet — mixing in progress state is what makes docs go stale.

### Task Tracker Integration

The project's `CLAUDE.md` names the tracker list/board explicitly; cross-commit progress lives in tracker comments, not docs or commit bodies. **Use only the tracker that project names — reaching for any other tracker, tool, or ad-hoc list is forbidden.** If no tracker is named, ask; do not pick one. The comment shape and commit-vs-tracker routing live in `references/task-tracker-integration.md`.

### Backlog Granularity

The unit filed in the tracker is a **Story**: one ADR if it's a decision, one playable/usable feature if it's implementation, 3–7 solo days either way — never a technical layer, never "look into X". Sub-Story detail is a commit, not a ticket. A backlog must also answer "if every item closes, what exists?" with a shippable result. The layer model, the splitting rules, and the product-completeness categories live in `references/backlog-granularity.md`.

### Resumption Workflow

Before resuming interrupted work (session stall, API disconnect, new session): read the **Known gaps** section of the last completed commit first. It is the intended entry point — do not re-derive state from scratch or re-scan the whole codebase. The full procedure lives in `references/resumption-workflow.md`.

## Workflow

Identify the task type and follow the matching path.

### Writing a commit message

1. Load `references/commit-messages.md`
2. Draft using the Commit Message Shape above
3. Run the Pre-Commit Check below before presenting

### Structuring or reviewing docs/

1. Load `references/docs-structure.md`
2. Place new docs by namespace; flag any progress/status content for removal to the tracker
3. Flag speculative/unimplemented content found outside `99_ideas/`

### Filing or reviewing an ADR

1. Load `references/adr-conventions.md`
2. Confirm the decision has the user's agreement, and pick the matching `Decided:` header form
3. Full-text search the tracker on every name the subject goes by, *then* write the Propagation table
4. Run that reference's `## Checklist` against the draft before presenting

### Setting up or reviewing task tracker integration

1. Load `references/task-tracker-integration.md`
2. Verify `CLAUDE.md` names the tracker list/board
3. Verify milestone comments follow the what+hash+next-point shape

### Authoring or reviewing a backlog

1. Load `references/backlog-granularity.md`
2. Check each item against the Story definition; fold Tasks into commits and split buckets
3. Answer the landing question and sweep the product-completeness categories

### Resuming interrupted work

1. Load `references/resumption-workflow.md`
2. Find the last completed commit (`git log -1`) and read its Known gaps
3. Cross-check against the tracker's latest comment before starting new work

### A convention question

1. Load the reference matching the question domain
2. Explain with concrete before/after examples

## Pre-Commit Check (before presenting a commit message)

Verify the drafted message against each item. If any check fails, fix it first — do not present a non-compliant message.

- [ ] Summary line states the effect, one line
- [ ] Change bullets are logical units, not a raw diff narration
- [ ] Verification names what was run and what it reported
- [ ] Known gaps is present — explicit "left undone" + reason, even if the answer is "none"
- [ ] No implementation-status or progress content leaked into a docs/ change bundled in the same commit
- [ ] Trailer present
- [ ] `references/commit-messages.md` was actually Read this session — SKILL.md is only the index; the phrasing rules and worked examples live there, so drafting from the Shape alone means drafting against an incomplete standard
- [ ] The `## Checklist` at the end of the loaded reference was run item-by-item against the drafted message

Non-commit paths have the same gate at Phase 2: the reference named by the chosen path must have been Read this session, and its end `## Checklist` run against the output — each reference is the only place its domain's definition of done exists.

## Reference Files

Each file below is the only place its listed content exists — SKILL.md deliberately does not restate it.

| File | Load When |
|------|-----------|
| references/commit-messages.md | Drafting or reviewing a commit message. Only source for: summary-line effect-vs-mechanism examples and the parenthetical preview pattern, the worked verification examples, known-gaps decision-record phrasing, when the bullet body may be skipped |
| references/docs-structure.md | Placing a new doc or auditing docs/. Only source for: the exists-vs-still-deciding placement heuristic, `99_ideas/` graduation/retirement rule, per-namespace README indexes, how to handle progress content found during review |
| references/adr-conventions.md | Filing, reviewing or superseding an ADR, or setting up `06_decisions/`. Only source for: the namespace slot and why a project that filled `04_` keeps it, append-only and the exactly-two-permitted-edits rule, the Superseded/Withdrawn vocabulary and why answering an open question is neither, the three permitted `Decided:` header forms and the consent-vs-having-been-shown distinction, the required sections, the tracker-search procedure behind the Propagation table, dated-numbers-in-ADR vs no-numbers-in-current-shape-docs |
| references/task-tracker-integration.md | Wiring a project's CLAUDE.md to its tracker or writing milestone comments. Only source for: the CLAUDE.md tracker-naming phrasing, commit-vs-tracker content routing table, three-part milestone comment shape, the project-designated-tracker-only rule |
| references/backlog-granularity.md | Filing, splitting, or auditing tracker items, or planning a milestone/roadmap. Only source for: the Milestone/Epic/Story/Task layer model and where filing stops, the one-ADR-or-one-feature Story definition, the vertical-slice and kind-of-data splitting rules, when an ADR is split out vs. folded in, the parallel-band and cost-deferred ordering rules, the landing question and the product-completeness category sweep |
| references/resumption-workflow.md | Resuming after a stall, API disconnect, or new session. Only source for: the two-command entry-point procedure, the deferral-reasoning rule (continue or explicitly un-defer), tracker cross-check ordering, resumption anti-patterns |

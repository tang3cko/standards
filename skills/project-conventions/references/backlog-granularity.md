# Backlog Granularity

`references/task-tracker-integration.md` covers *what* gets written where (commit vs. tracker) once
a task exists. This file covers the step before that: how big a tracker item is allowed to be, and
how to tell whether the list, taken as a whole, actually lands on a finished product.

## The four layers

| Layer | Role |
|-------|------|
| Milestone | A shippability stage — "all systems exist and can be verified", "content is filled in and it plays end to end", etc. |
| Epic | A system or domain |
| **Story** | **The unit of tracker entry** |
| Task | Implementation detail inside a Story. **Not filed** — a commit is enough |

Only Stories become tracker items. Milestones and Epics are groupings; Tasks are commits. Filing
Tasks buys nothing: the commit message already carries the change detail, and a tracker full of
sub-day items hides the shape of the project behind noise.

## What a Story is

- A design decision → **exactly one ADR**
- An implementation → **one feature you can play (or use) and confirm**
- Sized to close in **3–7 days of solo development**

If an item cannot be phrased as one of those two, it is not a Story yet.

## Rules

### Never split by technical layer

"Write the Core side", "write the View side", "write the doc listing them" are not Stories. Cut
**vertically**: a Story crosses every layer it needs to and ends at something observable.

Splitting by *kind of data* is the same mistake wearing a different hat. Two Stories like "the item
roster" and "the facility roster" look like a clean split, but facilities consume items — deciding
either one moves the other, so the split guarantees round trips. Split the chain by *stage* (or by
whatever axis makes each piece independently closable), not by which table the rows land in.

### "Look into X" / "flesh out X" is not a Story

Those close on nobody's judgement but the author's, so they never close at all. Write a **closing
condition a second person could evaluate**: what exists, what runs, what a test or a gate reports.

Field evidence: on one project, eleven bucket-shaped `[Design]` items ("flesh out the storage
system", "flesh out the construction system") sat untouched for over a day after filing, while
items cut as one-ADR-each closed without friction. The buckets were not harder work — they were
unjudgeable, so there was never a moment where starting felt possible.

### Split the ADR out only when its conclusion is a premise for *other* Stories

If the decision only affects the implementation sitting right behind it, keep the ADR as the first
task *inside* the implementation Story. Splitting it produces an empty vessel: the implementation
half's closing condition can only be written as "the shape decided in the ADR runs", which contains
no information until the ADR is written.

Split when a later Story cannot even be estimated until the decision lands. Then the ADR is a real
Story with a real closing condition, and the dependency is visible in the list.

### Mark the band that may run in parallel

Solo development serialized end to end has no escape hatch: the day the current Story stalls, the
whole list stalls. Name the Stories that can be picked up in any order alongside the main line —
tooling, measurement, input/camera work, infrastructure — and say so explicitly in the list.

### Pull forward whatever gets more expensive the later it lands

Typical examples:

- **Performance measurement** — measure before deciding, and before the scale that would break the
  design is baked into other work
- **The save container** — a version number and a migration hook, filed early, even if the payload
  is nearly empty. Designing save after the data shapes have multiplied means every shape changed
  without a migration path
- **Verification gates** — build the gate *before* the thing it verifies. A gate written afterwards
  gets its thresholds fitted to whatever the current behaviour happens to be, so a broken result is
  accepted as the baseline

## Landing verification

A backlog is not a to-do list; it is a claim that finishing it produces the product.

Re-ask this periodically, and always after any restructuring:

> If every item on this list is closed, top to bottom, what exists?

If the answer is not a sentence describing a finished, shippable thing, the list is broken — either
items are missing or a milestone is not actually a shippability stage.

Systems work alone never adds up to a product. Check every pass that none of these is absent from
the list:

- Save / load, including migration
- Title screen, new game, continue, settings
- Audio
- Localization (and that at least one non-primary language is verified end to end, not just wired)
- Balance passes against stated targets
- Performance at the intended final scale
- Long-session verification (the failure modes a one-hour balance pass structurally cannot see)
- Onboarding — verified, even if the claim is "none needed"

Field evidence: a backlog of 27 items was audited this way and could not answer the question. It
contained no content rosters, no save, no title screen, no audio, no balance, no performance, no
localization. Rebuilt against the checklist above it came to 64 items across five milestones, and
the milestone boundaries only became statable once the missing categories were present.

## Checklist

Run against every backlog authored, restructured, or reviewed before presenting:

- [ ] Only Stories are filed — no Tasks in the tracker, and Milestones/Epics are groupings, not items
- [ ] Every Story is either exactly one ADR or one playable/usable feature, sized 3–7 solo days
- [ ] No Story is a technical layer, and none splits a mutually-dependent pair by kind of data
- [ ] Every closing condition is evaluable by a second person — no "look into", "flesh out", "consider"
- [ ] Each split-out ADR has a named downstream Story that depends on it; the rest are folded into their implementation Story
- [ ] The Stories that may proceed in parallel with the main line are marked as such
- [ ] Cost-deferred work (performance measurement, the save container, verification gates) is placed early, and each gate precedes what it verifies
- [ ] The landing question is answered in one sentence describing a shippable result
- [ ] The product-completeness categories are each present or explicitly ruled out with a reason: save, title/new game/settings, audio, localization, balance, performance at final scale, long-session verification, onboarding

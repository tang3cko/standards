# Resumption Workflow

## The problem this solves

Sessions get interrupted: API disconnects, agent stalls, context resets, or simply a new session picking up where a previous one left off. Without a deliberate entry point, resuming means re-scanning the codebase and re-deriving state from scratch — expensive, and error-prone (easy to redo already-finished work, or miss an intentionally-deferred item and "fix" something that was correctly left alone).

## The entry point: the last completed commit

Before doing anything else, find the last completed commit and read its **Known gaps** section:

```
git log -1
git show <hash>   # read the full message, especially Known gaps
```

The Known gaps section is the intended resumption point — it was written for exactly this purpose. Treat it as the starting instruction, not as background context to skim. If it names a deferred item with a reason ("X is a separate multi-hour follow-up, explicitly scoped out"), that is a decision already made — don't silently redo the analysis of whether to tackle X now; either continue the deferral or make a new, equally explicit decision to un-defer it.

## Cross-check against the tracker

If a milestone comment exists in the task tracker (see references/task-tracker-integration.md) more recent than the last commit's context, read that too — it may describe cross-commit state the single commit's Known gaps doesn't capture (e.g., "commit N is done, but the human reviewer flagged Y, still pending").

## Why this is cheap

This works because commit messages and tracker comments are written *as* resumption points from the start (see references/commit-messages.md) — the cost of this workflow is proportional to how disciplined those artifacts were kept, not to the size of the codebase. A project that consistently writes Known gaps sections turns "where was I?" into a two-command lookup instead of a re-investigation.

## What NOT to do

- Don't re-scan the whole codebase or re-run a full audit before checking the last commit's Known gaps first
- Don't assume silence means "nothing left" — if a commit has no Known gaps, that should be an explicit "Known gaps: none", not an absence to interpret
- Don't treat a deferred item's reasoning as still open for debate unless something material has changed since it was deferred

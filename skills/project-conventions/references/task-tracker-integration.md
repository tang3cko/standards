# Task Tracker Integration

## Naming the tracker in CLAUDE.md

The project's `CLAUDE.md` states the tracker and the exact list/board it uses, so any agent picking up the project knows where to look without asking. Example phrasing:

> Task/progress tracking lives in `<tracker>` (list: `<org> - <project>`) — never keep implementation-status tables in docs

This single line does two jobs: it points to the tracker, and it restates the "no progress in docs" rule at the point where an agent is most likely to be tempted to break it (right before writing a status update somewhere).

## What goes in the tracker vs. what goes in a commit

| Content | Location |
|---------|----------|
| What changed in this commit, verified how, what's deferred from *this* commit's scope | Commit message (Known gaps) |
| Cross-commit progress: where a multi-commit feature stands, what's next across sessions | Task tracker |
| Percentage complete, feature status board | Task tracker |

A commit's Known gaps is scoped to that commit. The tracker is scoped to the feature/milestone across however many commits it takes.

## Milestone comments

When a significant chunk of work lands (a milestone, not every commit), add a comment to the relevant tracker item with:

1. What was done (brief, human-readable — not a diff dump)
2. The commit hash it corresponds to
3. The next resumption point — what to pick up next, and any decision that still needs making

This mirrors the commit-message Known-gaps pattern one level up: the comment is itself a resumption point, just at milestone granularity instead of commit granularity. A future session (or a different agent) should be able to read the latest tracker comment and know where to restart without re-reading the whole task history.

## Practical mechanics

Use the tracker the project names in its `CLAUDE.md`, through that tracker's own interface, and nothing else. Any other tracker, any side tool, any ad-hoc list or file kept "just for this session" is forbidden — a second place to look is how progress state goes stale and contradicts itself. If `CLAUDE.md` names no tracker, ask which one to use; do not choose one, and do not fall back to writing status into docs or the repo.

Whatever the tracker is, the mechanics are the same: search it for the existing item before writing, comment on that item, and don't open a new one for work that's just a commit-scoped detail — that's what the commit's own Known gaps section is for.

## Checklist

Run against every tracker setup or milestone comment before presenting:

- [ ] `CLAUDE.md` names the tracker *and* the exact list/board, in a line that also restates "no progress in docs"
- [ ] Only that named tracker was used — no other tool, no side list, and no tracker picked without being told which one
- [ ] Content is routed by the table: commit-scoped deferrals → the commit's Known gaps; cross-commit progress and status → the tracker
- [ ] Each milestone comment carries all three parts: what was done, the commit hash, and the next resumption point (including any open decision)
- [ ] The comment was attached to the existing task found by search — no new task created for a commit-scoped detail
- [ ] Nothing that belongs in the tracker was written into docs/ or a commit body instead

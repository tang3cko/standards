# Commit Messages

A commit message is not a changelog entry — it is a resumption point. Anyone (including a future agent, possibly you after a stall) must be able to read one commit message and know what changed, how it was verified, and what's still open.

## Shape

```
<summary line>

- <change detail>
- <change detail>
- <how it was verified>
- <known gaps>

🤖 Generated with Claude Code
```

Merge commits and one-line fixes (typo, trivial config change) can skip the bullet body — the summary line alone is enough when there is genuinely nothing to record.

## Summary line

Name the *effect*, not the mechanism. State what capability changed for the player/system, not which files were touched.

- Bad: "エラー処理を追加" (adds error handling — mechanism only)
- Good: "エラー検知ハードニング（E2Eがエンジンエラーで落ちる仕組み＋wipe回帰＋診断強化）" (hardens error detection — E2E now fails on engine errors, plus a wipe-flow regression test and better diagnostics)

The parenthetical is doing real work here: it previews the bullet list so a reader scanning `git log --oneline` gets the shape of the change without opening the full message.

## Change bullets

One bullet per logical change, not per file. Each bullet should be understandable without opening the diff. Note the *why* whenever a choice isn't the obvious one — a bullet that only restates the diff ("X.cs を編集") is not useful; a bullet that says what the edit accomplishes and why it was necessary is.

Example bullet that carries a design decision, not just a mechanical description:

> ステータスチップ: 候補者=未処理/処理済み、オーダー=処理中/完了
> （正誤は一切漏らさない手続き状態のみ — フィードバック喪失設計を維持）

This states the change (status chip states) *and* the constraint it upholds (never leak correctness feedback — a deliberate design property of the system), so a reader who wasn't there still knows why the states are shaped this way.

## Verification

Name what was run and what it reported, in the words the harness printed, so the next reader knows which claims rest on a run and which rest on reading:

- `run-tests.sh: pass. repro-view.sh: inside=YES, hidden-from-owner=YES` — the harnesses and what they actually printed
- `Tests: the suite passes; a new case pins the give-up branch` — what was added, by what it pins rather than by how many
- `Tests: not affected. No code changed.` — when that is the truth, which is a useful thing to say plainly

If a run did not complete, say so. A run that did not finish is void, not a pass.

## Known gaps

This is a decision record, not an apology. State explicitly what was left undone and why leaving it was the right call for this commit's scope.

- `既知の制約: SceneRunner内ReloadCurrentSceneの良性エラー1件はピン留めで許容` (known limitation: one benign error from SceneRunner's ReloadCurrentSceneScene is pinned/accepted rather than chased)
- `the full asset copy/animation rebuild/... is a separate multi-hour follow-up, explicitly scoped out here rather than attempted half-done`

The second example is the pattern to imitate generally: name the deferred work, and say *why* it's deferred (scope, time cost, separate concern) rather than leaving a silent gap. A commit with "Known gaps: none" is fine and often correct — write it explicitly rather than omitting the section, so a resuming reader doesn't have to guess whether the omission means "nothing left" or "forgot to check".

## What NOT to do

- Don't write a summary line that only names the file touched
- Don't bundle "what changed" and "what's still broken" into the same bullet — known gaps gets its own line(s) so a resuming reader can find it without re-reading the whole body
- Don't put progress/TODO tracking in the commit body for anything beyond this commit's own scope — that belongs in the task tracker, not git history (see references/task-tracker-integration.md)

## Checklist

SKILL.md's Pre-Commit Check gates section *presence*; this checklist gates the phrasing quality only this file defines. Run it on the drafted message:

- [ ] Summary line previews the change's shape (parenthetical or equivalent) so `git log --oneline` alone conveys it — not just a bare verb phrase
- [ ] Every bullet whose choice isn't obvious carries its *why* (the constraint or decision), not a diff narration
- [ ] Verification names the runs and their verdicts, and says plainly when a run did not complete
- [ ] Known gaps names each deferred item *and* the reason deferring was right for this scope; "Known gaps: none" written explicitly when true
- [ ] If the bullet body was skipped (merge/one-liner), confirmed there is genuinely nothing to record — not just a short diff
- [ ] Nothing in the body tracks work beyond this commit's own scope

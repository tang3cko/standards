---
name: unity-async
description: Write performant async Unity code and optimize projects following Tang3cko standards — UniTask, Awaitable, coroutines, caching, pooling, event-driven patterns. Use when writing async code or optimizing performance.
when_to_use: Triggers on "UniTask", "async", "await", "Coroutine", "パフォーマンス", "performance", "pooling", "Profiler", "caching", "最適化", "optimization".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Async & Performance

Help users write performant async code and optimize Unity projects following Tang3cko standards.

## Phase Tracking

**At workflow start (writing or optimizing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load reference      | async.md or performance.md"
TaskCreate: "Phase 2: Pick approach       | Async Approach Decision table"
TaskCreate: "Phase 3: Write/review code   | follow Core Rules, cancellation handling"
TaskCreate: "Phase 4: Self-Check          | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Async/performance questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the reference for the task domain | Always |
| 4 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

1. **UniTask over Coroutine for app/game code** — allocation-free, cancellable, structured async. Awaitable (built-in) is for dependency-free library code; as of Unity 6.5 it still lacks WhenAll/WhenAny and frame-based delays
2. **Every async method takes and propagates a CancellationToken** — tie lifetime to `destroyCancellationToken` or an owned `CancellationTokenSource` disposed in `OnDestroy`
3. **No unobserved exceptions** — fire-and-forget goes through `UniTask.Void`, `Awaitable.LogExceptionsAndForget()` (Unity 6.5+), or an explicit try-catch; never bare `async void` without handling
4. **Event-driven over polling** — react to changes instead of checking every frame
5. **Profile before optimizing** — use Unity Profiler to find actual bottlenecks; cache `GetComponent`/`Camera.main`, avoid per-frame allocations

## Workflow

Identify the task type and follow the matching path.

### Writing async code

1. Load `async.md` for async patterns (UniTask, Awaitable, Coroutine)
2. Pick the approach from the Async Approach Decision below
3. Generate code with proper cancellation handling
4. Run the Self-Check below before presenting

### Optimizing performance

1. Load `performance.md` for optimization patterns
2. Check common issues: missing caching (GetComponent, Camera.main), per-frame allocations, polling where events fit
3. Recommend specific optimizations with code examples
4. Categorize issues by priority (must fix, should fix, nice to have)

### An async/performance question

1. Load the relevant reference for the question domain
2. Explain with code examples and trade-offs

## Async Approach Decision

| Scenario | Approach | Why |
|----------|----------|-----|
| New async logic (app/game) | UniTask | Allocation-free, cancellable, WhenAll/PlayerLoopTiming |
| Library / zero-dependency code | Awaitable (Unity 6+) | Built-in, no external package |
| Simple delay/sequence | Coroutine | Readable for simple cases |
| Fire-and-forget | UniTask.Void / `LogExceptionsAndForget()` (6.5+) | Prevents unobserved exceptions |

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed. If any check fails, fix it first — do not present non-compliant code.

- [ ] Every async method accepts a `CancellationToken` and passes it to every awaited call
- [ ] Cancellation source is tied to object lifetime (`destroyCancellationToken`, or CTS cancelled+disposed in `OnDestroy`)
- [ ] No bare `async void` without exception handling; fire-and-forget uses the approved patterns
- [ ] No per-frame allocations or uncached `GetComponent`/`Camera.main` in hot paths
- [ ] No polling loop where an event/callback would work

## Reference Files

| File | Use When |
|------|----------|
| references/async.md | Awaitable, UniTask, Coroutines, fire-and-forget |
| references/performance.md | Caching, pooling, event-driven optimization |

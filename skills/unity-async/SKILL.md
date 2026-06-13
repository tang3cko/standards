---
name: unity-async
description: Write performant async Unity code and optimize projects following Tang3cko standards — UniTask, Awaitable, coroutines, caching, pooling, event-driven patterns. Use when writing async code or optimizing performance.
when_to_use: Triggers on "UniTask", "async", "await", "Coroutine", "パフォーマンス", "performance", "pooling", "Profiler", "caching", "最適化", "optimization".
model: sonnet
allowed-tools: Read Glob Grep Edit Write
---

# Unity Async & Performance

Help users write performant async code and optimize Unity projects following Tang3cko standards.

## Core Principles

1. **UniTask over Coroutine** - Allocation-free, cancellable, structured async
2. **Event-driven over polling** - React to changes instead of checking every frame
3. **Profile before optimizing** - Use Unity Profiler to find actual bottlenecks

## Workflow

Identify the task type and follow the matching path.

### Writing async code

1. Load `async.md` for async patterns (UniTask, Awaitable, Coroutine)
2. Pick the approach from the Async Approach Decision below
3. Generate code with proper cancellation handling
4. Verify CancellationToken propagation and error handling

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
| New async logic | UniTask | Allocation-free, cancellable |
| Unity 6+ native async | Awaitable | Built-in, no dependency |
| Simple delay/sequence | Coroutine | Readable for simple cases |
| Fire-and-forget | UniTask.Void | Prevents unobserved exceptions |

## Reference Files

| File | Use When |
|------|----------|
| references/async.md | Awaitable, UniTask, Coroutines |
| references/performance.md | Caching, pooling, event-driven optimization |

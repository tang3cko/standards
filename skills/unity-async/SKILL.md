---
name: unity-async
description: Write performant async Unity code following Tang3cko standards. UniTask, Awaitable, coroutines, caching, pooling. Use when optimizing performance or writing async code. Triggers on "UniTask", "async", "await", "Coroutine", "パフォーマンス", "performance", "pooling", "Profiler", "caching", "最適化", "optimization".
model: sonnet
allowed-tools: Read, Glob, Grep, Edit, Write
---

# Unity Async & Performance

Help users write performant async code and optimize Unity projects following Tang3cko standards.

## Core Principles

1. **UniTask over Coroutine** - Allocation-free, cancellable, structured async
2. **Event-driven over polling** - React to changes instead of checking every frame
3. **Profile before optimizing** - Use Unity Profiler to find actual bottlenecks

## When Invoked

### Step 1: Determine Task Type

- **Writing async code?** → Go to Step 2a
- **Optimizing performance?** → Go to Step 2b
- **Async/performance question?** → Go to Step 2c

### Step 2a: Writing Async Code

1. Load `async.md` for async patterns (UniTask, Awaitable, Coroutine)
2. Determine async approach:
   - New code → UniTask preferred
   - Unity 6+ native → Awaitable
   - Simple delays/sequences → Coroutine acceptable
3. Generate code with proper cancellation handling
4. Verify CancellationToken propagation and error handling

### Step 2b: Optimizing Performance

1. Load `performance.md` for optimization patterns
2. Check common performance issues:
   - Missing caching (GetComponent, Camera.main)
   - Unnecessary allocations in Update loops
   - Polling where events could be used
3. Recommend specific optimizations with code examples
4. Categorize issues by priority (P1/P2/P3)

### Step 2c: Async/Performance Question

1. Load relevant reference based on question domain
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

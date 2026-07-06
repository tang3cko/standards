---
name: unity-core
description: Write Unity C# code following Tang3cko core standards — naming, code organization, error handling, Input System, editor tooling. Use when writing or reviewing Unity C# code structure.
when_to_use: Triggers on "Unity", "C#", "naming", "null check", "InputReader", "namespace", "コード整理", "命名規則", "code style", "error handling".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unity Core Standards

Help users write, review, and refactor Unity C# code following Tang3cko core coding standards.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references   | matching the code domain (naming/comments/error-handling/input/editor-tooling)"
TaskCreate: "Phase 2: Write/review code | follow Core Rules"
TaskCreate: "Phase 3: Self-Check        | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Convention questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the references named by the workflow path | Always |
| 3 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

Apply these to every piece of Unity C# you write, edit, or approve. Detailed rationale lives in references/ — these tables are the contract.

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| Class/Interface | PascalCase, `I` prefix | `PlayerController`, `IDamageable` |
| Private field | camelCase | `playerHealth`, `moveSpeed` |
| Public field/Property | PascalCase | `MaxHealth`, `IsAlive` |
| Method | PascalCase, verb | `TakeDamage()`, `GetInventory()` |
| Boolean | `is/has/can/should` prefix (`was/were/are` allowed in past/state contexts) | `isAlive`, `hasKey`, `canJump` |
| Constant (`const` / `static readonly`) | PascalCase | `MaxHealth`, `DefaultSpeed` |
| Preprocessor symbol | UPPER_SNAKE_CASE | `UNITY_INCLUDE_TESTS` |
| ScriptableObject | `SO` suffix | `EnemyDataSO`, `QuestSO` |
| EventChannel | `EventChannelSO` suffix | `VoidEventChannelSO` |
| Namespace | `ProjectName.Category` | `ProjectName.Core`, `ProjectName.UI` |

### File Structure Order

Members appear in this order inside a class:

1. Fields (`[Header]` groups, then private)
2. Properties
3. Unity lifecycle (Awake → OnEnable → Start → Update → OnDisable → OnDestroy)
4. Public methods
5. Private methods
6. Editor-only (`#if UNITY_EDITOR`, e.g. `OnValidate`)

### Dependency Priority

```
1. Event-based       <- Complete decoupling (recommended)
2. SerializeField    <- Explicit, visible in Inspector
3. Find/GetComponent <- Fallback only
4. Singleton         <- Last resort
```

### Error Handling Essentials

- Null-conditional for optional event raises: `eventChannel?.RaiseEvent();`
- Validate `[SerializeField]` references in `OnValidate()` (editor-only), logging with `[{GetType().Name}]` prefix and `this` context
- Early return on invalid input with `Debug.LogError`; no deep nesting

### Anti-Patterns

Don't: Singleton-by-default, polling in `Update()` when events work, `Find`/`GetComponent` as primary strategy, hidden dependencies.
Do: EventChannel for cross-system communication, cache components in `Awake()`, RuntimeSet instead of `FindObjectsOfType`, validate serialized fields in `OnValidate()`.

## Workflow

Identify the task type and follow the matching path.

### Writing new code

1. Load the references matching the code domain (naming, comments, error-handling, input, editor-tooling)
2. Generate code following the Core Rules above and the loaded standards
3. Run the Self-Check below before presenting

### Reviewing code

1. Check against the Core Rules above plus the matching references
2. Note specific issues with line numbers
3. Categorize issues by priority (must fix, should fix, nice to have)

### A convention question

1. Load the reference matching the question domain
2. Explain with Bad/Good code examples where applicable

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant code.

- [ ] All identifiers match the Naming table (including `SO`/`EventChannelSO` suffixes and boolean prefixes)
- [ ] Members follow the File Structure Order; Unity lifecycle methods in lifecycle order
- [ ] Namespace is `ProjectName.Category` and matches the directory
- [ ] Dependencies use the highest viable rung of the Dependency Priority ladder
- [ ] `[SerializeField]` references validated in `OnValidate()`; external calls null-safe
- [ ] No Anti-Pattern present

## Reference Files

| File | Use When |
|------|----------|
| references/naming.md | Naming conventions in detail |
| references/code-organization.md | Directory structure, namespaces, Runtime/Editor boundary, SO Serializer |
| references/editor-tooling.md | Setup Editor pattern (`BuildAssets` / `WireScene`), idempotence, log suppression |
| references/comments.md | XML docs, Header/Tooltip attributes |
| references/error-handling.md | Null safety, OnValidate |
| references/input-system.md | InputReader pattern |

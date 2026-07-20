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

Null-safe optional event raises (`?.`), serialized references validated in `OnValidate()`, early return over nesting. The full acceptance criteria (try-catch patterns, log levels, Assert usage, context-rich logging) live in `references/error-handling.md` — load it whenever you write error-handling code.

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
- [ ] The references named by the chosen workflow path were actually Read this session — SKILL.md is only the index; the acceptance criteria live in references/, so skipping them means reviewing against an incomplete standard
- [ ] The `## Checklist` at the end of every reference loaded this session was run item-by-item against the work — those checklists are the per-domain definition of done

## Reference Files

Each file below is the only place its listed content exists — SKILL.md deliberately does not restate it.

| File | Load When |
|------|-----------|
| references/naming.md | Any identifier decision beyond the Naming table above. Only source for: event/EventChannel field naming (`on`/`On` + past tense), `RuntimeSetSO` suffix, boolean-prefix rules for parameters and properties |
| references/code-organization.md | Creating files/directories, choosing namespaces, consuming UPM packages, or any editor API use inside runtime classes. Only source for: Runtime/Editor Boundary patterns A–D, Pattern C's five conditions, AssetDatabase allow/escalate table, SO Serializer pattern, private UPM package vendoring |
| references/editor-tooling.md | Writing any `[MenuItem]` setup/scaffolding editor. Only source for: `BuildAssets()`/`WireScene()` phase split, idempotence requirements, scoped log suppression, prefab structure test pattern |
| references/comments.md | Adding XML docs, `[Header]`/`[Tooltip]` attributes, or deciding whether to comment. Only source for: when-NOT-to-document rules, TODO/FIXME/NOTE conventions |
| references/error-handling.md | Writing null checks, try-catch, logging, or `OnValidate()` bodies. Only source for: try-catch patterns for file I/O and `Resources.Load`, log-level guidance, Assert usage |
| references/input-system.md | Any Input System work. Only source for: full `InputReaderSO` implementation, action-map organization and switching, EventChannel bridge, mock-input testing pattern |

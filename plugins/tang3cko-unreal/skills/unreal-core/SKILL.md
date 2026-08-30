---
name: unreal-core
description: Write Unreal Engine C++ following Tang3cko core standards — naming, the reflection contract, pointer and validity rules, class selection, module dependencies, logging and asserts. Use when writing or reviewing Unreal C++ code structure.
when_to_use: Triggers on "Unreal", "UE5", "UPROPERTY", "UFUNCTION", "TObjectPtr", "Actor", "Subsystem", "Build.cs", "命名規則", "コード整理", "GC", "check", "UE_LOG".
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Unreal Core Standards

Tang3cko's Unreal Engine C++ standards for naming, the reflection contract, pointer and validity rules, class selection, module dependencies, logging and asserts. The Core Rules below are always in force; the acceptance criteria live in references/.

The rule broken most often is the Reflection Contract — a `UObject*` member declared without `UPROPERTY()`. It compiles, it runs, and the object is invisible to the garbage collector. It fails later, elsewhere, and usually only in a packaged build. Everything in the Silent Failures table below has the same shape: the compiler is happy and the engine says nothing.

Epic publishes a coding standard (`dev.epicgames.com/documentation/en-us/unreal-engine/epic-cplusplus-coding-standard-for-unreal-engine`) and calls following it mandatory, but that document is almost entirely about formatting. It says nothing about when to use `UPROPERTY`, when a Component beats an Actor, or when to Tick. Those are the rules below.

## Phase Tracking

**At workflow start (writing or reviewing path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references   | reflection-macros.md + memory-and-pointers.md (+ naming/class-design/module/diagnostics/errata as needed)"
TaskCreate: "Phase 2: Write/review code | follow Core Rules"
TaskCreate: "Phase 3: Self-Check        | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Convention questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read `reflection-macros.md` and `memory-and-pointers.md`, plus the references named by the workflow path | Always |
| 3 | Self-Check every written/edited file | Code was written or edited |

## Core Rules (always apply)

Apply these to every piece of Unreal C++ you write, edit, or approve. Detailed rationale lives in references/ — these tables are the contract.

### Naming

| Element | Convention | Example |
|---------|------------|---------|
| `UObject` subclass | `U` prefix, PascalCase | `ULobbySubsystem` |
| `AActor` subclass | `A` prefix | `APlayerCharacter` |
| `SWidget` subclass | `S` prefix | `SLobbyRow` |
| Abstract interface | `I` prefix (paired with a `U`-prefixed `UINTERFACE`) | `IDamageable` / `UDamageable` |
| Other class/struct | `F` prefix | `FLobbyMember` |
| Template | `T` prefix | `TRingBuffer` |
| Enum | `E` prefix, `enum class` | `ELobbyState` |
| Member/local/function | PascalCase, no underscores, no visibility marker | `PlayerHealth`, `TakeDamage()` |
| Boolean | `b` prefix | `bIsAlive`, `bHasKey` |
| Out parameter | `Out` prefix; `bOut` if also boolean | `OutHitResult`, `bOutSucceeded` |
| Macro | UPPER_SNAKE_CASE, `UE_` prefix | `UE_LOG_LOBBY_STATE` |
| Log category | `Log` prefix | `LogLobby` |
| File | Class name without the type prefix | `LobbySubsystem.h` |

Unreal Header Tool requires the type prefixes; they are not decoration. A `typedef` of a template instantiation is no longer a template and takes the prefix of what it became.

Visibility never changes a name: a private member is PascalCase like any other. Public members are declared first, private last.

### Reflection Contract

| Rule | Consequence of breaking it |
|------|---------------------------|
| Every `UObject*` member that must keep its target alive carries `UPROPERTY()` | The reference is invisible to the GC: not counted, not nulled |
| A `UObject` pointer that deliberately must *not* keep its target alive uses `TWeakObjectPtr`, never a raw pointer | Dangling pointer with no way to test it |
| Use `TObjectPtr<T>` for `UPROPERTY` members and containers, not `T*` | UHT can be configured to reject raw pointers; you also lose editor access tracking |
| Construct `UObject`s with `NewObject` at runtime, `CreateDefaultSubobject` in constructors — never `new` | Memory corruption; the engine owns this memory |
| `UObject` constructors set defaults and subobjects only; real initialization goes in `BeginPlay` | Constructors run at engine startup against the CDO |
| Pass `UObject`s by pointer, never by reference | Epic coding standard |
| Lambdas capture explicitly; never `[&]` or `[=]` when a `UObject` is in scope | Captured `UObject` pointers are invisible to the GC |
| Deferred lambdas use `CreateWeakLambda` / `CreateSPLambda` | The lambda outlives its captures |

`UPROPERTY(Transient)` is about serialization only. It does **not** exempt the member from garbage collection — the reference is still strong.

### Class Selection Ladder

Take the lowest rung that does the job.

```
1. USTRUCT / plain F-struct  <- Data with no identity, no lifetime, no reflection calls
2. UObject                   <- Needs reflection, GC, or Blueprint exposure; does not tick
3. UActorComponent           <- Reusable behaviour attached to an Actor; no transform
4. USceneComponent           <- The behaviour needs a transform
5. AActor                    <- Needs to exist in the world, be spawned, or replicate
6. UWorldSubsystem           <- One instance per world, torn down with the level
7. UGameInstanceSubsystem    <- One instance surviving level travel
```

Authority decides placement before convenience does: server-only rules go in the GameMode, which exists only on the server; state every machine needs goes in the GameState; per-player state goes in the PlayerState. A PlayerController exists on the server and on its owning client, and nowhere else.

Subsystems exist to avoid bolting more API onto an already busy class, and to avoid subclassing engine classes. Reach for one before inventing a manager Actor or a singleton. See `references/class-design.md` for the five subsystem lifetimes and `references/official-doc-errata.md` for where the official documentation undercounts them.

### Silent Failures

Each of these compiles, runs, and reports nothing.

| What you write | What actually happens |
|----------------|----------------------|
| `UObject*` member without `UPROPERTY()` | Collected out from under you |
| `SetActorTickEnabled(true)` alone | No-op unless `PrimaryActorTick.bCanEverTick` was set in the constructor |
| Logic split across Actor and Component tick | They default to different tick groups — `TG_PrePhysics` and `TG_DuringPhysics` |
| `check()` wrapping an expression with side effects | The expression vanishes in Test and Shipping |
| Behaviour that depends on a log firing | `UE_LOG` below Fatal compiles out in Test and Shipping |
| `CastChecked` as a safety net | Unchecked cast in Shipping; use `Cast` and branch |
| `GetWorld()` assumed non-null | Null on a CDO, and on an Actor whose Level is gone |
| `TWeakObjectPtr::IsValid()` off the game thread | Wrong answer during the GC mark phase |
| Adding a Component during play as a matter of course | Registration is not free |

### Module Dependencies

Public dependencies propagate to everything that depends on you; private ones do not. **Private is the default** — put a module in `PublicDependencyModuleNames` only when a *public header* of yours names one of its types, and prefer a forward declaration so it can stay private. Verify with a non-unity build rather than by reading the file.

### Formatting Essentials

Braces on their own line, always, including single-statement blocks. Tabs, width 4 — in C# too. `#pragma once`. Include what you use and never rely on an indirect include. `TEXT()` around every string literal. No `auto` and no structured bindings, except for binding a lambda, a verbose iterator type, or a type that cannot be spelled in template code. Never `const` on a return type. `switch` labels fallthrough explicitly and always has a `default`.

### Anti-Patterns

Don't: raw `UObject*` members, `new` on a `UObject`, logic accumulating in an Actor that a Component could own, a manager Actor where a Subsystem fits, singletons, `Tick` as the default for periodic work, `meta=` read from game logic, standard-library containers and strings outside interop code, namespaces around reflected types, `using` at global scope.

Do: `TObjectPtr` in reflected members, `IsValid()` before touching a `UObject` you did not just create, Components for reusable behaviour, Subsystems for global access with a defined lifetime, `ensure` for recoverable surprises and `check` for broken invariants, private module dependencies.

## Workflow

Identify the task type and follow the matching path.

### Writing new code

1. Load `reflection-macros.md` and `memory-and-pointers.md` — every reflected member and every `UObject` pointer is judged against them
2. Load the references matching the code domain:
   - Choosing a base class, a subsystem lifetime, or when to Tick → `class-design.md`
   - Identifier decisions beyond the Naming table → `naming.md`
   - Touching `Build.cs`, includes, or module layout → `module-and-includes.md`
   - Writing logs, `check`/`ensure`, or log categories → `diagnostics.md`
   - Any claim sourced from the official documentation → `official-doc-errata.md`
3. Generate code following the Core Rules above and the loaded standards
4. Run the Self-Check below before presenting

### Reviewing code

1. Load `reflection-macros.md`, `memory-and-pointers.md`, and `class-design.md`
2. Check against the Core Rules above and the matching references' `## Checklist`
3. Note specific issues with file and line
4. Categorize issues by priority (must fix, should fix, nice to have)

### A convention question

1. Load the reference matching the question domain; if the answer leans on a documentation page, load `official-doc-errata.md` too
2. Explain with Bad/Good code examples where applicable

## Self-Check (before presenting generated/edited code)

Verify every file you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant code.

- [ ] All identifiers match the Naming table, including type prefixes, `b` on booleans and `Out` on out parameters
- [ ] Public interface is declared before private implementation
- [ ] Every `UObject` reference is either a `UPROPERTY` or a deliberate `TWeakObjectPtr`, and reflected members use `TObjectPtr`
- [ ] The class sits on the lowest viable rung of the Class Selection Ladder, and its placement matches where authority lives
- [ ] Nothing in the Silent Failures table is present
- [ ] `Build.cs` dependencies are private unless a public header forces otherwise
- [ ] No Anti-Pattern present
- [ ] The references named by the chosen workflow path were actually Read this session — SKILL.md is only the index; the acceptance criteria live in references/, so skipping them means reviewing against an incomplete standard
- [ ] The `## Checklist` at the end of every reference loaded this session was run item-by-item against the work — those checklists are the per-domain definition of done

## Reference Files

Each file below is the only place its listed content exists — SKILL.md deliberately does not restate it.

| File | Load When |
|------|-----------|
| references/reflection-macros.md | Any `UPROPERTY` / `UFUNCTION` / `UCLASS` / `USTRUCT` / `UINTERFACE` declaration. Only source for: specifier selection, `BlueprintPure` not caching, the U/I interface pair, delegate declaration, why `meta=` must never be read at runtime |
| references/memory-and-pointers.md | Any `UObject` pointer, container of pointers, or lambda capturing one. Only source for: the pointer decision table, the wall between `UObject` and the smart-pointer library, the `IsValid` family, `FGCObject` and `TStrongObjectPtr`, `TObjectPtr`'s write barrier, asset references |
| references/class-design.md | Choosing a base class, a subsystem lifetime, or whether to Tick. Only source for: the authority map, the five subsystem lifetimes, `ShouldCreateSubsystem`, Actor initialization order, Tick discipline, the four ways `GetWorld()` returns null, USTRUCT and interface rules |
| references/naming.md | Any identifier decision beyond the Naming table above. Only source for: the `In` prefix, the `Handle`/`Process` ban, member order, log category naming, constants and namespaces |
| references/module-and-includes.md | Touching `Build.cs`, includes, or module layout. Only source for: the public/private dependency procedure, the directory meanings, why `PCHUsage` is not the IWYU switch the documentation implies, the two `Engine.h` files, LoadingPhase, and what a "pure logic core" costs |
| references/diagnostics.md | Writing `UE_LOG`, declaring a category, or choosing between `check` / `verify` / `ensure`. Only source for: verbosity selection, category declaration with a compile-time ceiling, the build-configuration matrix for `DO_CHECK` / `DO_ENSURE` / `NO_LOGGING`, the check/verify/ensure decision flow |
| references/official-doc-errata.md | Any claim or code sample sourced from the official documentation. Only source for: the six places the documentation contradicts or misleads against 5.8 engine source, and what the source actually says |

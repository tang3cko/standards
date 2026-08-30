# Logging and Asserts

The single fact that governs this whole file: **most of what you write here disappears in Test and Shipping builds.** Code that depends on a log firing or an assert evaluating is code that behaves differently in the build you ship.

Engine paths below are relative to `Engine/Source/` in an installed 5.8, with symbol names in place of line numbers — re-locate them in the source you actually have rather than trusting a line number.

## Build configuration matrix

All from `Runtime/Core/Public/Misc/Build.h`, one `#if UE_BUILD_DEBUG` / `#elif UE_BUILD_DEVELOPMENT` / `#elif UE_BUILD_TEST` / `#elif UE_BUILD_SHIPPING` block per configuration, each guarding the same `#ifndef`/`#define` for the macro named in the row.

| | Debug | Development | Test | Shipping |
|---|---|---|---|---|
| `DO_GUARD_SLOW` (the `Slow` variants) | 1 | 0 | 0 | 0 |
| `DO_CHECK` | 1 | 1 | `USE_CHECKS_IN_SHIPPING` | `USE_CHECKS_IN_SHIPPING` |
| `DO_ENSURE` | 1 | 1 | `USE_ENSURES_IN_SHIPPING` | `USE_ENSURES_IN_SHIPPING` |
| `NO_LOGGING` | 0 | 0 | `!USE_LOGGING_IN_SHIPPING` | `!USE_LOGGING_IN_SHIPPING` |

The three switches those cells defer to are defined earlier in the same file, each in its own independent `#ifndef` guard: `USE_LOGGING_IN_SHIPPING` defaults to `0`, `USE_CHECKS_IN_SHIPPING` defaults to `0`, and `USE_ENSURES_IN_SHIPPING` defaults to whatever `USE_CHECKS_IN_SHIPPING` is.

**Ship with the defaults.** Turning checks on in Shipping is a debugging measure, not a hardening one — and the Asserts documentation page agrees: "Projects should ship with USE_CHECKS_IN_SHIPPING set to its default value of 0." (`dev.epicgames.com/documentation/en-us/unreal-engine/asserts-in-unreal-engine`)

Read the Test column carefully — Test is not Development. Anything you rely on for a soak test or a performance run is already gone by then. The Asserts documentation page states otherwise for `verify`; see `official-doc-errata.md`.

## `check` versus `verify` versus `ensure`

```
Is the condition a broken invariant that means the program is wrong?
├─ Yes → Does the expression have side effects, or is its result needed?
│        ├─ No  → check(Expr)          removed entirely when disabled
│        └─ Yes → verify(Expr)         expression still evaluated when disabled
└─ No  → Is it a surprise you can recover from and want reported once?
         ├─ Yes → ensure(Expr) / ensureMsgf(Expr, ...)
         └─ No  → handle it as normal control flow, not an assert
```

`check` is the default. Use `verify` **only** because the expression must still run — the classic case is `verify(Thing->Initialize())`, where writing `check` would skip the initialization in Shipping and silently change behaviour.

`ensure` evaluates in every configuration but only reports to the crash reporter in Debug, Development, Test and Shipping-Editor. It reports **once per session** to avoid flooding; use the `Always` variants when you genuinely need every occurrence. Because it returns its condition, it reads well as a guard:

```cpp
if (ensureMsgf(bWasInitialized, TEXT("%s ticked before initialization"), *GetName()))
{
    // recover
}
```

Specialised forms worth knowing: `checkNoEntry()` for unreachable code, `checkNoReentry()` for code that must run once, `unimplemented()` for a virtual that should have been overridden. All are declared alongside `check`/`verify`/`ensure` in `Runtime/Core/Public/Misc/AssertionMacros.h`.

Never put a side effect inside `check`. Never `checkSlow` anything you need outside Debug.

## Verbosity

| Verbosity | Console | Log file | Use for |
|---|---|---|---|
| `Fatal` | yes | yes | Crashes the session. Fires even when logging is compiled out |
| `Error` | yes | yes | A commandlet or editor run should be considered failed |
| `Warning` | yes | yes | Something is wrong but the run continues |
| `Display` | yes | yes | Deliberately visible to whoever is watching the console |
| `Log` | **no** | yes | The default working verbosity. File only |
| `Verbose` | no | no | Enabled per-category on demand |
| `VeryVerbose` | no | no | Detail that would otherwise spam |

`Log` not reaching the console is the usual cause of "my tool printed nothing". Running a commandlet without `-stdout -FullStdOutLogOutput` will swallow it entirely, and the tool exits 0 — success and silence look identical.

Under `NO_LOGGING`, `UE_LOG` expands to a `if constexpr` that keeps only the `Fatal` branch. The arguments are not evaluated. A log line that also does work is a log line that stops doing that work in Shipping.

## Declaring a category

```cpp
// LobbyLog.h
DECLARE_LOG_CATEGORY_EXTERN(LogLobby, Log, All);

// LobbyLog.cpp
DEFINE_LOG_CATEGORY(LogLobby);
```

The second parameter is the runtime default verbosity; the third is the **compile-time ceiling**. Anything above the ceiling is not compiled in at all, and the macro `static_assert`s if the default exceeds the ceiling. `All` as the ceiling keeps `Verbose` available to `-LogCmds=` at runtime.

Pick the declaration form by where the category should be visible; the doc comment on each macro in `Runtime/Core/Public/Logging/LogCategory.h` says so directly:

| Form | Comment in the header | Use for |
|---|---|---|
| `DECLARE_LOG_CATEGORY_EXTERN` + `DEFINE_LOG_CATEGORY` | "declared in the header and paired with DEFINE_LOG_CATEGORY in the source. Accessible by all files that include the header." | The normal case |
| `DEFINE_LOG_CATEGORY_STATIC` | "This should ONLY be declared in a source file. Only accessible in that single file." | A category confined to one `.cpp` |
| `DECLARE_LOG_CATEGORY_CLASS` + `DEFINE_LOG_CATEGORY_CLASS` | "declare a logging category as a C++ "class static"" | A category that belongs to one class |

Both `DECLARE_LOG_CATEGORY_EXTERN` and `DECLARE_LOG_CATEGORY_CLASS` expand through the same private `UE_PRIVATE_DEFINE_LOG_CATEGORY` helper macro; the class form declares the resulting `FLogCategory##CategoryName` as a `static` member instead of a free `extern`, and its partner `DEFINE_LOG_CATEGORY_CLASS` defines it as `Class::FLogCategory##CategoryName Class::CategoryName;`. Do not reach for it merely to keep a category out of other files — that is `DEFINE_LOG_CATEGORY_STATIC`'s job.

One category per feature, named `Log` + the feature: `LogLobby`, `LogMenu`. Not one per class, and not one per module unless the module is the feature. This naming rule is canonical here — `naming.md` only points back to it.

`COMPILED_IN_MINIMUM_VERBOSITY` can lower the ceiling globally, but only in monolithic builds — the engine `#error`s otherwise.

## `UE_LOGFMT`

Structured logging, available since 5.2, needs `#include "Logging/StructuredLog.h"`. It takes positional or named parameters but **cannot mix the two styles**. Field names must match `[A-Za-z0-9_]+` and be unique within the event. Up to sixteen fields.

Prefer it where the log will be queried rather than read. Plain `UE_LOG` is fine for the rest.

## Checklist

- [ ] No `check` wraps an expression with a side effect or a needed return value — those are `verify`
- [ ] No behaviour depends on a log line executing, and no log argument performs work
- [ ] `ensure` is used for recoverable surprises, not as a softer `check` for genuine invariants
- [ ] Every new log category is declared with an explicit compile-time ceiling and named `Log` + feature
- [ ] Commandlets and tools that must print run with `-stdout -FullStdOutLogOutput`, or log at `Display` or above
- [ ] `USE_CHECKS_IN_SHIPPING` and `USE_LOGGING_IN_SHIPPING` are left at their defaults
- [ ] Anything relied upon during a Test-configuration run was checked against the build matrix above, not assumed to survive from Development

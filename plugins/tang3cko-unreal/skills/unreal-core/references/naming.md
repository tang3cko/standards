# Naming

Load this for any identifier decision beyond the table in SKILL.md.

## Beyond the prefix table

A `typedef` of a template instantiation is no longer a template, so it takes the prefix of the thing it became — `typedef TArray<FMyType> FArrayOfMyTypes;`, not `T`.

Disambiguate a template parameter from an alias with an `In` prefix.

Types and variables are nouns. Methods are verbs — either what the method does, or what it returns when it does nothing. A function returning `bool` asks a true/false question: `IsVisible()`, `ShouldClearBuffer()`.

**Avoid names beginning with `Handle` or `Process`.** Both verbs are ambiguous; they describe that something happens, not what. The one accepted exception is delegate handlers, where `HandleLobbyChanged` reads as "this is the response to that event" — keep that use narrow and name everything else for its effect.

Avoid anonymous literals at call sites. A named constant that says what the number means beats a comment explaining it.

Declare one variable per line, so each can carry its own comment.

## Log categories

Naming form and declaration mechanics are covered in `diagnostics.md` ("Declaring a category") — this file does not repeat them. In short: `Log` + the feature, e.g. `LogLobby`, `LogMenu`.

## Constants

Epic's coding standard does not name a convention for constants. Use PascalCase like any other identifier, and prefer `inline constexpr` at namespace scope in a `UE::` namespace over a `#define`. Macros cannot live in a namespace, which is exactly why they carry the `UE_` prefix instead.

## Namespaces

**Unreal Header Tool does not support namespaces.** Do not put a `UCLASS`, `USTRUCT` or `UENUM` inside one — attempting it is a UHT parse error, not a warning. New API that is not reflected should go in a `UE::` namespace, the convention the engine itself uses throughout `Runtime/Core`.

Never put a `using` declaration at global scope, not even in a `.cpp` — it breaks the unity build.

## Member order

Public interface first, private implementation last. The Coding Standard (`dev.epicgames.com/documentation/en-us/unreal-engine/epic-cplusplus-coding-standard-for-unreal-engine`) states the reasoning explicitly: "Classes should be organized with the reader in mind rather than the writer. Since most readers will use the public interface of the class, the public implementation should be declared first, followed by the class's private implementation."

Nothing flags a violation, so it is worth checking deliberately in review.

## Visibility never changes a name

A private member is named exactly as a public one would be. There is no camelCase tier, no leading underscore, no `m_`. `PlayerHealth` is `PlayerHealth` wherever it sits.

## Data assets

There is no suffix convention for a `UDataAsset` or `UPrimaryDataAsset` subclass — it takes the ordinary `U` prefix and a descriptive name: `ULobbySettings`, not `ULobbySettingsData`. Name it for what it configures.

## Checklist

- [ ] Every type carries the prefix its base class demands, and typedefs carry the prefix of what they became
- [ ] Booleans use `b`, not `is`/`has`/`can`
- [ ] Private members are PascalCase, not camelCase
- [ ] Public members and methods are declared before private ones
- [ ] No `Handle`/`Process` name outside a delegate handler
- [ ] Out parameters carry `Out`, and boolean out parameters carry `bOut`
- [ ] No reflected type sits inside a namespace, and no `using` sits at global scope
- [ ] New log categories follow `diagnostics.md`'s naming and declaration rules, not a local reinvention
- [ ] File names drop the type prefix

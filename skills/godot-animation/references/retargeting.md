# Two-Sided Retarget Setup

Full account of the failure log that led to this pipeline, and the setup steps for the pipeline itself.

## Why two-sided normalization

Godot's official retarget mechanism (`BoneMap` + `SkeletonProfileHumanoid` + `retarget/rest_fixer`) is designed to make rigs sharing **the same convention** interchangeable — it normalizes bone naming and, critically, rest *rotation*. Applying it to only one side (e.g. normalizing the animation source but leaving the character rig in its native naming/rest convention) leaves a rest-pose mismatch that no per-bone correction formula can close in general, because a pure rotation cannot represent every possible rest-pose relationship between two conventions (see the mirror-reflection case below).

Applying it to **both** sides means both skeletons end up exposing the same canonical bone names under matching per-bone rest rotations. At that point retargeting a clip is just copying `ROTATION_3D`/`POSITION_3D` keys verbatim from the source track to a same-named target track (only the NodePath prefix changes) — no math, no per-chain exceptions.

## The failure log (five attempts, in order)

| # | Technique | Result |
|---|---|---|
| 1 | Raw track copy by bone name (absolute local rotation copied as-is) | Numeric check ("pose differs from rest") passed, but limbs rendered twisted/collapsed. Root cause: matching bone names did not mean matching rest rotations |
| 2 | Delta-from-own-rest (`delta = srcRest⁻¹·srcPose(t)`; `tgtPose(t) = tgtRest·delta`) | Non-zero deltas confirmed motion, but the arm chain broke specifically — source and target rigs had different stances (A-pose vs T-pose, tens of degrees apart), which the delta formula's "both rests represent the same real stance" assumption doesn't hold under |
| 3 | Arm-chain exception added to #2 (copy arms raw, delta everything else) | Fixed the arms, but introduced a new class of bug: leg/ankle chains showed large numeric deltas ("bent 90 degrees") while rendering as still standing straight — a mismatch between numeric and visual truth |
| 4 | Same technique (#2 + #3) reapplied to a second, different character rig | The precondition used to justify skipping normalization ("bone names are 100% identical, so no retarget needed") turned out to be false — identical names, mismatched rest poses. Raw copy on this rig broke bodies by ~90 degrees |
| 5 | Extended the raw-copy exception from arms to legs | Made it worse (limbs read as disconnected), not better. Reverted |
| — | (untracked mid-session attempts) Global-rest-aligned FK synthesis | Premise ("same-named bones share a global rest orientation") measured false |
| — | (untracked mid-session attempts) Per-bone axis-correction conjugation (`M·delta·M⁻¹`) | Fixed legs/spine, but one rig's left/right clavicle rest rotations were in a **mirror-reflection** relationship to each other — no single conjugation matrix can correct both arms at once, because reflection is not a rotation. This is the finding that ended the ad-hoc-math approach entirely |

**General lesson**: bone-name equality is a necessary but not sufficient condition for safe track copying, in any form (raw or delta). Verify rest-pose numeric equality (or apply official normalization) before trusting a name-based bone correspondence — this failure repeated across two unrelated character rigs before the two-sided fix was adopted.

## Setup steps

1. **Confirm the target canonical profile**: `SkeletonProfileHumanoid`.
2. **Per source rig, build a `BoneMap`**: map each source skeleton's native bone name (whatever its naming convention — colon-prefixed, PascalCase, or another studio's convention) to the matching `SkeletonProfileHumanoid` canonical name (`Hips`, `Spine`, `LeftUpperArm`, `LeftUpperLeg`, etc.). Save as a `.tres`; one BoneMap can usually be reused across every asset that shares the source rig's naming convention.
3. **Apply to both `.import` files** — the character's and the animation source's:
   - `retarget/bone_map` → the `.tres` from step 2
   - `retarget/rest_fixer/normalize_position_tracks = true`
   - `reset_all_bone_poses_after_import = true`
   - `retarget_method = 1`
4. **Reimport both**, then verify (references/verification.md) that both sides now expose **identical per-bone rest rotations** under the shared canonical names before writing any extraction code. Only bone *positions* (proportions — arm/leg length differences between characters) are expected to differ; rotations should match.
5. **Extract with a verbatim per-track copy**: for each `ROTATION_3D`/`POSITION_3D` track on the source animation, create a same-typed track on the output addressed to the same canonical bone name (only the NodePath's node prefix changes, e.g. from the source scene's skeleton node name to the target scene's), and copy every key's time/value pair unchanged. No per-bone math.
6. **Root motion**: check whether the source clip's root-motion bone position track is stationary (in-place) or accumulates over the clip (baked root motion). If a looping locomotion clip accumulates position on the movement axis and movement is otherwise code-driven, zero only the accumulating axis/axes at extraction time — verify which axis by sampling first/last-key values directly rather than assuming; incidental sway on a non-movement axis should be kept.

## Extraction script shape

A one-shot headless script (run via the engine's script-execution mode) that:

1. Instantiates the source animation scene, locates its `AnimationPlayer`
2. For each configured clip: reads every track, copies rotation/position keys verbatim onto a freshly built `Animation`, rewrites only the NodePath's skeleton-node prefix
3. Adds each result to an `AnimationLibrary` and saves it as a project resource

Keep the script idempotent (safe to rerun, overwrites the same output path) and keep the "why" comments about rest-pose verification and root-motion axis findings inline — the next person extending it to a new clip/rig needs the same rest-pose check repeated, not assumed.

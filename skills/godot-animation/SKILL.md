---
name: godot-animation
description: Retarget humanoid animation FBX (Mixamo, Synty ANIMATION packs, etc.) onto Godot 4.x character rigs following Tang3cko standards — official BoneMap/rest_fixer normalization on both sides, banned ad-hoc retarget math, and the visual verification steps numeric gates alone miss. Use when importing or retargeting character animation, wiring BoneMap resources, or debugging a broken/twisted retargeted pose.
when_to_use: Triggers on "retarget", "リターゲット", "BoneMap", "SkeletonProfileHumanoid", "Mixamo", "Synty", "ANIMATION pack", "rig", "skeleton", "bone map", "rest pose", "rest_fixer", "アニメーション", "ボーンマップ", "レストポーズ", "スケルトン".
model: sonnet
allowed-tools: Read Glob Grep Edit Write TaskCreate TaskUpdate TaskList
---

# Godot Animation Retargeting

Help users import, retarget, and verify humanoid character animation in Godot 4.x + .NET projects following Tang3cko standards.

## Phase Tracking

**At workflow start (retargeting or debugging path), create tasks for each phase:**

```
TaskCreate: "Phase 1: Load references   | matching the task (retargeting/verification/gotchas)"
TaskCreate: "Phase 2: Apply/diagnose    | follow Core Rules; no ad-hoc per-bone math"
TaskCreate: "Phase 3: Verify            | numeric gate AND rendered-PNG visual check"
TaskCreate: "Phase 4: Self-Check        | MUST pass every item before presenting"
```

Update status as you progress: `in_progress` when starting, `completed` when done. Convention questions skip Phase Tracking.

**Execution checklist (verify at end):**

| Phase | Must Execute | Condition |
|-------|-------------|-----------|
| 1 | Read the references named by the workflow path | Always |
| 3 | Report both a numeric-gate result and a visually-inspected rendered frame | A retarget was produced or changed |
| 4 | Self-Check every produced BoneMap/import setting/track-copy script | Retarget config was written or edited |

## Core Rules (always apply)

Detailed rationale and the full failure log live in references/ — these tables are the contract.

### 1. Two-sided normalization is the only stable solution

Apply Godot's official `retarget/bone_map` + `retarget/bone_renamer` + `retarget/rest_fixer` (profile: `SkeletonProfileHumanoid`) to **both** the character FBX's `.import` and the animation FBX's `.import` — never one side only. Once both sides expose the same canonical bone names under matching rest rotations, extraction is a verbatim per-track copy (`ROTATION_3D`/`POSITION_3D` keys copied as-is, only the NodePath prefix rewritten) — **zero custom math**. If a custom per-bone formula (delta-from-rest, axis-conjugation, chain exceptions) seems necessary, that is a signal the normalization step was skipped or misconfigured, not a problem to solve with more math.

### 2. Banned ad-hoc retarget techniques (graveyard)

| Technique | Why it fails |
|---|---|
| Raw track copy by matching bone name | Bone-name equality does not imply rest-pose equality; copying the source's absolute local rotation onto a target bone with a different rest rotation twists/collapses the limb even though names line up 1:1 |
| Delta-from-own-rest (`delta = srcRest⁻¹·srcPose`; `tgtPose = tgtRest·delta`) | Assumes both rigs' rest poses represent the same real-world stance. Breaks across an A-pose vs T-pose source/target mismatch (tens of degrees off) |
| Per-chain exceptions (e.g. copy arms raw, delta everything else) | Patches one symptom while leaving others (numerically "bent" joints that render as still standing straight); accumulates into a maze of special cases that re-breaks on the next new rig |
| Per-bone axis-correction conjugation (`M·delta·M⁻¹`) | A pure rotation cannot represent a **mirror-reflection** relationship between a rig's left/right shoulder (clavicle) rest rotations — no conjugation matrix fixes both arms simultaneously |

If a fix under consideration resembles any row above, stop and apply Core Rule 1 instead.

### 3. Distrust marketing compatibility claims until verified in-engine

Vendor compatibility labels and "included in [Engine] collection" listings describe **marketing/packaging categories**, not verified rig data. Treat them as provisional until confirmed by dumping the actual skeleton:

- A pack's presence in an engine-specific storefront collection does not guarantee the animation clips' rest poses are externally referenceable — some animation packs bake their rest pose into the first frame of every clip (a known Godot limitation, godot#89244), which breaks the assumption Godot's automatic retarget path relies on
- A product line advertised as sharing one compatible rig can, in the same line, mix multiple **incompatible** skeletons under one marketing name
- Bone-name overlap alone (even 100%) is not compatibility — confirm rest-pose numbers match (references/verification.md)

### 4. Secondary bug checklist (distinct from retarget math, still bites)

- **Hand-authored `MeshInstance3D` missing an explicit `skeleton` NodePath**: Godot's importer auto-wires this; a hand-built `.tscn` placing `MeshInstance3D` under `Skeleton3D` does not get it for free. Symptom: the skeleton's bone poses update correctly (`get_bone_pose()` confirms it) but the mesh stays frozen in bind pose — **no error, no warning**. Only a rendered frame reveals it
- **Skin bind pose not recomputed after rest changes**: regenerating a character scene on a normalized skeleton without recomputing the skin's bind pose from the new rest warps the mesh
- **Mesh forward axis mismatch**: don't assume `-Z` forward. Confirm the mesh's un-rotated forward axis by rendering, not by convention — a `+Z`-forward mesh under `-Z`-forward logic (e.g. `LookAt`) walks backward, and any seat/placement transform tuned for `-Z` forward doubles the error
- **Clip length vs. actual hold/stay duration**: an `AnimationFinished`-triggered transition never fires if the gameplay state holding the animation ends before the clip's full length. Verify the numeric relationship (clip length vs. state duration), not just that the clip "looks fine" in isolation

### 5. Verification: numeric gate AND visual PNG, both required

A numeric gate ("pose differs from rest") only proves motion happened — it cannot detect a twisted/collapsed limb, since a broken pose still has nonzero deltas. Every retarget change requires both:

1. A numeric pose gate (position ratios AND orientation, not size alone — e.g. hip-height ratio, knee spread, ankle-in-front-of-hip)
2. A rendered PNG, opened and visually inspected (front **and** side angle; multiple time points for loops) — never treat "the render script exited 0" as inspection

See the godot-testing skill for the full three-stage gate design (numeric regression → visual → in-game live flow).

## Workflow

Identify the task type and follow the matching path.

### Setting up a new retarget (character + animation source)

1. Load references/retargeting.md
2. Dump both rigs' skeletons and compare rest poses per bone before writing any BoneMap (references/verification.md) — never assume from bone-name overlap alone
3. Build a `BoneMap` (`.tres`) from source bone names to `SkeletonProfileHumanoid` canonical names for each side that needs it
4. Set `retarget/bone_map`, `retarget/rest_fixer/normalize_position_tracks`, `reset_all_bone_poses_after_import`, and `retarget_method` on both the character's and the animation's `.import` files
5. Extract clips as a verbatim per-track copy once both sides confirm matching canonical rest rotations (Core Rule 1) — write no per-bone correction math
6. Run the Self-Check below before presenting

### Debugging a broken/twisted retargeted pose

1. Load references/verification.md and references/gotchas.md
2. Dump and compare rest poses for the affected bones first (Core Rule 3) — do not assume the existing BoneMap is correct because it was correct for a different rig
3. Check whether the current implementation matches a row in the Banned Techniques table (Core Rule 2); if so, replace it with two-sided normalization rather than patching the formula
4. If retarget math is confirmed already correct, walk the Secondary Bug Checklist (Core Rule 4) before suspecting the retarget again
5. Re-verify with both a numeric gate and a rendered PNG (Core Rule 5)

### A convention or compatibility question

1. Load the reference matching the question domain
2. Answer with the relevant Core Rule; flag any vendor compatibility claim in the question as provisional per Core Rule 3

## Self-Check (before presenting generated/edited retarget config or scripts)

Verify every file/config you wrote or changed against each item. If any check fails, fix it first — do not present non-compliant work.

- [ ] Both the character's and the animation's `.import` have `retarget/bone_map` + `retarget/rest_fixer` set to a shared `SkeletonProfileHumanoid`-based BoneMap
- [ ] No per-bone custom math (delta-from-rest, axis conjugation, chain exceptions) remains — extraction is verbatim track copy
- [ ] Rest poses were numerically compared before trusting a bone-name match (Core Rule 3)
- [ ] Every `MeshInstance3D` under a `Skeleton3D` has an explicit `skeleton` NodePath
- [ ] A numeric pose gate AND a visually-inspected rendered PNG both back the verification claim
- [ ] Any `AnimationFinished`-triggered transition was checked against the actual hold/stay duration, not assumed

## Reference Files

| File | Use When |
|------|----------|
| references/retargeting.md | Two-sided BoneMap/rest_fixer setup steps, the banned-technique failure log in full, extraction script shape |
| references/verification.md | Skeleton dump / rest-pose comparison procedure, numeric pose gates, rendered-PNG check, verification ordering |
| references/gotchas.md | Secondary bugs: skeleton NodePath omission, skin bind pose recompute, mesh forward axis, clip-length-vs-duration |

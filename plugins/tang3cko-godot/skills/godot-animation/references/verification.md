# Verification: Dumps, Rest-Pose Comparison, Gates, Visual Checks

Procedures for judging whether a rig/retarget/asset is actually correct, not just plausible. Ordered from cheapest/earliest to most expensive/final.

## 1. Skeleton dump before committing to a rig

Before buying or wiring up an FBX rig, dump its skeleton headlessly (a background import of the FBX that walks the armature and prints each bone's name, parent, local head position, and local rest rotation as a quaternion). This takes seconds and needs no GUI.

Use it to answer, cheaply, before any retarget work starts:

- How many bone names actually overlap between two rigs (not "should overlap based on the product description" — the actual count)
- Whether a rig is in T-pose or A-pose (matters for rest_fixer settings)
- Whether a newly acquired rig has a sane, consistent bone count/naming scheme at all

## 2. Rest-pose numeric comparison (don't trust name matches)

**Always do this before trusting "bone names match" as proof of compatibility.**

Take two skeleton dumps and, for every bone name common to both:

- Compare `head_local` position (report the delta in real-world units, e.g. centimeters — a large delta like tens of centimeters on a hand bone is immediately suspicious)
- Compare rest rotation as a quaternion difference, **mirror-tolerant**: since `q` and `-q` represent the same rotation, compute `min(max(|q1 - q2|), max(|q1 + q2|))` component-wise rather than a naive subtraction, or the comparison will report false mismatches

A rig pair can show 100% bone-name overlap and still have the large majority of those bones fail the rest-rotation comparison — name overlap and rest-pose compatibility are independent facts; check both.

**T-pose shortcut**: if both hands' `head_local` sit at shoulder height, spread wide and roughly symmetric left/right, the rig is in T-pose. Rigs whose rest pose is instead baked into the first frame of their animation clips (rather than kept as a separate referenceable rest) will fail this check in a characteristic way — that in turn is a sign the standard automatic retarget path doesn't apply cleanly and both-side normalization needs to be set up explicitly.

## 3. In-engine bone/track sampling

After import or after a retarget change, sample the actual runtime data to localize where a pose was lost:

- Enumerate the `Skeleton3D`'s bones and query each bone's pose rotation at a specific animation time
- Read an `Animation` resource's tracks directly (track path, key values) rather than trusting the editor preview

**Important caveat**: the engine's rotation tracks typically set an **absolute local rotation** directly, not a delta from rest. That means "this track's value differs from rest" proves nothing about correctness on its own — a badly twisted pose still has a nonzero difference from rest. Numeric track sampling must be paired with a check on the actual position/orientation semantics (gate #5), not read alone as a pass/fail signal.

## 4. AABB / scale sanity check

Before placing any new asset, compute its actual bounding box rather than trusting the asset's name or your mental model of its size ("wall", "table"). A one-line headless enumeration of each mesh's AABB catches gross scale mistakes (an asset several meters larger or smaller than assumed) before they're baked into level layout, and can also rule out the need for a manual scale correction by confirming the asset is already at the expected real-world scale.

## 5. Numeric pose gates (size AND orientation, always paired)

Design principle: **add a gate for every category of broken pose that has actually slipped through**, not a speculative exhaustive set up front.

A gate must constrain both magnitude and direction/orientation — a magnitude-only gate (e.g. "knee bend angle is in range") will pass a knee bent sideways instead of forward. Pair every magnitude gate with an orientation gate that would catch the sideways case.

Typical categories worth gating for a seated/locomoting humanoid: overall silhouette (limbs not colliding with themselves), seated hip-height ratio vs. standing, knee flexion range together with "ankle is in front of hip, not behind" and "foot is below knee", knee spread bounded by shoulder width, and — for locomotion — left/right ankle Z crossing over time (proof a stride actually alternates, not a slide).

## 6. Rendered PNG — required, not optional, after every gate pass

Numeric gates alone cannot detect torsion/twisting that keeps positions and angles within tolerance while looking wrong (see the caveat in §3). After every gate pass, on every retarget or clip change:

- Render both a front and a side view — a front-only render misses arm/limb anomalies visible only from the side
- For looping clips, render multiple time points across the loop (e.g. early/mid/late), not just t=0
- **Open and look at the rendered image yourself.** "The render script exited without error" or "the PNG was saved" is not verification — every slip-through in this pipeline's history involved a render that was produced but never actually inspected

If a headless renderer has no GPU context available, use a windowed/compatibility rendering mode that can still be driven non-interactively, and force a draw before capturing if the viewport doesn't refresh in a hidden window.

## 7. In-game live flow (composition-level bugs)

Individual clips passing gates 5–6 doesn't guarantee the composed behavior (state transitions, timing, blending with placement) is correct. Drive the actual game scene through its real entry point (the real button press, the real state machine), not a mock, and capture screenshots mid-flow. This is the only stage that catches bugs like a transition timer racing a clip's own length (see the SKILL.md Core Rule 4 clip-length-vs-duration item) — those bugs don't exist at the single-clip level.

## 8. Temporary instrumentation

Adding a short-lived log or counter (e.g. logging every animation-state transition, or counting how many times a given transition actually fired across N runs) is a cheap smoke test before reaching for a full visual check. Remove it before committing — check the diff for stray instrumentation left in.

## Ordering (each stage catches what the previous stage cannot)

```
1. Dump/measure (facts) → 2. Rest-pose comparison → 3. Numeric gate (cheap regression)
→ 4. Rendered PNG (visual validity) → 5. In-game live flow (composition) → 6. Real device/build (final gate)
```

See the godot-testing skill for how this maps onto that skill's three-stage gate design (numeric regression / visual / live flow) as reusable Tools/ scripts.

## Process notes

- Long-running headless commands (importers, engine batch scripts) should always run under a timeout — an import hang can otherwise freeze the whole session
- If an import hangs, reproduce against a clean working tree first to isolate whether a specific asset/config is the cause
- Confirm the provenance of a copied/inherited asset by byte comparison rather than assuming — this both settles licensing questions and avoids wasted debugging on an asset that turns out to be identical to a known-good one
- Never trust a subagent's or a prior pass's "visually verified" claim without opening the actual reported image yourself

## Checklist

- [ ] Rest-pose compatibility was judged from an actual skeleton dump comparison (positions in real-world units, mirror-tolerant quaternion difference), not from bone-name overlap or the product description
- [ ] Every numeric pose gate pairs a magnitude constraint with an orientation constraint that would catch the same joint bent the wrong way
- [ ] Rendered PNGs cover front AND side angles, multiple time points for loops, and were opened and inspected — a saved file or a zero exit code was not counted as inspection
- [ ] Composition-level behavior (transitions, timing, placement) was driven through the real game entry point, not judged from single-clip passes
- [ ] Temporary instrumentation (logs, counters) was removed — the diff was checked for strays

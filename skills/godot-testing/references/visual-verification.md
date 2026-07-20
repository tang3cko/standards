# Visual Verification (Layer 3: Numeric + Rendered Gates)

Layer 3 covers what neither Layer 1 (pure C#) nor Layer 2 (E2E wiring) can judge: whether a pose,
animation, or rendered scene actually *looks right*. It is a progression of three gates, each
added to close a specific blind spot the previous gate could not see.

## The Core Principle

**"There is a difference" does not imply "the difference is correct."** A test that only checks
"the value changed from rest / from the previous frame" can pass while the actual result is
broken — twisted limbs, a knee bent sideways, a pose that is numerically non-zero but kinematically
nonsensical. Every gate in this layer exists to narrow that gap between "changed" and "correct".

## The Three-Stage Gate

```
1. Numeric gate  →  2. Headless PNG render  →  3. In-game screenshot
```

Each stage exists because the stage before it has a proven blind spot — do not skip a stage on the
assumption that an earlier or later one "covers it".

### Stage 1 — Numeric gate

A `Tools/` script (GDScript or C#) that asserts measurable properties of a pose or animation:
hip-height ratio, torso uprightness, knee bend angle *and orientation*, hand-to-head distance, and
similar. Cheap, fast, runs every time as a regression check.

**Magnitude alone is not enough — pair it with orientation/sign.** A gate that only checks "is the
knee bent by roughly the right number of degrees" will pass a knee bent sideways, because the
magnitude looks plausible even though the direction is wrong. Add direction-aware checks alongside
magnitude checks:

- Knee: bend angle magnitude **and** "is the ankle in front of the hip" / "is the foot below the
  knee" (catches a knee bent in the wrong plane, which a magnitude-only gate cannot).
  Reaches the design range but can still resemble a walking pose bent sideways instead of forward.
- Arms: both hands below head height, hands within a bounded horizontal distance of the torso
  center (catches arms that fan out to an implausible angle — an "everyone is dancing" failure mode
  that a "differs from rest" check alone lets straight through, because a t-pose/a-pose delta looks
  non-zero either way).
- Walk cycle: left/right ankle Z-positions actually cross over time (proves a stride exists, not
  just that the legs moved).

**Add a gate for every failure pattern that has actually slipped through once** — do not
speculatively gate for failure modes that have never been observed; that produces a bloated,
low-signal check suite. Grow the gate list reactively, from real incidents.

### Stage 2 — Headless PNG render

Once the numeric gate passes, render actual frames and look at them. A numeric pass on a broken
pose is exactly the failure mode Stage 1 alone cannot catch by construction — only pixels reveal it.

```bash
godot-mono --path . --rendering-driver opengl3 res://verify_scene.tscn
```

- **`--headless` alone cannot render** — it forces the headless display driver, which only ever
  backs the dummy (non-drawing) rendering driver. To actually rasterize a frame, launch with a
  real rendering driver (`opengl3`/GL Compatibility works without a visible window in practice) and
  no `--headless` flag.
- Capture with `get_viewport().get_texture().get_image().save_png(path)`. If the viewport does not
  update in a non-visible window, force a draw first (e.g. `RenderingServer.force_draw()`).
- **Capture front and side, and multiple points in time for looping clips** (e.g. 0.2/0.5/0.8
  through the clip). A front-only capture has, in practice, missed arm-angle failures that were
  only visible from the side.
- **Open and actually look at every PNG.** "The script ran and saved a file" is not verification —
  runs that stopped at "saved" have let real breakage through.
- For the exact launch flags this project uses to take these screenshots, see the godot-headless
  skill rather than duplicating them here.

### Stage 3 — In-game screenshot

Instantiate the *real* scene, drive it through its *real* entry point (the actual button press or
state transition a player would trigger — not a mock, not a reflection shortcut), let it run, and
screenshot mid-flow.

This is the only stage that can catch **composition failures**: things that are fine in isolation
but break when combined with placement, timing, or other systems. A single animation clip can pass
both Stage 1 and Stage 2 in isolation and still never actually play in-game because its duration
exceeds the state's visible window — a bug that is invisible to per-clip verification and only
shows up once the real flow is exercised end to end with instrumentation (e.g. a trigger-count
counter) proving the branch was never taken.

## What Each Stage Catches That the One Before It Cannot

| Stage | Catches | Cannot catch |
|-------|---------|---------------|
| Numeric gate | Magnitude/orientation regressions, cheaply, every run | Anything that looks wrong despite passing numbers; composition across systems |
| Headless PNG | Poses that pass numerically but are visually broken (twisted limbs, wrong-plane bends) | Failures that only exist in the full runtime flow (timing, state composition) |
| In-game screenshot | Composition failures: correct-in-isolation elements that break when combined (timing, placement, cross-system state) | Nothing structurally, but is expensive — reserve it for flows, not every clip |

Even after all three internal stages pass, treat real end-user/playtester confirmation on the
actual running build as the final, non-substitutable gate for anything user-facing and visual —
internal gates reduce risk, they do not eliminate the value of a human looking at the real thing.

## Sub-Agent and Verification Operating Rules

- **A sub-agent's "it looks correct" report must be backed by a rendered image the reviewer
  actually opens.** A report that only cites numeric success ("pose differs from rest") is not
  sufficient evidence that a pose or visual result is correct — numeric success and visual
  correctness have diverged in practice. Do not accept "I verified it" without a path to an actual
  image, and do not treat opening that path as optional.
- **Search for known issues before building custom math.** Before hand-deriving a fix for a
  rig/retarget/animation-compatibility problem, search for whether it is already a documented,
  known issue (engine GitHub issues, forums, official docs) for the tool/format involved. Third-party
  asset "compatibility" claims are provisional until verified against a real rendered frame — the
  documented compatibility grain (e.g. product-line-wide vs. per-SKU) frequently does not match what
  actually ships in a given download.
- **Delegate in complete, reviewable units; keep judgment centralized.** Hand a sub-agent one
  self-contained unit of work (one commit's worth), and keep "look at the rendered result and decide
  what to try next" as the reviewer's job, not the sub-agent's.

## Checklist

- [ ] Every numeric gate pairs a magnitude check with an orientation/sign check — "bent the right amount" without "bent the right way" passes broken poses
- [ ] Each new gate answers a failure that actually slipped through once, not a speculative failure mode
- [ ] Renders were captured with a real rendering driver (not `--headless`, which forces the dummy driver), from front AND side, at multiple timestamps for looping clips
- [ ] Every produced PNG was actually opened and looked at — "the script saved a file" is not verification
- [ ] Any in-game (Stage 3) check drives the real scene through its real entry point, with instrumentation proving the intended branch actually ran
- [ ] Any "looks correct" claim (yours or a sub-agent's) cites an image path the reviewer opened, not just a numeric pass

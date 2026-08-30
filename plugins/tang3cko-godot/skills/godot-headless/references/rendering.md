# Offscreen PNG Rendering

The single most important trap in headless Godot work, and the harness pattern that works around
it. Verified on Godot 4.6.x across multiple internal projects.

## The trap: `--headless` forces the dummy rendering driver

`--headless` selects the "headless" **display** driver. On Godot 4.6.x, that display driver only
ever backs the "dummy" **rendering** driver — regardless of any `--rendering-driver opengl3` (or
similar) flag also passed on the same command line. `godot-mono --help`'s own doc string for
`--display-driver headless` says as much ("headless" ("dummy")).

The consequence: `GetViewport().GetTexture().GetImage()` either throws (null texture) or silently
produces a black/empty image. **Nothing in the exit code or logs flags this as an error** — the
process can exit 0, print "saved screenshot", and still have written a useless black PNG. This is
worse than a crash because it looks like success.

An older recipe of `--headless --rendering-driver opengl3` may be found in older project notes or
online examples; treat it as stale for 4.6.x. It has been directly observed to fail (null texture)
in practice.

### The fix

Omit `--headless` entirely. Pass `--audio-driver Dummy` directly instead (no need to route audio
through anything real for a screenshot-only run):

```
godot-mono --path <project> --rendering-driver opengl3 --audio-driver Dummy <scene> -- <args>
```

This opens an actual window — but in a CI runner or a dev machine without bringing it to the
foreground, it is effectively offscreen in practice, while still getting a real GL context. This
is the invocation `VerifyRender.cs.template` (and the SKILL.md workflow) always assumes.

## Harness anatomy

The proven pattern (see `assets/VerifyRender.cs.template`) is a one-shot `Node` that:

1. **Parses `OS.GetCmdlineUserArgs()`** for `--out=`, `--frames=`, and any project-specific
   arguments (input to inject, a camera/view to switch to, a time-skip amount)
2. **Loads and instantiates the real scene under verification** — `res://Scenes/Main/Main.tscn`
   or equivalent, not a stripped-down stand-in. The whole point of this gate (the second of the
   three visual gates in `SKILL.md` Core Rule 3) is to catch integration issues a numeric-only
   test structurally cannot: a value can be numerically correct while the actual rendered pose is
   twisted or the actual rendered layout is broken.
3. **Advances a fixed number of frames** in `_Process(double delta)`, optionally injecting input
   or switching views at a specific frame (not at `_Ready()`) — this proves the input *propagated
   through the running scene*, not merely that the scene booted with some baked-in state.
4. **Captures and quits** at the target frame: `GetViewport().GetTexture().GetImage().SavePng(path)`,
   then `GetTree().Quit(0)` a couple of frames later (not immediately — leaving a small buffer
   avoids racing the save against the process teardown).

## Designing verification arguments

Follow the target project's actual verification need rather than adding options speculatively.
Patterns observed to be useful:

- `--out=<path>` — always required; where the PNG lands. No sane default (a per-project scratch
  location invites stale-file confusion across runs).
- `--frames=N` — how many real frames to advance before capture. Defaults to a value long enough
  for one visible cycle of whatever's under test (an idle animation loop, a camera settle, etc).
- Input-injection args (e.g. `--pin=Name:x:z`, `--select=ID`) — parsed at `_Ready()`, but *applied*
  partway through the frame run (e.g. at `_frames / 2`), not immediately, so the render proves the
  input reached the running scene rather than being pre-baked into initial state.
- View-switch args (e.g. `--cam=N`, a mutually-exclusive set of "look through this instead") — if
  the harness supports several mutually exclusive views, document the priority order explicitly in
  the class doc comment (which one wins if two are passed) rather than leaving it to argument-parse
  order to decide silently.
- `--fast-forward-ms=MS` — only add this if the target project's simulation loop is *substep-safe*
  (a dt-clamped internal loop such that one large `Tick(MS)` call is equivalent to many small
  frame-driven ticks — see `godot-architecture` for this pattern). This lets a render prove a
  long-run behavior (e.g. a timeout path many real seconds out) without waiting thousands of real
  headless frames. If the project's Tick has no such guarantee, don't add this parameter — a large
  single-step call would produce a different result than normal play and the render would prove
  nothing about actual behavior.

## Gate 3: don't stop at the offscreen render

The offscreen PNG (gate 2) proves the scene renders and reacts to injected input in isolation. It
does not prove the scene behaves correctly when reached through the real game flow (menus, state
transitions, other systems running concurrently). Gate 3 — an in-game screenshot taken by actually
playing to the relevant state through the real UI/flow — is not optional for anything shipping;
see `SKILL.md` Core Rule 3. A sub-agent's textual claim that "the pose looks right" or "values
differ from rest, so it's working" is not a substitute for an attached image at either gate.

## References

- [../SKILL.md](../SKILL.md) - Core Rules 2 and 3
- [commands.md](commands.md) - Full CLI invocation reference
- [pitfalls.md](pitfalls.md) - Diagnosing a black/null screenshot after the fact

## Checklist

- [ ] The render invocation omits `--headless` and passes `--rendering-driver opengl3 --audio-driver Dummy`
- [ ] The harness loads the real scene under verification, not a stripped-down stand-in
- [ ] Injected input is applied partway through the frame run (not at `_Ready()`), so the render proves propagation through the running scene
- [ ] `Quit()` is deferred a couple of frames after `SavePng` — quitting immediately races the save against teardown
- [ ] `--fast-forward-ms` exists only if the project's simulation `Tick` is substep-safe; otherwise it was left out
- [ ] The produced PNG was actually opened and inspected, and gate 3 (in-game screenshot through real flow) remains planned for anything shipping

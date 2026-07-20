# Godot 4.x + .NET Gotchas

Pitfalls encountered in Godot 4.6 + .NET and how to avoid them. Most of these **fail silently**, which is why they are codified as conventions.

## NodePath null safety

An `[Export] NodePath` left uninitialized is null and throws NullReferenceException the moment `.IsEmpty` (or anything else) is called on it.

```csharp
// NG
[Export] private NodePath _path;

// OK
[Export] private NodePath _path = new();
```

## Typed Node exports do not resolve

`[Export]` of a typed node reference (e.g. `Camera3D`) is not automatically resolved from the NodePath Variant stored in `.tscn` (the C# source generator cannot convert it). It stays null at runtime.

```csharp
// NG (null at runtime)
[Export] private Camera3D _camera;

// OK
[Export] private NodePath _cameraPath;
private Camera3D _camera;

public override void _Ready()
{
    _camera = GetNodeOrNull<Camera3D>(_cameraPath);
}
```

## Never child an Area3D under a StaticBody3D

Placing an `Area3D` as a child of a `StaticBody3D` binds the inner `CollisionShape3D` to the outer body, and the Area stops being detectable.

Workaround: place them as **siblings** under a common parent (e.g. `Pivot : Node3D`).

```
Pivot (Node3D)
├── StaticBody3D
│   └── CollisionShape3D
└── Area3D
    └── CollisionShape3D
```

## Export property casing must match exactly

`[Export]` property names appear in `.tscn` files with the **identical spelling**. Unlike GDScript exports, there is no automatic snake_case conversion.

```csharp
[Export] public string PromptText { get; set; }
```

```ini
# .tscn
PromptText = "Open the door"   # OK
prompt_text = "Open the door"  # silently ignored
```

## .tres resources are cached by load path

A `.tres` file is cached by its load path; multiple references resolve to the **same instance**.

- For intentionally shared data (EventChannel, Validator, …) this is the benefit
- When per-instance state is required, set `resource_local_to_scene = true`

## QueueFree timing

`QueueFree()` executes at the end of the frame. Referencing a "deleted" node within the same frame is an error, so order operations carefully when deletion and related work happen in one frame.

```csharp
// Pickup: acquire item -> add to InventorySystem -> delete node
InventorySystem.Instance.AddItem(_item);
QueueFree();  // do not reference self after this within the frame
```

## InputEventJoypadMotion fires while idle

With a gamepad merely connected, `InputEventJoypadMotion` fires frequently with tiny axis values even at idle. A "last used device" detector that naively treats any joypad motion as Gamepad can never return to keyboard/mouse.

```csharp
// NG: stuck on Gamepad while a pad is connected
case InputEventJoypadMotion:
    return InputDeviceKind.Gamepad;

// OK: only accept when the absolute axis value exceeds a threshold (~0.5)
case InputEventJoypadMotion m when MathF.Abs(m.AxisValue) > 0.5f:
    return InputDeviceKind.Gamepad;
```

Extract the classification logic into a pure class (e.g. `InputDeviceClassifier` with `DefaultJoypadMotionThreshold = 0.5f`) so it stays testable.

## Autoload race conditions

Autoloads initialize before other nodes' `_Ready()`, so referencing an autoload's static `Instance` inside a node's `_Ready()` is safe. However, ordering **between autoloads** follows the autoload order in `project.godot` — make the order explicit when autoloads depend on each other.

## Descendant `_Ready()` fires before the ancestor's

Godot calls `_Ready()` depth-first: children under `Player` (`Label3D`, `Camera3D`, plain `Node`s) run before the `Player` controller's own `_Ready()`.

So if `Player` calls `AddToGroup("player")` in its `_Ready()`, a descendant calling `GetTree().GetFirstNodeInGroup("player")` in its own `_Ready()` gets `null`.

```csharp
// NG: in a descendant's _Ready() the ancestor is not in the group yet
public override void _Ready()
{
    _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
}
```

Workarounds:

- Lazily look up on the first `_Process()`
- Reference via known tree relations (`Owner`, `GetParent()`)
- Inject the dependency with an `[Export] NodePath`

## Camera3D.Current = false auto-promotes another camera

Assigning `Current = false` directly deactivates that camera, but if any other `Camera3D` is alive in the tree, Godot silently auto-promotes one of them to active. This defeats intentional "no camera active" states (e.g. switching to a 2D overview UI while several 3D cameras still exist off-screen).

```csharp
// NG: with other Camera3D nodes alive, one of them gets silently promoted
_camera.Current = false;

// OK: explicitly declines auto-promotion
_camera.ClearCurrent(enableNext: false);
```

Recurred across multiple camera nodes in the same project (main camera plus secondary/POV cameras) — treat it as a hard rule, not a one-off fix.

## Hand-authored MeshInstance3D freezes in bind pose (no warning)

When the Godot editor imports a rigged model, it auto-generates `skeleton = NodePath("..")` on each `MeshInstance3D` so the mesh finds its `Skeleton3D` parent. If you hand-author a `.tscn` and place a `MeshInstance3D` under a `Skeleton3D` yourself, that implicit property is **not** added.

The failure mode is deceptive: the `Skeleton3D`'s bone poses update correctly (verifiable via `get_bone_pose()`), but the GPU skin never re-deforms — the mesh plays back as a static T-pose with no error or warning. It only surfaces by actually rendering a frame; numeric checks on bone poses pass regardless.

```ini
# .tscn — required when authoring by hand under a Skeleton3D
[node name="Body" type="MeshInstance3D" parent="Skeleton3D"]
skeleton = NodePath("..")
```

## Never set Container anchors from code (godot#88788)

Calling `SetAnchorsAndOffsetsPreset()` (or setting `AnchorLeft`/`OffsetLeft`/etc.) from code on a `Container`-derived node collides with the container's own layout pass and corrupts positioning (tracked upstream as godot#88788).

- Full-rect children: use a helper that binds the child to the host's rect declaratively, or set anchors in the `.tscn` itself — never imperatively on a Container
- Floating/free-positioned panels: give the panel **no anchors** and let its parent shell set `Position` / `Size` / `CustomMinimumSize` manually instead
- Content that should flow with its container: use size flags (`SizeFlagsHorizontal = ExpandFill`) rather than anchors

## Full-screen overlay Control must use MouseFilter.Ignore

`MouseFilter.Pass` still makes the overlay itself a pick target — the overlay is checked first and the click event never reaches the controls beneath it, even though `Pass` sounds like it should let clicks through. `MouseFilter.Ignore` skips the overlay from picking entirely, so children stay clickable while the overlay itself blocks nothing.

```csharp
// NG: overlay still eats clicks meant for whatever is underneath
_overlay.MouseFilter = Control.MouseFilterEnum.Pass;

// OK: overlay is invisible to the picker, underlying UI stays clickable
_overlay.MouseFilter = Control.MouseFilterEnum.Ignore;
```

The inverse applies to a clickable row's own inner children (icons, labels inside a toast/notification row): give *those* `MouseFilter.Ignore` so the click reaches the row's own `_GuiInput` instead of being consumed by a child first.

## .gitignore patterns fold case on macOS — anchor them to the root

macOS's default `core.ignorecase = true` makes `.gitignore` matching case-insensitive. A bare pattern like `reports` (intended to ignore some report-dump folder) will also match `Scripts/Reports/` or `Tests/Reports/` anywhere in the tree — silently excluding legitimate source directories from version control. The files are never staged, never flagged by `git status` as anything unusual, and the breakage only surfaces later (e.g. a fresh clone or worktree fails to build because a referenced class was never committed).

```gitignore
# NG: case-folds and matches any "Reports" directory at any depth
reports

# OK: rooted, so it only matches the intended top-level folder
/reports/
```

Anchor project-specific ignore patterns to the root (`/name/`) unless you deliberately want a directory name ignored everywhere.

## Engine warnings: investigate once, record the verdict inline — never silently suppress

A recurring engine-level warning (e.g. an `ObjectDB` leak warning at shutdown) is not something to blanket-suppress or ignore out of habit. Spend the time once to determine whether it is a real leak or a known engine limitation, then leave the verdict as an inline comment — including the upstream issue number and what you did to verify it — so the next person doesn't have to re-investigate from scratch.

```csharp
// ObjectDB leak warning at shutdown is a known Godot limitation, not a game-code leak:
// AudioServer defers playback-object deletion to a mix pass that never runs after
// shutdown (godot#76745, godot#95484). Verified by experiment: stopping the stream
// mid-run removes the warning; stopping it in _ExitTree() does not. Nothing
// accumulates at runtime — safe to leave as-is.
```

## Signal unsubscribe: only required for subscriptions that outlive you

`_ExitTree()` unsubscription (see "Signal-Driven State Propagation" in code-patterns.md) is a lifetime rule, not a blanket one. It only matters when the object you subscribed to **outlives** the subscriber:

- Subscribing to a longer-lived owner (an autoload, a shell-owned service, a parent that survives child removal) — unsubscribe in `_ExitTree()`, or the dangling delegate keeps the subscriber alive as a GC root
- Subscribing between two nodes with the **same** lifetime (e.g. a shell and a UI element the shell itself owns and frees together) — no unsubscribe needed, since both are collected together anyway

Judge every subscription by "does the emitter live longer than me," not by a uniform "always unsubscribe" reflex — the latter adds boilerplate to same-lifetime pairs without preventing any leak.

## Mobile C# is experimental; iOS runs NativeAOT and rejects reflection

As of Godot 4.7.1 (latest stable, 2026-07), official C#/.NET platform support is stable on **desktop only**. Android and iOS export exist since Godot 4.2 but remain officially **experimental**; the web platform is unsupported for C# entirely. The runtimes also differ per platform: Android ships on Mono (JIT, linux-bionic), while iOS ships on .NET **NativeAOT** — a trimmed, ahead-of-time-compiled runtime with no dynamic code generation.

The practical consequence (verified in a production Godot project, 2026-07): reflection that Mono tolerates breaks on iOS, often silently or only at first use.

- Reflection-based `System.Text.Json` serialization is **disabled at runtime** on iOS — the first (de)serialize call throws `InvalidOperationException`. Shipped C# must use a source-generated `JsonSerializerContext` instead:

  ```csharp
  // NG on iOS: reflection-based serialization throws InvalidOperationException
  JsonSerializer.Serialize(saveData);

  // OK: source-generated context works on both runtimes
  [JsonSerializable(typeof(SaveData))]
  internal partial class SaveJsonContext : JsonSerializerContext { }

  JsonSerializer.Serialize(saveData, SaveJsonContext.Default.SaveData);
  ```

- Any new reflection or dynamic-code use in shipped code needs an explicit AOT-compatibility check before it lands. **"Works on Android" proves nothing for iOS** — the JIT runtime masks exactly the class of failure NativeAOT exposes.
- Even upstream acknowledges the residual risk: Godot roots its bindings and the game assemblies against trimming, but "some reflection usage may still break at runtime" (godotengine.org, "Current state of C# platform support in Godot 4.2").

Sources: docs.godotengine.org C#/.NET basics (platform support), godotengine.org article above, godot-proposals#7895.

## Label3D rotated 180° on Y mirrors its text

A `Label3D` under a `Camera3D` faces `-Z` by default, so it shows its back to the camera. Fixing that with a 180° Y rotation also flips the X axis — the text becomes a mirror image.

Fix it with billboarding instead of rotation:

```ini
[node name="Watch" type="Label3D" parent="Head/Camera3D"]
billboard = 1
```

## Checklist

- [ ] Every `[Export] NodePath` is initialized `= new()`; no typed node references are exported, and `[Export]` names in `.tscn` match the C# spelling exactly
- [ ] Scene structure avoids the silent traps: no `Area3D` under a `StaticBody3D`; hand-authored `MeshInstance3D` under a `Skeleton3D` carries `skeleton = NodePath("..")`
- [ ] UI code never sets anchors on Container-derived nodes; full-screen overlays use `MouseFilter.Ignore`; cameras are deactivated via `ClearCurrent(enableNext: false)`, never `Current = false`
- [ ] Lifecycle ordering respected: no same-frame self-reference after `QueueFree()`; no ancestor group-lookup inside a descendant's `_Ready()`; inter-autoload dependencies match the `project.godot` order
- [ ] Every signal subscription judged by lifetime: subscriptions to longer-lived emitters are released in `_ExitTree()`; same-lifetime pairs are left alone
- [ ] Shipped code is AOT-safe for iOS: JSON uses source-generated `JsonSerializerContext`, and any new reflection/dynamic-code use has an explicit AOT-compatibility check

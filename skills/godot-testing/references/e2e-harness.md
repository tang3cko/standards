# E2E Harness (GdUnit4Net + SceneRunner)

Layer 2 of the three-layer testing strategy. `Tests.E2E/` project-references the full game
assembly and drives real `.tscn` scenes through `ISceneRunner`, so it catches wiring bugs
(an event that never fires, a node a handler never finds) that Layer 1's link-referenced pure
C# tests structurally cannot see — they never touch the scene graph.

## Project Structure

```
ProjectRoot/
├── {Project}.csproj
├── Scenes/
├── Scripts/
├── Tests/                      # Layer 1 — see testing.md
└── Tests.E2E/
    ├── Tests.E2E.csproj        # Project-references the full game assembly
    ├── .runsettings
    └── {Feature}E2ETest.cs
```

### Tests.E2E.csproj pattern

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>Smith.Tests.E2E</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <!-- GdUnit4 needs GODOT_BIN and headless parameters from .runsettings -->
    <RunSettingsFilePath>$(MSBuildProjectDirectory)/.runsettings</RunSettingsFilePath>
    <!-- Godot.SourceGenerators (transitive via gdUnit4) requires the Godot project root -->
    <GodotProjectDir>$(MSBuildProjectDirectory)/../</GodotProjectDir>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.*" />
    <PackageReference Include="gdUnit4.api" Version="5.0.0" />
    <PackageReference Include="gdUnit4.test.adapter" Version="3.0.0" />
    <PackageReference Include="GodotSharp" Version="4.6.*" />
  </ItemGroup>
  <ItemGroup>
    <!-- Full game assembly: E2E drives real scenes, unlike link-referenced Tests/ -->
    <ProjectReference Include="../{Project}.csproj" />
  </ItemGroup>
</Project>
```

Unlike `Tests.csproj` in Layer 1 (link references to isolated pure-logic files), `Tests.E2E.csproj`
takes a full `<ProjectReference>` to the game assembly — the whole point of this layer is exercising
real wiring, not isolated logic.

## `.runsettings`

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <RunConfiguration>
    <!-- Godot runtime tests share one editor process; parallel runs would race on .godot -->
    <MaxCpuCount>1</MaxCpuCount>
    <TestSessionTimeout>300000</TestSessionTimeout>
    <EnvironmentVariables>
      <!-- GdUnit4 launches this binary to host [RequireGodotRuntime] tests -->
      <GODOT_BIN>/path/to/godot-mono</GODOT_BIN>
      <!-- Save-path sandbox override (see the project's SavePaths equivalent): redirects
           profile/session save files away from the real user:// playtest data so E2E runs
           are deterministic and never pollute (or depend on) the developer's real save.
           Process.Start inherits the parent environment by default, so this reaches the
           spawned Godot child the same way GODOT_BIN does. -->
      <PROJECT_SAVE_DIR>/tmp/{project}_e2e_sandbox</PROJECT_SAVE_DIR>
    </EnvironmentVariables>
  </RunConfiguration>
  <GdUnit4>
    <!-- The adapter launches Godot with CWD = Tests.E2E and 'path .', which has no
         project.godot; the trailing 'path ..' argument wins and targets the game project root -->
    <Parameters>--headless --audio-driver Dummy --path ..</Parameters>
    <DisplayName>FullyQualifiedName</DisplayName>
    <CaptureStdOut>true</CaptureStdOut>
    <!-- First run compiles the whole game project inside Godot; allow 2 minutes -->
    <CompileProcessTimeout>120000</CompileProcessTimeout>
  </GdUnit4>
</RunSettings>
```

Notes, all verified against real project runs:

- **`MaxCpuCount=1` is load-bearing, not a style choice.** `[RequireGodotRuntime]` tests share one
  Godot process; letting the test adapter parallelize would race two Godot instances against the
  same `.godot` cache and produce flaky, order-dependent failures. Do not "optimize" this away.
- **`GODOT_BIN`** must point at the mono/`.NET`-enabled Godot binary, not a vanilla build — GdUnit4
  spawns it as a child process to host `[RequireGodotRuntime]` tests.
- **`--path ..` beats the adapter's own `path .`.** The GdUnit4 test adapter launches Godot with
  `CWD = Tests.E2E` and its own `path .` argument, but `Tests.E2E` has no `project.godot`. Godot
  resolves repeated `--path`/`path` arguments last-wins, so the explicit trailing `--path ..` in
  `<Parameters>` is what actually targets the game project root. Dropping it silently breaks scene
  loading with a confusing "no project.godot found" failure.
- **`CompileProcessTimeout`** must cover a cold compile of the entire game project (not just the
  test assembly) — the first run in a session, or after a clean, easily exceeds the test adapter's
  default timeout. 120s has been sufficient in practice; raise it if the game project is large.

## Engine-Error-as-Test-Failure

A `GD.PushError` during a run is a real bug even when nothing explicitly asserts on it — e.g. a
save serializer silently failing deep in a signal handler. Left unwired, that failure is invisible
and CI stays green. Two pieces make it visible:

1. **Enable file logging, debug builds only**, in `project.godot`:

   ```ini
   [debug]

   ; File logging is off by default (base value) and only turned on for debug builds via the
   ; ".debug" feature-tag override, so exported release builds never pay the cost. GdUnit4Net's
   ; GodotExceptionMonitor scans this log file for "ERROR:" (GD.PushError) lines to fail E2E
   ; tests on engine errors that would otherwise only be visible via PushError text.
   file_logging/enable_file_logging=false
   file_logging/enable_file_logging.debug=true
   file_logging/log_path="user://logs/godot.log"
   ```

2. **Apply `[GodotExceptionMonitor]` at the test-suite class level.** GdUnit4Net scans the log file
   configured above for `ERROR:` lines during every stage (Before/After/BeforeTest/AfterTest/
   TestCase) and rethrows as `TestFailedException` if it finds one.

   ```csharp
   [TestSuite]
   [GodotExceptionMonitor]
   public class IntroSequenceE2ETest
   {
       // ...
   }
   ```

**Guarantee the mechanism itself with a self-check test** — do not just assume the wiring works.
Deliberately push an error and assert it surfaces as a test failure:

```csharp
[TestCase]
[RequireGodotRuntime]
[ThrowsException(typeof(TestFailedException), "E2E harness self-check: induced PushError")]
public void EngineErrorMonitor_PushError_IsCaughtAsTestFailure()
    => GD.PushError("E2E harness self-check: induced PushError");
```

If `project.godot` file logging or the monitor attribute ever gets removed, this test starts
failing (no exception thrown) instead of the whole mechanism silently going dark.

`[ThrowsException]` also pins a test to one *expected* benign engine error (e.g. a known harness
limitation) without masking a *different* one — GdUnit4's monitor reports only the first
PushError-type log line per test, so an expected pinned message can mask a real error that occurs
later in the same test. When you rely on this, cover the same flow elsewhere in the suite under
the unmasked class-level `[GodotExceptionMonitor]` so the residual gap has coverage too.

## Sandboxing Persistence

E2E tests read and write real save files through the real persistence code path — they must never
touch, and must never depend on, the developer's actual `user://` profile data.

- **Redirect via an environment variable**, set in `.runsettings` (`PROJECT_SAVE_DIR` above) and
  read by the project's save-path resolver. `Process.Start` inherits the parent environment by
  default, so the same variable that reaches `dotnet test` reaches the spawned Godot child.
- **Refuse to run if the variable is unset.** Do not fall back to a default path — a missing env
  var means the `.runsettings` wiring broke, and silently falling back risks touching the real
  save directory.

  ```csharp
  [BeforeTest]
  public void ClearSandbox()
  {
      _sandboxDir = System.Environment.GetEnvironmentVariable(SavePaths.SandboxEnvVar);
      if (string.IsNullOrEmpty(_sandboxDir))
      {
          throw new InvalidOperationException(
              $"{SavePaths.SandboxEnvVar} is not set - refusing to run against the real " +
              "user:// profile. Check Tests.E2E/.runsettings EnvironmentVariables.");
      }
      // ...
  }
  ```

- **Safety-check the path before recursively deleting it.** A misconfigured or empty sandbox
  variable must never resolve to the real save directory. Belt-and-suspenders, checked every time
  before `Directory.Delete(path, recursive: true)`:

  ```csharp
  var realUserDir = OS.GetUserDataDir();
  if (string.Equals(_sandboxDir, realUserDir, StringComparison.OrdinalIgnoreCase)
      || string.Equals(_sandboxDir, "user:/", StringComparison.OrdinalIgnoreCase)
      || string.IsNullOrWhiteSpace(_sandboxDir.Trim('/')))
  {
      throw new InvalidOperationException(
          $"{SavePaths.SandboxEnvVar}='{_sandboxDir}' looks unsafe to recursively delete.");
  }

  if (Directory.Exists(_sandboxDir))
      Directory.Delete(_sandboxDir, recursive: true);
  Directory.CreateDirectory(_sandboxDir);
  ```

  Check for both "identical to the real dir" and "empty/root-ish" — either one turned a sandbox
  reset into a real-data wipe in earlier, less defensive versions of this pattern.

## Writing a Ship-Bug Regression E2E

A regression test born from a bug that actually reached players carries more context than a normal
test. Its doc comment must cover three things, not just "what this test does":

1. **The original bug** — what broke, and what the user-visible symptom was.
2. **Why Layer 1 (pure C#) missed it** — was the bug in scene *wiring* (an event-driven gate that
   never re-evaluated, a signal nobody subscribed to) rather than in logic that was already unit
   tested? Naming this explicitly is what justifies spending an E2E test instead of adding another
   Layer 1 case.
3. **An assertion that the precondition was actually hit** — a regression test that merely runs the
   repro steps without confirming the triggering condition occurred can pass for the wrong reason
   (e.g. silently landing in the "already fits, no scroll needed" branch instead of the "overflows,
   must scroll" branch the bug depended on).

```csharp
/// <summary>
/// Regression coverage for the shipped bug: on a large/tall display the terms document fits
/// the viewport entirely, so it never emits a scroll event, and the old event-driven gate
/// wiring left the Agree button permanently disabled. IntroSequence now re-evaluates
/// TermsScrollGate every frame while the Terms stage is active — these tests drive the real
/// Main scene end to end so a regression in the WIRING (not just the pure TermsScrollGate
/// logic, already unit-tested in Layer 1) fails CI again.
/// </summary>
[TestSuite]
[GodotExceptionMonitor]
public class IntroSequenceE2ETest
{
    [TestCase]
    [RequireGodotRuntime]
    public async Task Terms_DocumentFitsViewport_AgreeUnlocksWithoutScrolling()
    {
        // ...
        var vbar = scroll.GetVScrollBar();
        // Confirms this run actually exercises the "fits" case (the shipped bug's
        // precondition), not incidentally passing for another reason
        AssertThat(vbar.MaxValue - vbar.Page <= 0.0).IsTrue();

        // No scroll input anywhere in this test — only frame simulation. Before the
        // IntroSequence._Process fix this stayed Disabled forever
        await runner.SimulateFrames(5);
        AssertThat(agreeButton.Disabled).IsFalse();
    }
}
```

## Choosing What Goes in Layer 2

| Target | Layer |
|--------|-------|
| Event-driven UI gate that must re-evaluate on state change, not just on the original signal | ◎ E2E |
| Scene boot sequence / first-run vs. returning-user branching | ◎ E2E |
| Save/load through the real persistence code path (not the serializer's pure logic) | ◎ E2E |
| A ship-bug whose root cause was wiring, not pure logic | ◎ E2E regression |
| Pure data transformation already reachable without a scene | × Layer 1 instead |
| Pose/rendering correctness | × Layer 3 (see references/visual-verification.md) |

## Running Tests

```bash
# In the Tests.E2E/ directory
dotnet test
```

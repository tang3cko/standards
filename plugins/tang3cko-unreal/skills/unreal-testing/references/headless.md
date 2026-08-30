# Running Tests Without a Human

Load this when wiring tests into a script, a hook, or CI.

Everything here exists because of one fact: **the automation runner exits 0 when the filter matches nothing.** A suite that silently stopped running looks exactly like a suite that passes.

## The command

```bash
"$UE/Engine/Binaries/Mac/UnrealEditor-Cmd" "$PROJECT/Project.uproject" \
  -ExecCmds="Automation RunTests <Filter>;Quit" \
  -unattended -nopause -nosplash -nullrhi \
  -ReportExportPath="$OUT" \
  -abslog="$OUT/run.log"
```

| Argument | Why |
|---|---|
| `-ExecCmds="…;Quit"` | Without `Quit` the editor stays up forever |
| `-unattended -nopause` | No dialog waits for a click that never comes |
| `-nullrhi` | No renderer. Drop it only if a test needs pixels |
| `-ReportExportPath` | Writes `index.json` and `index.html` — the only machine-readable result |
| `-abslog` | Pins the log somewhere the script can read |

`Automation RunTest <A>+<B>` runs a specific list; `Automation RunTests <Filter>` runs a substring match; `Group:MyGroup` runs a group defined in config. `-ResumeRunTest` with `-ReportExportPath` picks up from the first not-run entry, marking anything in-progress as failed — handled in `FAutomationControllerManager` (`Developer/AutomationController/Private/AutomationControllerManager.cpp`), which reads the previous `index.json` back in.

## The zero-match failure

A filter that matches nothing behaves differently from a filter that matches something, and the difference is not in the shell exit code. `FAutomationExecCmd::HandleRefreshTestCallback` in `Developer/AutomationController/Private/AutomationCommandline.cpp` calls `FAutomationExecCmd::LogCommandLineError` when no test matches, and no report is written for that run. `FAutomationExecCmd::Tick`, in the same file, calls `FPlatformMisc::RequestExitWithStatus`, requesting `-1` when the run recorded a critical error.

**The shell exit code is not reliable evidence either way.** What the engine requests and what the shell surfaces are not guaranteed to be the same small integer — something between `RequestExitWithStatus` and the shell (a crash handler, a signal, a wrapper) can transform it, and that transformation is platform-dependent. That is exactly why the exit code is not the thing to test. **Assert on the report instead.**

```bash
[[ -f "$OUT/index.json" ]] || { echo "no report — the filter matched nothing" >&2; exit 1; }
python3 - "$OUT/index.json" <<'EOF'
import json, sys
r = json.load(open(sys.argv[1]))
total = r["succeeded"] + r["failed"] + r["succeededWithWarnings"]
if total == 0:
    sys.exit("report contains zero tests")
if r["failed"]:
    sys.exit(f"{r['failed']} failed")
EOF
```

`index.json` carries `succeeded`, `succeededWithWarnings`, `failed`, `notRun`, `inProcess`, `totalDuration`, and a `tests` array with `fullTestPath`, `state` and `duration` per test. Those keys are the `UPROPERTY` names of `FAutomatedTestPassResults` (`Developer/AutomationController/Private/AutomationControllerManager.h`), serialised through `FJsonObjectConverter::UStructToJsonObjectString` (called from `Developer/AutomationController/Private/AutomationControllerManager.cpp`), which lower-cases the first letter by default (`FJsonObjectConverter::StandardizeCaseInPlace` in `Runtime/JsonUtilities/Private/JsonObjectConverter.cpp`).

Record the expected minimum count somewhere and check against it — a rename that drops half the suite otherwise passes.

## Logs that go nowhere

`unreal.log` and anything logged at `Display` do not reach stdout by default. A Python tool run through `-run=pythonscript` will execute, exit 0, and print nothing — indistinguishable from working.

`-stdout` is parsed where `Runtime/Launch/Private/LaunchEngineLoop.cpp` calls `InitializeStdOutDevice`; `-FullStdOutLogOutput` is parsed in the `FOutputDeviceStdOutput` constructor (`Runtime/Core/Private/Misc/OutputDeviceStdOut.cpp`), which raises `AllowedLogVerbosity` to `ELogVerbosity::All` and lifts the verbosity gate that `FOutputDeviceStdOutput::Serialize` and `FOutputDeviceStdOutput::SerializeRecord` apply. Without both flags, the gate drops everything below the default threshold.

Add `-stdout -FullStdOutLogOutput` to anything whose output you intend to read.

## Timing

Editor startup is the cost, not the tests — it dominates any single automation run by orders of magnitude, and it is paid once per process, not once per test. Budget for it, and **do not split a suite across several editor invocations** to parallelise — each one pays the startup again.

Give any CI step a timeout well above a cold start on the target machine, and treat a run that ends without writing a report as a failure regardless of how it ended.

## Screenshots

Automation screenshots and `HighResShot` do not capture UMG. In `Runtime/Engine/Private/UnrealClient.cpp`, the screenshot path takes a `if (GIsHighResScreenshot)` branch that hard-codes `const bool bShowUI = false;` — so a menu simply is not in the image. Anything about UI appearance has to be photographed from outside: launch the packaged build, drive it with `-ExecCmds`, and capture the game's own window. Reject a blank image mechanically, then have a person or an agent actually open it.

## Excluding tests

`AutomationTestExcludelist` is configured in ini:

```ini
+ExcludeTest=(Test="<test or section name>",Reason="<a reason>",Warn=False)
+Groups=(Name="Group1", Filters=((Contains=".Some String.")))
```

The entry struct is `FAutomationTestExcludelistEntry` (`Runtime/AutomationTest/Public/AutomationTestExcludelist.h`); the group struct is `FAutomatedTestGroup` (`Developer/AutomationController/Public/AutomationControllerSettings.h`).

Excluded tests report as **Skipped** with the reason attached — the state is set in `FAutomationWorkerModule::HandleRunTestsMessage` (`Runtime/AutomationWorker/Private/AutomationWorkerModule.cpp`), against the `EAutomationState` enum (`Runtime/AutomationTest/Public/AutomationState.h`). A reason is not optional in practice: a skip with no reason becomes permanent.

## Checklist

- [ ] Every automated run asserts that the report exists and that the test count is above an expected minimum — never on the exit code alone
- [ ] `-ExecCmds` ends with `;Quit`, and `-unattended -nopause` are present
- [ ] Anything whose stdout is read runs with `-stdout -FullStdOutLogOutput`
- [ ] CI timeouts account for cold editor startup, and the suite is not split across invocations to parallelise
- [ ] No UI appearance claim rests on `HighResShot` or an automation screenshot
- [ ] Every excluded test carries a written reason

# Tasks - Unity Rider CLI Pipeline

## Vision

A Rider plugin that runs **all** Unity Test Framework tests **without relying on the Unity Editor UI** (often heavy). Execution uses the **Unity CLI** in batch mode (`unity test`): headless Editor, Test Runner, NUnit/JUnit report.

The bundled **Unity Support** plugin ([Run and debug Unity tests](https://www.jetbrains.com/help/rider/Running_and_Debugging_Unity_Tests.html)) requires an open Editor plus `com.unity.ide.rider`. This project fills that gap: headless runner via CLI / Production Pipeline.

Everything below follows **TDD**: failing test "†’ minimal implementation "†’ refactor. `./gradlew :check` and .NET backend tests must pass per slice.

---

## Technical context (references)

### Three Test Runner tabs = three execution targets

| Test Runner UI | `-testPlatform` / `unity test --mode` | What runs |
|---|---|---|
| **Edit Mode** | `EditMode` | Editor tests |
| **Play Mode** | `PlayMode` | Play Mode **inside** the Editor |
| **Player** | a `BuildTarget` value (e.g. `StandaloneOSX`, `StandaloneWindows64`, `StandaloneLinux64`) | Play Mode in a **Player** for that platform |

Source: [Command-line reference - Unity Test Framework](https://docs.unity.com/en-us/engine/6000.7/manual/scripting/test-framework-introduction/reference-command-line) (`-testPlatform`).

### Unity CLI vs Pipeline package

| Tool | Role in this plugin |
|---|---|
| **Unity CLI** (`unity`) | Primary: `unity test` in batch mode **does not** need the Pipeline package |
| **`com.unity.pipeline`** | Secondary (later phase): `run_tests` against an already-connected Editor |

Sources:

- [Compare CLI and Pipeline package](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-cli-pipeline-package)
- [Unity CLI reference - `test`](https://docs.unity.com/en-us/unity-cli/unity-cli-reference)
- Pipeline `run_tests`: [build-and-compilation commands](https://docs.unity3d.com/Packages/com.unity.pipeline@0.3/manual/commands/build-and-compilation.html) (`mode`: `all` \| `editor` \| `playmode`)

### Canonical commands (MVP)

```bash
unity test /path/to/Project --mode EditMode  --report-format nunit --output ./tmp/edit.xml  --format json --timeout 600
unity test /path/to/Project --mode PlayMode  --report-format nunit --output ./tmp/play.xml  --format json --timeout 600
unity test /path/to/Project --mode StandaloneOSX --report-format nunit --output ./tmp/player.xml --format json --timeout 1800
```

Relevant CLI exit codes:

| Code | Meaning |
|---|---|
| `0` | All passed |
| `8` | Run finished; "‰¥1 test failed (`TESTS_FAILED`) - **do not** retry |
| `6` | Infrastructure (compile, license, crash, timeout, invalid platform) - retry possible |
| `2` | Bad arguments |

Report format: NUnit3 XML (`-testResults` / `--output`). See [Test Result XML Format (NUnit)](https://docs.nunit.org/articles/nunit/technical-notes/usage/Test-Result-XML-Format.html).

### Rider integration (ReSharper backend)

Unit Testing extension points on the .NET backend:

- `IUnitTestProvider` + `[UnitTestProvider]`
- `IUnitTestMetadataExplorer` / file explorer
- `IUnitTestElement` + `RecursiveRemoteTaskRunner` (or a host that starts an external process and reports via `IRemoteTaskServer`)

Source: [Unit Test Framework Support - ReSharper SDK](https://www.jetbrains.com/help/resharper/sdk/UnitTest.html).

Optional Kotlin frontend for MVP: RD protocol stub in `protocol/""¦/RdUnityCliPipelineModel.kt` - settings UI, progress, Player target selection.

Current scaffold: [resharper-rider-plugin](https://github.com/JetBrains/resharper-rider-plugin) (Gradle + RD + .NET backend).

---

## TDD rules for this repo

1. **Red "†’ green "†’ refactor** per numbered task.
2. Testable layers **without** a real Rider/Unity first (arg builder, XML parser, exit-code mapping).
3. Mockable process contracts (`IUnityCliProcess`); real UTF XML fixtures under `testData/`.
4. Real Unity integration only on tasks marked `[integration]` / nightly (opt-in).
5. Done means: automated test + implementation + checkbox updated here.

---

## Epic 0 - Plugin foundation

### 0.1 Plugin identity and dependencies
- [x] Update `plugin.xml`: name, id, description aligned with the product (Unity CLI Test Runner).
- [x] Declare dependency on Unity Support **or** document coexistence (do not break existing gutter/discovery).
- [x] Test: valid `plugin.xml` / `./gradlew :buildPlugin` produces a ZIP.

### 0.2 Detect a Unity project from the Rider solution
- [x] **TDD:** given a solution path, detect Unity root (`Assets/`, `Packages/manifest.json`, `ProjectSettings/ProjectVersion.txt`).
- [x] Return `UnityProjectInfo` (path, `editorVersion` from `ProjectVersion.txt`).
- [x] Negative: plain .NET solution -> provider inactive.

### 0.3 Resolve the `unity` CLI binary
- [ ] **TDD:** PATH resolution (`which unity` / `Get-Command`), settings override, clear error if missing.
- [ ] **TDD:** parse `unity --version --format json`; fail if CLI is below the documented minimum (current beta).
- [ ] Rider setting: CLI path + default timeout (`UNITY_TEST_TIMEOUT`).

---

## Epic 1 - `unity test` client (core, no UI)

### 1.1 Test-mode model
- [ ] **TDD:** enum/value object `UnityTestTarget`:
  - `EditMode`
  - `PlayMode`
  - `Player(BuildTarget)` - canonical Unity enum string (`StandaloneOSX`, `StandaloneWindows64`, ""¦)
- [ ] **TDD:** serialization "†” `--mode <value>` (CLI is case-insensitive).
- [ ] Supported BuildTargets table for MVP (desktop first); mobile/console = backlog.

### 1.2 `UnityTestCommandBuilder`
- [ ] **TDD:** builds argv:
  - `test <projectPath>`
  - `--mode ""¦`
  - `--filter ""¦` (optional)
  - `--output ""¦`
  - `--report-format nunit` (internal default)
  - `--format json`
  - `--timeout ""¦`
  - `--non-interactive` / `--no-banner` when applicable
- [ ] **TDD:** never inject reserved flags after `--` (`-batchmode`, `-runTests`, `-testPlatform`, ""¦) - the CLI rejects them.
- [ ] **TDD:** per-session isolated output path (Rider temp).

### 1.3 Process execution + exit codes
- [ ] **TDD:** wrapper `IUnityCliRunner.RunAsync(request, ct)` "†’ `UnityTestRunResult`:
  - `Outcome`: Passed \| FailedTests \| InfraError \| BadArgs \| Cancelled
  - mapping `0/8/6/2`
  - stdout JSON envelope (`success`, `errors[0].code`: `TESTS_FAILED` vs `TEST_RUN_ERROR` / `TEST_TIMED_OUT`)
  - XML path
- [ ] **TDD:** cancellation (SIGINT/kill) "†’ `Cancelled`, not FailedTests.
- [ ] **TDD:** process timeout aligned with `--timeout`.

### 1.4 NUnit3 XML parser (UTF)
- [ ] **TDD:** fixtures under `testData/nunit/` (passed, failed, ignored, parameterized, nested suites).
- [ ] Extract: fullname, classname, methodname, result, duration, message, stacktrace, assertions.
- [ ] **TDD:** empty suite / truncated XML "†’ typed error (infra), not """0 passed""".
- [ ] (Optional) JUnit parser if `--report-format junit` is used later.

### 1.5 Filters
- [ ] **TDD:** map Rider selection (assembly / fixture / method / category) "†’ `--filter` / documented editor-flag policy.
- [ ] Negation and regex: mirror UTF (`-testfilter`) to the extent the CLI exposes (`--filter`).

---

## Epic 2 - Run ALL tests (product MVP)

### 2.1 Run All - Edit Mode
- [ ] **TDD (unit):** """Run All EditMode""" session "†’ one `unity test --mode EditMode` "†’ results applied to the session model.
- [ ] Manual criterion: fixture project with "‰¥1 Edit Mode pass and "‰¥1 fail "†’ Rider shows correct green/red.

### 2.2 Run All - Play Mode (Editor)
- [ ] Same for `--mode PlayMode`.
- [ ] Document: headless Play Mode may need `-nographics` (forward after `--` once validated); note GPU/input-dependent test risk.

### 2.3 Run All - Player (BuildTarget)
- [ ] **TDD:** builder accepts `StandaloneOSX` / `StandaloneWindows64` / `StandaloneLinux64`.
- [ ] Setting: default """Player target""" = host platform.
- [ ] Higher default timeout (Player build); dedicated setting.
- [ ] Criterion: fixture Play Mode test runs with `--mode <BuildTarget>` and XML returns (local network / heartbeat - see `-playerHeartbeatTimeout` in UTF docs).
- [ ] Note: Player is the third UI tab (**Player**), not a third test attribute kind.

### 2.4 Run All - three modes in sequence ("""All platforms""")
- [ ] Action/session: EditMode "†’ PlayMode "†’ Player (configurable: which are enabled).
- [ ] **TDD:** failure in one mode does not skip the others (aggregate results); aggregate outcome: FailedTests if any XML has failures; InfraError if any run produced no XML.
- [ ] Progress: report current phase (Edit / Play / Player) in the Unit Tests window / notification.

### 2.5 Minimal Unity fixture project
- [ ] `testData/unity/SampleUnityProject` (or submodule) with:
  - 1 Edit Mode pass/fail
  - 1 Play Mode pass/fail
  - correct asmdefs ([Edit vs Play mode](https://docs.unity.com/en-us/engine/6000.6/manual/scripting/test-framework-introduction/getting-started/edit-mode-vs-play-mode-tests))
- [ ] `[integration]` script that runs all three modes when `UNITY_INTEGRATION=1` and CLI+Editor are installed.

---

## Epic 3 - Rider Unit Testing provider

### 3.1 Discovery
- [ ] Choose MVP strategy:
  - **A (fast):** reuse Unity Support / NUnit gutter discovery when present; our runner only **executes**.
  - **B:** own discovery via PSI (`[Test]`, `[UnityTest]`, Editor vs runtime asmdef).
- [ ] **TDD** for the chosen option (elements "†’ `IUnitTestElement` kinds).
- [ ] Document the decision briefly in code and here.

### 3.2 `IUnitTestProvider` + CLI task runner
- [ ] Stable provider id (e.g. `UnityCliTest`).
- [ ] Runners in the Unit Tests window selector: **Unity CLI - Edit Mode**, **Unity CLI - Play Mode**, **Unity CLI - Player**.
- [ ] `GetTaskSequence` / remote tasks that call `IUnityCliRunner` and map events:
  - suite started/finished
  - test started/finished (Pass/Fail/Ignore)
  - output + clickable stacktrace
- [ ] **TDD:** with XML fixture + fake process, the Rider session receives the expected event sequence (isolated host test).

### 3.3 Coexistence with Unity Editor runners
- [ ] Do not remove """Unity Editor - Edit/Play Mode""".
- [ ] Suggested default when the Editor is **not** connected: CLI.
- [ ] When the Editor is connected: still allow CLI (batch) - document the trade-off (two Editors / Library lock).

### 3.4 RD protocol (frontend "†” backend)
- [ ] Replace `myCall` / `myIconCall` stubs with a real model: settings, last run, CLI status, player target.
- [ ] RD generation test (`rdgen`) + minimal round-trip call.

---

## Epic 4 - Minimal Rider UX

### 4.1 Settings
- [ ] Unity CLI path
- [ ] Timeouts (Edit / Play / Player)
- [ ] Player `BuildTarget`
- [ ] Extra flags (allow-install? nographics?)
- [ ] Which modes are included in """Run All (CLI)"""

### 4.2 Error feedback
- [ ] Messages for: missing CLI, license, Safe Mode compile errors, timeout, invalid platform.
- [ ] Action: open Editor log / `--output` folder.

### 4.3 Cancel run
- [ ] Stop cancels the `unity test` process and cleans up the partial session.

---

## Epic 5 - Pipeline package (post-MVP)

Only after the batch CLI MVP is stable.

### 5.1 Detect Editor + Pipeline ready
- [ ] `unity status --format json` "†’ state `ready`.
- [ ] Fallback: if no Editor, use `unity test` (batch).

### 5.2 `unity command run_tests` / Pipeline API
- [ ] Map Pipeline `mode=editor|playmode|all` to our three targets.
- [ ] Player via Pipeline: validate current docs; if missing, keep batch CLI only for Player.
- [ ] **TDD** with an HTTP mock of the Pipeline server.

### 5.3 Preference """live Editor vs batch"""
- [ ] Setting: Auto \| Always batch \| Prefer live Pipeline.

---

## Epic 6 - Quality and release

### 6.1 Plugin test coverage
- [ ] Unit: builder, parser, exit codes, project detection at an agreed threshold.
- [ ] `./gradlew :check` in CI.
- [ ] Optional Unity integration job.

### 6.2 User documentation
- [ ] README: prerequisites (Unity CLI beta, installed Editor, license), how to run All / per mode, difference vs official Unity Support.
- [ ] Troubleshooting: Library lock, Safe Mode, Player heartbeat, exit 6 vs 8.

### 6.3 Packaging
- [ ] `buildPlugin` "†’ Marketplace / local install.
- [ ] CHANGELOG entry for """Run all Unity tests via CLI (Edit / Play / Player)""".

---

# Unity Rider CLI Pipeline

<p align="center">
  <img src="assets/logo.svg" alt="Unity Rider CLI Pipeline logo" width="160" />
</p>

A **JetBrains Rider** plugin that runs **Unity Test Framework** tests from Rider **without opening the Unity Editor UI**.

It drives the official [Unity CLI](https://docs.unity.com/en-us/unity-cli/unity-cli-reference) in batch mode (`unity test`): headless Editor, Test Runner, NUnit report, then surfaces results in Rider's Unit Tests window.

## Why

Rider""'s bundled Unity Support can already discover and run Unity tests, but it expects a **live Editor** plus `com.unity.ide.rider`. That works well when Unity is already open; it hurts when you only want to run the suite and the Editor is too heavy (or not open at all).

This plugin targets that gap:

| | Official Unity Support | This plugin (goal) |
|---|---|---|
| Needs Editor UI open | Yes | No |
| Runner | Unity Editor (Edit / Play Mode) | `unity test` (batch / headless) |
| Modes | Edit Mode, Play Mode | Edit Mode, Play Mode, **Player** |

## Idea

1. Detect a Unity project from the open Rider solution.
2. Resolve the `unity` CLI on the machine.
3. Run tests with the same targets as the Unity Test Runner tabs:

| Test Runner UI | CLI | What runs |
|---|---|---|
| **Edit Mode** | `--mode EditMode` | Editor tests |
| **Play Mode** | `--mode PlayMode` | Play Mode inside the Editor (still headless) |
| **Player** | `--mode <BuildTarget>` (e.g. `StandaloneOSX`) | Play Mode in a built Player |

4. Parse the NUnit XML report and show pass / fail / ignore in Rider.
5. Later (optional): prefer a live Editor via [`com.unity.pipeline`](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-cli-pipeline-package) when one is already connected; fall back to batch `unity test` otherwise.

Canonical invocations:

```bash
unity test /path/to/Project --mode EditMode       --report-format nunit --output ./tmp/edit.xml
unity test /path/to/Project --mode PlayMode       --report-format nunit --output ./tmp/play.xml
unity test /path/to/Project --mode StandaloneOSX  --report-format nunit --output ./tmp/player.xml
```

Exit codes that matter for the plugin:

| Code | Meaning |
|---|---|
| `0` | All passed |
| `8` | Run finished; at least one test failed |
| `6` | Infrastructure failure (compile, license, crash, timeout, ""?) |
| `2` | Bad arguments |

## Coexistence with Unity Support

This plugin does **not** replace Rider""'s bundled [Unity Support](https://www.jetbrains.com/help/rider/Running_and_Debugging_Unity_Tests.html) (`com.intellij.resharper.unity`). Gutters, discovery, and Editor-based Edit/Play Mode runners stay as they are. There is **no hard dependency** on Unity Support: the CLI runner still loads if that plugin is disabled. Later slices may add an optional dependency only if we reuse its discovery APIs.

## Status

Early scaffold (Rider plugin template + RD protocol). Product work is tracked in [Tasks.md](Tasks.md) and developed with **TDD**.

**Support policy (for now):**
- Unity **Editor**: current LTS only (`6000.0.x`, `2022.3.x`, final `f` builds). Tech Stream / alpha / beta / older LTS are ignored.
- Unity **CLI**: public beta line until Unity ships a stable CLI; minimum `1.0.0-beta.1`.

## Run (dev)

Requires .NET SDK 8+ and JDK 21.

**macOS / Linux**

```bash
./run.sh
```

**Windows**

```bat
run.bat
```

That prepares the Rider SDK / RD model, builds the .NET backend, and opens a test Rider with the plugin loaded (`:prepare`, `:compileDotNet`, `:runIde`).

## Build

```bash
./gradlew :buildPlugin   # ZIP under build/distributions
./gradlew :check         # tests (includes :testDotNet)
dotnet test src/dotnet/Rider.Plugins.UnityCliPipeline.Tests/Rider.Plugins.UnityCliPipeline.Tests.csproj
```

PRs and pushes to `main` run the [Quality Gate](.github/workflows/quality-gate.yml) workflow (Core unit tests + `PluginXmlTest`).

See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## References

- [Unity CLI - `test`](https://docs.unity.com/en-us/unity-cli/unity-cli-reference)
- [Unity Test Framework - command line](https://docs.unity.com/en-us/engine/6000.7/manual/scripting/test-framework-introduction/reference-command-line)
- [CLI vs Pipeline package](https://docs.unity.com/en-us/unity-production-pipeline/local-tools-cli/unity-cli-pipeline-package)
- [Rider - run Unity tests (official)](https://www.jetbrains.com/help/rider/Running_and_Debugging_Unity_Tests.html)

## License

See [LICENSE](LICENSE).

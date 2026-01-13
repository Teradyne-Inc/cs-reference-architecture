# ScanNetwork results: how `CoreInstanceTestResult` is used (Demo_CSRA + CSRA)

This document explains how the Demo_CSRA code path is intended to execute a ScanNetwork pattern(set), retrieve per-core/per-ICL results, and then datalog and/or diagnose those results.

## End-to-end flow (Demo_CSRA → CSRA)

Demo_CSRA’s ScanNetwork flow is structured as a 3-step pipeline:

1. **Execute pattern**
2. **Acquire results**
3. **(Optionally) datalog and/or diagnose**

### Demo entry point

- Demo entry point: [src/Demo/Demo_CSRA/ScanNetwork/ScanNetworkPattern.cs](../../src/Demo/Demo_CSRA/ScanNetwork/ScanNetworkPattern.cs)

Key calls (in order):

- During validation, Demo constructs a `ScanNetworkPatternInfo` that describes the pattern(set) and any CSV metadata.
- During body execution, Demo bursts the ScanNetwork pattern via:
  - `TheLib.Execute.ScanNetwork.RunPattern(...)`
- Immediately after burst, Demo acquires results via:
  - `TheLib.Acquire.ScanNetwork.PatternResults(...)`
    - This returns a `ScanNetworkPatternResults` object.
- Finally, Demo logs results via:
  - `TheLib.Datalog.TestScanNetwork(...)`
  - The exact logging behavior is controlled by `ScanNetworkDatalogOption` flags (log-by-core vs log-by-ICL, etc.).

### Acquire layer (CSRA)

Acquire is a thin pass-through:

- [src/Csra/TheLib/Acquire/ScanNetwork.cs](../../src/Csra/TheLib/Acquire/ScanNetwork.cs)

`PatternResults(...)` simply calls `scanNetworkPattern.GetScanNetworkPatternResults()`.

### Results container (CSRA)

The results model is:

- [src/Csra/Types/ScanNetworkPatternResults.cs](../../src/Csra/Types/ScanNetworkPatternResults.cs)

It contains two main dictionaries:

- `IclInstance`: per-ICL-instance results (`Dictionary<string, IclInstanceTestResult>`)
- `CoreInstance`: per-core-instance results (`Dictionary<string, CoreInstanceTestResult>`)

It is also responsible for:

- Aggregating core-level results from ICL-level results (via `ProcessCoreResults()`)
- Returning “failed core instance list per site”

## CSRA execution phases and how `TheLib` maps to them

Most CSRA test methods follow IG-XL’s “phased” execution model. You’ll typically see this pattern in Demo test methods:

- `if (TheExec.Flow.IsValidating) { ... }`
- `if (ShouldRunPreBody) { ... }`
- `if (ShouldRunBody) { ... }`
- `if (ShouldRunPostBody) { ... }`

Two concrete examples in Demo_CSRA are:

- ScanNetwork: [src/Demo/Demo_CSRA/ScanNetwork/ScanNetworkPattern.cs](../../src/Demo/Demo_CSRA/ScanNetwork/ScanNetworkPattern.cs)
- Continuity (parametric contact check): [src/Demo/Demo_CSRA/Continuity/Parametric.cs](../../src/Demo/Demo_CSRA/Continuity/Parametric.cs)

CSRA’s `Api.TheLib` is explicitly split along those responsibilities:

- [src/Csra/TheLib/TheLibManager.cs](../../src/Csra/TheLib/TheLibManager.cs)

### 1) Validation phase (compile-time / “check my arguments”)

**When:** `TheExec.Flow.IsValidating` is `true`.

**Goal:** Validate inputs and program resources before any real execution.

**Typical work done here:**

- Parse/validate user arguments (pinlists, CSV strings, enum values, ranges)
- Resolve and validate IG-XL resources (patterns exist, pinlists decompile, limits exist)
- Construct “info/config” objects that will be reused during execution (e.g., a `ScanNetworkPatternInfo`)
- Report validation failures as IG-XL template argument errors (via alert/validation helpers)

**Where it lives in CSRA:**

- `Api.TheLib.Validate.*` contains many of these guard rails and argument checks:
  - [src/Csra/TheLib/Validate.cs](../../src/Csra/TheLib/Validate.cs)
- `Api.Services.Alert` is the standard way to surface validation failures in a user-friendly way.

**Rule of thumb:** keep validation deterministic and avoid “real test” actions. (In practice, you generally avoid hardware actions here.)

### 2) Setup phase (prepare instruments/pattern context)

**When:** `ShouldRunPreBody` is `true`.

**Goal:** Put the tester (and DUT environment) into the state required for the measurement/execution.

**Typical work done here:**

- Apply levels/timing
- Apply DC force/meter settings
- Apply any reusable setup strings or configuration snapshots

**Where it lives in CSRA:**

- `Api.TheLib.Setup.*` for instrument setup blocks:
  - Example usage in ScanNetwork demo: calls `TheLib.Setup.LevelsAndTiming.Apply(true)`
- `Api.Services.Setup.Apply(...)` for higher-level, user-defined setup bundles.

### 3) Execute phase (do the actual action)

**When:** `ShouldRunBody` is `true`.

**Goal:** Perform the test action (burst patterns, run a measurement, run a search, etc.).

**Typical work done here:**

- Burst a digital pattern / pattern set
- Trigger a measurement/search
- Drive state machines that interact with IG-XL APIs

**Where it lives in CSRA:**

- `Api.TheLib.Execute.*` is the “do something” layer.
- For ScanNetwork, the demo calls:
  - `TheLib.Execute.ScanNetwork.RunPattern(...)`
  - Implementation stub: [src/Csra/TheLib/Execute/ScanNetwork.cs](../../src/Csra/TheLib/Execute/ScanNetwork.cs)

### 4) Acquire phase (read results back from IG-XL/hardware)

**When:** often during `ShouldRunBody` right after execution (sometimes in `PostBody`, depending on the test method).

**Goal:** Convert raw IG-XL/hardware results into typed CSRA result objects.

**Where it lives in CSRA:**

- `Api.TheLib.Acquire.*` is the “fetch results” layer.
- For ScanNetwork, the demo calls:
  - `TheLib.Acquire.ScanNetwork.PatternResults(...)`
  - Implementation: [src/Csra/TheLib/Acquire/ScanNetwork.cs](../../src/Csra/TheLib/Acquire/ScanNetwork.cs)
  - Which forwards to `ScanNetworkPatternInfo.GetScanNetworkPatternResults()`.

### 5) Datalog phase (turn results into limits/FTR/DTR)

**When:** `ShouldRunPostBody` is `true`.

**Goal:** Log results into IG-XL’s datalogging mechanisms (functional/parametric results, verbose text, etc.).

**Where it lives in CSRA:**

- `Api.TheLib.Datalog.*` wraps calls into `TheExec.Flow.*` APIs in a consistent, reusable way.
- For ScanNetwork, Demo_CSRA calls `TheLib.Datalog.TestScanNetwork(...)` with a `ScanNetworkDatalogOption` mask.

### Cross-cutting “Services” (available in all phases)

CSRA also exposes `Api.Services` for cross-cutting concerns (alerting, setup management, behavior flags, storage, etc.).

- `Api.Services` entry point: [src/Csra/Api.cs](../../src/Csra/Api.cs)

You’ll see Services used throughout validation/setup/execute/datalog, for example:

- Reporting validation failures (`Alert`)
- Applying named setup bundles (`Setup`)
- Controlling behaviors (e.g., offline-mode behavior)

## What `CoreInstanceTestResult` represents

- Type definition: [src/Csra/Types/CoreInstanceTestResult.cs](../../src/Csra/Types/CoreInstanceTestResult.cs)

This type represents **a single core instance’s test outcome**, tracked **per site**.

### Per-site pass/fail and validity

- `IsFailed` is a `Site<bool>` (IG-XL per-site container):
  - `true` means failed on that site
  - `false` means passed on that site
- `IsResultValid` is also a `Site<bool>` and is intended to indicate whether the core’s result is meaningful.
  - Example intent: the core result is only “valid” if all expected ICL instances under the core were tested (not masked, not disabled).
- `ErrorStatus` is a `Site<int>` and defaults to 0 per site.

### Optional datalog metadata

`CoreInstanceTestResult` carries optional metadata used by logging:

- `TestNumber`
- `TestName`

The intent is that the datalog layer can assign/log a stable identifier for “core X failed” (not just print text).

### Why it implements `IEnumerable<string>`

`CoreInstanceTestResult` also behaves like “the list of ICL instance names that belong to this core”:

- `_iclsInCore` holds the ICL instance names under this core.
- Enumerating the object yields those ICL names.

That makes it convenient for:

- Core aggregation logic (walk all ICLs under a core)
- Logging/debugging (“core A consists of ICL1, ICL2, …”)

## How Demo_CSRA expects to use these results

After `PatternResults(...)` returns, Demo_CSRA shows two logging “flavors”:

- **By core instance**
  - Iterate `result.CoreInstance` and log each core’s `IsFailed` (optionally only failed ones)
- **By ICL instance**
  - Iterate `result.IclInstance` and log each ICL’s `IsFailed`
  - Optionally include verbose details (e.g., via DTR)

Diagnosis is intended to use “failed core list per site” to selectively re-burst with different enable/disable contribution settings.

## Where the options come from

The logging options are defined as flags:

- [src/Csra/Types/Enums.cs](../../src/Csra/Types/Enums.cs)

Specifically the `ScanNetworkDatalogOption` enum (`LogByCoreInstance`, `LogByIclInstance`, etc.).

## Important reality check: stubs in this repo

As of the current repo state, large parts of ScanNetwork are scaffolded but not implemented: several methods throw `NotImplementedException`.

Notably:

- [src/Csra/Types/CoreInstanceTestResult.cs](../../src/Csra/Types/CoreInstanceTestResult.cs)
- [src/Csra/Types/ScanNetworkPatternResults.cs](../../src/Csra/Types/ScanNetworkPatternResults.cs)
- [src/Csra/Types/ScanNetworkPatternInfo.cs](../../src/Csra/Types/ScanNetworkPatternInfo.cs)
- [src/Csra/TheLib/Execute/ScanNetwork.cs](../../src/Csra/TheLib/Execute/ScanNetwork.cs)
- [src/Csra/TheLib/Datalog.cs](../../src/Csra/TheLib/Datalog.cs)

So the demo shows the **intended API and usage pattern**, but running this ScanNetwork demo path today will throw at runtime until those stubs are implemented.

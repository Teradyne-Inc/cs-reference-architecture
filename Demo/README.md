# Demo — IG-XL Test Program Examples

## What Is This Folder?

This folder contains the **IG-XL demo test program**: the project file, the ASCII program, the patterns, and the scripts that generate and load it. The C# test methods it exercises live outside this folder, under `ReferenceLibraries/`.

## Contents

| Folder / File | Purpose |
|---|---|
| `Demo.igxlProj` | IG-XL project file for generating and loading the demo program |
| `ASCIIProgram/` | IG-XL ASCII program files (flow, levels, timing, pins). Sub-programs: `common`, `spCS`, `spCSRA`, `spPortBridge`, `spVBT` |
| `Patterns/` | Digital test patterns used by the demo program |
| `IG-ComponentManager.json` | Required IG-XL / Oasis / SSL / Visual Studio versions, checked by IG-Link |
| `SimulatedConfig.txt` | Configuration for running offline / simulated |
| `_LoadProgram.cmd` | Generates the program and loads IG-XL via IGLinkCL |
| `_GenerateProgram.cmd` | Generates the program without loading IG-XL |
| `generate-subset-solution.cmd` | Template — edit it to assemble the subset solution you want; this specific one creates `SOC124.sln` |

There is no `.igxl` workbook in the folder. It is **generated** from `Demo.igxlProj` and the ASCII files when you run `_LoadProgram.cmd` or `_GenerateProgram.cmd`, which is why the ASCII program rather than a binary workbook is what's under version control.

## CsTestMethods vs CsraTestMethods — Understanding the Difference

Two parallel test method implementations cover the same test categories (Continuity, Functional, Leakage, Parametric, …). They solve the same problems by different means:

| Project | Location | Approach | Recommendation |
|---|---|---|---|
| **CsTestMethods** | `ReferenceLibraries/CsTestMethods/` | Calls IG-XL public APIs **directly** in straight C# | Usable, but **not recommended** — the code is tied to one device and has to be rewritten for the next program |
| **CsraTestMethods** | `ReferenceLibraries/CsraTestMethods/` | Calls the **C#RA library** (`TheLib`, `Services`) | **Recommended** — reusable across projects, typically only ~30% customization needed |

### Why Both Exist

The side-by-side comparison is **intentional**. `CsTestMethods` shows the traditional approach — functional, but coupled to one specific program. `CsraTestMethods` shows the same tests built from reusable blocks. To compare them, open the same test category (e.g. `Continuity/`) in both projects.

In the demo flow, the `spCS` and `spCSRA` ASCII sub-programs are what wire each implementation into IG-XL.

## Unit Tests

`ReferenceLibraries/UnitTests/` is the example unit test project — an inheritable `Base` class plus worked examples in `Example_UT.cs`. Copy it into your own solution as a starting point.

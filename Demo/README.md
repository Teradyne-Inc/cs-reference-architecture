# Demo — IG-XL Test Program Examples

## What Is This Folder?

This folder contains the **IG-XL demo test program** and a **raw C# test method implementation** that calls IG-XL APIs directly without using the CSRA library.

## Contents

| Folder / File | Purpose |
|---|---|
| `Demo_CS/` | C# test methods written in **raw C#** — calls IG-XL public APIs directly, without the CSRA library |
| `Demo_UT/` | Unit tests for the Demo_CS test methods |
| `Demo.igxlProj` | IG-XL project file for loading the demo program on the tester |
| `ASCIIProgram/` | IG-XL ASCII program files (flow, levels, timing, pins, etc.) |
| `Patterns/` | Digital test patterns used by the demo program |

## Demo_CS vs Csra_TestMethods — Understanding the Difference

The repository contains **two parallel test method implementations** with the same test categories (Continuity, Functional, Leakage, Parametric, etc.). They solve the same testing problems using different approaches:

| Project | Location | Approach | Reusability |
|---|---|---|---|
| **Demo_CS** | `src/Demo/Demo_CS/` | Calls IG-XL public APIs **directly** in straight C# | ❌ Project-specific — must be rewritten for each customer/chip |
| **Csra_TestMethods** | `src/Csra_TestMethods/` | Calls the **CSRA library** (`TheLib`, `Services`) | ✅ Reusable across projects — only ~30% customization needed |

### Why Both Exist

This side-by-side comparison is **intentional**. It demonstrates the value of the CSRA library:

- **Demo_CS** shows the *traditional* approach — functional, but tightly coupled to one specific program
- **Csra_TestMethods** shows the *CSRA* approach — the same tests achieved through reusable building blocks

To compare the two approaches, open the same test category (e.g., `Continuity/`) in both projects side-by-side.

## Unit Tests

Tests for the Demo_CS methods are in `src/Demo/Demo_UT/`.

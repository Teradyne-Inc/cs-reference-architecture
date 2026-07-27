# Tol — Thin Object Layer

## What Is This?

**Tol** (Thin Object Layer) provides **typed pin abstractions** for IG-XL hardware instruments. It wraps the raw IG-XL driver interfaces (`tlDriverPPMUPins`, `DriverDCVIPins`, `DriverDCVSPins`, `DriverDigitalPins`) into clean, strongly-typed C# interfaces.

## Why Does It Exist?

The IG-XL public API exposes pin access through loosely-typed driver objects. Tol adds a thin interface layer on top, so that the CSRA library (`Csra/`) can work with pins in a type-safe way without coupling directly to the raw IG-XL driver types.

## Key Types

| Interface | Implementation | Instrument Type |
|---|---|---|
| `IPpmuPins` | `PpmuPins` | Per-Pin Measurement Unit (UP2200/UP5000) |
| `IDcviPins` | `DcviPins` | DC Voltage/Current Instrument (UVI264) |
| `IDcvsPins` | `DcvsPins` | DC Voltage Source (UVS64/UVS256/UVS64HP) |
| `IDigitalPins` | `DigitalPins` | Digital channel hardware |

All pin types implement the generic `IPins<TPin>` base interface, which provides `Name`, `GetPinListItem()`, and `GetIndividualPins()`.

The `Factory` class provides protected factory methods for creating pin instances, used by `Csra.Types.PinsFactory`.

## Relationship to Csra

```
Tol (this project)          Csra/Types/Pins.cs
┌─────────────────┐         ┌──────────────────────┐
│ IPpmuPins        │◄────────│ Pins class resolves   │
│ IDcviPins        │         │ pin groups and routes  │
│ IDcvsPins        │         │ to the correct Tol     │
│ IDigitalPins     │         │ instrument type        │
└─────────────────┘         └──────────────────────┘
```

## Unit Tests

Tests for this project are in `src/UT/Tol_UT/`.

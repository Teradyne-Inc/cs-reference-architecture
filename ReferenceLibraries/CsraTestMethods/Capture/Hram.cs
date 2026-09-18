using System;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using Tol;
using static Csra.Api;

namespace CsraTestMethods.Capture {

    /// <summary>
    /// Test methods that capture digital pattern execution data using HRAM (Hardware RAM), either failing cycle indices
    /// (<see cref="CaptureFails"/>) or raw digitized device data (<see cref="CaptureStv"/>). Both methods run the configured
    /// pattern once, read the requested data back via <see cref="HramCapture"/>, and datalog per-pin/per-site counts of the
    /// data captured (failing cycles for <see cref="CaptureFails"/>, samples read for <see cref="CaptureStv"/>).
    /// </summary>
    [TestClass(Creation.TestInstance), Serializable]
    public class Hram : TestCodeBase {

        // Fields to be used by CaptureFails Test Method only.
        private List<PatternInfo> _failPattern;
        private Pins _failPins;
        private PinSite<int[]> _failData;

        // Fields to be used by CaptureStv Test Method only.
        private List<PatternInfo> _stvPattern;
        private Pins _stvPins;
        private PinSite<double[]> _stvData;

        // Validates and resolves the pattern/pins/captureLimit/timeDomain for a single capture call. Called once per test
        // row, during the IsValidating pass only — results are written via `out` into method-specific fields
        // (_failPattern/_failPins or _stvPattern/_stvPins) so CaptureFails and CaptureStv never share mutable state,
        // even if both are bound to the same TestInstance across different Test-sheet rows.
        private void ValidateCapture(Pattern pattern, PinList pinList, int captureLimit, string timeDomain, out List<PatternInfo> validatedPattern, out Pins validatedPins) {
            TheLib.Validate.Pins(pinList, nameof(pinList), out validatedPins);
            TheLib.Validate.Pattern(pattern, nameof(pattern), out validatedPattern);

            if (validatedPattern is { Count: > 1 }) {
                Services.Alert.Error($"Only a single pattern is supported; '{pattern}' resolves to {validatedPattern.Count} patterns.", 1);
                validatedPattern = null;
            }

            if (captureLimit < -1) {
                Services.Alert.Error($"Capture limit '{captureLimit}' is not supported; must be -1 (max depth), 0 (hardware default), or a positive value.", 1);
            }

            // An empty timeDomain targets the System domain, which is always valid and not listed in Patterns(...).TimeDomains.
            // Checking it against the resolved pattern's own time domains catches a typo'd/mismatched domain here, during
            // validation, instead of only surfacing it later as a hardware error when HramCapture is constructed in the body.
            if (!string.IsNullOrEmpty(timeDomain) && validatedPattern is { Count: 1 }) {
                // If validatedPattern[0].TimeDomain is null or empty (e.g. a non-multi-domain pattern with no listed
                // domains), there is nothing to validate timeDomain against — skip the check rather than reject it.
                string[] patternTimeDomains = (validatedPattern[0].TimeDomain ?? string.Empty).Split(',').Select(d => d.Trim()).Where(d => !string.IsNullOrEmpty(d)).ToArray();
                if (patternTimeDomains.Length > 0 && !patternTimeDomains.Contains(timeDomain, StringComparer.OrdinalIgnoreCase)) {
                    Services.Alert.Error($"Time domain '{timeDomain}' is not defined for pattern '{pattern}'; expected one of: {string.Join(", ", patternTimeDomains)}.", 1);
                }
            }
        }

        /// <summary>
        /// Captures failing cycle indices for the configured pattern/pins using HRAM.
        /// </summary>
        /// <param name="pattern">Pattern name to capture on.</param>
        /// <param name="pinList">Pin list to capture on. Must resolve to digital pins — non-digital pins are validated by <see cref="TheLib"/>.Validate.Pins but leave <c>Pins.Digital</c> <see langword="null"/>, which is only detected when the <see cref="HramCapture"/> is constructed in the body.</param>
        /// <param name="captureLimit">Maximum number of cycles to store in HRAM. Must be <c>-1</c> (max depth), <c>0</c> (hardware default), or a positive value; validated at validation time and, redundantly, by <see cref="HramCapture"/> itself.</param>
        /// <param name="timeDomain">Optional. Digital time domain to configure. Default value is an empty string (targets the <b>System</b> time domain). If non-empty, must match one of the time domains associated with the resolved <paramref name="pattern"/>; validated at validation time.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        /// <remarks>
        /// <para>
        /// Only a single resolved pattern is supported; if <paramref name="pattern"/> resolves to more than one pattern,
        /// validation fails with an error and the pattern list is discarded so the body does not run.
        /// </para>
        /// <para>
        /// A non-empty <paramref name="timeDomain"/> is checked, at validation time, against the time domains associated
        /// with the resolved pattern (<see cref="PatternInfo.TimeDomain"/>); a mismatch is alerted as an error during
        /// validation instead of only surfacing later as a hardware error when <see cref="HramCapture"/> is constructed
        /// in the body.
        /// </para>
        /// <para>
        /// The <see cref="HramCapture"/> is constructed <i>inside</i> the try block, alongside running the pattern and
        /// reading data, so any exception thrown by the constructor itself (for example <c>ArgumentNullException</c> when
        /// <c>Pins.Digital</c> is <see langword="null"/>, or <c>ArgumentOutOfRangeException</c> for an invalid
        /// <paramref name="captureLimit"/>) is caught and alerted the same as a failure running the pattern or reading
        /// captured data, via <see cref="Services.Alert"/>. The capture is always reset in a <c>finally</c> block; this is
        /// a no-op if construction itself failed, since <c>capture</c> is still <see langword="null"/> in that case.
        /// </para>
        /// <para>
        /// The datalogged parametric reports, per pin/site, the number of failing cycles captured — not the cycle offsets
        /// themselves. Pins with zero failures on a site (or pins with no digital component at all) log a count of <c>0</c>.
        /// </para>
        /// </remarks>
        [TestMethod, Steppable, CustomValidation]
        public void CaptureFails(Pattern pattern, PinList pinList, int captureLimit, string timeDomain = "", string setup = "") {
            if (TheExec.Flow.IsValidating) {
                ValidateCapture(pattern, pinList, captureLimit, timeDomain, out _failPattern, out _failPins);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                HramCapture capture = null;
                try {
                    capture = new HramCapture(CaptType.Fail, _failPins.Digital, captureLimit, timeDomain);
                    TheLib.Execute.Digital.RunPattern(_failPattern[0]);
                    _failData = capture.ReadFails();
                } catch (Exception ex) {
                    Services.Alert.Error($"HRAM Fail capture failed ({ex.GetType().Name}): {ex.Message}");
                } finally {
                    capture?.Reset();
                }
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(BuildPerPinSiteCounts(_failPins, _failData));
            }

        }

        /// <summary>
        /// Captures raw STV device data for the configured pattern/pins using HRAM.
        /// </summary>
        /// <param name="pattern">Pattern name to capture on.</param>
        /// <param name="pinList">Pin list to capture on. Must resolve to digital pins — non-digital pins are validated by <see cref="TheLib"/>.Validate.Pins but leave <c>Pins.Digital</c> <see langword="null"/>, which is only detected when the <see cref="HramCapture"/> is constructed in the body.</param>
        /// <param name="captureLimit">Maximum number of cycles to store in HRAM. Must be <c>-1</c> (max depth), <c>0</c> (hardware default), or a positive value; validated at validation time and, redundantly, by <see cref="HramCapture"/> itself.</param>
        /// <param name="timeDomain">Optional. Digital time domain to configure. Default value is an empty string (targets the <b>System</b> time domain). If non-empty, must match one of the time domains associated with the resolved <paramref name="pattern"/>; validated at validation time.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        /// <remarks>
        /// <para>
        /// Only a single resolved pattern is supported; if <paramref name="pattern"/> resolves to more than one pattern,
        /// validation fails with an error and the pattern list is discarded so the body does not run.
        /// </para>
        /// <para>
        /// A non-empty <paramref name="timeDomain"/> is checked, at validation time, against the time domains associated
        /// with the resolved pattern (<see cref="PatternInfo.TimeDomain"/>); a mismatch is alerted as an error during
        /// validation instead of only surfacing later as a hardware error when <see cref="HramCapture"/> is constructed
        /// in the body.
        /// </para>
        /// <para>
        /// The <see cref="HramCapture"/> is constructed <i>inside</i> the try block, alongside running the pattern and
        /// reading data, so any exception thrown by the constructor itself (for example <c>ArgumentNullException</c> when
        /// <c>Pins.Digital</c> is <see langword="null"/>, or <c>ArgumentOutOfRangeException</c> for an invalid
        /// <paramref name="captureLimit"/>) is caught and alerted the same as a failure running the pattern or reading
        /// captured data, via <see cref="Services.Alert"/>. The capture is always reset in a <c>finally</c> block; this is
        /// a no-op if construction itself failed, since <c>capture</c> is still <see langword="null"/> in that case.
        /// </para>
        /// <para>
        /// The datalogged parametric reports, per pin/site, the number of raw samples read back (each digitized value is
        /// <c>0</c>, <c>1</c>, or <c>255</c> for midband/undefined, per <see cref="HramCapture.ReadStv"/>) — not the sample
        /// values themselves. Pins with no captured data on a site (or pins with no digital component at all) log a count of <c>0</c>.
        /// Because <see cref="HramCapture.ReadStv"/> reads back the maximum captured-cycle count across all active sites (to
        /// avoid truncating whichever site captured the most), this logged count is <i>uniform</i> across every captured
        /// pin/site — it is not each site's actual capture depth, and is not a meaningful per-site measurement on its own.
        /// </para>
        /// </remarks>
        [TestMethod, Steppable, CustomValidation]
        public void CaptureStv(Pattern pattern, PinList pinList, int captureLimit, string timeDomain = "", string setup = "") {
            if (TheExec.Flow.IsValidating) {
                ValidateCapture(pattern, pinList, captureLimit, timeDomain, out _stvPattern, out _stvPins);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                HramCapture capture = null;
                try {
                    capture = new HramCapture(CaptType.STV, _stvPins.Digital, captureLimit, timeDomain);
                    TheLib.Execute.Digital.RunPattern(_stvPattern[0]);
                    _stvData = capture.ReadStv();
                } catch (Exception ex) {
                    Services.Alert.Error($"HRAM Stv capture failed ({ex.GetType().Name}): {ex.Message}");
                } finally {
                    capture?.Reset();
                }
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(BuildPerPinSiteCounts(_stvPins, _stvData));
            }
        }

        // Builds a per-pin/per-site count of captured data-array lengths (failing cycles for _failData, samples for _stvData).
        // Pins with no captured data on a site, or with no digital component at all, report a count of 0.
        private PinSite<int> BuildPerPinSiteCounts<T>(Pins pins, PinSite<T[]> capturedData) {
            PinSite<int> counts = new();
            foreach (IDigitalPins pin in pins?.Digital?.GetIndividualPins() ?? Enumerable.Empty<IDigitalPins>()) {
                Site<T[]> dataPin = capturedData?.FirstOrDefault(p => p.PinName == pin.Name);
                Site<int> siteCounts = new() { PinName = pin.Name };
                ForEachSite(site => siteCounts[site] = dataPin?[site]?.Length ?? 0);
                counts.Add(siteCounts);
            }
            return counts;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using Tol;
using static Csra.Api;

namespace CsraTestMethods.Capture {

    [TestClass(Creation.TestInstance), Serializable]
    public class Dssc : TestCodeBase {

        private List<PatternInfo> _pattern;
        private Pins _pins;
        private PinSite<double[]> _waveformData;

        /// <summary>
        /// Captures high-speed digital waveform samples for the configured pattern/pins using DSSC.
        /// </summary>
        /// <param name="pattern">Pattern name to capture on.</param>
        /// <param name="pinList">Pin list to capture on.</param>
        /// <param name="signalName">DSSC capture signal name to add to the capture chain.</param>
        /// <param name="sampleSize">Number of samples to capture. Defaults to 65536.</param>
        /// <param name="sampleRate">Sample rate in Hz. Defaults to 200000.0 (200 kHz).</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        [TestMethod, Steppable, CustomValidation]
        public void CaptureWaveform(Pattern pattern, PinList pinList, string signalName, int sampleSize = 65536, double sampleRate = 200000.0, string setup = "") {
            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);

                if (string.IsNullOrWhiteSpace(signalName)) {
                    Services.Alert.Error("A DSSC capture signal name is required.", 1);
                }

                if (sampleSize <= 0) {
                    Services.Alert.Error("Sample size must be greater than zero.", 1);
                }

                if (double.IsNaN(sampleRate) || double.IsInfinity(sampleRate) || sampleRate <= 0) {
                    Services.Alert.Error("Sample rate must be a finite, positive value.", 1);
                }

                if (_pattern is { Count: > 1 }) {
                    Services.Alert.Error($"Only a single pattern is supported; '{pattern}' resolves to {_pattern.Count} patterns.", 1);
                    _pattern = null;
                }
            } else {

                if (ShouldRunPreBody) {
                    TheLib.Setup.LevelsAndTiming.Apply(true);
                    Services.Setup.Apply(setup);
                }

                if (ShouldRunBody) {
                    DsscCapture capture = null;
                    try {
                        capture = new DsscCapture(_pins.Digital, signalName, _pattern[0].Name, sampleSize, sampleRate);
                        TheLib.Execute.Digital.RunPattern(_pattern[0]);
                        _waveformData = capture.ReadStv();
                    } catch (Exception ex) {
                        Services.Alert.Error($"DSSC capture failed ({ex.GetType().Name}): {ex.Message}");
                    } finally {
                        capture?.Reset();
                    }
                }

                if (ShouldRunPostBody) {
                    PinSite<int> sampleCounts = new();
                    if (_pins?.Digital is not null) {
                        foreach (IDigitalPins pin in _pins.Digital.GetIndividualPins()) {
                            Site<double[]> wavePin = _waveformData?.FirstOrDefault(p => p.PinName == pin.Name);
                            Site<int> counts = new() { PinName = pin.Name };
                            ForEachSite(site => counts[site] = wavePin?[site]?.Length ?? 0);
                            sampleCounts.Add(counts);
                        }
                    }
                    TheLib.Datalog.TestParametric(sampleCounts);
                }
            }
        }
    }
}

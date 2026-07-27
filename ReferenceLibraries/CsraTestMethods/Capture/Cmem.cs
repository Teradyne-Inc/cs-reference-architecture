using System;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using Tol;
using static Csra.Api;

namespace CsraTestMethods.Capture {

    [TestClass(Creation.TestInstance), Serializable]
    public class Cmem : TestCodeBase {

        private List<PatternInfo> _pattern;
        private Pins _pins;
        private PinSite<int[]> _failData;
        private PinSite<double[]> _stvData;
        private tlCMEMCaptureSource _captureSource;

        /// <summary>
        /// Captures failing cycle indices for the configured pattern/pins using CMEM.
        /// </summary>
        /// <param name="pattern">Pattern name to capture on.</param>
        /// <param name="pinList">Pin list to capture on.</param>
        /// <param name="captureSource">CMEM capture source to use for the Fail capture.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        [TestMethod, Steppable, CustomValidation]
        public void CaptureFails(Pattern pattern, PinList pinList, string captureSource, string setup = "") {
            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);

                if (captureSource.ToLower() != "passfaildata" && captureSource.ToLower() != "patpassfaildata") {
                    Services.Alert.Error($"Capture source '{captureSource}' is not supported.", 1);
                } else if (captureSource.ToLower() == "patpassfaildata") {
                    _captureSource = tlCMEMCaptureSource.PatPassFailData;
                } else if (captureSource.ToLower() == "passfaildata") {
                    _captureSource = tlCMEMCaptureSource.PassFailData;
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
                    CmemCapture capture = new(CmemCaptType.Fail, _captureSource, _pins.Digital);
                    try {
                        TheLib.Execute.Digital.RunPattern(_pattern[0]);
                        _failData = capture.ReadFails();

                    } catch (Exception ex) {
                        Services.Alert.Error($"CMEM Fail capture failed ({ex.GetType().Name}): {ex.Message}");
                    } finally {
                        capture.Reset();
                    }
                }

                if (ShouldRunPostBody) {
                    PinSite<int> failCounts = new();
                    if (_pins?.Digital is not null) {
                        foreach (IDigitalPins pin in _pins.Digital.GetIndividualPins()) {
                            Site<int[]> failPin = _failData?.FirstOrDefault(p => p.PinName == pin.Name);
                            Site<int> counts = new() { PinName = pin.Name };
                            ForEachSite(site => counts[site] = failPin?[site]?.Length ?? 0);
                            failCounts.Add(counts);
                        }
                    }
                    TheLib.Datalog.TestParametric(failCounts);
                }
            }
        }

        /// <summary>
        /// Captures raw STV device data for the configured pattern/pins using CMEM.
        /// </summary>
        /// <param name="pattern">Pattern name to capture on.</param>
        /// <param name="pinList">Pin list to capture on.</param>
        /// <param name="captureSource">CMEM capture source to use for the STV capture.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        [TestMethod, Steppable, CustomValidation]
        public void CaptureStv(Pattern pattern, PinList pinList, string captureSource, string setup = "") {
            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);

                if (captureSource.ToLower() != "dutdata" && captureSource.ToLower() != "patdutdata") {
                    Services.Alert.Error($"Capture source '{captureSource}' is not supported.", 1);
                } else if (captureSource.ToLower() == "patdutdata") {
                    _captureSource = tlCMEMCaptureSource.PatDutData;
                } else if (captureSource.ToLower() == "dutdata") {
                    _captureSource = tlCMEMCaptureSource.DutData;
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
                    CmemCapture capture = new(CmemCaptType.STV, _captureSource, _pins.Digital);
                    try {
                        TheLib.Execute.Digital.RunPattern(_pattern[0]);
                        _stvData = capture.ReadStv();
                    } catch (Exception ex) {
                        Services.Alert.Error($"CMEM Stv capture failed ({ex.GetType().Name}): {ex.Message}");
                    } finally {
                        capture.Reset();
                    }
                }

                if (ShouldRunPostBody) {
                    PinSite<int> sampleCounts = new();
                    if (_pins?.Digital is not null) {
                        foreach (IDigitalPins pin in _pins.Digital.GetIndividualPins()) {
                            Site<double[]> stvPin = _stvData?.FirstOrDefault(p => p.PinName == pin.Name);
                            Site<int> counts = new() { PinName = pin.Name };
                            ForEachSite(site => counts[site] = stvPin?[site]?.Length ?? 0);
                            sampleCounts.Add(counts);
                        }
                    }
                    TheLib.Datalog.TestParametric(sampleCounts);
                }
            }
        }
    }
}

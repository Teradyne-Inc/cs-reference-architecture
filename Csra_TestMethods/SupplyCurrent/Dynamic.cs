using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.SupplyCurrent {

    [TestClass(Creation.TestInstance), Serializable]
    public class Dynamic : TestCodeBase {

        private Pins _pins;
        private PinSite<Samples<double>> _pinsMeasuredSamples;
        private PatternInfo _patternInfo;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Performs a predefined number of current measurements synchronized by a Flag stop with the pattern.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="measureRange">The range for measurement.</param>
        /// <param name="clampValue">The value to clamp.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="stops">The number of stops executing a strobe on the instrument outside of the pattern - need to match with the pattern</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList pinList, double forceValue, double measureRange, double clampValue, double waitTime, Pattern pattern,
                        int stops, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _patternInfo);
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                TheLib.Validate.GreaterOrEqual(stops, 1, nameof(stops));
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
                _patternInfo.SetFlags = (int)CpuFlag.A;
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.SetForceAndMeter(_pins, TLibOutputMode.ForceVoltage, forceValue, forceValue, clampValue, Measure.Current, measureRange);
                TheLib.Execute.Digital.StartPattern(_patternInfo);
                for (int i = 0; i < stops; i++) {
                    TheLib.Execute.Digital.ContinueToConditionalStop(_patternInfo, () => {
                        TheLib.Execute.Wait(waitTime);
                        TheLib.Acquire.Dc.Strobe(_pins);
                    });
                }
                TheLib.Execute.Digital.WaitPatternDone(_patternInfo);
                _pinsMeasuredSamples = TheLib.Acquire.Dc.ReadMeasuredSamples(_pins, stops);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_pinsMeasuredSamples, forceValue, "V");
            }
        }
        #endregion
    }
}

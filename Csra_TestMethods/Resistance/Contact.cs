using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Resistance {

    [TestClass(Creation.TestInstance), Serializable]
    public class Contact : TestCodeBase {

        private Pins _pinsFirst;
        private PinSite<double> _measFirst;
        private PinSite<double> _measSecond;
        private PinSite<double> _forceFirst;
        private PinSite<double> _forceSecond;
        private PinSite<double> _resistanceValue;
        private TLibOutputMode _outputMode;
        private Measure _measureMode;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Performs a resistance measurement by forcing two values of voltage or current and measuring two currents or voltages.
        /// </summary>
        /// <param name="forcePin">Pin to force and measure.</param>
        /// <param name="forceMode">The mode for forcing (e.g., Voltage or Current).</param>
        /// <param name="forceFirstValue">First value to force.</param>
        /// <param name="forceSecondValue">Second value to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measureFirstRange">The range for first measurement.</param>
        /// <param name="measureSecondRange">The range for second measurement.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList forcePin, string forceMode, double forceFirstValue, double forceSecondValue, double clampValueOfForcePin,
            double measureFirstRange, double measureSecondRange, double waitTime = 0.0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePin, nameof(forcePin), out _pinsFirst);
                TheLib.Validate.Enum(forceMode.ToLower(), nameof(forceMode), out _outputMode);
                _measureMode = _outputMode == TLibOutputMode.ForceVoltage ? Measure.Current : Measure.Voltage;
                _forceFirst = new PinSite<double>(forcePin, forceFirstValue);
                _forceSecond = new PinSite<double>(forcePin, forceSecondValue);
                _containsDigitalPins = _pinsFirst.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pinsFirst);
                TheLib.Setup.Dc.Connect(_pinsFirst);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.SetForceAndMeter(_pinsFirst, _outputMode, forceFirstValue, forceFirstValue, clampValueOfForcePin, _measureMode,
                    measureFirstRange);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirst);
                TheLib.Setup.Dc.SetForceAndMeter(_pinsFirst, _outputMode, forceSecondValue, forceSecondValue, clampValueOfForcePin, _measureMode,
                    measureSecondRange, false);
                TheLib.Execute.Wait(waitTime);
                _measSecond = TheLib.Acquire.Dc.Measure(_pinsFirst);
                _resistanceValue = (_outputMode == TLibOutputMode.ForceVoltage) ? (_forceFirst - _forceSecond) / (_measFirst - _measSecond) :
                    (_measFirst - _measSecond) / (_forceFirst - _forceSecond);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pinsFirst);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pinsFirst);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion
    }
}

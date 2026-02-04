using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Resistance {

    [TestClass(Creation.TestInstance), Serializable]
    public class RdsOn : TestCodeBase {

        private Pins _allPins;
        private Pins _allMeasPins;
        private Pins _pinsFirst;
        private Pins _pinsSecond;
        private Pins _pinsFirstMeas;
        private Pins _pinsSecondMeas;
        private PinSite<double> _measFirst;
        private PinSite<double> _measSecond;
        private PinSite<double> _forceFirst;
        private PinSite<double> _forceSecond;
        private PinSite<double> _resistanceValue;
        private PinSite<double> _labelVoltage;
        private TLibOutputMode _outputMode;
        private Measure _measureMode;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Performs a resistance measurement by forcing voltage or current and measuring current or voltage on the same pin.
        /// </summary>
        /// <param name="forcePin">Pin to force and measure.</param>
        /// <param name="forceMode">The mode for forcing (e.g., Voltage or Current).</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="measureRange">The range for the measurement.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        /// <param name="labelOfStoredVoltage">Optional. Label of a reference voltage from a previously stored measurement.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList forcePin, string forceMode, double forceValue, double measureRange, double waitTime = 0,
            string labelOfStoredVoltage = "", string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePin, nameof(forcePin), out _pinsFirst);
                if (labelOfStoredVoltage != "") _labelVoltage = new PinSite<double>(forcePin, double.Parse(labelOfStoredVoltage));
                else _labelVoltage = new PinSite<double>(forcePin, 0.0);
                TheLib.Validate.Enum(forceMode.ToLower(), nameof(forceMode), out _outputMode);
                _measureMode = _outputMode == TLibOutputMode.ForceVoltage ? Measure.Current : Measure.Voltage;
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _forceFirst = new PinSite<double>(forcePin, forceValue);
                _containsDigitalPins = _pinsFirst.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pinsFirst);
                TheLib.Setup.Dc.Connect(_pinsFirst);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.SetForceAndMeter(_pinsFirst, _outputMode, forceValue, forceValue, measureRange, _measureMode, measureRange);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirst);
                _resistanceValue = (_outputMode == TLibOutputMode.ForceVoltage) ? (_forceFirst - _labelVoltage) / _measFirst :
                    (_measFirst - _labelVoltage) / _forceFirst;
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pinsFirst);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pinsFirst);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion

        /// <summary>
        /// Performs a resistance measurement by forcing voltage or current on one pin and measuring current or voltage on second pin.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceMode">The mode for forcing (e.g., Voltage or Current).</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measurePin">Pin to measure.</param>
        /// <param name="measureRange">The range for measurement.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        /// <param name="labelOfStoredVoltage">Optional. Label of a reference voltage from a previously stored measurement.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region TwoPinsOneForceOneMeasure
        [TestMethod, Steppable, CustomValidation]
        public void TwoPinsOneForceOneMeasure(PinList forcePin, string forceMode, double forceValue, double clampValueOfForcePin, PinList measurePin,
            double measureRange, double waitTime = 0, string labelOfStoredVoltage = "", string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePin, nameof(forcePin), out _pinsFirst);
                TheLib.Validate.Pins(measurePin, nameof(measurePin), out _pinsFirstMeas);
                _forceFirst = new PinSite<double>(forcePin, forceValue);
                if (labelOfStoredVoltage != "") _labelVoltage = new PinSite<double>(forcePin, double.Parse(labelOfStoredVoltage));
                else _labelVoltage = new PinSite<double>(forcePin, 0.0);
                TheLib.Validate.Enum(forceMode.ToLower(), nameof(forceMode), out _outputMode);
                _measureMode = _outputMode == TLibOutputMode.ForceVoltage ? Measure.Current : Measure.Voltage;
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _allPins = new Pins(forcePin);
                _allPins.Add(measurePin);
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.ForceHiZ(_pinsFirstMeas);
                TheLib.Setup.Dc.Force(_pinsFirst, _outputMode, forceValue, forceValue, clampValueOfForcePin);
                TheLib.Setup.Dc.SetMeter(_pinsFirstMeas, _measureMode, measureRange);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirstMeas);
                _resistanceValue = (_outputMode == TLibOutputMode.ForceVoltage) ? (_forceFirst - _labelVoltage) / _measFirst :
                    (_measFirst - _labelVoltage) / _forceFirst;
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion

        /// <summary>
        /// Performs a resistance delta measurement by forcing two force values on one pin and measuring with a second pin.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceMode">Force Mode of force pin (e.g., Voltage or Current).</param>
        /// <param name="forceFirstValue">First value to force.</param>
        /// <param name="forceSecondValue">Second value to force to calculate a delta.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measurePin">Pin to measure.</param>
        /// <param name="measureFirstRange">First range for the measurement.</param>
        /// <param name="measureSecondRange">Second range for the measurement.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region TwoPinsDeltaForceDeltaMeasure
        [TestMethod, Steppable, CustomValidation]
        public void TwoPinsDeltaForceDeltaMeasure(PinList forcePin, string forceMode, double forceFirstValue, double forceSecondValue,
            double clampValueOfForcePin, PinList measurePin, double measureFirstRange, double measureSecondRange, double waitTime = 0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePin, nameof(forcePin), out _pinsFirst);
                TheLib.Validate.Pins(measurePin, nameof(measurePin), out _pinsFirstMeas);
                TheLib.Validate.Enum(forceMode.ToLower(), nameof(forceMode), out _outputMode);
                _measureMode = _outputMode == TLibOutputMode.ForceVoltage ? Measure.Current : Measure.Voltage;
                _forceFirst = new PinSite<double>(forcePin, forceFirstValue);
                _forceSecond = new PinSite<double>(forcePin, forceSecondValue);
                _allPins = new Pins(forcePin);
                _allPins.Add(measurePin);
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.ForceHiZ(_pinsFirstMeas);
                TheLib.Setup.Dc.Force(_pinsFirst, _outputMode, forceFirstValue, forceFirstValue, clampValueOfForcePin);
                TheLib.Setup.Dc.SetMeter(_pinsFirstMeas, _measureMode, measureFirstRange);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirstMeas);

                TheLib.Setup.Dc.Force(_pinsFirst, _outputMode, forceSecondValue, forceSecondValue, clampValueOfForcePin);
                TheLib.Setup.Dc.SetMeter(_pinsFirstMeas, _measureMode, measureSecondRange);
                TheLib.Execute.Wait(waitTime);
                _measSecond = TheLib.Acquire.Dc.Measure(_pinsFirstMeas);

                _resistanceValue = (_outputMode == TLibOutputMode.ForceVoltage) ? (_forceFirst - _forceSecond) / (_measFirst - _measSecond) :
                    (_measFirst - _measSecond) / (_forceFirst - _forceSecond);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion

        /// <summary>
        /// Performs a resistance measurement by forcing a current on one pin and measuring on two other pins.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceCurrentPin">Current to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measureFirstPin">First pin to measure voltage.</param>
        /// <param name="measureRangeFirstPin">Measure range of first measure pin.</param>
        /// <param name="measureSecondPin">Second pin to measure voltage.</param>
        /// <param name="measureRangeSecondPin">Measure range of second measure pin.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        /// <param name="setup">Optional. Name of the setup set to be applied through the setup service.</param>
        #region ThreePinsOneForceTwoMeasure
        [TestMethod, Steppable, CustomValidation]
        public void ThreePinsOneForceTwoMeasure(PinList forcePin, double forceCurrentPin, double clampValueOfForcePin, PinList measureFirstPin,
            double measureRangeFirstPin, PinList measureSecondPin, double measureRangeSecondPin, double waitTime = 0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePin, nameof(forcePin), out _pinsFirst);
                TheLib.Validate.Pins(measureFirstPin, nameof(measureFirstPin), out _pinsFirstMeas);
                TheLib.Validate.Pins(measureSecondPin, nameof(measureSecondPin), out _pinsSecondMeas);
                _forceFirst = new PinSite<double>(forcePin, forceCurrentPin);
                _allPins = new Pins(string.Join(", ", forcePin, measureFirstPin, measureSecondPin));
                _allMeasPins = new Pins(string.Join(", ", measureFirstPin, measureSecondPin));
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.ForceHiZ(_allMeasPins);
                TheLib.Setup.Dc.Force(_pinsFirst, TLibOutputMode.ForceCurrent, forceCurrentPin, forceCurrentPin, clampValueOfForcePin);
                TheLib.Setup.Dc.SetMeter(_pinsFirstMeas, Measure.Voltage, measureRangeFirstPin);
                TheLib.Setup.Dc.SetMeter(_pinsSecondMeas, Measure.Voltage, measureRangeSecondPin);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirstMeas);
                _measSecond = TheLib.Acquire.Dc.Measure(_pinsSecondMeas);
                _resistanceValue = (_measFirst - _measSecond) / _forceFirst;
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion

        /// <summary>
        /// Performs a resistance delta measurement by forcing a current on first pin, voltage on second pin and measuring a delta voltage on two other pins.
        /// </summary>
        /// <param name="forceFirstPin">First pin to force Current.</param>
        /// <param name="forceValueFirstPin">Value to force on first pin.</param>
        /// <param name="clampValueOfForceFirstPin">Clamp Value of the first force pin. May also set its range.</param>
        /// <param name="forceSecondPin">Second pin to force Voltage.</param>
        /// <param name="forceValueSecondPin">Value to force on second pin.</param>
        /// <param name="clampValueOfForceSecondPin">Clamp Value of the second force pin. May also set its range.</param>
        /// <param name="measureFirstPin">First pin to measure Voltage.</param>
        /// <param name="measureSecondPin">Second pin to measure Voltage.</param>
        /// <param name="measureRangeFirstPin">Measure range of first measure pins.</param>
        /// <param name="measureRangeSecondPin">Measure range of second measure pins.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        /// <param name="setup">Optional. Name of the setup set to be applied through the setup service.</param>
        #region FourPinsTwoForceTwoMeasure
        [TestMethod, Steppable, CustomValidation]
        public void FourPinsTwoForceTwoMeasure(PinList forceFirstPin, double forceValueFirstPin, double clampValueOfForceFirstPin, PinList forceSecondPin,
            double forceValueSecondPin, double clampValueOfForceSecondPin, PinList measureFirstPin, PinList measureSecondPin, double measureRangeFirstPin,
            double measureRangeSecondPin, double waitTime = 0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forceFirstPin, nameof(forceFirstPin), out _pinsFirst);
                TheLib.Validate.Pins(forceSecondPin, nameof(forceSecondPin), out _pinsSecond);
                TheLib.Validate.Pins(measureFirstPin, nameof(measureFirstPin), out _pinsFirstMeas);
                TheLib.Validate.Pins(measureSecondPin, nameof(measureSecondPin), out _pinsSecondMeas);
                _allPins = new Pins(string.Join(", ", forceFirstPin, forceSecondPin, measureFirstPin, measureSecondPin));
                _allMeasPins = new Pins(string.Join(", ", measureFirstPin, measureSecondPin));
                _forceFirst = new PinSite<double>(forceFirstPin, forceValueFirstPin);
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.ForceHiZ(_allMeasPins);
                TheLib.Setup.Dc.Force(_pinsFirst, TLibOutputMode.ForceCurrent, forceValueFirstPin, forceValueFirstPin, clampValueOfForceFirstPin);
                TheLib.Setup.Dc.Force(_pinsSecond, TLibOutputMode.ForceVoltage, forceValueSecondPin, forceValueSecondPin, clampValueOfForceSecondPin);
                TheLib.Setup.Dc.SetMeter(_pinsFirstMeas, Measure.Voltage, measureRangeFirstPin);
                TheLib.Setup.Dc.SetMeter(_pinsSecondMeas, Measure.Voltage, measureRangeSecondPin);
                TheLib.Execute.Wait(waitTime);
                _measFirst = TheLib.Acquire.Dc.Measure(_pinsFirstMeas);
                _measSecond = TheLib.Acquire.Dc.Measure(_pinsSecondMeas);
                _resistanceValue = (_measFirst - _measSecond) / _forceFirst;
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_resistanceValue);
            }
        }
        #endregion
    }
}

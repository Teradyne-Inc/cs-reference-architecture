using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class SingleCondition : TestCodeBase {

        private Pins _pinsAll;
        private Pins _pinsForce;
        private Pins _pinsMeasure;
        private PinSite<double> _measPins;
        private PatternInfo _pattern;
        private TLibOutputMode _outputMode;
        private Measure _measureMode;
        private double _outputRangeValue;
        private bool _containsDigitalPins = false;
        private bool _measPinListIsNullOrEmpty = false;
        private bool _patternIsValid = false;

        /// <summary>
        /// Parametric measurement by setting up all force Pins, then measuring all force or optionally different measure Pins.
        /// </summary>
        /// <param name="forcePinList">Comma separated list of pin or pin groups representing the DC setup and/or measurement.</param>
        /// <param name="forceMode">Force mode for each pin or pin group.</param>
        /// <param name="forceValue">Force voltage or current for all pins or pin groups.</param>
        /// <param name="clampValue">Clamp voltage or current for all pina or pin groups.</param>
        /// <param name="measureWhat">Measure either voltage or current for all measure pins or pin groups.</param>
        /// <param name="measureRange">Expected voltage or current for all pins or pin groups to set the range.</param>
        /// <param name="sampleSize">Optional. Number of samples to average for all pins or pin groups.</param>
        /// <param name="measPinList">Optional. Comma separted list of measurement pins or pin groups, if different from forcePinList.</param>
        /// <param name="waitTime">Optional. Settling time used after pin setup.</param>
        /// <param name="setup">Optional. Setup settting to preconfigure the dib or device.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList forcePinList, string forceMode, double forceValue, double clampValue, string measureWhat, double measureRange,
            int sampleSize = 1, PinList measPinList = null, double waitTime = 0.0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pinsForce);
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pinsAll);
                _measPinListIsNullOrEmpty = string.IsNullOrEmpty(measPinList);
                if (_measPinListIsNullOrEmpty) {
                    _pinsMeasure = new Pins(forcePinList);
                } else {
                    TheLib.Validate.Pins(measPinList, nameof(measPinList), out _pinsMeasure);
                    _pinsAll.Add(measPinList);
                }
                TheLib.Validate.Enum(forceMode, nameof(forceMode), out _outputMode);
                TheLib.Validate.Enum(measureWhat, nameof(measureWhat), out _measureMode);
                _outputRangeValue = (_outputMode == TLibOutputMode.ForceVoltage) ? clampValue : forceValue;
                _containsDigitalPins = _pinsAll.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pinsAll);
                TheLib.Setup.Dc.Connect(_pinsAll);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.Force(_pinsForce, _outputMode, forceValue, forceValue, clampValue);
                TheLib.Setup.Dc.SetMeter(_pinsMeasure, _measureMode, measureRange, outputRangeValue: (!_measPinListIsNullOrEmpty) ? _outputRangeValue : null);
                TheLib.Execute.Wait(waitTime);
                _measPins = TheLib.Acquire.Dc.Measure(_pinsMeasure, sampleSize);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pinsAll);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pinsAll);
                TheLib.Datalog.TestParametric(_measPins, forceValue);
            }
        }
        #endregion

        /// <summary>
        /// Parametric measurement by running preconditioning pattern, setting up force Pins and then measuring all force or optionally different measure Pins.
        /// </summary>
        /// <param name="forcePinList">Comma separated list of pin or pin groups representing the DC setup and/or measurement.</param>
        /// <param name="forceMode">Force mode for each pin or pin group.</param>
        /// <param name="forceValue">Force voltage or current for all pins or pin groups.</param>
        /// <param name="clampValue">Clamp voltage or current for all pina or pin groups.</param>
        /// <param name="preconditionPat">Pattern to run to precondition the device before the parametric test.</param>
        /// <param name="measureWhat">Measure either voltage or current for all measure pins or pin groups.</param>
        /// <param name="measureRange">Expected voltage or current for all pins or pin groups to set the range.</param>
        /// <param name="sampleSize">Optional. Number of samples to average for all pins or pin groups.</param>
        /// <param name="measPinList">Optional. Comma separted list of measurement pins or pin groups, if different from forcePinList.</param>
        /// <param name="waitTime">Optional. Settling time used after pin setup.</param>
        /// <param name="setup">Optional. Setup settting to preconfigure the dib or device.</param>
        #region Preconditioning
        [TestMethod, Steppable, CustomValidation]
        public void PreconditionPattern(PinList forcePinList, string forceMode, double forceValue, double clampValue, Pattern preconditionPat,
            string measureWhat, double measureRange, int sampleSize = 1, PinList measPinList = null, double waitTime = 0.0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pinsForce);
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pinsAll);
                _patternIsValid = !string.IsNullOrEmpty(preconditionPat);
                _measPinListIsNullOrEmpty = string.IsNullOrEmpty(measPinList);
                if (_patternIsValid) {
                    TheLib.Validate.Pattern(preconditionPat, nameof(preconditionPat), out _pattern);
                }
                if (_measPinListIsNullOrEmpty) {
                    _pinsMeasure = new Pins(forcePinList);
                } else {
                    TheLib.Validate.Pins(measPinList, nameof(measPinList), out _pinsMeasure);
                    _pinsAll.Add(measPinList);
                }
                TheLib.Validate.Enum(forceMode, nameof(forceMode), out _outputMode);
                TheLib.Validate.Enum(measureWhat, nameof(measureWhat), out _measureMode);
                _outputRangeValue = _outputMode == TLibOutputMode.ForceVoltage ? clampValue : forceValue;
                _containsDigitalPins = _pinsAll.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_patternIsValid) TheLib.Execute.Digital.RunPattern(_pattern);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pinsAll);
                TheLib.Setup.Dc.Connect(_pinsAll);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.Force(_pinsForce, _outputMode, forceValue, forceValue, clampValue);
                TheLib.Setup.Dc.SetMeter(_pinsMeasure, _measureMode, measureRange, outputRangeValue: (!_measPinListIsNullOrEmpty) ? _outputRangeValue
                    : null);
                TheLib.Execute.Wait(waitTime);
                _measPins = TheLib.Acquire.Dc.Measure(_pinsMeasure, sampleSize); 
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pinsAll);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pinsAll);
                TheLib.Datalog.TestParametric(_measPins, forceValue);
            }
        }
        #endregion
    }
}

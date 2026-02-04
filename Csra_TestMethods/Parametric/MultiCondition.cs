using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class MultiCondition : TestCodeBase {

        private Pins _pins;
        private Pins[] _pinsForceGroups;
        private Pins[] _pinsMeasureGroups;
        private int[] _sampleSizes;
        private double[] _forceValues;
        private double[] _clampValues;
        private double[] _measureRanges;
        private Measure[] _measureModes;
        private TLibOutputMode[] _outputModes;
        private PatternInfo _pattern;
        private PinSite<double> _measureValues;
        private bool _containsDigitalPins = false;
        private bool _patternSpecified = false;

        /// <summary>
        /// Parametric measurement by setting up all force Pins, then measuring all force or optionally different measure Pins.
        /// </summary>
        /// <param name="forcePinList">Comma separated list of pin or pin groups representing the DC setup and/or measurement.</param>
        /// <param name="forceModes">Comma separated list of the force modes for each pin or pin group.</param>
        /// <param name="forceValues">Comma separated list of force voltages or currents for each pin or pin group.</param>
        /// <param name="clampValues">Comma separated list of clamp voltages or currents for each pin or pin group.</param>
        /// <param name="measureModes">Comma separated list of the measure modes for each pin or pin group.</param>
        /// <param name="measureRanges">Comma separated list of the measure ranges for each pin or pin group.</param>
        /// <param name="sampleSizes">Optional. Comma separated list of number of samples to average for each pin or pin group.</param>
        /// <param name="measPinList">Optional. Comma separated list of measurement pin or pin groups, if different from forcePinList.</param>
        /// <param name="waitTime">Optional. The wait time before the measurement.</param>
        /// <param name="setup">Optional. Setup set to configure the dib or device.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList forcePinList, string forceModes, string forceValues, string clampValues, string measureModes, string measureRanges,
            string sampleSizes = "1", PinList measPinList = null, double waitTime = 0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pins);
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
                TheLib.Validate.MultiCondition(forcePinList, p => new Pins(p), nameof(_pinsForceGroups), out _pinsForceGroups);
                if (string.IsNullOrEmpty(measPinList)) _pinsMeasureGroups = _pinsForceGroups;
                else {
                    TheLib.Validate.Pins(measPinList, nameof(measPinList), out _);
                    TheLib.Validate.MultiCondition(measPinList, p => new Pins(p), nameof(_pinsMeasureGroups), out _pinsMeasureGroups);
                    _pins.Add(measPinList);
                }
                TheLib.Validate.MultiCondition(forceModes, nameof(forceModes), out _outputModes, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(forceValues, double.Parse, nameof(forceValues), out _forceValues, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(clampValues, double.Parse, nameof(clampValues), out _clampValues, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(measureModes, nameof(measureModes), out _measureModes, _pinsMeasureGroups.Length);
                TheLib.Validate.MultiCondition(measureRanges, double.Parse, nameof(measureRanges), out _measureRanges, _pinsMeasureGroups.Length);
                TheLib.Validate.MultiCondition(sampleSizes, int.Parse, nameof(sampleSizes), out _sampleSizes, _pinsMeasureGroups.Length);
            }
            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.Force(_pinsForceGroups, _outputModes, _forceValues, _forceValues, _clampValues, [true]);
                TheLib.Setup.Dc.SetMeter(_pinsMeasureGroups, _measureModes, _measureRanges);
                TheLib.Execute.Wait(waitTime);
                _measureValues = TheLib.Acquire.Dc.Measure(_pinsMeasureGroups, _sampleSizes);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_measureValues);
            }
        }
        #endregion

        /// <summary>
        /// Parametric measurement by running preconditioning pattern, setting up force Pins and then measuring all force or optionally different measure Pins.
        /// </summary>
        /// <param name="forcePinList">Comma separated list of pin or pin groups representing the DC setup and/or measurement.</param>
        /// <param name="forceModes">Comma separated list of the force modes for each pin or pin group.</param>
        /// <param name="forceValues">Comma separated list of force voltages or currents for each pin or pin group.</param>
        /// <param name="clampValues">Comma separated list of clamp voltages or currents for each pin or pin group.</param>
        /// <param name="preconditionPat">Pattern to run to precondition the device before the parametric test.</param>
        /// <param name="measureModes">Comma separated list of the measure modes for each pin or pin group.</param>
        /// <param name="measureRanges">Comma separated list of the measure ranges for each pin or pin group.</param>
        /// <param name="sampleSizes">Optional. Comma separated list of number of samples to average for each pin or pin group.</param>
        /// <param name="measPinList">Optional. Comma separated list of measurement pin or pin groups, if different from forcePinList.</param>
        /// <param name="waitTime">Optional. The wait time before the measurement.</param>
        /// <param name="setup">Optional. Setup set to configure the dib or device.</param>
        #region Preconditioning
        [TestMethod, Steppable, CustomValidation]
        public void PreconditionPattern(PinList forcePinList, string forceModes, string forceValues, string clampValues, Pattern preconditionPat,
            string measureModes, string measureRanges, string sampleSizes = "1", PinList measPinList = null, double waitTime = 0, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pins);
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
                TheLib.Validate.MultiCondition(forcePinList, p => new Pins(p), nameof(forcePinList), out _pinsForceGroups);
                if (string.IsNullOrEmpty(measPinList)) _pinsMeasureGroups = _pinsForceGroups;
                else {
                    TheLib.Validate.Pins(measPinList, nameof(measPinList), out _);
                    TheLib.Validate.MultiCondition(measPinList, p => new Pins(p), nameof(measPinList), out _pinsMeasureGroups);
                    _pins.Add(measPinList);
                }
                _patternSpecified = !string.IsNullOrEmpty(preconditionPat);
                if (_patternSpecified) TheLib.Validate.Pattern(preconditionPat, nameof(preconditionPat), out _pattern);
                else Services.Alert.Error("Invalid Pattern. The pattern is null or empty.");
                TheLib.Validate.MultiCondition(forceModes, nameof(forceModes), out _outputModes, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(forceValues, double.Parse, nameof(forceValues), out _forceValues, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(clampValues, double.Parse, nameof(clampValues), out _clampValues, _pinsForceGroups.Length);
                TheLib.Validate.MultiCondition(measureModes, nameof(measureModes), out _measureModes, _pinsMeasureGroups.Length);
                TheLib.Validate.MultiCondition(measureRanges, double.Parse, nameof(measureRanges), out _measureRanges, _pinsMeasureGroups.Length);
                TheLib.Validate.MultiCondition(sampleSizes, int.Parse, nameof(sampleSizes), out _sampleSizes, _pinsMeasureGroups.Length);
            }
            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_patternSpecified) TheLib.Execute.Digital.RunPattern(_pattern);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.Force(_pinsForceGroups, _outputModes, _forceValues, _forceValues, _clampValues, [true]);
                TheLib.Setup.Dc.SetMeter(_pinsMeasureGroups, _measureModes, _measureRanges);
                TheLib.Execute.Wait(waitTime);
                _measureValues = TheLib.Acquire.Dc.Measure(_pinsMeasureGroups, _sampleSizes);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_measureValues);
            }
        }
        #endregion
    }
}


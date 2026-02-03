using System;
using System.Linq;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Search {

    [TestClass(Creation.TestInstance), Serializable]
    public class Parametric : TestCodeBase {

        private Pins _allPins;
        private Pins _forcePin;
        private Pins _measurePin;
        private Site<double> _results;
        private List<Site<double>> _measuredDcValues;
        private bool _containsDigitalPins = false;
        private double _voltageRange;
        private const int _notFoundResult = -999;

        /// <summary>
        /// Voltage values across the entire range are traversed without being evaluated during the linear search.
        /// Subsequently, the device input condition for which the output pin voltage exceeds the threshold is determined.
        /// </summary>
        /// <param name="forcePins">The pins that are being forced.</param>
        /// <param name="measurePin">The pin that is being measured. The measurement is performed for a single pin.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="threshold">The value for which the output meets the required condition for the searched input value.</param>
        /// <param name="clampCurrent">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearFull
        [TestMethod, Steppable, CustomValidation]
        public void LinearFull(PinList forcePins, PinList measurePin, double from, double to, int count, double threshold, double clampCurrent,
            double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePins, nameof(forcePins), out _forcePin);
                TheLib.Validate.Pins(measurePin, nameof(measurePin), out _measurePin);
                TheLib.Validate.GreaterOrEqual(count, 2, nameof(count));
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                if (_measurePin.Count() != 1) Services.Alert.Error($"Only one pin can be used for measurement. The number of measurement pins is invalid.");
                _allPins = new Pins($"{forcePins.Value}, {measurePin.Value}");
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
                _voltageRange = (from > to) ? from : to;
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                _measuredDcValues = [];
                TheLib.Setup.Dc.ForceHiZ(_measurePin);
                TheLib.Setup.Dc.ForceV(_forcePin, from, clampCurrent, _voltageRange, clampCurrent, true);
                TheLib.Setup.Dc.SetMeter(_measurePin, Measure.Voltage, _voltageRange);
                double increment = TheLib.Acquire.Search.LinearFullFromToCount(from, to, count, (forceValue) => {
                    TheLib.Setup.Dc.ForceV(_forcePin, forceValue, gateOn: false);
                    TheLib.Execute.Wait(waitTime);
                    _measuredDcValues.Add(TheLib.Acquire.Dc.Measure(_measurePin).First());
                });
                _results = TheLib.Execute.Search.LinearFullProcess(_measuredDcValues, from, increment, 0, _notFoundResult, (measuredValue) => threshold < measuredValue);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_results);
            }
        }
        #endregion

        /// <summary>
        /// Voltage values across the range are traversed with an evaluation performed at each iteration, and the search is terminated once the objective has
        /// been reached across all sites. The device input condition for which the output pin voltage exceeds the threshold is then determined.
        /// </summary>
        /// <param name="forcePins">The pins that are being forced.</param>
        /// <param name="measurePin">The pin that is being measured. The measurement is performed for a single pin.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="threshold">The value for which the output meets the required condition for the searched input value.</param>
        /// <param name="clampCurrent">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearStop
        [TestMethod, Steppable, CustomValidation]
        public void LinearStop(PinList forcePins, PinList measurePin, double from, double to, int count, double threshold, double clampCurrent,
            double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(forcePins, nameof(forcePins), out _forcePin);
                TheLib.Validate.Pins(measurePin, nameof(measurePin), out _measurePin);
                TheLib.Validate.GreaterOrEqual(count, 2, nameof(count));
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                if (_measurePin.Count() != 1) Services.Alert.Error($"Only one pin can be used for measurement. The number of measurement pins is invalid.");
                _allPins = new Pins($"{forcePins.Value}, {measurePin.Value}");
                _containsDigitalPins = _allPins.ContainsFeature(InstrumentFeature.Digital);
                _voltageRange = (from > to) ? from : to;
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_allPins);
                TheLib.Setup.Dc.Connect(_allPins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.ForceHiZ(_measurePin);
                TheLib.Setup.Dc.ForceV(_forcePin, from, clampCurrent, _voltageRange, clampCurrent, true);
                TheLib.Setup.Dc.SetMeter(_measurePin, Measure.Voltage, _voltageRange);
                _results = TheLib.Acquire.Search.LinearStopFromToCount(from, to, count, 0, _notFoundResult, (forceValue) => {
                    TheLib.Setup.Dc.ForceV(_forcePin, forceValue, gateOn: false);
                    TheLib.Execute.Wait(waitTime);
                    return TheLib.Acquire.Dc.Measure(_measurePin).First();
                }, (measuredValue) => threshold < measuredValue);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_allPins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_allPins);
                TheLib.Datalog.TestParametric(_results);
            }
        }
        #endregion
    }
}

using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Leakage {

    [TestClass(Creation.TestInstance), Serializable]
    public class Groups : TestCodeBase {

        private Pins _pins;
        private Pins[] _pinSteps;
        private PinSite<double> _meas;
        private PatternInfo _pattern;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Measures leakage currents by applying a bias voltage in a sequential process carried out on each pin or group of pins,
        /// according to the structure defined in the <param name="pinList"> parameter.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="voltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="baseVoltage">The voltage value applied to all pins other than the one currently measured in the sequence.
        /// Intended to prevent any cross-pin interference.</param>
        /// <param name="waitTime">The settling time before the measurement.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList pinList, double voltage, double currentRange, double baseVoltage, double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                TheLib.Validate.MultiCondition(pinList, p => new Pins(p), nameof(_pinSteps), out _pinSteps);
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                _meas = new();
                TheLib.Setup.Dc.SetForceAndMeter(_pins, TLibOutputMode.ForceVoltage, baseVoltage, baseVoltage, currentRange, Measure.Current, currentRange);
                foreach (var pin in _pinSteps) {
                    TheLib.Setup.Dc.ForceV(pin, voltage, currentRange, voltage, gateOn: false);
                    TheLib.Execute.Wait(waitTime);
                    _meas.AddRange(TheLib.Acquire.Dc.Measure(pin));
                    TheLib.Setup.Dc.ForceV(pin, baseVoltage, currentRange, baseVoltage, gateOn: false);
                }
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_meas, voltage);
            }
        }
        #endregion

        /// <summary>
        /// Runs a pattern and then measures leakage currents by applying a bias voltage in a sequential process carried out on each pin or group of pins,
        /// according to the structure defined in the <param name="pinList"> parameter.
        /// </summary>
        /// <param name="pattern">Pattern to be executed.</param>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="voltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="baseVoltage">The voltage value applied to all pins other than the one currently measured in the sequence.
        /// Intended to prevent any cross-pin interference.</param>
        /// <param name="waitTime">The settling time before the measurement.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Preconditioning
        [TestMethod, Steppable, CustomValidation]
        public void Preconditioning(Pattern pattern, PinList pinList, double voltage, double currentRange, double baseVoltage, double waitTime,
            string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                TheLib.Validate.MultiCondition(pinList, p => new Pins(p), nameof(_pinSteps), out _pinSteps);
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                TheLib.Execute.Digital.RunPattern(_pattern);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                _meas = new();
                TheLib.Setup.Dc.SetForceAndMeter(_pins, TLibOutputMode.ForceVoltage, baseVoltage, baseVoltage, currentRange, Measure.Current, currentRange);
                foreach (var pin in _pinSteps) {
                    TheLib.Setup.Dc.ForceV(pin, voltage, currentRange, voltage, gateOn: false);
                    TheLib.Execute.Wait(waitTime);
                    _meas.AddRange(TheLib.Acquire.Dc.Measure(pin));
                    TheLib.Setup.Dc.ForceV(pin, baseVoltage, currentRange, baseVoltage, gateOn: false);
                }
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_meas, voltage);
            }
        }
        #endregion
    }
}

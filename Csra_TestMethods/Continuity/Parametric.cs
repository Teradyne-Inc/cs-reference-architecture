using System;
using System.Linq;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Csra.Api;

namespace Demo_CSRA.Continuity {

    [TestClass(Creation.TestInstance), Serializable]
    public class Parametric : TestCodeBase {

        private Pins _pins;
        private Pins[] _pinSteps;
        private PinSite<double> _meas;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Checks if the tester resources have electrical contact with DUT and if any pin is short-circuited with another signal pin or power supply. The 
        /// measurement is done parallel, all at once.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="current">The current to force.</param>
        /// <param name="clampVoltage">The value to clamp for force pin.</param>
        /// <param name="voltageRange">The voltage range for measurement.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Parallel
        [TestMethod, Steppable, CustomValidation]
        public void Parallel(PinList pinList, double current, double clampVoltage, double voltageRange, double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                TheLib.Setup.Dc.Connect(_pins);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Dc.SetForceAndMeter(_pins, TLibOutputMode.ForceCurrent, current, current, clampVoltage, Measure.Voltage, voltageRange);
                TheLib.Execute.Wait(waitTime);
                _meas = TheLib.Acquire.Dc.Measure(_pins);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_meas, current, "A");
            }
        }
        #endregion

        /// <summary>
        /// Checks if the tester resources have electrical contact with DUT and if any pin is short-circuited with another signal pin or power supply. The 
        /// measurement is done serially, one at a time.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="current">The current to force.</param>
        /// <param name="clampVoltage">The value to clamp for force pin.</param>
        /// <param name="voltageRange">The voltage range for measurement.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Serial
        [TestMethod, Steppable, CustomValidation]
        public void Serial(PinList pinList, double current, double clampVoltage, double voltageRange, double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(pinList, nameof(pinList), out _pins);
                _pinSteps = _pins.Select(pin => new Pins(pin.Name)).ToArray();
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
                TheLib.Setup.Dc.ForceV(_pins, 0, outputModeVoltage: true);
                foreach (var pin in _pinSteps) {
                    TheLib.Setup.Dc.SetForceAndMeter(pin, TLibOutputMode.ForceCurrent, current, current, clampVoltage, Measure.Voltage, voltageRange, false);
                    TheLib.Execute.Wait(waitTime);
                    _meas.Add(TheLib.Acquire.Dc.Measure(pin).First());
                    TheLib.Setup.Dc.ForceV(pin, 0, outputModeVoltage: true, gateOn: false);
                }
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_meas, current, "A");
            }
        }
        #endregion
    }
}

using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Continuity {

    [TestClass(Creation.TestInstance), Serializable]
    public class Supply : TestCodeBase {

        private Pins _pins;
        private PinSite<double> _meas;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Checks if the tester has electrical contact with pins of a DUT.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="forceVoltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(PinList pinList, double forceVoltage, double currentRange, double waitTime, string setup = "") {

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
                TheLib.Setup.Dc.SetForceAndMeter(_pins, TLibOutputMode.ForceVoltage, forceVoltage, forceVoltage, currentRange, Measure.Current, currentRange);
                TheLib.Execute.Wait(waitTime);
                _meas = TheLib.Acquire.Dc.Measure(_pins);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Dc.Disconnect(_pins);
                if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                TheLib.Datalog.TestParametric(_meas, forceVoltage, "V");
            }
        }
        #endregion
    }
}

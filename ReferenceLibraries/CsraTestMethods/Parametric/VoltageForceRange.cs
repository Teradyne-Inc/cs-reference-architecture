using System;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Csra.Api;

namespace CsraTestMethods.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class VoltageForceRange : TestCodeBase {

        private Pins _pins;
        private PinSite<double> _meas;
        private bool _containsDigitalPins = false;

        /// <summary>
        /// Regression test for the DCVS <c>SetForceAndMeter</c> force-range defect.
        /// <para>
        /// Forces a voltage and meters voltage on a DCVS pin using a force voltage range (<paramref name="forceRange"/>)
        /// that is wider than the meter range (<paramref name="measureRange"/>). DCVS has no independent meter voltage
        /// range, so the instrument must program <paramref name="forceRange"/> as its voltage range. When the defect is
        /// present, <paramref name="measureRange"/> is wrongly used as the DCVS voltage range and forcing
        /// <paramref name="forceVoltage"/> (which exceeds <paramref name="measureRange"/>) raises <c>DCVS:0003</c>.
        /// </para>
        /// <para>
        /// Reproduce the failure (on a build without the fix) by choosing
        /// <paramref name="forceVoltage"/> &gt; the hardware range that <paramref name="measureRange"/> snaps to, while
        /// keeping <paramref name="forceVoltage"/> &lt;= <paramref name="forceRange"/>.
        /// Tester-validated values: <c>forceVoltage = 5</c>, <c>forceRange = 5.5</c>, <c>clampCurrent = 0.1</c>, <c>measureRange = 1.5</c>.
        /// </para>
        /// <para>
        /// Note two hardware constraints observed during bring-up: <paramref name="forceRange"/> must be a <b>supported</b>
        /// DCVS voltage range (an unsupported value such as 6 V raises <c>DCVS:0004</c>, since some DCVS variants only
        /// support e.g. 3 V and 5.5 V), and <paramref name="clampCurrent"/> must cover the load current at
        /// <paramref name="forceVoltage"/> (too low raises the <c>DCVS:9024</c> source fold current-limit alarm).
        /// </para>
        /// </summary>
        /// <param name="pinList">List of DCVS pin or pin group names.</param>
        /// <param name="forceVoltage">The voltage to force. Choose a value larger than <paramref name="measureRange"/> to exercise the defect (e.g. 5 V).</param>
        /// <param name="forceRange">The DCVS voltage (force) range. Must cover <paramref name="forceVoltage"/> and be a supported DCVS hardware range (e.g. 5.5 V; an unsupported value such as 6 V raises <c>DCVS:0004</c>).</param>
        /// <param name="clampCurrent">The current clamp for the force pin. Size it to cover the load current at <paramref name="forceVoltage"/> (e.g. 0.1 A); too low raises the <c>DCVS:9024</c> source foldback alarm.</param>
        /// <param name="measureRange">The meter voltage range. Choose a value smaller than <paramref name="forceVoltage"/> to exercise the defect (e.g. 1.5 V).</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Parallel
        [TestMethod, Steppable, CustomValidation]
        public void Parallel(PinList pinList, double forceVoltage, double forceRange, double clampCurrent, double measureRange, double waitTime, string setup = "") {

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
                // Reproduce the tester's linear-search usage: ramp the forced voltage across several points up to
                // forceVoltage while keeping the wider forceRange as the DCVS voltage range. Each SetForceAndMeter call
                // must keep VoltageRange == forceRange; without the fix, measureRange is used as the DCVS voltage range
                // and any ramp point with forced voltage > measureRange raises DCVS:0003. Toggling the gate between
                // steps mirrors the search re-forcing the measure pin on every iteration.
                const int rampSteps = 5;
                for (int step = 1; step <= rampSteps; step++) {
                    double rampVoltage = forceVoltage * step / rampSteps;
                    TheLib.Setup.Dc.SetForceAndMeter(_pins, DcOutputMode.ForceVoltage, rampVoltage, forceRange, clampCurrent, DcMeterMode.Voltage, measureRange);
                    TheLib.Setup.Dc.Modify(_pins, gate: true);
                    TheLib.Execute.Wait(waitTime);
                }
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

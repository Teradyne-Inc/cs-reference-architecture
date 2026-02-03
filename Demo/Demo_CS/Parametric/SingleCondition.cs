using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class SingleCondition : TestCodeBase {

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
        [TestMethod]
        public void Baseline(PinList forcePinList, string forceMode, double forceValue, double clampValue, string measureWhat, double measureRange,
            int sampleSize = 1, PinList measPinList = null, double waitTime = 0.0) {

            // Pre-Body
            var ppmu = TheHdw.PPMU.Pins(forcePinList);
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins(forcePinList).Disconnect();
            ppmu.Connect();

            // Body
            ppmu.ForceI(forceValue, forceValue);
            ppmu.ClampVHi.Value = clampValue;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> result = ppmu.Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Average).ToPinSite<double>();
            ppmu.Gate = tlOnOff.Off;

            // Post-Body
            TheHdw.PPMU.Pins(forcePinList).Disconnect();
            TheHdw.Digital.Pins(forcePinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceResults: tlLimitForceResults.Flow);

        }

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
        [TestMethod]
        public void PreConditionPattern(PinList forcePinList, string forceMode, double forceValue, double clampValue, Pattern preconditionPat,
            string measureWhat, double measureRange, int sampleSize = 1, PinList measPinList = null, double waitTime = 0.0) {

            // Pre-Body
            var ppmu = TheHdw.PPMU.Pins(forcePinList);
            var pattern = TheHdw.Patterns(preconditionPat);
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            pattern.Load();
            pattern.Start();
            TheHdw.Digital.TimeDomains(TheHdw.Patterns(preconditionPat).TimeDomains).Patgen.HaltWait();
            TheHdw.Digital.Pins(forcePinList).Disconnect();
            ppmu.Connect();

            // Body
            ppmu.ForceI(forceValue, forceValue);
            ppmu.ClampVHi.Value = clampValue;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> result = ppmu.Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Average).ToPinSite<double>();
            ppmu.Gate = tlOnOff.On;

            // Post-Body
            TheHdw.PPMU.Pins(forcePinList).Disconnect();
            TheHdw.Digital.Pins(forcePinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: result, ForceVal: forceValue, ForceResults: tlLimitForceResults.Flow);
        }
    }
}

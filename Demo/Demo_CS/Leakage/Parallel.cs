using System;
using System.Xml.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Leakage {

    [TestClass]
    public class Parallel : TestCodeBase {

        /// <summary>
        /// Measures leakage currents by applying bias voltage to all pins simultaneously, in a parallel testing process.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="voltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="waitTime">The settling time before the measurement.</param>
        #region Baseline
        [TestMethod]
        public void Baseline(PinList pinList, double voltage, double currentRange, double waitTime) {

            var ppmu = TheHdw.PPMU.Pins(pinList);

            //ShouldRunPreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // apply setup 'InitLeakageTest'
            TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = ChInitState.Lo;
            TheHdw.Digital.Pins("porta").InitState = ChInitState.Off;
            TheHdw.SettleWait(1 * s);

            TheHdw.Digital.Pins(pinList).Disconnect();
            ppmu.Connect();

            //ShouldRunBody
            ppmu.ForceV(voltage, currentRange);
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            var meas = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();

            //ShouldRunPostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(pinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: voltage, ForceUnit: UnitType.Custom, CustomForceunit: "",
                ForceResults: tlLimitForceResults.Flow);

        }
        #endregion

        /// <summary>
        /// Runs a pattern and then measures leakage currents by applying bias voltage to all pins simultaneously, using a parallel testing process.
        /// </summary>
        /// <param name="pattern">Pattern to be executed, has the effect of setup: 'InitLeakageTest'.</param>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="voltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="waitTime">The settling time before the measurement.</param>
        #region Preconditioning
        [TestMethod]
        public void Preconditioning(Pattern pattern, PinList pinList, double voltage, double currentRange, double waitTime) {

            var ppmu = TheHdw.PPMU.Pins(pinList);

            //ShouldRunPreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Patterns(pattern).Load();
            TheHdw.Patterns(pattern).Start();
            TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.HaltWait();

            TheHdw.Digital.Pins(pinList).Disconnect();
            ppmu.Connect();

            //ShouldRunBody
            ppmu.ForceV(voltage, currentRange);
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            var meas = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();

            //ShouldRunPostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(pinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: voltage, ForceUnit: UnitType.Custom, CustomForceunit: "",
                ForceResults: tlLimitForceResults.Flow);
        }
        #endregion
    }
}

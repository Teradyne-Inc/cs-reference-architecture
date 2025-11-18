using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class MultiCondition : TestCodeBase {

        /// <summary>
        /// Parametric measurement of output voltage on PortB when the device drives high.
        /// </summary>
        /// <param name="waitTime">Optional. The wait time in front of the measurement.</param>
        [TestMethod]
        public void Baseline(double waitTime = 0) {

            // PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, false, false, tlRelayMode.Powered);
            var force0 = TheHdw.PPMU.Pins("nLEAB, nOEAB");
            var force5 = TheHdw.PPMU.Pins("nLEBA, nOEBA, porta");
            var forceI = TheHdw.PPMU.Pins("portb");
            var ppmu = TheHdw.PPMU.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb");
            var digital = TheHdw.Digital.Pins("nLEAB, nOEAB, nLEBA, nOEBA, porta, portb");
            digital.Disconnect();
            ppmu.Connect();

            // Body
            force5.ForceV(5 * V, 2 * mA);
            force0.ForceV(0 * V, 2 * mA);
            forceI.ForceI(-500 * uA);
            forceI.ClampVHi.Value = 6.5 * V;
            forceI.ClampVLo.Value = 0 * V;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> meas = forceI.Read().ToPinSite<double>();

            // PostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            digital.Connect();
            TheExec.Flow.TestLimit(meas, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Parametric measurement of output voltage on PortB when the device drives high.
        /// </summary>
        /// <param name="preconditionPat">Pattern to run to precondition the device before the parametric test.</param>
        /// <param name="waitTime">Optional. The wait time in front of the measurement.</param>
        [TestMethod]
        public void PreconditionPattern(Pattern preconditionPat, double waitTime = 0) {

            // PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            var pattern = TheHdw.Patterns(preconditionPat);
            pattern.Load();
            pattern.Start();
            TheHdw.Digital.TimeDomains(pattern.TimeDomains).Patgen.HaltWait();
            var portA = TheHdw.PPMU.Pins("porta");
            var portB = TheHdw.PPMU.Pins("portb");
            var ppmu = TheHdw.PPMU.Pins("porta, portb");
            var digital = TheHdw.Digital.Pins("porta, portb");
            digital.Disconnect();
            ppmu.Connect();

            // Body
            portA.ForceV(5 * V, 2 * mA);
            portB.ForceI(-500 * uA);
            portB.ClampVHi.Value = 6.5 * V;
            portB.ClampVLo.Value = 0 * V;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> meas = portB.Read().ToPinSite<double>();

            // PostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            digital.Connect();
            TheExec.Flow.TestLimit(meas, ForceResults: tlLimitForceResults.Flow);
        }
    }
}


using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class PatternHandshake : TestCodeBase {

        /// <summary>
        /// Runs the specified pattern and executes the stopAction at each occurance of stopFlag in the pattern. Every
        /// value returned from the stopAction(s) will be datalogged.
        /// </summary>
        /// <param name="pattern">Pattern name to be executed.</param>
        /// <param name="stopFlag">Pattern flag to stop at.</param>
        /// <param name="numberOfStops">Number of total stop in the pattern.</param>
        /// <param name="stopAction">Action to be called at each stop.</param>
        /// <param name="testFunctional">Whether to test the functional results.</param>
        [TestMethod]
        public void Baseline(PinList measurePins, Pattern pattern, int stopFlag, int numberOfStops, string stopAction, bool testFunctional) {

            // PreBody
            Site<bool> patResult = new Site<bool>();
            List<PinSite<double>> returnValue = new List<PinSite<double>>();
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // Body
            TheHdw.PPMU.Pins(measurePins).ForceI(0, 0.002);
            TheHdw.PPMU.Pins(measurePins).Gate = tlOnOff.Off;
            TheHdw.Patterns(pattern).Start();
            for (int i = 0; i < numberOfStops; i++) {
                TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.FlagWait((int)CpuFlag.A, 0);
                TheHdw.SetSettlingTimer(0.001);
                TheHdw.Digital.Pins(measurePins).Disconnect();
                TheHdw.PPMU.Pins(measurePins).Connect();
                TheHdw.SetSettlingTimer(0.001);
                returnValue.Add(TheHdw.PPMU.Pins(measurePins).Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Array).ToPinSite<double>());
                TheHdw.PPMU.Pins(measurePins).Disconnect();
                TheHdw.Digital.Pins(measurePins).Connect();
                TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.Continue(0, (int)CpuFlag.A);
            }
            if (testFunctional) patResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();

            // PostBody
            if (testFunctional) TheExec.Flow.FunctionalTestLimit(patResult, pattern);
            foreach (var value in returnValue) {
                TheExec.Flow.TestLimit(ResultVal: value, ForceResults: tlLimitForceResults.Flow);
            }

        }
    }
}

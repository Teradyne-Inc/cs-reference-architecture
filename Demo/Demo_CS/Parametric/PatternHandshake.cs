using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using System.Linq;

namespace Demo_CS.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class PatternHandshake : TestCodeBase {

        /// <summary>
        /// Runs the specified pattern and executes the stopAction at each occurance of CpuFlag.A in the pattern. Every
        /// value returned from the stopAction(s) will be datalogged.
        /// </summary>
        /// <param name="pattern">Pattern name to be executed.</param>
        /// <param name="numberOfStops">Number of total stop in the pattern.</param>
        /// <param name="testFunctional">Whether to test the functional results.</param>
        [TestMethod]
        public void Baseline(Pattern pattern, int numberOfStops, bool testFunctional = true) {

            // PreBody
            var dcvs = TheHdw.DCVS.Pins("vcc");
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // Body
            dcvs.Meter.Mode = tlDCVSMeterMode.Current;
            dcvs.SetCurrentRanges(0.2 * A, 0.2 * A);
            TheHdw.Patterns(pattern).Start();
            for (int i = 0; i < numberOfStops; i++) {
                TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.FlagWait((int)CpuFlag.A, 0); 
                dcvs.Meter.Strobe();
                TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.Continue(0, (int)CpuFlag.A);
            }

            PinSite<double[]> returnValue = dcvs.Meter.Read(tlStrobeOption.NoStrobe, numberOfStops, Format: tlDCVSMeterReadingFormat.Array).ToPinSite<double[]>();
            Site<bool> patResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();

            // PostBody
            if (testFunctional) TheExec.Flow.FunctionalTestLimit(patResult, pattern);
            for (int i = 0; i < numberOfStops; i++) {
                Site<double> result = returnValue.Single().GetElement(i);
                TheExec.Flow.TestLimit(ResultVal: result, Unit: UnitType.Amp, ForceUnit: UnitType.Volt, ForceResults: tlLimitForceResults.Flow);
            }
        }
    }
}

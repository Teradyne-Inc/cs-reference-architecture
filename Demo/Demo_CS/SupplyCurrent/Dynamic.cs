using System;
using System.Linq;
using System.Security.Policy;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.SupplyCurrent {

    [TestClass]
    public class Dynamic : TestCodeBase {

        /// <summary>
        /// Performs a predefined number of current measurements synchronized by a Flag stop with the pattern.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="measureRange">The range for measurement.</param>
        /// <param name="clampValue">The value to clamp.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="stops">The number of stops executing a strobe on the instrument outside of the pattern - need to match with the pattern</param>
        [TestMethod]
        public void Baseline(PinList pinList, double forceValue, double measureRange, double clampValue, double waitTime, Pattern pattern, int stops) {

            // Should Run Pre-Body
            int patternSetFlag = (int)CpuFlag.A;
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            var dcvs = TheHdw.DCVS.Pins(pinList);
            dcvs.Connect();

            // Should Run Body
            dcvs.Mode = tlDCVSMode.Voltage;
            dcvs.Voltage.Value = forceValue;
            dcvs.SetCurrentRanges(clampValue, measureRange);
            double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
            dcvs.CurrentLimit.Source.FoldLimit.Level.Value = measureRange;
            dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = measureRange > sinkFoldLimit ? sinkFoldLimit : measureRange;
            dcvs.Gate = true;

            TheHdw.Patterns(pattern).Start();

            for (int i = 0; i < stops; i++) {
                TheHdw.Digital.Patgen.FlagWait(patternSetFlag, 0);
                TheHdw.SetSettlingTimer(waitTime);
                dcvs.Meter.Strobe();
                TheHdw.Digital.Patgen.Continue(0, patternSetFlag);
            }

            TheHdw.Digital.Patgen.HaltWait();
            PinSite<Samples<double>> meas = dcvs.Meter.Read(tlStrobeOption.NoStrobe, SampleSize: stops, Format: tlDCVSMeterReadingFormat.Array).ToPinSite<Samples<double>>();

            // Should Run Post-Body
            dcvs.Gate = false;
            dcvs.Disconnect();
            TheExec.Flow.TestLimit(meas, Unit: UnitType.Amp, PinName: pinList, CompareMode: tlLimitCompareType.EachSample, ForceVal: forceValue, ForceUnit: UnitType.Volt,
ForceResults: tlLimitForceResults.Flow);
        }
    }
}

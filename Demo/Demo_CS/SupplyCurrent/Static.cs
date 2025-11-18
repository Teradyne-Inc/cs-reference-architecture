using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.SupplyCurrent {

    [TestClass]
    public class Static : TestCodeBase {

        /// <summary>
        /// Executes a supply current test with the specified parameters.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="measureRange">The range for measurement.</param>
        /// <param name="clampValue">The value to clamp.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        [TestMethod]
        public void Baseline(PinList pinList, double forceValue, double measureRange, double clampValue, double waitTime) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            var dcvs = TheHdw.DCVS.Pins(pinList);
            dcvs.Connect();

            dcvs.Mode = tlDCVSMode.Voltage;
            dcvs.Voltage.Value = forceValue;
            dcvs.SetCurrentRanges(clampValue, measureRange);

            double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
            dcvs.CurrentLimit.Source.FoldLimit.Level.Value = clampValue;
            dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = clampValue > sinkFoldLimit ? sinkFoldLimit : clampValue;
            dcvs.Gate = true;

            TheHdw.SetSettlingTimer(waitTime);
            var meas = TheHdw.DCVS.Pins(pinList).Meter.Read();
            dcvs.Gate = false;
            dcvs.Disconnect();

            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: "V",
                ForceResults: tlLimitForceResults.Flow);
        }
    }
}

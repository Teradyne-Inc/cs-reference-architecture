using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Continuity {

    [TestClass]
    public class Supply : TestCodeBase {

        /// <summary>
        /// Checks if the tester has electrical contact with pins of a DUT
        /// </summary>
        /// <param name="pinList">List of pins or pin group names.</param>
        /// <param name="forceVoltage">The force voltage value.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        [TestMethod]
        public void Baseline(PinList pinList, double forceVoltage, double currentRange, double waitTime) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("TDI,TMS").InitState = ChInitState.Off;
            var dcvs = TheHdw.DCVS.Pins(pinList);
            dcvs.Connect();

            dcvs.Mode = tlDCVSMode.Voltage;
            dcvs.Voltage.Value = forceVoltage;
            dcvs.CurrentRange.Value = currentRange;
            var sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
            dcvs.CurrentLimit.Source.FoldLimit.Level.Value = currentRange;
            dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = currentRange > sinkFoldLimit ? sinkFoldLimit : currentRange;
            dcvs.Gate = true;
            TheHdw.SetSettlingTimer(waitTime);

            var meas = dcvs.Meter.Read(tlStrobeOption.Strobe, 1, Format: tlDCVSMeterReadingFormat.Average).ToPinSite<double>();
            dcvs.Gate = false;
            dcvs.Disconnect();
            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: forceVoltage, ForceUnit: UnitType.Custom, CustomForceunit: "V",
                ForceResults: tlLimitForceResults.Flow);
        }
    }
}

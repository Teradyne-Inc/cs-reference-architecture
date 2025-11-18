using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Continuity {

    [TestClass]
    public class Parametric : TestCodeBase {

        /// <summary>
        /// Checks if the tester resources have electrical contact with DUT and if any pin is short-circuited with another signal pin or power supply. The 
        /// measurement is done parallel, all at once.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="current">The current to force.</param>
        /// <param name="clampVoltage">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        [TestMethod]
        public void Parallel(PinList pinList, double current, double clampVoltage, double waitTime) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins(pinList).Disconnect();
            var ppmu = TheHdw.PPMU.Pins(pinList);
            ppmu.Connect();

            ppmu.ForceI(current, current);
            if (current >= 0) {
                ppmu.ClampVHi.Value = clampVoltage > ppmu.ClampVHi.Max ? ppmu.ClampVHi.Max : clampVoltage;
                ppmu.ClampVLo.Value = ppmu.ClampVLo.Min;
            } else {
                ppmu.ClampVHi.Value = ppmu.ClampVHi.Max;
                ppmu.ClampVLo.Value = clampVoltage < ppmu.ClampVLo.Min ? ppmu.ClampVLo.Min : clampVoltage;
            }
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            var meas = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();

            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(pinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: current, ForceUnit: UnitType.Custom, CustomForceunit: "A",
                ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Checks if the tester resources have electrical contact with DUT and if any pin is short-circuited with another signal pin or power supply. The 
        /// measurement is done serially, one at a time.
        /// </summary>
        /// <param name="pinList">List of pin or pin group names.</param>
        /// <param name="current">The current to force.</param>
        /// <param name="clampVoltage">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time after forcing.</param>
        [TestMethod]
        public void Serial(PinList pinList, double current, double clampVoltage, double waitTime) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins(pinList).Disconnect();
            var ppmu = TheHdw.PPMU.Pins(pinList);
            ppmu.Connect();

            ppmu.ForceV(0 * V);
            ppmu.Gate = tlOnOff.On;
            TheExec.DataManager.DecomposePinList(pinList, out string[] pins, out _);
            PinSite<double> meas = new();
            foreach (var pin in pins) {
                var ppmuPin = TheHdw.PPMU.Pins(pin);
                ppmuPin.ForceI(current, current);
                if (current >= 0) {
                    ppmuPin.ClampVHi.Value = clampVoltage > ppmuPin.ClampVHi.Max ? ppmuPin.ClampVHi.Max : clampVoltage;
                    ppmuPin.ClampVLo.Value = ppmuPin.ClampVLo.Min;
                } else {
                    ppmuPin.ClampVHi.Value = ppmuPin.ClampVHi.Max;
                    ppmuPin.ClampVLo.Value = clampVoltage < ppmuPin.ClampVLo.Min ? ppmuPin.ClampVLo.Min : clampVoltage;
                }
                TheHdw.SetSettlingTimer(waitTime);
                meas.Add(ppmuPin.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).SinglePinToSite<double>());
                ppmuPin.ForceV(0 * V);
            }

            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(pinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: meas, ForceVal: current, ForceUnit: UnitType.Custom, CustomForceunit: "A",
                ForceResults: tlLimitForceResults.Flow);
        }
    }
}

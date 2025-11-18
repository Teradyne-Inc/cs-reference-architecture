using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;

namespace TI245_Csharp {
    [TestClass]
    public class Seq_Leakage : TestCodeBase {

        [TestMethod]
        public void SeqLeakage(PinList SeqLeakPins, double ForceV_IiH, double ForceV_IiL, double waitTime, PinList Init_HiPins, PinList Init_LoPins, double I_Meas_Range = 0.0) {

            //int Site;
            string[] PinArr;
            int PinCount;
            int i;
            PinListData measVal = new PinListData();

            // Connect all signal pins (digital_pins) to the pin electronics and apply levels
            // TheHdw.Digital.ApplyLevelsTiming True, True, False, tlPowered, Init_HiPins.Value, Init_LoPins.Value
            TheHdw.PPMU.Pins(SeqLeakPins).Gate = tlOnOff.Off; // insure all ppmu's are gated off

            // separate pingroup into individual pins
            TheExec.DataManager.DecomposePinList(SeqLeakPins, out PinArr, out PinCount);

            // Loop for Leakage High (ForceV_IiH)
            for (i = 0; i <= PinCount - 1; i++) {
                {
                    var withBlock = TheHdw.PPMU[PinArr[i]];
                    TheHdw.Digital.DisconnectPins(PinArr[i]); // disconnect DUT pin from PE
                    withBlock.Connect();                // connect the ppmu to DUT pin
                    withBlock.Gate = tlOnOff.On;            // turn on ppmu
                    withBlock.ForceV(ForceV_IiH, I_Meas_Range); // force voltage, set measure and range
                    TheHdw.Wait(waitTime); // let force value stabilize prior to measurement
                    measVal.Value = withBlock.Read(tlPPMUReadWhat.Measurements); // make the measurement

                    // Setup OFFLINE Simulation by stuffing the pinlistdata variable with simulation data
                    if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                        ForEachSite(Site => {
                            measVal.Pins[PinArr[i]].set_Value(Site, 0.00000019 + (new Random().NextDouble() / 25000000));
                        });

                    }

                    // test the "measVal" against the limits
                    TheExec.Flow.TestLimit(ResultVal: measVal, Unit: UnitType.Amp, ForceVal: ForceV_IiH, ForceUnit: UnitType.Volt, ForceResults: tlLimitForceResults.Flow);
                    withBlock.Gate = tlOnOff.Off; // gate the ppmu off on the tested pin
                    withBlock.Disconnect();  // disconnect the ppmu from the tested pin
                    TheHdw.Digital.ConnectPins(PinArr[i]); // connect the tested pin back to the PE
                }
            }

            // Loop for Leakage Low (ForceV_IiL)
            for (i = 0; i <= PinCount - 1; i++) {
                {
                    var withBlock = TheHdw.PPMU[PinArr[i]];
                    TheHdw.Digital.DisconnectPins(PinArr[i]); // disconnect DUT pin from PE
                    withBlock.Connect();                    // connect the ppmu to DUT pin
                    withBlock.Gate = tlOnOff.On;                // turn on ppmu
                    withBlock.ForceV(ForceV_IiL, I_Meas_Range); // force voltage, set measure and range
                    TheHdw.Wait(waitTime); // let force value stabilize prior to measurement
                    measVal.Value = withBlock.Read(tlPPMUReadWhat.Measurements); // make the measurement

                    // Setup OFFLINE Simulation by stuffing the pinlistdata variable with simulation data
                    if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                        ForEachSite(Site => {
                            measVal.Pins[PinArr[i]].set_Value(Site, -0.000014 - (new Random().NextDouble() / 110000));
                        });

                    }

                    // test the "measVal" against the limits
                    TheExec.Flow.TestLimit(ResultVal: measVal, Unit: UnitType.Amp, ForceVal: ForceV_IiL, ForceUnit: UnitType.Volt, ForceResults: tlLimitForceResults.Flow);
                    withBlock.Gate = tlOnOff.Off; // gate the ppmu off on the tested pin
                    withBlock.Disconnect();  // disconnect the ppmu from the tested pin
                    TheHdw.Digital.ConnectPins(PinArr[i]); // connect the tested pin back to the PE
                }
            }

            // The '|' operator for enum flags in C# is the same as '+' in VB.NET 
            // https://www.alanzucconi.com/2015/07/26/enum-flags-and-bitwise-operators/
            TheHdw.PPMU.Pins(SeqLeakPins).Reset(tlResetOption.Connections | tlResetOption.Settings); // reset ppmu connections and settings
            TheHdw.PPMU.Pins(SeqLeakPins).Gate = tlOnOff.Off;
        }
    }
}

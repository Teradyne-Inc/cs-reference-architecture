using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS {
    [TestClass]
    public class Leakage_OLD : TestCodeBase {
        [TestMethod]
        public void Leakage_T(PinList ppmu, double forceCond1, double forceCond2, double forceIRange, double measIRang, PinList driveLoPins,
            PinList driveHiPins, PinList floatPins, tlPPMUMode mode = tlPPMUMode.ForceVMeasureI) {
            // ''''Apply HSD levels, Init States, Float Pins  and PowerSupply pin values''''
            // ''''Connect all pins,load levels,load timings,no hot-switching''''
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered, driveHiPins, driveHiPins, floatPins);

            // setup PPMU
            TheHdw.Digital.Pins(ppmu).Disconnect();
            TheHdw.PPMU.Pins(ppmu).Connect();
            TheHdw.PPMU.Pins(ppmu).ForceV(Voltage: forceCond1, MeasureCurrentRange: measIRang, SettlingTime: 0.005); //10.60.xx
            //TheHdw.PPMU.Pins(PPMU).ForceV(Voltage: ForceCond1, MeasureCurrentRange: MeasIRang, SettlingTime: 0.005, AltVoltage: 0); //99.99.90
            TheHdw.PPMU.Pins(ppmu).Gate = tlOnOff.On;

            IPinListData meas1 = TheHdw.PPMU.Pins(ppmu).Read(tlPPMUReadWhat.Measurements);

            TheHdw.PPMU.Pins(ppmu).ForceV(Voltage: forceCond2, MeasureCurrentRange: measIRang, SettlingTime: 0.005); //10.60.xx
            //TheHdw.PPMU.Pins(PPMU).ForceV(Voltage: ForceCond2, MeasureCurrentRange: MeasIRang, SettlingTime: 0.005, AltVoltage: 0); //99.99.90

            IPinListData meas2 = TheHdw.PPMU.Pins(ppmu).Read(tlPPMUReadWhat.Measurements);

            // clean up and DataLog
            TheHdw.PPMU.Pins(ppmu).Gate = tlOnOff.Off;
            TheHdw.PPMU.Pins(ppmu).Disconnect();
            TheHdw.Digital.Pins(ppmu).Connect();

            TheExec.Flow.TestLimit(meas1, TName: "Leakage1", Unit: UnitType.Amp, ForceVal: forceCond1, ForceResults: tlLimitForceResults.Flow);
            TheExec.Flow.TestLimit(meas2, TName: "Leakage2", Unit: UnitType.Amp, ForceVal: forceCond2, ForceResults: tlLimitForceResults.Flow);
        }
    }
}

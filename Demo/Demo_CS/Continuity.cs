using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS {
    [TestClass]
    public class Continuity_OLD : TestCodeBase {
        [TestMethod]
        public void Continuity_cs(PinList digitalPins, PinList powerPin, double powerPinVoltage, double powerPinCurrent, double powerPinCurrentRange,
            double ppmuCurrentValue, string tNames_ = "Continuity_CS") {

            // ''' Dimension object as PinListData to contain PPMU measured results
            PinListData ppmuMeasure = new PinListData();

            // ''' Offline simulation variables
            string[] pinNameArray;
            int numPins;

            // '''Disconnect All_Dig Pin Electronics from pins in order to connect PPMU's''''
            TheHdw.Digital.Pins(digitalPins).Disconnect();

            // ''' Setup VCC to 0V
            {
                var withBlock = TheHdw.DCVS.Pins(powerPin);
                withBlock.Gate = false;
                withBlock.Disconnect(tlDCVSConnectWhat.Default);
                withBlock.Mode = tlDCVSMode.Voltage;
                withBlock.Voltage.Output = tlDCVSVoltageOutput.Main;
                withBlock.Voltage.Value = powerPinVoltage;
                withBlock.CurrentRange.Value = powerPinCurrentRange;
                withBlock.Connect(tlDCVSConnectWhat.Default);
                withBlock.Gate = true;
            }

            // '''Program All_Dig PPMU Pins to force CurrentValue. Connect the PPMU's and Gate on'''''
            {
                var withBlock = TheHdw.PPMU.Pins(digitalPins);
                withBlock.ForceI(ppmuCurrentValue);
                withBlock.Connect();
                withBlock.Gate = tlOnOff.On;
            }

            /// Make Measurements on PPMU pins and store in pinlistdata''''
            ppmuMeasure.Value = TheHdw.PPMU.Pins(digitalPins).Read(tlPPMUReadWhat.Measurements);

            // '''Setup OFFLINE Simulation  ''''''''''''''''''''''''''''''''''''''''''''''''''''''
            var rand = new Random();
            if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                TheExec.DataManager.DecomposePinList(digitalPins, out pinNameArray, out numPins);
                for (int i = 0; i <= numPins - 1; i++) {
                    ForEachSite(site => {
                        ppmuMeasure.Pins[i].set_Value(site, -0.5 - (new Random().NextDouble() / 23));
                    });
                }
            }
            // Disconnect PPMU from digital channels
            {
                var withBlock = TheHdw.PPMU.Pins(digitalPins);
                withBlock.ForceI(0);
                withBlock.Gate = tlOnOff.Off;
                withBlock.Disconnect();
            }

            // ''''''''DATALOG RESULTS''''''''''''''''''''''''''''
            TheExec.Flow.TestLimit(ResultVal: ppmuMeasure, Unit: UnitType.Volt, ForceVal: ppmuCurrentValue, ForceUnit: UnitType.Amp,
                ForceResults: tlLimitForceResults.Flow);

        }
    }
}

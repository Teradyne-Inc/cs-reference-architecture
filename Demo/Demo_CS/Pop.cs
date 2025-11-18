using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS {
    [TestClass]
    public class Pop : TestCodeBase {

        [TestMethod]
        public void DcvsPop(PinList powerPin, Pattern patName, int numMeasPoints) {
            PinListData dcvsMeas = new PinListData();
            double measValue;
            //long testNumber;
            //object site;
            //string channel;
            //double i;

            // apply HSD levels and PowerSupply pin values, timing and init states
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // setup DCVS parameters that can not be controlled by the PSet
            {
                var _ = TheHdw.DCVS.Pins(powerPin);
                _.Gate = false;
                _.Voltage.Output = tlDCVSVoltageOutput.Main;
                _.Mode = tlDCVSMode.Voltage;
                _.CurrentLimit.Source.FoldLimit.Level.Value = 0.625;
                //_.LocalCapacitor = tlDCVSOnOffAuto.Off;
                _.Gate = true;
            }

            // After Debug, move "CreatePset()" to
            // ExecInterpose function "OnProgramValidated()" in the Exec_IP_Module
            CreatePset();

            // Load & Run Pattern, wait for PAT to finish
            TheHdw.Patterns(patName).Load();
            TheHdw.Patterns(patName).Start();
            TheHdw.Digital.Patgen.HaltWait();
            TheHdw.Wait(0.03);

            // by stuffing the pinListdata variable with simulation data
            // Setup OFFLINE Simulation
            if (TheExec.TesterMode == tlLangTestModeType.Offline) {
                dcvsMeas.AddPin(powerPin.Value);
                ForEachSite(site => {
                    dcvsMeas.Pins[powerPin].set_Value(site, 0.028 + new Random().NextDouble() / 99);
                    measValue = (double)dcvsMeas.Pin(powerPin).get_Value(site);
                });
            } else {
                dcvsMeas.Value = TheHdw.DCVS.Pins(powerPin).Meter.Read(tlStrobeOption.NoStrobe, numMeasPoints, -1, tlDCVSMeterReadingFormat.Array);
            }

            TheExec.Flow.TestLimit(ResultVal: dcvsMeas, ForceResults: tlLimitForceResults.Flow,CompareMode: tlLimitCompareType.Average, ForceVal: 5.0);
        }

        public void CreatePset() {
            // 1, Add new PSets call "DCVSPSet"
            TheHdw.DCVS.Pins("vcc").PSets.Add("DCVSPSet");

            // 2, Set parameters for DCVSPSet
            {
                var _ = TheHdw.DCVS.Pins("vcc").PSets["DCVSPSet"];
                _.Voltage.Main.Value = 5.0;
                _.CurrentRange.Value = 0.1;
                _.Meter.Mode = tlDCVSMeterMode.Current;
                //_.Meter.CurrentRange.Value = 0.2;
                _.CurrentLimit.Sink.FoldLimit.Level.Value = 0.05;
                _.CurrentLimit.Source.FoldLimit.Level.Value = 0.05;
                _.Capture.SampleSize.Value = 4;
                _.Apply();
            }
        }
    }
}

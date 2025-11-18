using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using System.IO;    

namespace Demo_CS {
    [TestClass]
    public class Functional_CS : TestCodeBase {
        [TestMethod]
        public void FunctionalCS(Pattern patternFile) {

            // ''''Apply HSD levels, Init States, Float Pins  and PowerSupply pin values''''
            // ''''Connect all pins,load levels,load timings,no hot-switching''''
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // '''Test pattern ti245_func. Set pass fail flag as Always report to datalog
            // '''and set it to not stop on first failure '''''
            TheHdw.Patterns(patternFile).Test(PFType.Always, 0);

        }

        [TestMethod]
        public void FunctionalScanCS(Pattern patternFile, bool enableScanLogging, string scanLogPins, int maxFailsPerPin, tlDatalogScanResultMode scanLogMode) {

            // ''''Apply HSD levels, Init States, Float Pins  and PowerSupply pin values''''
            // ''''Connect all pins,load levels,load timings,no hot-switching''''
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            // '''Set up the datalog for logging scan fail diagnostic data to STDF'''
            if (enableScanLogging) {
                SetupForScanLogging(scanLogPins, maxFailsPerPin, tlDatalogScanCaptureFormat.Cycle, scanLogMode);
            }

            //___ Setup CMEM ____________________________________________________________
            TheHdw.Digital.CMEM.SetCaptureConfig(-1, CmemCaptType.Fail, tlCMEMCaptureSource.PassFailData);
            TheHdw.Digital.CMEM.CaptureLimitMode = tlDigitalCMEMCaptureLimitMode.Enable;
            TheHdw.Digital.CMEM.CaptureLimit = maxFailsPerPin;
            TheHdw.Digital.Patgen.ScanBurstEnabled = true;

            // '''Test pattern ti245_func. Set pass fail flag as Always report to datalog
            // '''and set it to not stop on first failure '''''
            TheHdw.Patterns(patternFile).Test(PFType.Always, 0);

            //TheHdw.Patterns(PatternFile).Start();
            //TheHdw.Digital.Patgen.HaltWait();
            //var burstResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite;
            //TheExec.Flow.FunctionalTestLimit(burstResult, PatternFile);

        }

        /// <summary>
        /// Setup the datalog for logging scan fail diagnostic data to STDF
        /// </summary>
        /// <param name="logPins"></param>
        /// <param name="scanFailsPerPin"></param>
        /// <param name="logFormat"></param>
        /// <param name="logResultMode"></param>
        private void SetupForScanLogging(string logPins, int scanFailsPerPin, tlDatalogScanCaptureFormat logFormat, tlDatalogScanResultMode logResultMode) {
            TheExec.Datalog.Setup.ScanSetup().set_EnableScanLogging(Value: true);
            TheExec.Datalog.Setup.ScanSetup().set_CaptureFormat(Value: logFormat);
            TheExec.Datalog.Setup.ScanSetup().set_ResultMode(Value: logResultMode);
            TheExec.Datalog.Setup.ScanSetup().set_PinList(Value: logPins);
            TheExec.Datalog.Setup.ScanSetup().set_CMEMCaptureLimit(Value: scanFailsPerPin);
            TheExec.Datalog.ApplySetup();
        }
    }
}

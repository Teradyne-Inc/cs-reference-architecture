using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Csra;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra.TheLib.Execute {
    public class ScanNetwork : ILib.IExecute.IScanNetwork {
        public void RunDiagnosis(ScanNetworkPatternInfo scanNetworkPattern, ScanNetworkPatternResults nonDiagnosisResults,
            int captureLimit = 3000, bool concurrentDiagnosis = false) {
            // Setup CMEM for Diagnosis reburst
            SetupCmemForDiagnosis(captureLimit);
            // Execute the Diagnosis reburst(es) and generating fail logs.
            if (concurrentDiagnosis) {
                scanNetworkPattern.ExecuteDiagnosisCoresConcurrent(nonDiagnosisResults);
            } else {
                scanNetworkPattern.ExecuteDiagnosisCoresSequential(nonDiagnosisResults);
            }
        }
        
        public void RunPattern(ScanNetworkPatternInfo scanNetworkPattern) {
            // setup CMEM for Initial burst
            scanNetworkPattern.SetupNonDiagnosisBurst();
            // Execute the non-Diagnosis pattern(set) burst,
            // it will automatically reburst if CMEM is full, which means failed instance list is inconclusive.
            scanNetworkPattern.ExecuteNonDiagnosisBurst();
        }

        /// <summary>
        /// this may belong to somewhere else, TBD.
        /// </summary>
        /// <param name="captureLimit"></param>
        internal static void SetupCmemForDiagnosis(int captureLimit) {
            var cmem = TestCodeBase.TheHdw.Digital.CMEM;
            cmem.CentralFields = tlCMEMCaptureFields.ModCycle;
            cmem.SetCaptureConfig(-1, CmemCaptType.Fail, tlCMEMCaptureSource.PassFailData, true);
            cmem.CaptureLimitMode = tlDigitalCMEMCaptureLimitMode.EnableResetOnModule;
            cmem.CaptureLimit = captureLimit;
            TestCodeBase.TheHdw.Digital.Patgen.ScanBurstEnabled = true;
        }
    }
}

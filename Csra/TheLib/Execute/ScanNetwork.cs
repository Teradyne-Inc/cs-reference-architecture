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
            bool concurrentDiagnosis = false) {
            if (concurrentDiagnosis) {
                throw new NotImplementedException("Concurrent diagnosis not implemented yet.");
            } else {
                throw new NotImplementedException("Sequential-core diagnosis not implemented yet.");
            }
        }
        
        public void RunPattern(ScanNetworkPatternInfo scanNetworkPattern) {
            throw new NotImplementedException();
        }
    }
}

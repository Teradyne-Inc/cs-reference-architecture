using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using Csra;

namespace Csra.TheLib.Acquire {
    public class ScanNetwork : ILib.IAcquire.IScanNetwork {
        public ScanNetworkPatternResults PatternResults(ScanNetworkPatternInfo scanNetworkPattern) {
            return scanNetworkPattern.GetScanNetworkPatternResults();
        }
    }
}

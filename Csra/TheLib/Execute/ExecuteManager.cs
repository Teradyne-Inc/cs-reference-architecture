using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using Csra.Interfaces;

namespace Csra.TheLib.Execute {

    internal class ExecuteManager : ILib.IExecute {

        protected internal ExecuteManager() {
            Digital = new Digital();
            ScanNetwork = new ScanNetwork();
            Search = new Search();
        }

        public ILib.IExecute.IDigital Digital { get; private set; }

        public ILib.IExecute.IScanNetwork ScanNetwork { get; private set; }

        public ILib.IExecute.ISearch Search { get; private set; }

        public void Configure(ILib.IExecute.IDigital digital = null, ILib.IExecute.IScanNetwork scanNetwork = null, ILib.IExecute.ISearch search = null) {
            Digital = digital ?? new Digital();
            ScanNetwork = scanNetwork ?? new ScanNetwork();
            Search = search ?? new Search();
        }

        /// <summary>
        /// Sets the hardware wait time, if staticWait is true will force all existing waits to complete before starting the hardware wait, otherwise the
        /// wait is performed in parallel to any already active hardware wait.
        /// </summary>
        /// <param name="time">The time to wait in seconds</param>
        /// <param name="staticWait">If true, all existing hardware waits will complete before applying this hardware wait. If false, wait can
        /// occur in parallel with any existing hardware wait.</param>
        /// <param name="timeout">Maximum timeout of the hardware wait, defaults to 100ms</param>
        public void Wait(double time, bool staticWait = false, double timeout = 100 * ms) {
            if (staticWait) {
                TheHdw.SettleWait(timeout);
                TheHdw.Wait(time);
            } else {
                TheHdw.SetSettlingTimer(time);
            }
        }
    }
}

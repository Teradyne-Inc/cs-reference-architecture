using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;

namespace Csra.TheLib.Acquire {

    internal class AcquireManager : ILib.IAcquire {

        protected internal AcquireManager() {
            Digital = new Digital();
            Dc = new Dc();
            Search = new Search();
            ScanNetwork = new ScanNetwork();
        }

        public ILib.IAcquire.IDigital Digital { get; private set; }

        public ILib.IAcquire.IDc Dc { get; private set; }

        public ILib.IAcquire.ISearch Search { get; private set; }

        public ILib.IAcquire.IScanNetwork ScanNetwork { get; private set; }

        public void Configure(ILib.IAcquire.IDc dc = null, ILib.IAcquire.IDigital digital = null, ILib.IAcquire.IScanNetwork scanNetwork = null,
            ILib.IAcquire.ISearch search = null) {
            Digital = digital ?? new Digital();
            Dc = dc ?? new Dc();
            Search = search ?? new Search();
            ScanNetwork = scanNetwork ?? new ScanNetwork();
        }
    }
}

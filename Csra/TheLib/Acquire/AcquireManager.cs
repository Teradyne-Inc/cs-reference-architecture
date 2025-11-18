using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;

namespace Csra.TheLib.Acquire {
    /// <exclude />
    public class AcquireManager : ILib.IAcquire {
        public ILib.IAcquire.IDigital Digital => new Digital();

        public ILib.IAcquire.IDc Dc => new Dc();

        public ILib.IAcquire.ISearch Search => new Search();

        public ILib.IAcquire.IScanNetwork ScanNetwork => new ScanNetwork(); 
    }
}

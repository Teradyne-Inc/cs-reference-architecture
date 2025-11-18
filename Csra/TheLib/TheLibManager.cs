
using Csra.Interfaces;
using Csra.TheLib.Acquire;

namespace Csra.TheLib {
    /// <exclude />
    public class TheLibManager : ILib {
        public ILib.IValidate Validate => new Validate();

        public ILib.ISetup Setup => new Setup.SetupManager();

        public ILib.IExecute Execute => new Execute.ExecuteManager();

        public ILib.IAcquire Acquire => new Acquire.AcquireManager();

        public ILib.IDatalog Datalog => new Datalog();
    }
}

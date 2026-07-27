using Csra.Interfaces;

namespace Csra.TheLib {

    internal class TheLibManager : ILib {

        protected internal TheLibManager() {
            Validate = new Validate();
            Setup = new Setup.SetupManager();
            Execute = new Execute.ExecuteManager();
            Acquire = new Acquire.AcquireManager();
            Datalog = new Datalog();
        }

        public ILib.IValidate Validate { get; private set; }

        public ILib.ISetup Setup { get; private set; }

        public ILib.IExecute Execute { get; private set; }

        public ILib.IAcquire Acquire { get; private set; }

        public ILib.IDatalog Datalog { get; private set; }

        public void Configure(ILib.IValidate validate = null, ILib.ISetup setup = null, ILib.IExecute execute = null, ILib.IAcquire acquire = null, ILib.IDatalog datalog = null) {
            Validate = validate ?? new Validate();
            Setup = setup ?? new Setup.SetupManager();
            Execute = execute ?? new Execute.ExecuteManager();
            Acquire = acquire ?? new Acquire.AcquireManager();
            Datalog = datalog ?? new Datalog();
        }
    }
}

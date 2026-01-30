using System;
using Csra.Interfaces;

namespace Csra.Services {

    /// <exclude />
    [Serializable]
    public class ServiceManager : IServices {

        protected internal ServiceManager() {
            Alert = Services.Alert.Instance;
            Behavior = Services.Behavior.Instance;
            Setup = Services.Setup.Instance;
            Storage = Services.Storage.Instance;
            Transaction = Services.Transaction.Instance;
        }

        public IAlertService Alert { get; private set; }
        public IBehaviorService Behavior { get; private set; }
        public ISetupService Setup { get; private set; }
        public IStorageService Storage { get; private set; }
        public ITransactionService Transaction { get; private set; }

        public void Configure(IAlertService alert = null, IBehaviorService behavior = null, ISetupService setup = null, IStorageService storage = null,
            ITransactionService transaction = null) {
            Alert = alert ?? Services.Alert.Instance;
            Behavior = behavior ?? Services.Behavior.Instance;
            Setup = setup ?? Services.Setup.Instance;
            Storage = storage ?? Services.Storage.Instance;
            Transaction = transaction ?? Services.Transaction.Instance;
        }
    }
}


using Csra.Interfaces;

namespace Csra.Services {

    /// <exclude />
    public class ServiceManager {

        private IAlertService _alert = null;
        private IBehaviorService _behavior = null;
        private ISetupService _setup = null;
        private IStorageService _storage = null;
        private ITransactionService _transaction = null;

        internal void SetupServiceManagerMoq(IAlertService alert, IBehaviorService behavior, ISetupService setup, IStorageService storage,
            ITransactionService transaction) {
            _alert = alert;
            _behavior = behavior;
            _setup = setup;
            _storage = storage;
            _transaction = transaction;
        }

        public IAlertService Alert => _alert ??= new Alert();
        public IBehaviorService Behavior => _behavior ??= new Behavior();
        public ISetupService Setup => _setup ??= new Setup();
        public IStorageService Storage => _storage ??= new Storage();
        public ITransactionService Transaction => _transaction ??= new Transaction();
    }
}

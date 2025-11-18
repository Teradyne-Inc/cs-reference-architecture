using Csra.TheLib;
using Csra.Services;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra {
    /// <exclude />
    public static class Api {

        [DoNotSync]
        private static ILib _theLib = null;
        [DoNotSync]
        private static ServiceManager _services = null;
        public static ILib TheLib => _theLib ??= new TheLibManager();
        public static ServiceManager Services => _services ??= new ServiceManager();

        public static Info_ Info =>  new Info_();

        public static void SetupCsraMoq(ILib lib = null, IAlertService alert = null, IStorageService storage = null, ISetupService setup = null,
            IBehaviorService behavior = null, ITransactionService transaction = null) {
            _theLib = lib;
            Services.SetupServiceManagerMoq(alert, behavior, setup, storage, transaction);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exclude/>
        public sealed class Info_ {

            public string Version => Csra.Info.VersionDefinition;
        }
    }
}

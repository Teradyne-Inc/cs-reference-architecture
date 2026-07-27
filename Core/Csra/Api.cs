using Csra.TheLib;
using Csra.Services;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra {

    /// <exclude />
    public static class Api {

        [DoNotSync]
        private static ILib _theLib;
        private static IServices _services;
        static Api() {
            _theLib = new TheLibManager();
            _services = new ServiceManager();
        }

        public static ILib TheLib => _theLib;

        public static IServices Services => _services;

        public static Info_ Info => new();

        /// <summary>
        /// Exchanges the default branch implementations with user-provided implementations.
        /// </summary>
        /// <param name="lib">User-provided implementation of <see cref="ILib"/>.</param>
        /// <param name="services">User-provided implementation of <see cref="IServices"/>.</param>
        /// <remarks>
        /// If any parameter is <c>null</c>, the method will revert to the original implementation 
        /// provided by the csra library.
        /// </remarks>
        public static void Configure(ILib lib = null, IServices services = null) {
            _theLib = lib ?? new TheLibManager();
            _services = services ?? new ServiceManager();
        }

        /// <exclude/>
        public sealed class Info_ {

            public string Version => Csra.Info.VersionDefinition;
        }
    }
}

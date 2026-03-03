using Csra.Interfaces;

namespace Csra.TheLib.Setup {

    internal class SetupManager : ILib.ISetup {

        protected internal SetupManager() {
            Dc = new Dc();
            Digital = new Digital();
            LevelsAndTiming = new LevelsAndTiming();
        }

        public ILib.ISetup.IDc Dc { get; private set; }

        public ILib.ISetup.IDigital Digital { get; private set; }

        public ILib.ISetup.ILevelsAndTiming LevelsAndTiming { get; private set; }

        public void Configure(ILib.ISetup.IDc dc = null, ILib.ISetup.IDigital digital = null, ILib.ISetup.ILevelsAndTiming levelsAndTiming = null) {
            Dc = dc ?? new Dc();
            Digital = digital ?? new Digital();
            LevelsAndTiming = levelsAndTiming ?? new LevelsAndTiming();
        }
    }
}

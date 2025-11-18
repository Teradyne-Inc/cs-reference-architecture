using Csra.Interfaces;

namespace Csra.TheLib.Setup {
    /// <exclude />
    public class SetupManager : ILib.ISetup {

        public ILib.ISetup.IDc Dc => new Dc();

        public ILib.ISetup.IDigital Digital => new Digital();

        public ILib.ISetup.ILevelsAndTiming LevelsAndTiming => new LevelsAndTiming();
    }
}

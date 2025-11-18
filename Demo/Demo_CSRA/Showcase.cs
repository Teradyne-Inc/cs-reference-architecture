using static Csra.Api;
using Csra;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CSRA {

    [TestClass]
    public class Showcase : TestCodeBase {

        [TestMethod]
        public void UseExtensionMethod() {
            TheLib.Setup.Dc.CustomerExtension("firstArgument", 2);
        }

        #region CustomizeTestMethod
        private Site<bool> _patResult;
        private PatternInfo _patternInfoA;
        private PatternInfo _patternInfoB;

        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern patternA, Pattern patternB, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                _patternInfoA = new PatternInfo(patternA, true);
                _patternInfoB = new PatternInfo(patternB, true);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Execute.Digital.RunPattern(_patternInfoA);
                TheLib.Execute.Digital.RunPattern(_patternInfoB);
                _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestFunctional(_patResult, patternA);
            }
        }
        #endregion
    }
}

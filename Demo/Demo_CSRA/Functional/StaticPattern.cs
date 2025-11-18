using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Functional {

    [TestClass(Creation.TestInstance), Serializable]
    public class StaticPattern : TestCodeBase {

        private Site<bool> _patResult;
        private PatternInfo _patternInfo;

        /// <summary>
        /// Executes a functional test with the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="testFunctional">Whether to log the functional result.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, bool testFunctional, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _patternInfo);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Execute.Digital.RunPattern(_patternInfo);
                _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                if (testFunctional) TheLib.Datalog.TestFunctional(_patResult, pattern);
            }
        }
        #endregion
    }
}

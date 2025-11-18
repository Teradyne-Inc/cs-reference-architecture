using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Functional {

    [TestClass]
    public class StaticPattern : TestCodeBase {

        /// <summary>
        /// Executes a functional test with the specified pattern.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="testFunctional">Whether to log the functional result.</param>
        [TestMethod]
        public void Baseline(Pattern pattern, bool testFunctional) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Patterns(pattern).Start();
            TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.HaltWait();
            Site<bool> result = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();
            if (testFunctional) TheExec.Flow.FunctionalTestLimit(result, pattern);
        }
    }
}

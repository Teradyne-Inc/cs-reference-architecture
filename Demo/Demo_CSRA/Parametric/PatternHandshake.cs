using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CSRA.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class PatternHandshake : TestCodeBase {

        [MethodHandleTarget]
        public static List<PinSite<double>> ExampleAction(PatternInfo pattern, int stop) {
            List<PinSite<double>> result = new();
            Pins vcc = new("vcc");
            TheLib.Setup.Dc.SetMeter(vcc, Measure.Current, rangeValue: 0.2 * A, outputRangeValue: 0.2 * A);
            result.Add(TheLib.Acquire.Dc.Measure(vcc, Measure.Current));
            return result;
        }

        private PatternInfo _pattern;
        private MethodHandle<Func<PatternInfo, int, List<PinSite<double>>>> _stopAction;
        private List<PinSite<double>> _values;
        private Site<bool> _patResult;

        /// <summary>
        /// Runs the specified pattern and executes the stopAction at each occurance of stopFlag in the pattern. Every
        /// value returned from the stopAction(s) will be datalogged.
        /// </summary>
        /// <param name="pattern">Pattern name to be executed.</param>
        /// <param name="stopFlag">Pattern flag to stop at.</param>
        /// <param name="numberOfStops">Number of total stop in the pattern.</param>
        /// <param name="stopAction">Action to be called at each stop.</param>
        /// <param name="testFunctional">Whether to test the functional results.</param>
        /// <param name="setup">Optional. Actin to configure the dib or device.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, int stopFlag, int numberOfStops, string stopAction, bool testFunctional, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.GreaterOrEqual(stopFlag, 0, nameof(stopFlag));
                TheLib.Validate.GreaterOrEqual(numberOfStops, 0, nameof(numberOfStops));
                _pattern.SetFlags = stopFlag;
                TheLib.Validate.MethodHandle(stopAction, nameof(stopAction), out _stopAction);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                _values = TheLib.Execute.Digital.RunPatternConditionalStop(_pattern, numberOfStops, _stopAction.Execute);
                if (testFunctional) _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                if (testFunctional) TheLib.Datalog.TestFunctional(_patResult, pattern);
                foreach (var value in _values) {
                    TheLib.Datalog.TestParametric(value);
                }
            }
        }
        #endregion
    }
}

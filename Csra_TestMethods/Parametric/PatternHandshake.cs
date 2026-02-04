using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class PatternHandshake : TestCodeBase {

        /// <summary>
        /// The interface implements IExecutable and should be implemented by the class
        /// that is passed to the TestMethod by its fully-qualified name.
        /// Result must be cleared before use otherwise it contains results from previous runs.
        /// </summary>
        public interface IPatternHandshakeBaselineExecutable : IExecutable {
            public Pins Vcc { get; set; }
            public List<PinSite<double>> Result { get; }
        }

        private PatternInfo _pattern;
        private IPatternHandshakeBaselineExecutable _executableObject;
        private Site<bool> _patResult;

        /// <summary>
        /// Runs the specified pattern and executes executableClass's Execute() at each occurance of stopFlag in the pattern. Every
        /// result will be datalogged.
        /// </summary>
        /// <param name="pattern">Pattern name to be executed.</param>
        /// <param name="stopFlag">Pattern flag to stop at.</param>
        /// <param name="numberOfStops">Number of total stop in the pattern.</param>
        /// <param name="executableClass">Fully-qualified name of class which contains properties and Execute() to be called at each stop.</param>
        /// <param name="testFunctional">Whether to test the functional results.</param>
        /// <param name="setup">Optional. Action to configure the dib or device.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, int stopFlag, int numberOfStops, string executableClass, bool testFunctional, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.GreaterOrEqual(stopFlag, 0, nameof(stopFlag));
                TheLib.Validate.GreaterOrEqual(numberOfStops, 0, nameof(numberOfStops));
                _pattern.SetFlags = stopFlag;
                TheLib.Validate.GetObjectByClassName(executableClass, out _executableObject);
                _executableObject.Vcc = new Pins("vcc");
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Execute.Digital.RunPatternConditionalStop(_pattern, numberOfStops, _executableObject);
                if (testFunctional) _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                if (testFunctional) TheLib.Datalog.TestFunctional(_patResult, pattern);
                foreach (PinSite<double> value in _executableObject.Result) {
                    TheLib.Datalog.TestParametric(value);
                }
            }
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Trim {

    [TestClass(Creation.TestInstance), Serializable]
    public class Digital : TestCodeBase {

        private Pins _pins;
        private PatternInfo _pattern;
        private Site<int> _values;
        private Site<double> _valuesBinary;
        private List<Site<bool>> _measurementsTrip;
        private List<Site<int>> _measurementsTarget;
        private Site<double> _results;
        private PinSite<Samples<int>> _readWords;
        private const int _notFoundResult = -999;
        private int _moduleCount = 256; // Default module count for demo pattern, can be adjusted as needed

        /// <summary>
        /// Use linear stop search with trip criteria to find where the test pattern result change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearStopTrip
        [TestMethod, Steppable, CustomValidation]
        public void LinearStopTrip(Pattern pattern, int from, int to, int count, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                _values = TheLib.Acquire.Search.LinearStopFromToCount(from, to, count, 0, _notFoundResult, (modIndex) => {
                    TheHdw.Patterns(_pattern.Name + $":mod{modIndex}").Start();
                    TheHdw.Digital.Patgen.HaltWait();
                    return TheLib.Acquire.Digital.PatternResults();
                },
                patResult => patResult
                );
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(_values);
            }
        }
        #endregion

        /// <summary>
        /// Use linear stop search with target value to find which test step's output is closest to the target by capturing HRAM data.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="capPins">The pins that are used to capture data through HRAM.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        /// <param name="target">The (numeric) target output value to be searched.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearStopTarget
        [TestMethod, Steppable, CustomValidation]
        public void LinearStopTarget(Pattern pattern, PinList capPins, int from, int to, int count, int target, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(capPins, nameof(capPins), out _pins);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Digital.ReadHram(8, CaptType.STV, TrigType.First, false, 0);
                _values = TheLib.Acquire.Search.LinearStopFromToCount(from, to, count, 0, _notFoundResult, (modIndex) => {
                    Site<int> result = new Site<int>(-1);
                    TheHdw.Patterns(_pattern.Name + $":hram_mod{modIndex}").Start();
                    TheHdw.Digital.Patgen.HaltWait();
                    _readWords = TheLib.Acquire.Digital.ReadWords(_pins, 0, 8, 8, tlBitOrder.LsbFirst);
                    ForEachSite(site => {
                        result[site] = true ? modIndex : _readWords[0][site][0]; // Simulate data process
                    });
                    return result;
                },
                target
                );
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Digital.ReadHram(0, CaptType.None, TrigType.Never, false, 0);
                TheLib.Datalog.TestParametric(_values);
            }
        }
        #endregion

        /// <summary>
        /// Use linear full search with trip criteria to find where the test pattern result change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearFullTrip
        [TestMethod, Steppable, CustomValidation]
        public void LinearFullTrip(Pattern pattern, int from, int to, int count, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                _measurementsTrip = [];
                double increment = TheLib.Acquire.Search.LinearFullFromToCount(from, to, count, (modIndex) => {
                    TheHdw.Patterns(_pattern.Name + $":mod{modIndex}").Start();
                    TheHdw.Digital.Patgen.HaltWait();
                    _measurementsTrip.Add(TheLib.Acquire.Digital.PatternResults());
                });
                _results = TheLib.Execute.Search.LinearFullProcess(_measurementsTrip, from, increment, 0, _notFoundResult, condition => condition);
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(_results);
            }
        }
        #endregion

        /// <summary>
        /// Use linear full search with target value to find which test step's output is closest to the target by capturing HRAM data.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="capPins">The pins that are used to capture data through HRAM.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of steps to execute.</param>
        /// <param name="target">The (numeric) target output value to be searched.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearFullTarget
        [TestMethod, Steppable, CustomValidation]
        public void LinearFullTarget(Pattern pattern, PinList capPins, int from, int to, int count, int target, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(capPins, nameof(capPins), out _pins);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                _measurementsTarget = [];
                TheLib.Setup.Digital.ReadHram(8, CaptType.STV, TrigType.First, false, 0);
                double increment = TheLib.Acquire.Search.LinearFullFromToCount(from, to, count, (modIndex) => {
                    Site<int> result = new Site<int>(-1);
                    TheHdw.Patterns(_pattern.Name + $":hram_mod{modIndex}").Start();
                    TheHdw.Digital.Patgen.HaltWait();
                    _readWords = TheLib.Acquire.Digital.ReadWords(_pins, 0, 8, 8, tlBitOrder.LsbFirst);
                    ForEachSite(site => {
                        result[site] = true ? modIndex : _readWords[0][site][0]; // Simulate data process
                    });
                    _measurementsTarget.Add(result);
                });
                _results = TheLib.Execute.Search.LinearFullProcess(_measurementsTarget, from, increment, 0, target);
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Digital.ReadHram(0, CaptType.None, TrigType.Never, false, 0);
                TheLib.Datalog.TestParametric(_results);
            }
        }
        #endregion

        /// <summary>
        /// Use binary search with trip criteria to find where the test pattern result change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The lower boundary of the search range.</param>
        /// <param name="to">The upper boundary of the search range.</param>
        /// <param name="minDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.</param>
        /// <param name="invertedOutput">A flag indicating whether the output is inverted, affecting the search logic.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region BinaryTrip
        [TestMethod, Steppable, CustomValidation]
        public void BinaryTrip(Pattern pattern, int from, int to, double minDelta, bool invertedOutput, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                for (int i = 0; i < _moduleCount; i++) {
                    string name = _pattern.Name + $":mod{i}";
                    PatternInfo patternTemp = new PatternInfo(name, true);
                }
            }

            if (ShouldRunBody) {
                var patSpec = new SiteVariant();
                _valuesBinary = TheLib.Acquire.Search.BinarySearch(from, to, minDelta, invertedOutput, (modIndex) => {
                    ForEachSite(site => {
                        patSpec[site] = _pattern.Name + $":mod{(int)modIndex[site]}";
                    });
                    TheHdw.PatternsPerSite(patSpec).Start();
                    TheHdw.PatternsPerSite(patSpec).HaltWait();
                    return TheLib.Acquire.Digital.PatternResults();
                },
                patResult => patResult,
                _notFoundResult
                );
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(_valuesBinary);
            }
        }
        #endregion

        /// <summary>
        /// Use binary search with target value to find which test step's output is closest to the target by capturing HRAM data.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="capPins">The pins that are used to capture data through HRAM.</param>
        /// <param name="from">The lower boundary of the search range.</param>
        /// <param name="to">The upper boundary of the search range.</param>
        /// <param name="minDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.</param>
        /// <param name="invertedOutput">A flag indicating whether the output is inverted, affecting the search logic.</param>
        /// <param name="target">The (numeric) target output value for which the corresponding input condition is searched.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region BinaryTarget
        [TestMethod, Steppable, CustomValidation]
        public void BinaryTarget(Pattern pattern, PinList capPins, int from, int to, double minDelta, bool invertedOutput, int target, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                if (!string.IsNullOrEmpty(pattern)) TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(capPins, nameof(capPins), out _pins);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                for (int i = 0; i < _moduleCount; i++) {
                    string name = _pattern.Name + $":hram_mod{i}";
                    PatternInfo patternTemp = new PatternInfo(name, true);
                }
            }

            if (ShouldRunBody) {
                var patSpec = new SiteVariant();
                TheLib.Setup.Digital.ReadHram(8, CaptType.STV, TrigType.First, false, 0);
                _valuesBinary = TheLib.Acquire.Search.BinarySearch(from, to, minDelta, invertedOutput, (modIndex) => {
                    Site<int> result = new(-1);
                    ForEachSite(site => {
                        patSpec[site] = _pattern.Name + $":hram_mod{(int)modIndex[site]}";
                    });
                    TheHdw.PatternsPerSite(patSpec).Start();
                    TheHdw.PatternsPerSite(patSpec).HaltWait();
                    _readWords = TheLib.Acquire.Digital.ReadWords(_pins, 0, 8, 8, tlBitOrder.LsbFirst);
                    ForEachSite(site => {
                        result[site] = true ? (int)modIndex[site] : _readWords[0][site][0]; // Simulate data process
                    });
                    return result;
                },
                target
                );
            }

            if (ShouldRunPostBody) {
                TheLib.Setup.Digital.ReadHram(0, CaptType.None, TrigType.Never, false, 0);
                TheLib.Datalog.TestParametric(_valuesBinary);
            }
        }
        #endregion
    }
}

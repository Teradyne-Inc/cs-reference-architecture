using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Trim {

    [TestClass]
    public class Digital : TestCodeBase {

        private const int NO_FOUND_RESULT = -999;

        /// <summary>
        /// Use linear stop search with trip criteria to find where the test pattern value change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        [TestMethod]
        public void LinearStopTrip(Pattern pattern, int from, int to, int count) {

            Site<int> results = new(NO_FOUND_RESULT);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            string timeDomain = TheHdw.Patterns(pattern).TimeDomains;
            int inValue = from;
            int increment = (to - from) / (count - 1);

            for (int i = 0; i < count; i++) {
                TheHdw.Patterns(pattern.Value + $":mod{inValue}").Start();
                TheHdw.Digital.TimeDomains(timeDomain).Patgen.HaltWait();
                Site<bool> value = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();
                ForEachSite(site => {
                    if (results[site] == NO_FOUND_RESULT && value[site]) results[site] = inValue;
                });
                inValue += increment;
                if (results.All(s => s != NO_FOUND_RESULT)) break;
            }

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Use linear stop search with target value to find which test step's output is closest to the target by capturing HRAM data.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="capPins">The pins that are used to capture data through HRAM.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        /// <param name="target">The (numeric) target output value to be searched.</param>
        [TestMethod]
        public void LinearStopTarget(Pattern pattern, PinList capPins, int from, int to, int count, int target) {

            Site<int> results = new Site<int>(NO_FOUND_RESULT);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            int inValue = from;
            int increment = (to - from) / (count - 1);

            var hram = TheHdw.Digital.HRAM;
            hram.SetTrigger(TrigType.First, false, 0, true);
            hram.CaptureType = CaptType.STV;
            hram.Size = 8;

            Digital digital = new Digital();
            Site<int> outValue1 = digital.OneMeasurement(pattern, pattern.Value + $":hram_mod{inValue}", capPins, inValue);
            Site<int> delta1 = outValue1 - target;
            inValue += increment;
            for (int i = 1; i < count; i++) {
                Site<int> outValue2 = digital.OneMeasurement(pattern, pattern.Value + $":hram_mod{inValue}", capPins, inValue);
                Site<int> delta2 = outValue2 - target;

                ForEachSite(site => {
                    if (results[site] == NO_FOUND_RESULT) {
                        if (Math.Sign(delta1[site]) != Math.Sign(delta2[site])) results[site] = inValue;
                    }
                });
                inValue += increment;
                if (results.All(s => s != NO_FOUND_RESULT)) break;
            }

            hram.SetTrigger(TrigType.Never, false, 0, true);
            hram.CaptureType = CaptType.None;
            hram.Size = 0;

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Use linear full search with trip criteria to find where the test pattern value change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of count to execute.</param>
        [TestMethod]
        public void LinearFullTrip(Pattern pattern, int from, int to, int count) {

            List<Site<bool>> measurements = new();
            Site<int> results = new(NO_FOUND_RESULT);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            string timeDomain = TheHdw.Patterns(pattern).TimeDomains;
            int inValue = from;
            int increment = (to - from) / (count - 1);
            for (int i = 0; i < count; i++) {
                TheHdw.Patterns(pattern.Value + $":mod{inValue}").Start();
                TheHdw.Digital.TimeDomains(timeDomain).Patgen.HaltWait();
                measurements.Add(TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite());
                inValue += increment;
            }

            Site<int> tripIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            ForEachSite(site => {
                for (int index = 0; index < measurements.Count; index++) {
                    if (tripIndexLocal[site] == -1 && measurements[index][site]) tripIndexLocal[site] = index;
                }
            });

            ForEachSite(site => {
                results[site] = tripIndexLocal[site] > -1 ? tripIndexLocal[site] * increment + from : NO_FOUND_RESULT;
            });

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Use linear full search with target value to find which test step's output is closest to the target by capturing HRAM data.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="capPins">The pins that are used to capture data through HRAM.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The total number of steps to execute.</param>
        /// <param name="target">The (numeric) target output value to be searched.</param>
        [TestMethod]
        public void LinearFullTarget(Pattern pattern, PinList capPins, int from, int to, int count, int target) {

            List<Site<int>> measurements = new();
            Site<int> results = new Site<int>(NO_FOUND_RESULT);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            int inValue = from;
            int increment = (to - from) / (count - 1);

            var hram = TheHdw.Digital.HRAM;
            hram.SetTrigger(TrigType.First, false, 0, true);
            hram.CaptureType = CaptType.STV;
            hram.Size = 8;

            Digital digital = new Digital();
            for (int i = 0; i < count; i++) {
                Site<int> value = digital.OneMeasurement(pattern, pattern.Value + $":hram_mod{inValue}", capPins, inValue);
                measurements.Add(value);
                inValue += increment;
            }

            ForEachSite(site => {
                var deltas = measurements.Select(v => Math.Abs(v[site] - target)).ToList();
                dynamic closestDeltaSoFar = deltas[0];
                int closestIndexSoFar = 0;
                for (int i = 1; i < measurements.Count; i++) {
                    if (Math.Abs(deltas[i]) < Math.Abs(closestDeltaSoFar)) {
                        closestDeltaSoFar = deltas[i];
                        closestIndexSoFar = i;
                    }
                }
                results[site] = measurements[closestIndexSoFar][site];
            });

            hram.SetTrigger(TrigType.Never, false, 0, true);
            hram.CaptureType = CaptType.None;
            hram.Size = 0;

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// Use binary search with trip criteria to find where the test pattern result change from fail to pass.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="from">The lower boundary of the search range.</param>
        /// <param name="to">The upper boundary of the search range.</param>
        /// <param name="minDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.</param>
        /// <param name="invertedOutput">A flag indicating whether the output is inverted, affecting the search logic.</param>
        [TestMethod]
        public void BinaryTrip(Pattern pattern, int from, int to, int minDelta, bool invertedOutput) {

            int moduleCount = 256;
            Site<int> results = new(NO_FOUND_RESULT);
            Site<bool> alwaysTripped = new(true);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            for (int i = 0; i < moduleCount; i++) {
                string name = pattern.Value + $":mod{i}";
                TheHdw.Patterns(name).Threading.Enable = true;
                TheHdw.Patterns(name).ValidateThreading();
            }

            Site<int> inValue = new();
            Site<int> fromPerSite = new(from);
            Site<int> toPerSite = new(to);
            Site<int> stepSize = new();
            Site<bool> value;
            SiteVariant patSpec = new();
            do {
                ForEachSite(site => {
                    inValue[site] = (fromPerSite[site] + toPerSite[site]) / 2;
                    patSpec[site] = pattern.Value + $":mod{inValue[site]}";
                });
                TheHdw.PatternsPerSite(patSpec).Start();
                TheHdw.PatternsPerSite(patSpec).HaltWait();
                value = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();

                ForEachSite(site => {
                    if (value[site]) {
                        results[site] = inValue[site];
                        stepSize[site] = toPerSite[site] - inValue[site];
                        toPerSite[site] = inValue[site];
                    } else {
                        alwaysTripped[site] = false;
                        stepSize[site] = inValue[site] - fromPerSite[site];
                        fromPerSite[site] = inValue[site];
                    }
                });
            } while (stepSize.Any(x => x >= minDelta));
            ForEachSite(site => {
                if (alwaysTripped[site]) results[site] = NO_FOUND_RESULT;
            });

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

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
        [TestMethod]
        public void BinaryTarget(Pattern pattern, PinList capPins, int from, int to, int minDelta, bool invertedOutput, int target) {

            int moduleCount = 256;
            Site<int> results = new(NO_FOUND_RESULT);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            for (int i = 0; i < moduleCount; i++) {
                string name = pattern.Value + $":hram_mod{i}";
                TheHdw.Patterns(name).Threading.Enable = true;
                TheHdw.Patterns(name).ValidateThreading();
            }

            var hram = TheHdw.Digital.HRAM;
            hram.SetTrigger(TrigType.First, false, 0, true);
            hram.CaptureType = CaptType.STV;
            hram.Size = 8;

            Site<int> fromPerSite = new(from);
            Site<int> toPerSite = new(to);
            Site<int> inValue = new();
            Site<int> stepSize = new();
            bool first = true;
            Site<int> value;
            SiteVariant patSpec = new();
            Digital digital = new();
            Site<int> devAbsBest = new(0);
            do {
                ForEachSite(site => {
                    inValue[site] = (fromPerSite[site] + toPerSite[site]) / 2;
                    patSpec[site] = pattern.Value + $":hram_mod{inValue[site]}";
                });
                value = digital.OneMeasurementPatternsPerSite(patSpec, capPins, inValue);

                ForEachSite(site => {
                    int devAbs = Math.Abs(value[site] - target);
                    if (devAbs < devAbsBest[site] || first) {
                        devAbsBest[site] = devAbs;
                        results[site] = inValue[site];
                    }

                    if (value[site] > target) {
                        stepSize[site] = toPerSite[site] - inValue[site];
                        toPerSite[site] = inValue[site];
                    } else {
                        stepSize[site] = inValue[site] - fromPerSite[site];
                        fromPerSite[site] = inValue[site];
                    }
                });
                first = false;
            } while (stepSize.Any(x => x >= minDelta));

            hram.SetTrigger(TrigType.Never, false, 0, true);
            hram.CaptureType = CaptType.None;
            hram.Size = 0;

            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        private Site<int> OneMeasurement(Pattern pattern, string modName, PinList capPins, int inValue) {
            Site<int> result = new Site<int>(-1);
            TheHdw.Patterns(modName).Start();
            TheHdw.Digital.TimeDomains(TheHdw.Patterns(pattern).TimeDomains).Patgen.HaltWait();
            PinSite<Samples<int>> readWords = new();

            TheExec.DataManager.DecomposePinList(capPins, out string[] pins, out _);
            foreach (string pin in pins) {
                ISiteLong[] hramWords = (ISiteLong[])TheHdw.Digital.Pins(pin).HRAM.ReadDataWord(0, 8, 8, tlBitOrder.LsbFirst);
                Site<Samples<int>> siteWords = new Site<Samples<int>>();
                siteWords.PinName = pin;
                ForEachSite(site => {
                    int[] wordValues = hramWords.Select(word => (int)word[site]).ToArray();
                    siteWords[site] = new Samples<int>(wordValues);
                });
                readWords.Add(siteWords);
                ForEachSite(site => {
                    result[site] = true ? inValue : readWords[0][site][0]; // Simulate data process
                });
            }
            return result;
        }

        private Site<int> OneMeasurementPatternsPerSite(SiteVariant patSpec, PinList capPins, Site<int> modIndex) {
            Site<int> result = new Site<int>(-1);
            TheHdw.PatternsPerSite(patSpec).Start();
            TheHdw.PatternsPerSite(patSpec).HaltWait();
            PinSite<Samples<int>> readWords = new();

            TheExec.DataManager.DecomposePinList(capPins, out string[] pins, out _);
            foreach (string pin in pins) {
                ISiteLong[] hramWords = (ISiteLong[])TheHdw.Digital.Pins(pin).HRAM.ReadDataWord(0, 8, 8, tlBitOrder.LsbFirst);
                Site<Samples<int>> siteWords = new Site<Samples<int>>();
                siteWords.PinName = pin;
                ForEachSite(site => {
                    int[] wordValues = hramWords.Select(word => (int)word[site]).ToArray();
                    siteWords[site] = new Samples<int>(wordValues);
                });
                readWords.Add(siteWords);
                ForEachSite(site => {
                    result[site] = true ? modIndex[site] : readWords[0][site][0]; // Simulate data process
                });
            }
            return result;
        }
    }
}

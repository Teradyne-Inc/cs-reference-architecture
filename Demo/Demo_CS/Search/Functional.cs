using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Search {

    [TestClass]
    public class Functional : TestCodeBase {

        private const int _notFoundResult = -999;

        /// <summary>
        /// The measurements across the entire range are traversed without being evaluated during the linear search, after which the device input condition
        /// for which the pattern passes is provided. 
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="forcePins">The pins that are being forced. The support pin types can be DC(DCVI, DCVS and PPMU) and Digital(Vih level).</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        [TestMethod]
        public void LinearFull(Pattern pattern, string forcePins, double from, double to, int count, double waitTime) {

            List<Site<bool>> measurements = [];

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Unpowered);
            var dcvs = TheHdw.DCVS.Pins(forcePins);
            dcvs.Connect();
            dcvs.Voltage.Value = from;
            dcvs.Gate = true;
            TheHdw.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling

            string timeDomain = TheHdw.Patterns(pattern).TimeDomains;
            double inValue = from;
            double increment = (to - from) / (count - 1);
            for (int i = 0; i < count; i++) {
                dcvs.Voltage.Main.Value = inValue;
                TheHdw.Wait(waitTime);
                TheHdw.Patterns(pattern).Start();
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
            Site<double> results = new Site<double>();
            ForEachSite(site => {
                results[site] = tripIndexLocal[site] > -1 ? tripIndexLocal[site] * increment + from : _notFoundResult;
            });

            dcvs.Gate = false;
            dcvs.Disconnect();
            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// The measurements across the range are traversed with an evaluation performed at each iteration, and the search is stopped once the pattern passes
        /// (on all sites). The device input condition for which the pattern passes is then provided.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="forcePins">The pins that are being forced. The support pin types can be DC(DCVI, DCVS and PPMU) and Digital(Vih level).</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        [TestMethod]
        public void LinearStop(Pattern pattern, string forcePins, double from, double to, int count, double waitTime) {

            Site<double> results = new Site<double>(_notFoundResult);

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Unpowered);
            var dcvs = TheHdw.DCVS.Pins(forcePins);
            dcvs.Connect();
            dcvs.Voltage.Value = from;
            dcvs.Gate = true;
            TheHdw.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling

            string timeDomain = TheHdw.Patterns(pattern).TimeDomains;
            double inValue = from;
            double increment = (to - from) / (count - 1);
            for (int i = 0; i < count; i++) {
                dcvs.Voltage.Main.Value = inValue;
                TheHdw.Wait(waitTime);
                TheHdw.Patterns(pattern).Start();
                TheHdw.Digital.TimeDomains(timeDomain).Patgen.HaltWait();
                Site<bool> value = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();
                ForEachSite(site => {
                    if (results[site] == _notFoundResult && value[site]) results[site] = inValue;
                });
                inValue += increment;
                if (results.All(s => s != _notFoundResult)) break;
            }

            dcvs.Gate = false;
            dcvs.Disconnect();
            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }

        /// <summary>
        /// The search range is divided in half at each iteration, with a check performed on the midpoint, and the search stops once the target condition is
        /// met. The input value where the condition passes is then returned as the result.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="forcePins">The pins that are being forced. The support pin types can be DC(DCVI, DCVS and PPMU) and Digital(Vih level).</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="minDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        [TestMethod]
        public void Binary(Pattern pattern, string forcePins, double from, double to, double minDelta, double waitTime) {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Unpowered);
            var dcvs = TheHdw.DCVS.Pins(forcePins);
            dcvs.Connect();
            dcvs.Voltage.Value = (from + to) / 2;
            dcvs.Gate = true;
            TheHdw.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling

            Site<double> results = new Site<double>(_notFoundResult);
            Site<bool> alwaysTripped = new Site<bool>(true);

            string timeDomain = TheHdw.Patterns(pattern).TimeDomains;
            Site<double> inValue = new((from + to) / 2);
            double delta = (to - from) / 2;
            bool done = false;
            Site<bool> value;
            do {
                ForEachSite(site => {
                    dcvs.Voltage.Main.Value = inValue[site];
                });
                TheHdw.Wait(waitTime);
                TheHdw.Patterns(pattern).Start();
                TheHdw.Digital.TimeDomains(timeDomain).Patgen.HaltWait();
                value = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();
                done = delta <= minDelta;
                delta /= 2;
                ForEachSite(site => {
                    if (value[site]) {
                        results[site] = inValue[site];
                        inValue[site] -= delta;
                    } else {
                        alwaysTripped[site] = false;
                        inValue[site] += delta;
                    }
                });
            } while (!done);
            ForEachSite(site => {
                if (alwaysTripped[site]) results[site] = _notFoundResult;
            });

            dcvs.Gate = false;
            dcvs.Disconnect();
            TheExec.Flow.TestLimit(ResultVal: results, ForceResults: tlLimitForceResults.Flow);
        }
    }
}

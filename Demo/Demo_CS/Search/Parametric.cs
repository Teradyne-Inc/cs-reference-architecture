using System;
using System.Linq;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Search {

    [TestClass]
    public class Parametric : TestCodeBase {

        private tlDriverPPMUPins _allPins;
        private tlDriverPPMUPins _forcePin;
        private tlDriverPPMUPins _measurePin;
        private Site<double> _results;
        private List<Site<double>> _measuredDcValues;
        private const int _notFoundResult = -999;

        /// <summary>
        /// Voltage values across the entire range are traversed without being evaluated during the linear search.
        /// Subsequently, the device input condition for which the output pin voltage exceeds the threshold is determined.
        /// </summary>
        /// <param name="forcePins">The pins that are being forced.</param>
        /// <param name="measurePin">The pin that is being measured. The measurement is performed for a single pin.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="threshold">The value for which the output meets the required condition for the searched input value.</param>
        /// <param name="clampCurrent">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        #region LinearFull
        [TestMethod]
        public void LinearFull(PinList forcePins, PinList measurePin, double from, double to, int count, double threshold, double clampCurrent,
            double waitTime) {

            _allPins = TheHdw.PPMU.Pins($"{forcePins.Value}, {measurePin.Value}");
            _forcePin = TheHdw.PPMU.Pins(forcePins);
            _measurePin = TheHdw.PPMU.Pins(measurePin);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = ChInitState.Lo;
            TheHdw.SettleWait(1 * s);
            TheHdw.Digital.Pins($"{forcePins.Value}, {measurePin.Value}").Disconnect();
            _allPins.Connect();

            //Body
            _measuredDcValues = [];
            _measurePin.ForceI(0);
            _measurePin.Gate = tlOnOff.Off;
            _forcePin.ForceV(from, clampCurrent);
            _forcePin.Gate = tlOnOff.On;
            double forceValue = from;
            double increment = (to - from) / (count - 1); // both end points included
            for (int i = 0; i < count; i++) {
                _forcePin.ForceV(forceValue);
                TheHdw.SetSettlingTimer(waitTime);
                Site<double> meas = _measurePin.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>().First();
                _measuredDcValues.Add(meas);
                forceValue += increment;
            }
            Site<int> tripIndexLocal = new(-1); // initialize with -1 to indicate no trip found
            ForEachSite(site => { // this could be done with a convoluted & unreadable LINQ statement
                for (int index = 0; index < _measuredDcValues.Count; index++) {
                    if (tripIndexLocal[site] == -1 && threshold < _measuredDcValues[index][site]) tripIndexLocal[site] = index;
                }
            });
            _results = tripIndexLocal.Select(index => index > -1 ? (index * increment + from) : _notFoundResult);

            //PostBody
            _allPins.Gate = tlOnOff.Off;
            _allPins.Disconnect();
            TheHdw.Digital.Pins($"{forcePins.Value}, {measurePin.Value}").Connect();
            TheExec.Flow.TestLimit(ResultVal: _results, ForceUnit: UnitType.Custom, ForceResults: tlLimitForceResults.Flow);
        }
        #endregion

        /// <summary>
        /// Voltage values across the range are traversed with an evaluation performed at each iteration, and the search is terminated once the objective has
        /// been reached across all sites. The device input condition for which the output pin voltage exceeds the threshold is then determined.
        /// </summary>
        /// <param name="forcePins">The pins that are being forced.</param>
        /// <param name="measurePin">The pin that is being measured. The measurement is performed for a single pin.</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="count">The number of input points for which the search is performed.</param>
        /// <param name="threshold">The value for which the output meets the required condition for the searched input value.</param>
        /// <param name="clampCurrent">The value to clamp for force pin.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        #region LinearStop
        [TestMethod]
        public void LinearStop(PinList forcePins, PinList measurePin, double from, double to, int count, double threshold, double clampCurrent,
            double waitTime) {

            _allPins = TheHdw.PPMU.Pins($"{forcePins.Value}, {measurePin.Value}");
            _forcePin = TheHdw.PPMU.Pins(forcePins);
            _measurePin = TheHdw.PPMU.Pins(measurePin);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = ChInitState.Lo;
            TheHdw.SettleWait(1 * s);
            TheHdw.Digital.Pins($"{forcePins.Value}, {measurePin.Value}").Disconnect();
            _allPins.Connect();

            //Body
            _measurePin.ForceI(0);
            _measurePin.Gate = tlOnOff.Off;
            _forcePin.ForceV(from, clampCurrent);
            _forcePin.Gate = tlOnOff.On;
            double forceValue = from;
            double increment = (to - from) / (count - 1); // both end points included
            Site<int> tripIndex = new(-1); // initialize with -1 to indicate no trip found
            int index = 0;
            do {
                _forcePin.ForceV(forceValue);
                TheHdw.SetSettlingTimer(waitTime);
                Site<double> meas = _measurePin.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>().First();
                ForEachSite(site => {
                    if (tripIndex[site] == -1 && threshold < meas[site]) tripIndex[site] = index;
                });
                forceValue += increment;
                index++;
            } while (forceValue <= to && tripIndex.Any(s => s == -1));
            _results = tripIndex.Select(index => index > -1 ? (index * increment + from) : _notFoundResult);

            //PostBody
            _allPins.Gate = tlOnOff.Off;
            _allPins.Disconnect();
            TheHdw.Digital.Pins($"{forcePins.Value}, {measurePin.Value}").Connect();
            TheExec.Flow.TestLimit(ResultVal: _results, ForceUnit: UnitType.Custom, ForceResults: tlLimitForceResults.Flow);
        }
        #endregion
    }
}

using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Search {

    [TestClass(Creation.TestInstance), Serializable]
    public class Functional : TestCodeBase {

        private PatternInfo _pattern = null;
        private Pins _pins;
        private Site<double> _values;
        private List<Site<bool>> _measurements;
        private Site<double> _results;
        private bool _containsDcviDcvs = false;
        private bool _containsDigitalPins = false;
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
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearFull
        [TestMethod, Steppable, CustomValidation]
        public void LinearFull(Pattern pattern, PinList forcePins, double from, double to, int count, double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(forcePins, nameof(forcePins), out _pins);
                TheLib.Validate.GreaterOrEqual(count, 2, nameof(count));
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _containsDcviDcvs = _pins.ContainsFeature(InstrumentFeature.Dcvi) || _pins.ContainsFeature(InstrumentFeature.Dcvs);
                if (_containsDcviDcvs) _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDcviDcvs) {
                    if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                    TheLib.Setup.Dc.Connect(_pins);
                }
            }

            if (ShouldRunBody) {
                _measurements = [];
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.ForceV(_pins, from);
                    TheLib.Execute.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling
                }
                double increment = TheLib.Acquire.Search.LinearFullFromToCount(from, to, count, (forceValue) => {
                    if (_containsDcviDcvs) TheLib.Setup.Dc.Modify(_pins, voltage: forceValue);
                    else TheLib.Setup.Digital.ModifyPinsLevels(pins: _pins, levelsType: ChPinLevel.Vih, levelsValue: forceValue);
                    TheLib.Execute.Wait(waitTime);
                    TheLib.Execute.Digital.RunPattern(_pattern);
                    _measurements.Add(TheLib.Acquire.Digital.PatternResults());
                });
                _results = TheLib.Execute.Search.LinearFullProcess(_measurements, from, increment, 0, _notFoundResult, condition => condition);
            }

            if (ShouldRunPostBody) {
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.Disconnect(_pins);
                    if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                }
                TheLib.Datalog.TestParametric(_results);
            }
        }
        #endregion

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
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region LinearStop
        [TestMethod, Steppable, CustomValidation]
        public void LinearStop(Pattern pattern, PinList forcePins, double from, double to, int count, double waitTime, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(forcePins, nameof(forcePins), out _pins);
                TheLib.Validate.GreaterOrEqual(count, 2, nameof(count));
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _containsDcviDcvs = _pins.ContainsFeature(InstrumentFeature.Dcvi) || _pins.ContainsFeature(InstrumentFeature.Dcvs);
                if (_containsDcviDcvs) _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDcviDcvs) {
                    if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                    TheLib.Setup.Dc.Connect(_pins);
                }
            }

            if (ShouldRunBody) {
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.ForceV(_pins, from);
                    TheLib.Execute.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling
                }
                _values = TheLib.Acquire.Search.LinearStopFromToCount(from, to, count, 0, _notFoundResult, (forceValue) => {
                    if (_containsDcviDcvs) TheLib.Setup.Dc.Modify(_pins, voltage: forceValue);
                    else TheLib.Setup.Digital.ModifyPinsLevels(pins: _pins, levelsType: ChPinLevel.Vih, levelsValue: forceValue);
                    TheLib.Execute.Wait(waitTime);
                    TheLib.Execute.Digital.RunPattern(_pattern);
                    return TheLib.Acquire.Digital.PatternResults();
                },
                patResult => patResult
                );
            }

            if (ShouldRunPostBody) {
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.Disconnect(_pins);
                    if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                }
                TheLib.Datalog.TestParametric(_values);
            }
        }
        #endregion

        /// <summary>
        /// The search range is divided in half at each iteration, with a check performed on the midpoint, and the search stops once the target condition is
        /// met. The input value where the condition passes is then returned as the result.
        /// </summary>
        /// <param name="pattern">The pattern to run.</param>
        /// <param name="forcePins">The pins that are being forced. The support pin types can be DC(DCVI, DCVS and PPMU) and Digital(Vih level).</param>
        /// <param name="from">The starting value of the linear input ramp.</param>
        /// <param name="to">The stopping value of the linear input ramp.</param>
        /// <param name="minDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.</param>
        /// <param name="invertedOutput">A flag indicating whether the output is inverted, affecting the search logic.</param>
        /// <param name="waitTime">The wait time per step during ramp execution, used to delay measurement after each force transition.</param>
        /// <param name="setup">Optional. The name of the setup set to be applied through the setup service.</param>
        #region Binary
        [TestMethod, Steppable, CustomValidation]
        public void Binary(Pattern pattern, PinList forcePins, double from, double to, double minDelta, bool invertedOutput, double waitTime,
            string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(forcePins, nameof(forcePins), out _pins);
                TheLib.Validate.GreaterThan(minDelta, 0, nameof(minDelta));
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
                _containsDcviDcvs = _pins.ContainsFeature(InstrumentFeature.Dcvi) || _pins.ContainsFeature(InstrumentFeature.Dcvs);
                if (_containsDcviDcvs) _containsDigitalPins = _pins.ContainsFeature(InstrumentFeature.Digital);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
                if (_containsDcviDcvs) {
                    if (_containsDigitalPins) TheLib.Setup.Digital.Disconnect(_pins);
                    TheLib.Setup.Dc.Connect(_pins);
                }
            }

            if (ShouldRunBody) {
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.ForceV(_pins, (from + to) / 2);
                    TheLib.Execute.Wait(waitTime); // first step may be bigger than the subsequent ones, use 2x settling
                }
                _values = TheLib.Acquire.Search.BinarySearch(from, to, minDelta, invertedOutput, (forceValue) => {
                    ForEachSite(site => {
                        if (_containsDcviDcvs) TheLib.Setup.Dc.Modify(_pins, voltage: forceValue[site]);
                        else TheLib.Setup.Digital.ModifyPinsLevels(pins: _pins, levelsType: ChPinLevel.Vih, levelsValue: forceValue[site]);
                    });
                    TheLib.Execute.Wait(waitTime);
                    TheLib.Execute.Digital.RunPattern(_pattern);
                    return TheLib.Acquire.Digital.PatternResults();
                },
                patResult => patResult,
                _notFoundResult
                );
            }

            if (ShouldRunPostBody) {
                if (_containsDcviDcvs) {
                    TheLib.Setup.Dc.Disconnect(_pins);
                    if (_containsDigitalPins) TheLib.Setup.Digital.Connect(_pins);
                }
                TheLib.Datalog.TestParametric(_values);
            }
        }
        #endregion
    }
}

using System;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Timing {

    [TestClass(Creation.TestInstance), Serializable]
    public class Frequency : TestCodeBase {

        private PatternInfo _pattern;
        private Pins _measurePins;
        private PinSite<double> _freqMeasure;
        private FreqCtrEventSrcSel _eventSource;
        private FreqCtrEventSlopeSel _eventSlope;

        /// <summary>
        /// Measures the frequency of the given digital pin(s) with the frequency counter while the pattern is executing.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="pinList">Digital pin(s) to measure the frequency.</param>
        /// <param name="waitTime">Optional. Time to wait between the pattern start and the start of the frequency measurement.</param>
        /// <param name="measureWindow">Optional. Time to measure the frequency, longer measure times yield more accurate results.</param>
        /// <param name="eventSource">Optional. The event source for the frequency counter (VOH, VOL or BOTH).</param>
        /// <param name="eventSlope">Optional. The event slope for the frequency counter (Positive or Negative).</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, PinList pinList, double waitTime = 0, double measureWindow = 10 * ms, string eventSource = "VOH",
            string eventSlope = "Positive", string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(pinList, nameof(pinList), out _measurePins);
                TheLib.Validate.Enum(eventSource, nameof(eventSource), out _eventSource);
                TheLib.Validate.Enum(eventSlope, nameof(eventSlope), out _eventSlope);
                TheLib.Validate.InRange(measureWindow, 2.5 * ns, 10.7 * s, nameof(measureWindow)); // 2.5ns and 10.7s are instrument limits on UP2200
                TheLib.Validate.InRange(waitTime, 0, 600, nameof(waitTime));
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Digital.FrequencyCounter(_measurePins, measureWindow, _eventSource, _eventSlope);
                TheLib.Execute.Digital.StartPattern(_pattern);
                TheLib.Execute.Wait(waitTime, true);
                _freqMeasure = TheLib.Acquire.Digital.MeasureFrequency(_measurePins);
                TheLib.Execute.Digital.ForcePatternHalt(_pattern);
            }

            if (ShouldRunPostBody) {
                TheLib.Datalog.TestParametric(_freqMeasure);
            }
        }
        #endregion
    }
}

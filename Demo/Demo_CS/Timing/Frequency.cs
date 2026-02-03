using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Demo_CS.Timing {
    [TestClass]
    public class Frequency : TestCodeBase {


        /// <summary>
        /// Measures the frequency of the given digital pin(s) with the frequency counter while the pattern is executing.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="pinList">Digital pin(s) to measure the frequency.</param>
        /// <param name="waitTime">Optional. Time to wait between the pattern start and the start of the frequency measurement.</param>
        /// <param name="measureWindow">Optional. Time to measure the frequency, longer measure times yield more accurate results.</param>
        /// <param name="eventSource">Optional. The event source for the frequency counter (VOH, VOL or BOTH).</param>
        /// <param name="eventSlope">Optional. The event slope for the frequency counter (Positive or Negative).</param>
        #region Baseline
        [TestMethod]
        public void Baseline(Pattern pattern, PinList pinList, double waitTime = 0, double measureWindow = 10 * ms, string eventSource = "VOH",
            string eventSlope = "Positive") {

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            DriverDigPinsFreqCtr freqPin = TheHdw.Digital.Pins(pinList).FreqCtr;

            freqPin.Clear();

            freqPin.EventSource = string.Equals(eventSource, "VOH", StringComparison.OrdinalIgnoreCase) ? FreqCtrEventSrcSel.VOH :
                    string.Equals(eventSource.ToUpper(), "VOL", StringComparison.OrdinalIgnoreCase) ? FreqCtrEventSrcSel.VOL : FreqCtrEventSrcSel.BOTH;

            freqPin.EventSlope = string.Equals(eventSlope, "Positive", StringComparison.OrdinalIgnoreCase) ? FreqCtrEventSlopeSel.Positive :
                    FreqCtrEventSlopeSel.Negative;

            freqPin.Enable = FreqCtrEnableSel.IntervalEnable;
            freqPin.Interval = measureWindow;

            TheHdw.Patterns(pattern).Load();
            TheHdw.Patterns(pattern).Start();

            TheHdw.Wait(waitTime);

            freqPin.Start();

            PinSite<double> freqMeasure = freqPin.MeasureFrequency().ToPinSite<double>();

            TheHdw.Digital.Patgen.Halt();

            TheExec.Flow.TestLimit(ResultVal: freqMeasure, ForceResults: tlLimitForceResults.Flow);

        }
        #endregion
    }
}

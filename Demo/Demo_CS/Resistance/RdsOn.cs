using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Resistance {

    [TestClass]
    public class RdsOn : TestCodeBase {

        /// <summary>
        /// Performs a resistance measurement by forcing voltage or current and measuring current or voltage on the same pin.
        /// </summary>
        /// <param name="forcePin">Pin to force and measure.</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="measureRange">The range for the measurement.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        #region Baseline
        [TestMethod]
        public void Baseline(PinList forcePin, double forceValue, double measureRange, double waitTime = 0) {

            var ppmu = TheHdw.PPMU.Pins(forcePin);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins(forcePin).Disconnect();
            ppmu.Connect();

            //Body
            ppmu.ForceI(forceValue, forceValue);
            ppmu.ClampVHi.Value = ppmu.ClampVHi.Max;
            ppmu.ClampVLo.Value = measureRange;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> meas = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = meas / forceValue;

            //PostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(forcePin).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: "",
                ForceResults: tlLimitForceResults.Flow);
        }
        #endregion

        /// <summary>
        /// Performs a resistance measurement by forcing voltage on one pin and measuring current on second pin.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceValue">The value to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measurePin">Pin to measure.</param>
        /// <param name="labelOfStoredVoltage">Optional. Label of a reference voltage from a previously stored measurement.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        #region TwoPinsOneForceOneMeasure
        [TestMethod]
        public void TwoPinsOneForceOneMeasure(PinList forcePin, double forceValue, double clampValueOfForcePin, PinList measurePin,
                string labelOfStoredVoltage = "", double waitTime = 0) {

            var force = TheHdw.PPMU.Pins(forcePin);
            var measure = TheHdw.PPMU.Pins(measurePin);
            double labelOfStored = double.Parse(labelOfStoredVoltage);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("clk_src").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins(forcePin).Disconnect();
            TheHdw.Digital.Pins(measurePin).Disconnect();
            force.Connect();
            measure.Connect();

            //Body
            measure.ForceI(0);
            measure.Gate = tlOnOff.Off;
            force.ForceI(forceValue, forceValue);
            force.ClampVHi.Value = clampValueOfForcePin;
            force.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> meas = measure.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = (meas - labelOfStored) / forceValue;

            //PostBody
            force.Disconnect();
            measure.Disconnect();
            force.Gate = tlOnOff.Off;
            measure.Gate = tlOnOff.Off;
            TheHdw.Digital.Pins(forcePin).Connect();
            TheHdw.Digital.Pins(measurePin).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceVal: forceValue, ForceUnit: UnitType.Custom, CustomForceunit: "",
              ForceResults: tlLimitForceResults.Flow);
        }
        #endregion

        /// <summary>
        /// Performs a resistance delta measurement by forcing two force values on one pin and measuring with a second pin.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceFirstValue">First value to force.</param>
        /// <param name="forceSecondValue">Second value to force to calculate a delta.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measurePin">Pin to measure.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        #region TwoPinsDeltaForceDeltaMeasure
        [TestMethod]
        public void TwoPinsDeltaForceDeltaMeasure(PinList forcePin, double forceFirstValue, double forceSecondValue, double clampValueOfForcePin,
                PinList measurePin, double waitTime = 0) {

            var force = TheHdw.PPMU.Pins(forcePin);
            var measure = TheHdw.PPMU.Pins(measurePin);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("clk_src").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins(forcePin).Disconnect();
            TheHdw.Digital.Pins(measurePin).Disconnect();
            force.Connect();
            measure.Connect();

            //Body
            measure.ForceI(0);
            measure.Gate = tlOnOff.Off;
            force.ForceI(forceFirstValue, forceFirstValue);
            force.ClampVHi.Value = clampValueOfForcePin;
            force.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measFirst = measure.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            force.ForceI(forceSecondValue, forceSecondValue);
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measSecond = measure.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = (measFirst - measSecond) / (forceFirstValue - forceSecondValue);

            //PostBody
            force.Disconnect();
            measure.Disconnect();
            force.Gate = tlOnOff.Off;
            measure.Gate = tlOnOff.Off;
            TheHdw.Digital.Pins(forcePin).Connect();
            TheHdw.Digital.Pins(measurePin).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceUnit: UnitType.Custom, CustomForceunit: "",
              ForceResults: tlLimitForceResults.Flow);
        }
        #endregion
    }
}

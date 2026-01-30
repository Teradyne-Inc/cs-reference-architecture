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

        /// <summary>
        /// Performs a resistance measurement by forcing a current on one pin and measuring on two other pins.
        /// </summary>
        /// <param name="forcePin">Pin to force.</param>
        /// <param name="forceCurrentPin">Current to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="measureFirstPin">First pin to measure voltage.</param>
        /// <param name="measureSecondPin">Second pin to measure voltage.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        #region ThreePinsOneForceTwoMeasure
        [TestMethod]
        public void ThreePinsOneForceTwoMeasure(PinList forcePin, double forceCurrentPin, double clampValueOfForcePin, PinList measureFirstPin,
             PinList measureSecondPin, double waitTime) {

            PinList digPins = string.Join(", ", forcePin, measureFirstPin, measureSecondPin);
            PinList measurePins = string.Join(", ", measureFirstPin, measureSecondPin);
            var force = TheHdw.PPMU.Pins(forcePin);
            var measureFirst = TheHdw.PPMU.Pins(measureFirstPin);
            var measureSecond = TheHdw.PPMU.Pins(measureSecondPin);
            var digitalPins = TheHdw.PPMU.Pins(digPins);
            var measPins = TheHdw.PPMU.Pins(measurePins);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("TDI,TMS").InitState = ChInitState.Off;
            TheHdw.Digital.Pins(digPins).Disconnect();
            digitalPins.Connect();

            //Body
            measPins.ForceI(0);
            measPins.Gate = tlOnOff.Off;
            force.ForceI(forceCurrentPin, forceCurrentPin);
            force.ClampVHi.Value = clampValueOfForcePin;
            force.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measFirst = measureFirst.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> measSecond = measureSecond.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = (measFirst - measSecond) / forceCurrentPin;

            //PostBody
            digitalPins.Disconnect();
            digitalPins.Gate = tlOnOff.Off;
            TheHdw.Digital.Pins(digPins).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceVal: forceCurrentPin, ForceUnit: UnitType.Custom, CustomForceunit: "",
              ForceResults: tlLimitForceResults.Flow);
        }
        #endregion

        /// <summary>
        /// Performs a resistance delta measurement by forcing a current on first pin, voltage on second pin and measuring a delta voltage on two other pins.
        /// </summary>
        /// <param name="forceFirstPin">First pin to force Current.</param>
        /// <param name="forceValueFirstPin">Value to force on first pin.</param>
        /// <param name="clampValueOfForceFirstPin">Clamp Value of the first force pin. May also set its range.</param>
        /// <param name="forceSecondPin">Second pin to force Voltage.</param>
        /// <param name="forceValueSecondPin">Value to force on second pin.</param>
        /// <param name="clampValueOfForceSecondPin">Clamp Value of the second force pin. May also set its range.</param>
        /// <param name="measureFirstPin">First pin to measure Voltage.</param>
        /// <param name="measureSecondPin">Second pin to measure Voltage.</param>
        /// <param name="waitTime">Optional. Wait time after forcing.</param>
        #region FourPinsTwoForceTwoMeasure
        [TestMethod]
        public void FourPinsTwoForceTwoMeasure(PinList forceFirstPin, double forceValueFirstPin, double clampValueOfForceFirstPin, PinList forceSecondPin,
            double forceValueSecondPin, double clampValueOfForceSecondPin, PinList measureFirstPin, PinList measureSecondPin, double waitTime) {

            PinList digPinList = string.Join(", ", forceFirstPin, measureFirstPin, measureSecondPin);
            PinList measurePins = string.Join(", ", measureFirstPin, measureSecondPin);
            var forceFirst = TheHdw.PPMU.Pins(forceFirstPin);
            var forceSecond = TheHdw.DCVS.Pins(forceSecondPin);
            var measureFirst = TheHdw.PPMU.Pins(measureFirstPin);
            var measureSecond = TheHdw.PPMU.Pins(measureSecondPin);
            var digitalPins = TheHdw.PPMU.Pins(digPinList);
            var measPins = TheHdw.PPMU.Pins(measurePins);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins("TDI,TMS").InitState = ChInitState.Off;
            TheHdw.Digital.Pins(digPinList).Disconnect();
            digitalPins.Connect();

            //Body
            measPins.ForceI(0);
            measPins.Gate = tlOnOff.Off;
            forceFirst.ForceI(forceValueFirstPin, forceValueFirstPin);
            forceFirst.ClampVHi.Value = clampValueOfForceFirstPin;
            forceFirst.Gate = tlOnOff.On;
            forceSecond.Mode = tlDCVSMode.Voltage;
            forceSecond.CurrentRange.Value = clampValueOfForceSecondPin;
            forceSecond.VoltageRange.Value = forceValueSecondPin;
            forceSecond.Voltage.Value = forceValueSecondPin;
            double sinkFoldLimit = forceSecond.CurrentLimit.Sink.FoldLimit.Level.Max;
            forceSecond.CurrentLimit.Sink.FoldLimit.Level.Value = clampValueOfForceSecondPin > sinkFoldLimit ? sinkFoldLimit : clampValueOfForceSecondPin;
            forceSecond.CurrentLimit.Source.FoldLimit.Level.Value = clampValueOfForceSecondPin;
            forceSecond.Gate = true;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measFirst = measureFirst.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> measSecond = measureSecond.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = (measFirst - measSecond) / forceValueFirstPin;

            //PostBody
            digitalPins.Disconnect();
            digitalPins.Gate = tlOnOff.Off;
            TheHdw.Digital.Pins(digPinList).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceVal: forceValueFirstPin, ForceUnit: UnitType.Custom, CustomForceunit: "",
              ForceResults: tlLimitForceResults.Flow);
        }
        #endregion
    }
}

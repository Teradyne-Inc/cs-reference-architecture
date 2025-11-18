using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Resistance {

    [TestClass]
    public class Contact : TestCodeBase {

        /// <summary>
        /// Performs a resistance measurement by forcing two values of current and measuring two voltages.
        /// </summary>
        /// <param name="forcePin">Pin to force and measure.</param>
        /// <param name="forceFirstValue">First value to force.</param>
        /// <param name="forceSecondValue">Second value to force.</param>
        /// <param name="clampValueOfForcePin">Clamp Value of the force pin. May also set its range.</param>
        /// <param name="waitTime">Optional. The wait time after forcing.</param>
        #region Baseline
        [TestMethod]
        public void Baseline(PinList forcePin, double forceFirstValue, double forceSecondValue, double clampValueOfForcePin, double waitTime) {

            var ppmu = TheHdw.PPMU.Pins(forcePin);

            //PreBody
            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);
            TheHdw.Digital.Pins(forcePin).Disconnect();
            ppmu.Connect();

            //Body
            ppmu.ForceI(forceFirstValue, forceFirstValue);
            ppmu.ClampVHi.Value = ppmu.ClampVHi.Max;
            ppmu.ClampVLo.Value = clampValueOfForcePin;
            ppmu.Gate = tlOnOff.On;
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measFirst = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            ppmu.ForceI(forceSecondValue, forceSecondValue);
            TheHdw.SetSettlingTimer(waitTime);
            PinSite<double> measSecond = ppmu.Read(tlPPMUReadWhat.Measurements, 1, tlPPMUReadingFormat.Average).ToPinSite<double>();
            PinSite<double> resistance = (measFirst - measSecond) / (forceFirstValue - forceSecondValue);

            //PostBody
            ppmu.Gate = tlOnOff.Off;
            ppmu.Disconnect();
            TheHdw.Digital.Pins(forcePin).Connect();
            TheExec.Flow.TestLimit(ResultVal: resistance, ForceUnit: UnitType.Custom, CustomForceunit: "",
                ForceResults: tlLimitForceResults.Flow);
        }
        #endregion
    }
}

using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    public interface IPpmuPins : IPins<IPpmuPins> {

        /// <summary>
        /// Handle to legacy IG-XL object.
        /// </summary>
        public tlDriverPPMUPins HardwareApi { get; }

        /// <summary>
        /// The PPMU gate state.
        /// </summary>
        public IValuePerSite<bool> Gate { get; }

        /// <summary>
        /// The PPMU high clamp voltage.
        /// </summary>
        public IValuePerSiteRange<double> ClampVHi { get; }

        /// <summary>
        /// The PPMU low clamp voltage.
        /// </summary>
        public IValuePerSiteRange<double> ClampVLo { get; }

        /// <summary>
        /// The PPMU force voltage.
        /// </summary>
        public IValuePerSite<double> Voltage { get; }

        /// <summary>
        /// The PPMU force current.
        /// </summary>
        public IValuePerSite<double> Current { get; }

        /// <summary>
        /// Connects the specified DUT pins to the PPMU.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Connect(bool? gate = null);

        /// <summary>
        /// Disconnects the specified DUT pins from the PPMU.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Disconnect(bool? gate = null);

        /// <summary>
        /// Performs a single measurement (strobe) on the PPMU, to read back the value later.
        /// </summary>
        public void Strobe();

        /// <summary>
        /// Reads the PPMU measurement value, from a previous strobe.
        /// </summary>
        /// <returns>Returns the measurement value.</returns>
        public PinSite<double> Read();

        /// <summary>
        /// Performs measurement(s) on the PPMU, returning the average of measured value(s).
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples.</param>
        /// <returns>Returns an average value.</returns>
        public PinSite<double> Measure(int sampleSize = 1);

        /// <summary>
        /// Performs measurement(s) on the PPMU, returning the measured samples.
        /// </summary>
        /// <param name="sampleSize">The number of samples.</param>
        /// <returns>Returns the measured samples.</returns>
        public PinSite<Samples<double>> MeasureSamples(int sampleSize);

        /// <summary>
        /// Sets the Force current, with voltage clamps.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVHi">Optional. The voltage for voltage clamp high.</param>
        /// <param name="clampVLo">Optional. The voltage for voltage clamp low.</param>
        /// <param name="currentRange">Optional. The force current range.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceI(double forceCurrent, double? clampVHi = null, double? clampVLo = null, double? currentRange = null, bool? gate = null);

        /// <summary>
        /// Sets the Force current, and configures the meter to measure voltage.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVHi">The voltage for voltage clamp high.</param>
        /// <param name="clampVLo">The voltage for voltage clamp low.</param>
        /// <param name="forceCurrentRange">The force current range.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceISetMeterV(double forceCurrent, double clampVHi, double clampVLo, double forceCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the Force voltage, with current clamp.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="measureCurrentRange">Optional. The current range for measurement.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceV(double forceVoltage, double? measureCurrentRange = null, bool? gate = null);

        /// <summary>
        /// Sets the Force voltage, and configures the meter to measure current.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="measureCurrentRange">The current range for measurement.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceVSetMeterI(double forceVoltage, double measureCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the Force voltage, and configures the meter to measure voltage.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="measureCurrentRange">The current range for measurement.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceVSetMeterV(double forceVoltage, double measureCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the PPMU to high impedance mode.
        /// </summary>
        public void ForceHiZ();

        /// <summary>
        /// Configure ramp on PPMU.
        /// </summary>
        /// <param name="signalName">The name of the signal.</param>
        /// <param name="startingValue">The value to start at.</param>
        /// <param name="incrementValue">The value to increment by.</param>
        /// <param name="incrementPeriod">The period between each increment.</param>
        /// <param name="startDelay">The delay between the start and the first increment within the ramp.</param>
        /// <param name="incrementCount">The number of increments within the ramp.</param>
        public void ConfigureRamp(string signalName, double startingValue, double incrementValue, double incrementPeriod, double startDelay, int incrementCount);

        /// <summary>
        /// Run pattern asynchronous with the pre-configured ramp.
        /// </summary>
        /// <param name="patternName">The name of the pattern to run in sync with the ramp.</param>
        /// <param name="signalName">The name of the signal of the ramp.</param>
        /// <param name="timeout">The desired timeout period to wait while the PPMU sources.</param>
        public void RunPatternSyncRamp(string patternName, string signalName, double timeout);

        /// <summary>
        /// Configure pattern to control PPMU. Functionality exclusive to the UP5000.
        /// </summary>
        /// <param name="numSamplesPerStrobe">The number of samples per strobe.</param>
        /// <param name="readFormat">The read format for PPMU Capture.</param>
        public void ConfigurePatternControl(int numSamplesPerStrobe, tlPPMUPatternControlReadFormat readFormat);
    }
}

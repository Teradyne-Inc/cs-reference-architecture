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
    }
}

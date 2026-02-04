using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    public interface IDcvsPins : IPins<IDcvsPins> {

        /// <summary>
        /// Handle to legacy IG-XL object.
        /// </summary>
        public DriverDCVSPins HardwareApi { get; }

        /// <summary>
        /// Gets the DCVI meter properties.
        /// </summary>
        public IDcvsMeter Meter { get; }

        //public DriverDCVSCapture Capture { get; }
        //public DriverDCVSSource Source { get; }

        // Properties with per-site support

        /// <summary>
        /// The DCVS force or clamp voltage.
        /// </summary>
        public IValuePerSiteRange<double> Voltage { get; }

        /// <summary>
        /// The DCVS voltage range.
        /// </summary>
        public IValuePerSiteRange<double> VoltageRange { get; }

        /// <summary>
        /// The DCVS force or clamp current.
        /// </summary>
        public IValuePerSite<double> Current { get; }

        /// <summary>
        /// The DCVS current range.
        /// </summary>
        public IValuePerSiteRange<double> CurrentRange { get; }

        /// <summary>
        /// The DCVS gate state.
        /// </summary>
        public IValuePerSite<bool> Gate { get; }

        /// <summary>
        /// The DCVS force mode settings, e.g. Voltage, Current.
        /// </summary>
        public IValuePerSite<tlDCVSMode> Mode { get; }

        /// <summary>
        /// The DCVS bandwidth setting.
        /// </summary>
        public IValuePerSiteRange<double> Bandwidth { get; }    // dcvi nominalbandwidth & dcvs Bandwidthsetting

        /// <summary>
        /// Connects the specified DUT pins to the DCVS.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Connect(bool? gate = null);

        /// <summary>
        /// Disconnects the specified DUT pins from the DCVS.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Disconnect(bool? gate = null);

        /// <summary>
        /// Sets the Meter to current mode.
        /// </summary>
        /// <param name="currentRange">Optional. The current range for measurement.</param>
        /// <param name="filter">Optional. Specifies the filter bandwidth, which attenuates high-frequency noise.</param>
        /// <param name="forceCurrentRange">Optional. The current force range.</param>
        public void SetMeterI(double? currentRange = null, double? filter = null, double? forceCurrentRange = null);

        /// <summary>
        /// Sets the force and meter current ranges.
        /// </summary>
        /// <param name="forceRange">The force current range.</param>
        /// <param name="meterRange">The meter current range.</param>
        public void SetCurrentRanges(double forceRange, double meterRange);

        /// <summary>
        /// Sets the Meter to voltage mode.
        /// </summary>
        /// <param name="voltageRange">Optional. The voltage range for measurement.</param>
        /// <param name="filter">Optional. Specifies the filter bandwidth, which attenuates high-frequency noise.</param>
        public void SetMeterV(double? voltageRange = null, double? filter = null);

        /// <summary>
        /// Performs multiple measurements (strobes) on the DCVS, to read back the values later. 
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        public void Strobe(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Reads the DCVS measurement values, from previous strobes.
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples to average.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the average of those measured samples.</returns>
        public PinSite<double> Read(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Reads the DCVS measurement samples, from previous strobes.
        /// </summary>
        /// <param name="sampleSize">The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the measured samples.</returns>
        public PinSite<Samples<double>> ReadSamples(int sampleSize, double sampleRate = -1);

        /// <summary>
        /// Performs measurement(s) on the DCVS, returning the average of measured value(s).
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the average of those measured samples.</returns>
        public PinSite<double> Measure(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Performs measurement(s) on the DCVS, returning the measured samples.
        /// </summary>
        /// <param name="sampleSize">The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the measured samples.</returns>
        public PinSite<Samples<double>> MeasureSamples(int sampleSize, double sampleRate = -1);

        /// <summary>
        /// Sets the force current, changing its force mode.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVoltage">Optional. The voltage to clamp.</param>
        /// <param name="voltageRange">Optional. The voltage range.</param>
        /// <param name="currentRange">Optional. The current range for measurement.</param>
        /// <param name="setCurrentMode">Optional. To set the force mode to current.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceI(double forceCurrent, double? clampVoltage = null, double? currentRange = null, double? voltageRange = null, bool setCurrentMode = true, bool? gate = null);

        /// <summary>
        /// Sets the force voltage, changing its force mode.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="clampCurrent">Optional. The current to clamp.</param>
        /// <param name="voltageRange">Optional. The voltage range.</param>
        /// <param name="currentRange">Optional. The current range.</param>
        /// <param name="setVoltageMode">Optional. To set the force mode to voltage.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceV(double forceVoltage, double? clampCurrent = null, double? voltageRange = null, double? currentRange = null, bool setVoltageMode = false, bool? gate = null);

        /// <summary>
        /// Sets the Force voltage, and configures the meter to measure voltage.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="clampCurrent">The current to clamp.</param>
        /// <param name="measureVoltageRange">The voltage range for measurement.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceVSetMeterV(double forceVoltage, double clampCurrent, double measureVoltageRange, bool? gate = null);

        /// <summary>
        /// Sets the Force voltage, and configures the meter to measure current.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="clampCurrent">The current to clamp.</param>
        /// <param name="measureCurrentRange">The current range for measurement.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceVSetMeterI(double forceVoltage, double clampCurrent, double measureCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the Force current, and configures the meter to measure current.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVoltage">The voltage to clamp.</param>
        /// <param name="measureCurrentRange">The current range for measurement.</param>
        /// <param name="forceCurrentRange">The current range for forcing.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceISetMeterI(double forceCurrent, double clampVoltage, double measureCurrentRange, double forceCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the Force current, and configures the meter to measure voltage.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVoltage">The voltage to clamp.</param>
        /// <param name="forceCurrentRange">The current range for forcing.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceISetMeterV(double forceCurrent, double clampVoltage, double forceCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the DCVS to high impedance mode.
        /// </summary>
        /// <param name="clampValue">Optional. The voltage to clamp.</param>
        public void ForceHiZ(double? clampValue = null);
    }

    public interface IDcvsMeter {

        /// <summary>
        /// The DCVS meter current range.
        /// </summary>
        public IValuePerSiteRange<double> CurrentRange { get; }

        /// <summary>
        /// The DCVS meter filter setting.
        /// </summary>
        public IValuePerSiteRange<double> Filter { get; } // bypass missing?

        /// <summary>
        /// The DCVS meter voltage range.
        /// </summary>
        public IValuePerSiteRange<double> VoltageRange { get; } // bypass missing?

        /// <summary>
        /// The DCVS meter mode settings, e.g. Voltage, Current.
        /// </summary>
        public IValuePerSite<tlDCVSMeterMode> Mode { get; }
    }
}

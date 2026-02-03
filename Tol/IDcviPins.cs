using Teradyne.Igxl.Interfaces.Public;

namespace Tol {

    public interface IDcviPins : IPins<IDcviPins> {

        /// <summary>
        /// Handle to legacy IG-XL object.
        /// </summary>
        public DriverDCVIPins HardwareApi { get; }

        /// <summary>
        /// Gets the DCVI meter properties.
        /// </summary>
        public IDcviMeter Meter { get; }

        //public DriverDCVICapture Capture { get; }
        //public DriverDCVISource Source { get; }

        /// <summary>
        /// The DCVI force or clamp voltage.
        /// </summary>
        public IValuePerSite<double> Voltage { get; }

        /// <summary>
        /// The DCVI voltage range.
        /// </summary>
        public IValuePerSiteRange<double> VoltageRange { get; }

        /// <summary>
        /// The DCVI force or clamp current.
        /// </summary>
        public IValuePerSite<double> Current { get; }

        /// <summary>
        /// The DCVI current range.
        /// </summary>
        public IValuePerSiteRange<double> CurrentRange { get; }

        /// <summary>
        /// The DCVI gate state.
        /// </summary>
        public IValuePerSite<tlDCVGate> Gate { get; }

        /// <summary>
        /// The DCVI force mode settings, e.g. Voltage, Current.
        /// </summary>
        public IValuePerSite<tlDCVIMode> Mode { get; }

        /// <summary>
        /// The DCVI bandwidth setting.
        /// </summary>
        public IValuePerSiteRange<double> Bandwidth { get; }

        /// <summary>
        /// The DCVI positive compliance range.
        /// </summary>
        public IValuePerSiteRange<double> ComplianceRangePositive { get; }

        /// <summary>
        /// The DCVI negative compliance range.
        /// </summary>
        public IValuePerSiteRange<double> ComplianceRangeNegative { get; }

        /// <summary>
        /// Connects the specified DUT pins to the DCVI.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Connect(bool? gate = null);

        /// <summary>
        /// Disconnects the specified DUT pins from the DCVI.
        /// </summary>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void Disconnect(bool? gate = null);

        /// <summary>
        /// Sets the current and current range.
        /// </summary>
        /// <param name="current">The current to force.</param>
        /// <param name="currentRange">The current range for measurement.</param>
        public void SetCurrentAndRange(double current, double currentRange);

        /// <summary>
        /// Sets the meter to current mode.
        /// </summary>
        /// <param name="currentRange">Optional. The current range for measurement.</param>
        /// <param name="hardwareAverage">Optional. Samples to average in hardware.</param>
        /// <param name="filter">Optional. Specifies the filter bandwidth, which attenuates high-frequency noise.</param>
        public void SetMeterI(double? currentRange = null, int? hardwareAverage = null, double? filter = null);

        /// <summary>
        /// Sets the meter to voltage mode.
        /// </summary>
        /// <param name="voltageRange">Optional. The voltage range for measurement.</param>
        /// <param name="hardwareAverage">Optional. Samples to average in hardware.</param>
        /// <param name="filter">Optional. Specifies the filter bandwidth, which attenuates high-frequency noise.</param>
        public void SetMeterV(double? voltageRange = null, int? hardwareAverage = null, double? filter = null);

        /// <summary>
        /// Sets the voltage and voltage range.
        /// </summary>
        /// <param name="voltage">The voltage to force.</param>
        /// <param name="voltageRange">The voltage range for measurement.</param>
        public void SetVoltageAndRange(double voltage, double voltageRange);

        /// <summary>
        /// Performs measurement(s) (strobe) on the DCVI, to read back the value(s) later.
        /// </summary>
        /// <param name="sampleSize">Optional.The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        public void Strobe(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Reads the DCVI measurement value, from previous strobe.
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples to average.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the average of those measured samples.</returns>
        public PinSite<double> Read(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Reads the DCVI measurement samples, from previous strobe.
        /// </summary>
        /// <param name="sampleSize">The number of samples.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the measured samples.</returns>
        public PinSite<Samples<double>> ReadSamples(int sampleSize, double sampleRate = -1);

        /// <summary>
        /// Performs measurement(s) on the DCVI, returning the average of measured value(s).
        /// </summary>
        /// <param name="sampleSize">Optional. The number of samples to average.</param>
        /// <param name="sampleRate">Optional. The sample rate for the measurement, defaults to fastest sampling rate of first pin.</param>
        /// <returns>Returns the average of those measured samples.</returns>
        public PinSite<double> Measure(int sampleSize = 1, double sampleRate = -1);

        /// <summary>
        /// Performs measurement(s) on the DCVI, returning the measured samples.
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
        /// <param name="voltageRange">The voltage range.</param>
        /// <param name="currentRange">Optional. The current range for measurement.</param>
        /// <param name="setCurrentMode">Optional. To set the force mode to current.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceI(double forceCurrent, double? clampVoltage = null, double? currentRange = null, double? voltageRange = null, bool setCurrentMode = true,  bool? gate = null);

        /// <summary>
        /// Sets the force voltage, changing its force mode.
        /// </summary>
        /// <param name="forceVoltage">The voltage to force.</param>
        /// <param name="clampCurrent">Optional. The current for the clamp.</param>
        /// <param name="voltageRange">Optional. The voltage range for measurement.</param>
        /// <param name="currentRange">Optional. The current range.</param>
        /// <param name="setVoltageMode">Optional. To set the force mode to voltage.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceV(double forceVoltage, double? clampCurrent = null, double? voltageRange = null, double? currentRange = null, bool setVoltageMode = true, bool? gate = null);

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
        /// <param name="forceCurrentRange">The current range to force.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceISetMeterI(double forceCurrent, double clampVoltage, double measureCurrentRange, double forceCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the Force current, and configures the meter to measure voltage.
        /// </summary>
        /// <param name="forceCurrent">The current to force.</param>
        /// <param name="clampVoltage">The voltage to clamp.</param>
        /// <param name="measureVoltageRange">The voltage range for measurement.</param>
        /// <param name="forceCurrentRange">The current range to force.</param>
        /// <param name="gate">Optional. The gate state after setting.</param>
        public void ForceISetMeterV(double forceCurrent, double clampVoltage, double measureVoltageRange, double forceCurrentRange, bool? gate = null);

        /// <summary>
        /// Sets the DCVI to high impedance mode.
        /// </summary>
        public void ForceHiZ();
    }

    public interface IDcviMeter {

        /// <summary>
        /// The DCVI meter current range.
        /// </summary>
        public IValuePerSiteRange<double> CurrentRange { get; }

        /// <summary>
        /// The DCVI meter filter setting.
        /// </summary>
        public IValuePerSiteRange<double> Filter { get; } // bypass missing?

        /// <summary>
        /// The DCVI meter voltage range.
        /// </summary>
        public IValuePerSiteRange<double> VoltageRange { get; } // bypass missing?

        /// <summary>
        /// The DCVI meter mode setting, e.g. Voltage, Current.
        /// </summary>
        public IValuePerSite<tlDCVIMeterMode> Mode { get; }

        /// <summary>
        /// The DCVI meter hardware averaging setting.
        /// </summary>
        public IValuePerSiteRange<double> HardwareAverage { get; }
    }
}

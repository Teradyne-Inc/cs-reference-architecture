using System;
using System.Collections.Generic;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace Csra.Interfaces {

    /// <summary>
    /// The interface for the EntryPoint.
    /// </summary>
    public interface ILib {

        /// <summary>
        /// The accessor for the Validate branch.
        /// </summary>
        public IValidate Validate { get; }

        /// <summary>
        /// The accessor for the Setup branch.
        /// </summary>
        public ISetup Setup { get; }

        /// <summary>
        /// The accessor for the Execute branch.
        /// </summary>
        public IExecute Execute { get; }

        /// <summary>
        /// The accessor for the Acquire branch.
        /// </summary>
        public IAcquire Acquire { get; }

        /// <summary>
        /// The accessor for the Datalog branch.
        /// </summary>
        public IDatalog Datalog { get; }

        public interface IValidate {

            /// <summary>
            /// Checks if a numeric value is within a range (including).
            /// </summary>
            /// <typeparam name="T">The type specified for the provided parameters.</typeparam>
            /// <param name="value">The value to be checked against the provided upper and lower threshold.</param>
            /// <param name="from">The lower threshold.</param>
            /// <param name="to">The upper threshold.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the `value` is within the provided range; otherwise, <see langword="false"/>.</returns>
            public bool InRange<T>(T value, T from, T to, string argumentName) where T : IComparable<T>;

            /// <summary>
            /// Checks if a numeric value is greater or equal to a bound.
            /// </summary>
            /// <typeparam name="T">The type specified for the provided parameters.</typeparam>
            /// <param name="value">The value to be checked against the provided boundary.</param>
            /// <param name="boundary">The boundary that the value must be greater than or equal to.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the `value` is greater than or equal to the provided boundary; otherwise, <see langword="false"/>.</returns>
            public bool GreaterOrEqual<T>(T value, T boundary, string argumentName) where T : IComparable<T>;

            /// <summary>
            /// Checks if a numeric value is greater than the provided boundary.
            /// </summary>
            /// <typeparam name="T">The type specified for the provided parameters.</typeparam>
            /// <param name="value">The value to be checked against the provided boundary.</param>
            /// <param name="boundary">The boundary that the value must be greater than.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the 'value' is greater than the provided boundary; otherwise, <see langword="false"/>.</returns>
            public bool GreaterThan<T>(T value, T boundary, string argumentName) where T : IComparable<T>;

            /// <summary>
            /// Checks if a numeric value is less or equal to a bound.
            /// </summary>
            /// <typeparam name="T">The type specified for the provided parameters.</typeparam>
            /// <param name="value">The value to be checked against the provided boundary.</param>
            /// <param name="boundary">The boundary that the value must be greater than or equal to.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the `value` is less than or equal to the provided boundary; otherwise, <see langword="false"/>.</returns>
            public bool LessOrEqual<T>(T value, T boundary, string argumentName) where T : IComparable<T>;

            /// <summary>
            /// Checks if a numeric value is less than the provided boundary.
            /// </summary>
            /// <typeparam name="T">The type specified for the provided parameters.</typeparam>
            /// <param name="value">The value to be checked against the provided boundary.</param>
            /// <param name="boundary">The boundary that the value must be less than.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the `value` is less than the provided boundary; otherwise, <see langword="false"/>.</returns>
            public bool LessThan<T>(T value, T boundary, string argumentName) where T : IComparable<T>;

            /// <summary>
            /// Checks for valid pattern spec and creates patternInfo object. 
            /// </summary>
            /// <param name="pattern">The pattern file to check.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="patternInfo">A new <see cref="PatternInfo"/> object containing the provided pattern.</param>
            /// <param name="threading">Optional. Indicate whether threading should be used. Validation will fail if threading is not supported by the pattern.</param>
            /// <returns><see langword="true"/> if the `pattern` exists and creates the new PatternInfo() object; otherwise, <see langword="false"/>.</returns>
            public bool Pattern(Pattern pattern, string argumentName, out PatternInfo patternInfo, bool threading = true);

            /// <summary>
            /// Checks for C#RA supported pin spec and creates the object.
            /// </summary>
            /// <param name="pinlist">The pinlist to be checked.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="pins">A new <see cref="Csra.Pins"/> object containing all of the resovled pins.</param>
            /// <returns><see langword="true"/> if the `pinList` was comprised of valid pins and creates the Pins() object; otherwise, <see langword="false"/>.
            /// </returns>
            public bool Pins(PinList pinlist, string argumentName, out Pins pins);

            /// <summary>
            /// Checks if the provided string can be parsed to the specified enum type and create the enum value.
            /// </summary>
            /// <typeparam name="T">The existing Enumeration to be parsed.</typeparam>
            /// <param name="value">The string to parse for. Case-Insensitive, specify only the enum member part.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="enumValue">The enumeration value to output in the event that the provided value was successfully parsed in the
            /// provided enumeration.</param>
            /// <returns><see langword="true"/> if the `value` was found within the provided Enumeration and successfully parsed; otherwise,
            /// <see langword="false"/>.</returns>
            public bool Enum<T>(string value, string argumentName, out T enumValue) where T : struct, Enum;

            /// <summary>
            /// Checks multi-condition validity and creates the data array.
            /// </summary>
            /// <typeparam name="T">The target (output) type of the parser function.</typeparam>
            /// <param name="csv">Comma separated values to split and parse.</param>
            /// <param name="parser">Delegate used to parse comma separated list.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="conditions">Output Array of parsed values sourced from the provided string.</param>
            /// <param name="referenceCount">Optional. If specified, the reference count to verify. Will report an error if the resulting array size is >1
            /// (SingleCondition) and does not match (MultiCondition).</param>
            /// <returns><see langword="true"/> if the output array was successfully created; otherwise, <see langword="false"/>.</returns>
            public bool MultiCondition<T>(string csv, Func<string, T> parser, string argumentName, out T[] conditions, int? referenceCount = null);

            /// <summary>
            /// Checks multi-condition validity and creates the data array.
            /// </summary>
            /// <typeparam name="T">The target (output) enum-type of the parser function.</typeparam>
            /// <param name="csv">Comma separated values to split and parse. Case-Insensitive, specify only the enum member part.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="conditions">Output Array of parsed values sourced from the provided string.</param>
            /// <param name="referenceCount">Optional. If specified, the reference count to verify. Will report an error if the resulting array size is >1
            /// (SingleCondition) and does not match (MultiCondition).</param>
            /// <returns><see langword="true"/> if the output array was successfully created; otherwise, <see langword="false"/>.</returns>
            public bool MultiCondition<TEnum>(string csv, string argumentName, out TEnum[] conditions, int? referenceCount = null) where TEnum : struct, Enum;

            /// <summary>
            /// Checks for valid method handle spec and creates the object.
            /// </summary>
            /// <typeparam name="T">The target delegate and it's accompanying parameter types.</typeparam>
            /// <param name="fullyQualifiedName">Fully qualified name of the method to be checked.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <param name="method">Delegate to be created if the fullyQualifiedName is found to be a valid method handle spec.</param>
            /// <returns><see langword="true"/> if the `fullyqualifiedName` was found to match an existing method and the new delegate was created;
            /// otherwise, <see langword="false"/>.</returns>
            public bool MethodHandle<T>(string fullyQualifiedName, string argumentName, out MethodHandle<T> method) where T : Delegate;

            /// <summary>
            /// Checks for whether `condition` == <see langword="true"/>.
            /// </summary>
            /// <param name="condition">Condition to be checked.</param>
            /// <param name="problemReasonResolutionMessage">Validation failure message describing the `problem`, `reason`, and `resolution`.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            /// <returns><see langword="true"/> if the condition == <see langword="true"/>; Otherwise, <see langword="false"/>.</returns>
            public bool IsTrue(bool condition, string problemReasonResolutionMessage, string argumentName);

            /// <summary>
            /// Raises an unconditional validation error.
            /// </summary>
            /// <param name="problemReasonResolutionMessage">Validation failure message clearly describing the problem, reason, and resolution message.</param>
            /// <param name="argumentName">The argument name used to indicate to IG-XL which test instance parameter failed.</param>
            public void Fail(string problemReasonResolutionMessage, string argumentName);

            /// <summary>
            /// Validate arguments within Test Method Templates.
            /// </summary>
            /// <param name="pins">Pins parameter to be validated.</param>
            /// <param name="gate">Optional. Gate parameter to be validated.</param>
            /// <param name="mode">Optional. Mode parameter to be validated.</param>
            /// <param name="voltage">Optional. Voltage parameter to be validated.</param>
            /// <param name="voltageAlt">Optional. VoltageAlt parameter to be validated.</param>
            /// <param name="current">Optional. Current parameter to be validated.</param>
            /// <param name="voltageRange">Optional. Voltage Range parameter to be validated.</param>
            /// <param name="currentRange">Optional. Current Range parameter to be validated.</param>
            /// <param name="forceBandwidth">Optional. Force Bandwidth parameter to be validated.</param>
            /// <param name="meterMode">Optional. Meter Mode parameter to be validated.</param>
            /// <param name="meterVoltageRange">Optional. Meter Voltage Range parameter to be validated.</param>
            /// <param name="meterCurrentRange">Optional. Meter Current Range parameter to be validated.</param>
            /// <param name="meterBandwidth">Optional. Meter Bandwidth parameter to be validated.</param>
            /// <param name="sourceFoldLimit">Optional. Source Fold Limit parameter to be validated.</param>
            /// <param name="sinkFoldLimit">Optional. Sink Fold Limit parameter to be validated.</param>
            /// <param name="sourceOverloadLimit">Optional. Source Overload Limit parameter to be validated.</param>
            /// <param name="sinkOverloadLimit">Optional. Sink Overload Limit parameter to be validated.</param>
            /// <param name="voltageAltOutput">Optional. Voltage Alt Output parameter to be validated.</param>
            /// <param name="bleederResistor">Optional. Bleeder Resistor parameter to be validated.</param>
            /// <param name="complianceBoth">Optional. Compliance Both parameter to be validated.</param>
            /// <param name="compliancePositive">Optional. Compliance Positive parameter to be validated.</param>
            /// <param name="complianceNegative">Optional. Compliance Negative parameter to be validated.</param>
            /// <param name="clampHiV">Optional. Clamp High V parameter to be validated.</param>
            /// <param name="clampLoV">Optional. Clamp Low V parameter to be validated.</param>
            /// <param name="highAccuracy">Optional. High Accuracy parameter to be validated.</param>
            /// <param name="settlingTime">Optional. Settling Time parameter to be validated.</param>
            /// <param name="hardwareAverage">Optional. Hardware Average parameter to be validated.</param>
            public void Dc(Pins pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
                double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null,
                Measure? meterMode = null, double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null,
                double? sourceFoldLimit = null, double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null,
                bool? voltageAltOutput = null, bool? bleederResistor = null, double? complianceBoth = null, double? compliancePositive = null,
                double? complianceNegative = null, double? clampHiV = null, double? clampLoV = null, bool? highAccuracy = null, double? settlingTime = null,
                double? hardwareAverage = null);
        }

        /// <summary>
        /// The interface for the Setup branch.
        /// </summary>
        public interface ISetup {

            /// <summary>
            /// The accessor for the Dc branch.
            /// </summary>
            public IDc Dc { get; }

            /// <summary>
            /// The accessor for the Digital branch.
            /// </summary>
            public IDigital Digital { get; }

            /// <summary>
            /// The accessor for the LevelsAndTiming branch.
            /// </summary>
            public ILevelsAndTiming LevelsAndTiming { get; }

            /// <summary>
            /// The interface for the LevelsAndTiming branch.
            /// </summary>
            public interface ILevelsAndTiming {

                /// <summary>
                /// Apply Connections, Levels and Timing, in either powered or unpowered mode.
                /// </summary>
                /// <param name="connectAllPins">Optional. If true: Connect all pins.</param>
                /// <param name="unpowered">Optional. If true: power down instruments and power supplies before connecting all pins.</param>
                /// <param name="levelRampSequence">Optional. If true: will ramp all levels with a predefined slew rate and sequence.</param>
                public void Apply(bool connectAllPins = false, bool unpowered = false, bool levelRampSequence = false);

                /// <summary>
                /// Apply Connections, Level, Timing and set init states, in either powered or unpowered mode.
                /// </summary>
                /// <param name="connectAllPins">Optional. If true: Connect all pins.</param>
                /// <param name="unpowered">Optional. If true: power down instruments and power supplies before connecting all pins.</param>
                /// <param name="levelRampSequence">Optional. If true: will ramp all levels with a predefined slew rate and sequence.</param>
                /// <param name="initPinsHi">Optional. Pin or pingroup to initialize to drive state high.</param>
                /// <param name="initPinsLo">Optional. Pin or pingroup to initialize to drive state low.</param>
                /// <param name="initPinsHiZ">Optional. Pin or pingroup to initialize to drive state tri-state.</param>
                public void ApplyWithPinStates(bool connectAllPins = false, bool unpowered = false, bool levelRampSequence = false,
                    Pins initPinsHi = null, Pins initPinsLo = null, Pins initPinsHiZ = null);

            }

            /// <summary>
            /// The interface for the Dc branch.
            /// </summary>
            public interface IDc {

                /// <summary>
                /// Connects all power and digital pins from level context.
                /// </summary>
                public void ConnectAllPins();

                /// <summary>
                /// Connects and optionally gates on/off the pins depending on its instrument feature (PPMU, DCVI, DCVS,...).
                /// </summary>
                /// <param name="pins">The pins to connect.</param>
                /// <param name="gateOn">Optional. Default no gate change, True for gate on the pins after connecting.</param>
                public void Connect(Pins pins, bool gateOn = false);

                /// <summary>
                /// Disconnects and optionally gates on/off the pins depending on its instrument feature (PPMU, DCVI, DCVS,...).
                /// It will disconnect in HiZ mode rather then of forcing 0V or 0A on VIs.
                /// </summary>
                /// <param name="pins">The pins to disconnect.</param>
                /// <param name="gateOff">Optional. Default gate off (HiZ) the pins before disconnecting, False no gate change.</param>
                public void Disconnect(Pins pins, bool gateOff = true);

                /// <summary>
                /// Sets the force current, force voltage or high impedance of the pins.
                /// </summary>
                /// <param name="pins">The pins to force.</param>
                /// <param name="mode">The mode for forcing (e.g., Voltage or Current).</param>
                /// <param name="forceValue">The value to force.</param>
                /// <param name="forceRange">The range for force value.</param>
                /// <param name="clampValue">When forcing Voltage it sets the current limit and when forcing Current it sets the voltage range.</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void Force(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, bool gateOn = true);

                /// <summary>
                /// Sets the force current, force voltage or high impedance of each element in pinGroups.
                /// </summary>
                /// <param name="pinGroups">Array of pin or pin groups.</param>
                /// <param name="modes">Array of the mode for each pin or pin group.</param>
                /// <param name="forceValues">Array of force values for each pin or pin group.</param>
                /// <param name="forceRanges">Array of force ranges for each pin or pin group.</param>
                /// <param name="clampValues">Array of clamp values for each pin or pin group.</param>
                /// <param name="gateOn">Optional. Array of gate state for each pin or pin group, default gate on for all pin or pin group.</param>
                public void Force(Pins[] pinGroups, TLibOutputMode[] modes, double[] forceValues, double[] forceRanges, double[] clampValues,
                    bool[] gateOn = null);

                /// <summary>
                /// Sets the Force current of a DC instrument. Assumes the instrument was already setup to the right modes.
                /// </summary>
                /// <param name="pins">The pins to force the current.</param>
                /// <param name="forceCurrent">The current to force.</param>
                /// <param name="clampHiV">Optional. Sets the voltage for voltage clamp high.</param>
                /// <param name="clampLoV">Optional. Sets the voltage for voltage clamp low.</param>
                /// <param name="outputModeCurrent">Optional. Sets to true to switch to force current mode (if the mode was not previously set).</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void ForceI(Pins pins, double forceCurrent, double? clampHiV = null, double? clampLoV = null, bool outputModeCurrent = false,
                    bool gateOn = true);

                /// <summary>
                /// Sets the Force Current and the range of a DC instrument. Assuming that the instrument was already setup for remaining parameters.
                /// </summary>
                /// <param name="pins">The pins to force the current.</param>
                /// <param name="forceCurrent">The current to force.</param>
                /// <param name="clampHiV">Sets the voltage for voltage clamp high.</param>
                /// <param name="currentRange">Expected current to set the current range.</param>
                /// <param name="voltageRange">Optional. Expected voltage to set the voltage range or to program the voltage for DCVS.</param>
                /// <param name="outputModeCurrent">Optional. Sets to true to switch to force current mode (if the mode was not previously set).</param>
                /// <param name="clampLoV">Optional. Sets the voltage for voltage clamp low.</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void ForceI(Pins pins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange = null,
                    bool outputModeCurrent = false, bool gateOn = true, double? clampLoV = null);

                /// <summary>
                /// Programs the force Voltage of a DC instrument. Simplest Method: It assumes the instrument is already in the right mode (FI,FV) and required
                /// ranges.
                /// </summary>
                /// <param name="pins">The pins to force the voltage.</param>
                /// <param name="forceVoltage">The force voltage that will be set.</param>
                /// <param name="clampCurrent">Optional. Current clamp value.</param>
                /// <param name="outputModeVoltage">Optional. Sets to true to switch to force voltage mode (if the mode was not previously set).</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void ForceV(Pins pins, double forceVoltage, double? clampCurrent = null, bool outputModeVoltage = false, bool gateOn = true);

                /// <summary>
                /// Programs the force Voltage, measure range and many other parameters of a DC instrument - Advanced method with additional parameters.
                /// </summary>
                /// <param name="pins">The pins to force the voltage.</param>
                /// <param name="forceVoltage">The force voltage value.</param>
                /// <param name="clampCurrent">Current clamp value.</param>
                /// <param name="voltageRange">Expected voltage to set the Voltage range, if required, else use the forceVoltage.</param>
                /// <param name="currentRange">Optional. Expected current to set the current range.</param>
                /// <param name="outputModeVoltage">Optional. Sets to true to switch to force voltage mode (if the mode was not previously set).</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void ForceV(Pins pins, double forceVoltage, double clampCurrent, double voltageRange, double? currentRange = null,
                    bool outputModeVoltage = false, bool gateOn = true);

                /// <summary>
                /// Sets to High Impedance mode.
                /// </summary>
                /// <param name="pins">The pins to set in HiZ.</param>
                public void ForceHiZ(Pins pins);

                /// <summary>
                /// Sets the measurement interface of the instruments DCVI and DCVS.
                /// </summary>
                /// <param name="pins">The pins to set meter parameters.</param>
                /// <param name="meterMode">Set the mode to measure Voltage or Current.</param>
                /// <param name="rangeValue">Current or Voltage range depending on the selected mode.</param>
                /// <param name="filterValue">Optional. Sets the filter value.</param>
                /// <param name="hardwareAverage">Optional. Sets the hardware average for the specified DCVI pins.</param>
                /// <param name="outputRangeValue">Optional. Current range for DCVS when you want to set the current mode - for other cases, ignore
                /// this.</param>
                public void SetMeter(Pins pins, Measure meterMode, double rangeValue, double? filterValue = null, int? hardwareAverage = null,
                    double? outputRangeValue = null);

                /// <summary>
                /// Sets the measurements interface of the instruments DCVI and DCVS.
                /// </summary>
                /// <param name="pinGroups">Array of pin or pin groups.</param>
                /// <param name="meterModes">Array of settings measurements mode voltage and current.</param>
                /// <param name="rangeValues">Array of current and voltage range depending on the selected modes.</param>
                /// <param name="filterValues">Optional. Array of filter values.</param>
                /// <param name="hardwareAverages">Optional. Array of hardware average for the specified DCVI pins.</param>
                /// <param name="outputRangeValues">Optional. Array of current range for DCVS when you want to set the current mode - for other cases,
                /// ignore this.
                /// </param>
                public void SetMeter(Pins[] pinGroups, Measure[] meterModes, double[] rangeValues, double[] filterValues = null, int[] hardwareAverages = null,
                    double[] outputRangeValues = null);

                /// <summary>
                /// Programs the force and the meter's measure parameters interface for the dc instruments.
                /// </summary>
                /// <param name="pins">The pins to set force and meter parameters.</param>
                /// <param name="mode">Set the output mode to TlibOutputMode Voltage, Current or HiZ.</param>
                /// <param name="forceValue">Force voltage or current value.</param>
                /// <param name="forceRange">Voltage or current to set the force range.</param>
                /// <param name="clampValue">Current or voltage clamp value. Note: For PPMU it programs either clampVHi or clampVLo depending if sourcing or
                /// sinking current.</param>
                /// <param name="meterMode">Set the meter's measure mode to measure voltage or current.</param>
                /// <param name="measureRange">Set the meter's measure range to the expected current or voltage.</param>
                /// <param name="gateOn">Optional. Default gate on the pins after after the settings,False no gate change.</param>
                public void SetForceAndMeter(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode,
                    double measureRange, bool gateOn = true);

                /// <summary>
                /// Selectively program or modify any DC instrument parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="parameters">The object through which each parameter can be set.</param>
                public void Modify(Pins pins, DcParameters parameters);

                /// <summary>
                /// Selectively program or modify any DC instrument parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="gate">Optional. Sets the gate.</param>
                /// <param name="mode">Optional. Sets the operating mode.</param>
                /// <param name="voltage">Optional. Sets the output voltage.</param>
                /// <param name="voltageAlt">Optional. Sets the alternate output voltage.</param>
                /// <param name="current">Optional. Sets the output current.</param>
                /// <param name="voltageRange">Optional. Sets the voltage range.</param>
                /// <param name="currentRange">Optional. Sets the current range.</param>
                /// <param name="forceBandwidth">Optional. Sets the output compensation bandwidth.</param>
                /// <param name="meterMode">Optional. Sets the meter mode.</param>
                /// <param name="meterVoltageRange">Optional. Sets the meter voltage range.</param>
                /// <param name="meterCurrentRange">Optional. Sets the meter current range.</param>
                /// <param name="meterBandwidth">Optional. Sets the meter filter.</param>
                /// <param name="sourceFoldLimit">Optional. Sets the source fold limit.</param>
                /// <param name="sinkFoldLimit">Optional. Sets the sink fold limit.</param>
                /// <param name="sourceOverloadLimit">Optional. Sets the source overload limit.</param>
                /// <param name="sinkOverloadLimit">Optional. Sets the sink overload limit.</param>
                /// <param name="voltageAltOutput">Optional. Sets the output DAC used to force voltage (true for alternate or false for main).</param>
                /// <param name="bleederResistor">Optional. Sets the bleeder resistor�s connection state.</param>
                /// <param name="complianceBoth">Optional. Sets both compliance ranges.</param>
                /// <param name="compliancePositive">Optional. Sets the positive compliance range.</param>
                /// <param name="complianceNegative">Optional. Sets the negative compliance range.</param>
                /// <param name="clampHiV">Optional. Sets the high voltage clamp value.</param>
                /// <param name="clampLoV">Optional. Sets the low voltage clamp value.</param>
                /// <param name="highAccuracy">Optional. Sets the enabled state of the high accuracy measure voltage.</param>
                /// <param name="settlingTime">Optional. Sets the required additional settling time for the high accuracy measure voltage mode.</param>
                /// <param name="hardwareAverage">Optional. Sets the meter hardware average value.</param>
                public void Modify(Pins pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
                    double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null,
                    Measure? meterMode = null, double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null,
                    double? sourceFoldLimit = null, double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null,
                    bool? voltageAltOutput = null, bool? bleederResistor = null, double? complianceBoth = null, double? compliancePositive = null,
                    double? complianceNegative = null, double? clampHiV = null, double? clampLoV = null, bool? highAccuracy = null, double? settlingTime = null,
                    double? hardwareAverage = null);

            }

            /// <summary>
            /// The interface for the Digital branch.
            /// </summary>
            public interface IDigital {

                /// <summary>
                /// Connect digital pins to the digital driver and comparator
                /// </summary>
                /// <param name="pins">Pins to be connected, must contain digital pins</param>
                public void Connect(Pins pins);

                /// <summary>
                /// Disconnect digital pins from the digital driver and comparator
                /// </summary>
                /// <param name="pins">Pins to be disconnected, must contain digital pins</param>
                public void Disconnect(Pins pins);

                /// <summary>
                /// Configures the digital instrument frequency counter.
                /// </summary>
                /// <param name="pins">Digital pin(s) to be measured.</param>
                /// <param name="measureWindow">Time to measure the frequency.</param>
                /// <param name="eventSource">The frequency counters comparator threshold.</param>
                /// <param name="eventSlope">The frequency counters event slope.</param>
                public void FrequencyCounter(Pins pins, double measureWindow, FreqCtrEventSrcSel eventSource, FreqCtrEventSlopeSel eventSlope);

                /// <summary>
                /// Configures the tester to read all the vector data using HRAM.
                /// </summary>
                public void ReadAll();

                /// <summary>
                /// Configures the tester to read the failing vector data using HRAM.
                /// </summary>
                public void ReadFails();

                /// <summary>
                /// Configures the tester to read the data from vectors containing the STV statement.
                /// </summary>
                public void ReadStoredVectors();

                /// <summary>
                /// Configures the tester for HRAM read back.
                /// </summary>
                /// <param name="captureLimit">Maximum number of vectors to be captured</param>
                /// <param name="captureType">Cycle type to be captured by HRAM, options are all, fail or stv</param>
                /// <param name="triggerType">Type of trigger for capture cycles, optiona are fail, first or never</param>
                /// <param name="waitForEvent">Sets whether the trigger waits for a cycle, vector or loop event</param>
                /// <param name="preTriggerCycleCount">The number of cycles to capture before the trigger cycle.</param>
                /// <param name="stopOnFull">Whether to stop the HRAM once it is full.</param>
                public void ReadHram(int captureLimit, CaptType captureType, TrigType triggerType, bool waitForEvent, int preTriggerCycleCount);

                /// <summary>
                /// Selectively program or modify any digital instrument pins parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="parameters">The object through which each parameter can be set.</param>
                public void ModifyPins(Pins pins, DigitalPinsParameters parameters);

                /// <summary>
                /// Selectively program or modify any digital instrument Pins parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="alarmType">Optional. Sets the alarm type for the specified pins.</param>
                /// <param name="alarmBehavior">Optional. Sets the alarm behavior for the specified pins.</param>
                /// <param name="disableCompare">Optional. Disables the comparators for the specified pins</param>
                /// <param name="disableDrive">Optional. Disables the drivers for the specified pins</param>
                /// <param name="initState">Optional. Sets the initial state of the pins</param>
                /// <param name="startState">Optional. Sets the start state of the pins</param>
                /// <param name="calibrationExcluded">Optional. Sets the specified pins to be excluded from job dependent calibration</param>
                /// <param name="calibrationHighAccuracy">Optional. Enables or disables calibration high accuracy mode for the specified pins	</param>
                public void ModifyPins(Pins pins, tlHSDMAlarm? alarmType = null, tlAlarmBehavior? alarmBehavior = null, bool? disableCompare = null,
                    bool? disableDrive = null, ChInitState? initState = null, ChStartState? startState = null, bool? calibrationExcluded = null,
                    bool? calibrationHighAccuracy = null);

                /// <summary>
                /// Selectively program or modify any digital instrument pins levels parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="parameters">The object through which each parameter can be set.</param>
                public void ModifyPinsLevels(Pins pins, DigitalPinsLevelsParameters parameters);

                /// <summary>
                /// Selectively program or modify any digital instrument pins levels parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="differentialLevelsType">Optional. Sets the differential levels type for the specified pins</param>
                /// <param name="differentialLevelsValue">Optional. Sets the specified differential pin level type for the specified pins</param>
                /// <param name="differentialLevelsValuesType">Optional. Sets the differential levels values type for the specified pins</param>
                /// <param name="differentialLevelsValues">Optional. Sets the specified differential pin levels values type for the specified pins</param>
                /// <param name="levelsDriverMode">Optional. Sets the driver mode for the specified pins</param>
                /// <param name="levelsType">Optional. Sets the level type for the specified pins</param>
                /// <param name="levelsValue">Optional. Sets the value for the specified level type on the specified pins</param>
                /// <param name="levelsValuePerSite">Optional. Sets the value for the specified level type for the specified pins on each site</param>
                /// <param name="levelsValues">Optional. Sets the value for the specified level value for each specified site and each specified pin</param>
                public void ModifyPinsLevels(Pins pins, ChDiffPinLevel? differentialLevelsType = null, double? differentialLevelsValue = null,
                    TLibDiffLvlValType[] differentialLevelsValuesType = null, double[] differentialLevelsValues = null, tlDriverMode? levelsDriverMode = null,
                    ChPinLevel? levelsType = null, double? levelsValue = null, SiteDouble levelsValuePerSite = null, PinListData levelsValues = null);

                /// <summary>
                /// Selectively program or modify any digital instrument pins timing parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="parameters">The object through which each parameter can be set.</param>
                public void ModifyPinsTiming(Pins pins, DigitalPinsTimingParameters parameters);

                /// <summary>
                /// Selectively program or modify any digital instrument pins timing parameter.
                /// </summary>
                /// <param name="pins">The pins to set.</param>
                /// <param name="timingClockOffset">Optional. Sets the offset value between a DQS bus and a DUT clock in a DDR Protocol Aware test program for
                /// the specified pins</param>
                /// <param name="timingClockPeriod">Optional. Sets the current value for the period for the specified clock pins</param>
                /// <param name="timingDisableAllEdges">Optional. Disables all edges (drive and compare) for the specified pins</param>
                /// <param name="timingEdgeSet">Optional. Sets the edgeset name for the specified pins</param>
                /// <param name="timingEdgeVal">Optional. Sets the timing edge for the specified pins</param>
                /// <param name="timingEdgeEnabled">Optional. Sets the enabled state for the specified pins and timing edge</param>
                /// <param name="timingEdgeTime">Optional. Sets the edge value for the specified pins and timing edge</param>
                /// <param name="timingRefOffset">Optional. Sets the offset value between the specified source synchronous reference (clock) pin and its data
                /// pins</param>
                /// <param name="timingSetup1xDiagnosticCapture">Optional. Sets up special dual-bit diagnostic capture in CMEM fail capture (LFVM) memory using
                /// the 1X
                /// pin setup for the specified pins and Time Sets sheet name</param>
                /// <param name="timingSrcSyncDataDelay">Optional. Sets the strobe reference data delay for individual source synchronous data pins</param>
                /// <param name="timingOffsetType">Optional. Sets the timing offset type for the specified pins</param>
                /// <param name="timingOffsetValue">Optional. Sets the timing offset value for the specified pins</param>
                /// <param name="timingOffsetEnabled">Optional. Sets the timing offset enabled state for the specified pins</param>
                /// <param name="timingOffsetSelectedPerSite">Optional. Sets the active offset index value for the specified pins on each site</param>
                /// <param name="timingOffsetValuePerSiteIndex">Optional. Set the timing offset index value. The valid index range is 0-7</param>
                /// <param name="timingOffsetValuePerSiteValue">Optional. Sets the current value for the offset at a specific index location that is to be
                /// applied to the timing values for the specified pins on each site</param>
                /// <param name="autoStrobeEnabled">Optional. Enable state of the AutoStrobe engine for the specified pins</param>
                /// <param name="autoStrobeNumSteps">Optional. Sets the number of steps on the AutoStrobe engines for the specified pins</param>
                /// <param name="autoStrobeSamplesPerStep">Optional. Sets the number of samples per step on the AutoStrobe engines for the specified
                /// pins</param>
                /// <param name="autoStrobeStartTime">Optional. Sets the start time on the AutoStrobe engines for the specified pins</param>
                /// <param name="autoStrobeStepTime">Optional. Sets the step time on the AutoStrobe engines for the specified pins</param>
                /// <param name="freeRunningClockEnabled">Optional. Sets the enable state of the free-running clock for the specified pins</param>
                /// <param name="freeRunningClockFrequency">Optional. Sets the frequency of the free-running clock for the specified pins</param>
                /// <param name="freqCtrEnable">Optional. Sets the frequency counter’s enable state for the specified pins</param>
                /// <param name="freqCtrEventSlope">Optional. Sets the frequency counter’s event slope for the specified pins</param>
                /// <param name="freqCtrEventSource">Optional. Sets the frequency counter’s event source for the specified pins</param>
                /// <param name="freqCtrInterval">Optional. Sets the duration of time to capture the frequency counter data for the specified pins</param>
                public void ModifyPinsTiming(Pins pins, double? timingClockOffset = null, double? timingClockPeriod = null,
                    bool? timingDisableAllEdges = null, string timingEdgeSet = null, chEdge? timingEdgeVal = null, bool? timingEdgeEnabled = null,
                    double? timingEdgeTime = null, double? timingRefOffset = null, string timingSetup1xDiagnosticCapture = null,
                    double? timingSrcSyncDataDelay = null, tlOffsetType? timingOffsetType = null, double? timingOffsetValue = null,
                    bool? timingOffsetEnabled = null, SiteLong timingOffsetSelectedPerSite = null, int? timingOffsetValuePerSiteIndex = null,
                    SiteDouble timingOffsetValuePerSiteValue = null, AutoStrobeEnableSel? autoStrobeEnabled = null, int? autoStrobeNumSteps = null,
                    int? autoStrobeSamplesPerStep = null, double? autoStrobeStartTime = null, double? autoStrobeStepTime = null,
                    bool? freeRunningClockEnabled = null, double? freeRunningClockFrequency = null, FreqCtrEnableSel? freqCtrEnable = null,
                    FreqCtrEventSlopeSel? freqCtrEventSlope = null, FreqCtrEventSrcSel? freqCtrEventSource = null, double? freqCtrInterval = null);
            }
        }

        /// <summary>
        /// The interface for the Execute branch.
        /// </summary>
        public interface IExecute {

            /// <summary>
            /// The accessor for the Digital branch.
            /// </summary>
            public IDigital Digital { get; }

            /// <summary>
            /// The accessor for the ScanNetwork branch.
            /// </summary>
            public IScanNetwork ScanNetwork { get; }

            /// <summary>
            /// The accessor for the Search branch.
            /// </summary>
            public ISearch Search { get; }

            /// <summary>
            /// Waits for the given time by updating the SettleWait timer, or - optionally - enforces a static wait.
            /// </summary>
            /// <param name="time">The wait time in seconds.</param>
            /// <param name="staticWait">Optional. Whether to enforce a static wait.</param>
            /// <param name="timeout">Optional. The timeout.</param>
            public void Wait(double time, bool staticWait = false, double timeout = 100 * ms);

            /// <summary>
            /// The interface for the Digital branch.
            /// </summary>
            public interface IDigital {

                /// <summary>
                /// Starts the pattern burst for the given pattern without waiting for it to complete.
                /// </summary>
                /// <param name="patternInfo">Pattern to start.</param>
                public void StartPattern(PatternInfo patternInfo);

                /// <summary>
                /// Starts the pattern burst for the given SiteVariant without waiting for it to complete.
                /// </summary>
                /// <param name="sitePatterns">Sites to start pattern.</param>
                public void StartPattern(SiteVariant sitePatterns);

                /// <summary>
                /// Starts the pattern burst for the given pattern and waits for it to complete.
                /// Equivalent to calling <see cref="StartPattern"/> and <see cref="WaitPatternDone"/> in sequence.
                /// </summary>
                /// <param name="patternInfo">Pattern to run.</param>
                public void RunPattern(PatternInfo patternInfo);

                /// <summary>
                /// Starts the pattern burst for the given SiteVariant and waits for it to complete.
                /// Equivalent to calling <see cref="StartPattern"/> and <see cref="WaitPatternDone"/> in sequence.
                /// </summary>
                /// <param name="sitePatterns">Sites run pattern.</param>
                public void RunPattern(SiteVariant sitePatterns);

                /// <summary>
                /// Waits for the given pattern to complete execution before returning. Works for both threaded and non-threaded patterns.
                /// </summary>
                /// <param name="patternInfo">Pattern to wait for completion.</param>
                public void WaitPatternDone(PatternInfo patternInfo);

                /// <summary>
                /// Stops the given pattern. Works for both threaded and non-threaded patterns.
                /// </summary>
                /// <param name="patternInfo">Pattern to halt.</param>
                public void ForcePatternHalt(PatternInfo patternInfo);

                /// <summary>
                /// Stops the currently running non-threaded pattern.
                /// </summary>
                public void ForcePatternHalt();

                /// <summary>
                /// Runs a pattern executing the specified func action at each conditional stop.
                /// </summary>                
                /// <param name="pattern">Pattern to be executed.</param>
                /// <param name="numberOfStops">Number of stops in the pattern.</param>
                /// <param name="func">Func action to be called at each stop.</param>
                /// <returns>Concatenated list of all the measurements taken at each stop.</returns>
                public List<PinSite<double>> RunPatternConditionalStop(PatternInfo pattern, int numberOfStops, Func<PatternInfo, int,
                    List<PinSite<double>>> func);

                /// <summary>
                /// Continues a pattern to the next conditional stop and executed the action.
                /// </summary>
                /// <param name="pattern">Pattern to be executed.</param>
                /// <param name="action">Action to be called at the stop.</param>
                public void ContinueToConditionalStop(PatternInfo pattern, Action action);
            }

            /// <summary>
            /// The interface for the ScanNetwork branch.
            /// </summary>
            public interface IScanNetwork
            {
                /// <summary>
                /// Runs the ScanNetwork pattern(set) and reburst if needed until all icl instances' pass/fail results are obtained.
                /// </summary>
                /// <param name="scanNetworkPattern">The <see cref="ScanNetworkPatternInfo"/> Object that is associated with the ScanNetwork pattern(set).
                /// </param>
                public void RunPattern(ScanNetworkPatternInfo scanNetworkPattern);

                /// <summary>
                /// Runs diagnosis reburst on failed core instances, which is obtained from the ScanNetwork pattern results.
                /// </summary>
                /// <param name="scanNetworkPattern">The <see cref="ScanNetworkPatternInfo"/> Object that is associated with the ScanNetwork pattern(set).
                /// </param>
                /// <param name="nonDiagnosisResults">The acquired ScanNetwork pattern results which contains failed core list.</param>
                /// <param name="concurrentDiagnosis">Optional. Whether to perform diagnosis on multiple core instances concurrently per reburst.</param>
                public void RunDiagnosis(ScanNetworkPatternInfo scanNetworkPattern, ScanNetworkPatternResults nonDiagnosisResults,
                    bool concurrentDiagnosis = false);
            }

            /// <summary>
            /// The interface for the Search branch.
            /// </summary>
            public interface ISearch {

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
                    out Site<int> closestIndex);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <param name="closestOut">Output - contains the output value of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
                    out Site<int> closestIndex, out Site<Tout> closestOut);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
                    out Site<int> closestIndex);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition resulting in an output closest to the (numeric)
                /// target. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <param name="closestOut">Output - contains the output value of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tout outTarget,
                    out Site<int> closestIndex, out Site<Tout> closestOut);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria, out Site<int> tripIndex);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <param name="tripOut">Output - contains the output value of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(List<Site<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria, out Site<int> tripIndex);

                /// <summary>
                /// Processes the measurements of a linear full search to find the device input condition satisfying the trip criteria on the output.
                /// Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="outValues">The collected measurements for all executed steps.</param>
                /// <param name="inFrom">The starting value of the linear input ramp.</param>
                /// <param name="inIncrement">The per-step increment of the linear input ramp.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <param name="tripOut">Output - contains the output value of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearFullProcess<Tin, Tout>(Site<Samples<Tout>> outValues, Tin inFrom, Tin inIncrement, Tin inOffset, Tin inNotFoundResult,
                    Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut);
            }
        }

        /// <summary>
        /// The interface for the Acquire branch.
        /// </summary>
        public interface IAcquire {

            /// <summary>
            /// The accessor for the Dc branch.
            /// </summary>
            public IDc Dc { get; }

            /// <summary>
            /// The accessor for the Digital branch.
            /// </summary>
            public IDigital Digital { get; }

            /// <summary>
            /// The accessor for the ScanNetwork branch.
            /// </summary>
            public IScanNetwork ScanNetwork { get; }

            /// <summary>
            /// The accessor for the Search branch.
            /// </summary>
            public ISearch Search { get; }

            /// <summary>
            /// The interface for the Dc branch.
            /// </summary>
            public interface IDc {

                /// <summary>
                /// Performs a single measurement for the set of pins provided.
                /// </summary>  
                /// <param name="pins">The pins to measure.</param>
                /// <param name="meterMode">Optional. Set the mode to measure Voltage or Current.</param>
                /// <returns>Returns a value.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<double> Measure(Pins pins, Measure? meterMode = null);

                /// <summary>
                /// Performs multiple measurements for the set of pins provided.
                /// </summary>
                /// <param name="pins">The pins to measure.</param> 
                /// <param name="sampleSize">The number of samples.</param>
                /// <param name="sampleRate">Optional. The sampling rate.</param>
                /// <param name="meterMode">Optional. Set the mode to measure Voltage or Current.</param>
                /// <returns>Returns an average value.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<double> Measure(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null);

                /// <summary>
                /// Performs multiple measurements for the set of each element in pinGroups.
                /// </summary>
                /// <param name="pinGroups">Array of pin or pin groups.</param>
                /// <param name="sampleSizes">Array of number of samples.</param>
                /// <param name="sampleRates">Optional. Array of sampling rate.</param>
                /// <param name="meterModes">Optional. Array of settings measurements mode voltage and current.</param>
                /// <returns>Returns a set of measurements.</returns>
                /// <exception cref="Exception">Appears when an element of pinGroups contains different types of pins - temporary limitation in functionality.
                /// </exception>
                public PinSite<double> Measure(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null);

                /// <summary>
                /// Performs multiple measurements for the set of pins provided.
                /// </summary>
                /// <param name="pins">The pins to measure.</param> 
                /// <param name="sampleSize">The number of samples.</param> 
                /// <param name="sampleRate">Optional. The sampling rate.</param>
                /// <param name="meterMode">Optional. Set the mode to measure Voltage or Current.</param>
                /// <returns>Returns a set of measurements.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<Samples<double>> MeasureSamples(Pins pins, int sampleSize, double? sampleRate = null, Measure? meterMode = null);

                /// <summary>
                /// Performs multiple measurements for the set of each element in pinGroups.
                /// </summary>
                /// <param name="pinGroups">Array of pin or pin groups.</param>
                /// <param name="sampleSizes">Array of number of samples.</param>
                /// <param name="sampleRates">Optional. Array of sampling rate.</param>
                /// <param name="meterModes">Optional. Array of settings measurements mode voltage and current.</param>
                /// <returns>Returns a set of measurements.</returns>
                /// <exception cref="Exception">Appears when an element of pinGroups contains different types of pins - temporary limitation in functionality.
                /// </exception>
                public PinSite<Samples<double>> MeasureSamples(Pins[] pinGroups, int[] sampleSizes, double[] sampleRates = null, Measure[] meterModes = null);

                /// <summary>
                /// Allows configuration and control of capture parameters for the specified pins.
                /// </summary>
                /// <param name="pins">List of pin or pin group names.</param>
                /// <param name="signalName">The name of the read signal.</param>
                /// <returns>Returns a value.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<double> ReadCaptured(Pins pins, string signalName);

                /// <summary>
                /// Allows configuration and control of capture parameters for the specified pins.
                /// </summary>
                /// <param name="pins">List of pin or pin group names.</param>
                /// <param name="signalName">The name of the read signal.</param>
                /// <returns>Returns a set of measurements.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<Samples<double>> ReadCapturedSamples(Pins pins, string signalName);

                /// <summary>
                /// Performs multiple readings of measurements, depending on the sample size parameter, for the set of pins provided.
                /// </summary>
                /// <param name="pins">The pins to read measurements.</param> 
                /// <param name="sampleSize">The number of samples.</param>
                /// <param name="sampleRate">Optional. The sampling rate.</param> 
                /// <returns>Returns an average value.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<double> ReadMeasured(Pins pins, int sampleSize, double? sampleRate = null);

                /// <summary>
                /// Performs multiple readings of measurements, depending on the sample size parameter, for the set of pins provided.
                /// </summary>
                /// <param name="pins">The pins to read measurements.</param> 
                /// <param name="sampleSize">The number of samples.</param> 
                /// <param name="sampleRate">Optional. The sampling rate.</param> 
                /// <returns>Returns a set of measurements.</returns>
                /// <exception cref="Exception">Appears when pinList contains different types of pins - temporary limitation in functionality.</exception>
                public PinSite<Samples<double>> ReadMeasuredSamples(Pins pins, int sampleSize, double? sampleRate = null);

                /// <summary>
                /// Performs a single measurement (strobe) on the according instrument, to read back the value later.
                /// </summary>
                /// <param name="pins">The pins to be measured.</param>
                public void Strobe(Pins pins);

                /// <summary>
                /// Performs multiple measurements (strobes) on the according instrument, to read back the value later.
                /// </summary>
                /// <param name="pins">The pins to be measured.</param>
                /// <param name="sampleSize">Number of measurements (Strobes).Ignored for PPMU.</param>
                /// <param name="sampleRate">Optional. The sampling rate. Default is the current hardware setting. Ignored for PPMU.</param>
                public void StrobeSamples(Pins pins, int sampleSize, double? sampleRate = null);

            }

            /// <summary>
            /// The interface for the Digital branch.
            /// </summary>
            public interface IDigital {

                /// <summary>
                /// Measures and returns the frequency configured by Setup.Digital.FrequencyCounter.
                /// </summary>
                /// <param name="pins">List of pin or pin group names.</param>
                /// <returns>The measured frequency for each pin in Hz.</returns>
                public PinSite<double> MeasureFrequency(Pins pins);

                public Site<bool> PatternResults();

                /// <summary>
                /// Reads captured pin data from HRAM and returns raw results as IPinListData
                /// </summary>
                /// <param name="pins">Pin names, must contain digital pins</param>
                /// <param name="startIndex">Optional. Index to start capture</param>
                /// <param name="cycle">Optional. Cycle of data to capture for pins in 2x or 4x modes</param>
                /// <returns>Raw captured pin data</returns>
                /// <exception cref="ArgumentException"></exception>
                public PinSite<Samples<int>> Read(Pins pins, int startIndex = 0, int cycle = 0);

                /// <summary>
                /// Reads pin data from specified cycles in HRAM for the specified pin, groups the data into words of a specified size, and populates
                /// these words into an array of SiteLong objects
                /// </summary>
                /// <param name="pins">Pin names, digital pins required</param>
                /// <param name="startIndex">Index to start data processing</param>
                /// <param name="length">Number of bits to process</param>
                /// <param name="wordSize">Number of bits in each word</param>
                /// <param name="bitOrder">Order of bits, either msbForst or lsbFirst</param>
                /// <returns>Word values in array of ISiteLong</returns>
                /// <exception cref="ArgumentException"></exception>
                public PinSite<Samples<int>> ReadWords(Pins pins, int startIndex, int length, int wordSize, tlBitOrder bitOrder);
            }

            /// <summary>
            /// The interface for the ScanNetwork branch.
            /// </summary>
            public interface IScanNetwork
            {
                /// <summary>
                /// Retrieve the per core/icl instance results of the specified ScanNetwork pattern object from its latest execution
                /// (TheLib.Execute.ScanNetwork.RunPattern(ScanNetworkPatternObject)).
                /// </summary>
                /// <param name="scanNetworkPattern">The <see cref="ScanNetworkPatternInfo"/> Object that is associated with the ScanNetwork pattern(set).</param>
                /// <returns><see cref="ScanNetworkPatternResults"/> object that contains per core/icl instance results</returns>
                public ScanNetworkPatternResults PatternResults(ScanNetworkPatternInfo scanNetworkPattern);
            }
            /// <summary>
            /// The interface for the Search branch.
            /// </summary>
            public interface ISearch {

                /// <summary>
                /// Performs a floating point binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing
                /// <paramref name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the (numeric) target. The number of
                /// steps executed equals log2((<paramref name="inTo"/> - <paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up to the next 
                /// integer. The search ends when the minimum delta is reached, the reported result lies within +/- <paramref name="inMinDelta"/> from the 
                /// ideal result.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// Must be >0.</param>
                /// <param name="invertingLogic">A flag indicating whether the output is inverted, meaning increasing output values are a result of decreasing
                /// input values.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value for which the corresponding input condition is searched.</param>
                /// <returns>The input value resulting in an output closest to the target. The worst case delta to the exact input is +/-
                /// (<paramref name="inMinDelta"/> / 2) if it can be reached given the search range.</returns>
                public Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
                    Site<Tout>> oneMeasurement, Tout outTarget);

                /// <summary>
                /// Performs a floating point binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing
                /// <paramref name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the (numeric) target. The number of
                /// steps executed equals log2((<paramref name="inTo"/> - <paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up to the next
                /// integer. The search ends when the minimum delta is reached, the reported result lies within +/- <paramref name="inMinDelta"/> from the ideal
                /// result. Additionally provides the output value for the input step found.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">A flag indicating whether the output is inverted, meaning increasing output values are a result of decreasing
                /// input values.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value for which the corresponding input condition is searched.</param>
                /// <param name="outResult">Output - contains the output value for the input found.</param>
                /// <returns>The input value resulting in an output closest to the target. The worst case delta to the exact input is +/-
                /// (<paramref name="inMinDelta"/> / 2) if it can be reached given the search range.</returns>
                public Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<Tout> outResult);

                /// <summary>
                /// Performs a floating point binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing
                /// <paramref name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the trip criteria's inflection point.
                /// The number of steps executed equals log2((<paramref name="inTo"/> -<paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up
                /// to the next integer. The search ends when the minimum delta is reached, the reported result lies within <paramref name="inMinDelta"/> from
                /// the ideal result, matching criteria's direction. If no transition was found, the method returns <paramref name="inNotFoundResult"/>.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">Determines whether the logic is inverted. <c>false</c> means higher input values increase the likelihood of
                /// meeting the trip criteria. <c>true</c> means lower input values do.</param>
                /// /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <returns>The input value resulting in an output fulfilling the trip criteria and be closest to it's inflection point. The worst case
                /// deviation from the exact input is (<paramref name="inMinDelta"/>.</returns>
                public Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, double inNotFoundResult);

                /// <summary>
                /// Performs a floating point binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing
                /// <paramref name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the trip criteria's inflection point.
                /// The number of steps executed equals log2((<paramref name="inTo"/> -<paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up
                /// to the next integer. The search ends when the minimum delta is reached, the reported result lies within <paramref name="inMinDelta"/> from
                /// the ideal result, matching criteria's direction. Additionally provides the output value of the input step found. If no transition was found,
                /// the method returns <paramref name="inNotFoundResult"/>.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">Determines whether the logic is inverted. <c>false</c> means higher input values increase the likelihood of
                /// meeting the trip criteria. <c>true</c> means lower input values do.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outResult">Output - contains the output value of the input found.</param>
                /// <returns>The input value resulting in an output fulfilling the trip criteria and be closest to it's inflection point. The worst case
                /// deviation from the exact input is (<paramref name="inMinDelta"/>.</returns>
                public Site<double> BinarySearch<Tout>(double inFrom, double inTo, double inMinDelta, bool invertingLogic, Func<Site<double>,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, double inNotFoundResult, out Site<Tout> outResult);

                /// <summary>
                /// Performs an integer binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing <paramref
                /// name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the target. The number of steps executed does
                /// not exceed log2((<paramref name="inTo"/> - <paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up to the next integer. The
                /// search ends when the best input value (within the specified <paramref name="inMinDelta"/> resolution) is reached. 
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// Must be >0.</param>
                /// <param name="invertingLogic">A flag indicating whether the output is inverted, meaning increasing output values are a result of decreasing
                /// input values.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value for which the corresponding input condition is searched.</param>
                /// <returns>The input value resulting in an output closest to the target. The worst case delta to the exact input is +/-
                /// (<paramref name="inMinDelta"/> / 2) if it can be reached given the search range.</returns>
                public Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>, Site<Tout>> oneMeasurement,
                    Tout outTarget);

                /// <summary>
                /// Performs an integer binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing <paramref
                /// name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the target. The number of steps executed does
                /// not exceed log2((<paramref name="inTo"/> - <paramref name="inFrom"/>) / <paramref name="inMinDelta"/>), rounded up to the next integer. The
                /// search ends when the best input value (within the specified <paramref name="inMinDelta"/> resolution) is reached.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">A flag indicating whether the output is inverted, meaning increasing output values are a result of decreasing
                /// input values.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value for which the corresponding input condition is searched.</param>
                /// <param name="outResult">Output - contains the output value for the input found.</param>
                /// <returns>The input value resulting in an output closest to the target. The worst case delta to the exact input is +/-
                /// (<paramref name="inMinDelta"/> / 2) if it can be reached given the search range.</returns>
                public Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>, Site<Tout>> oneMeasurement,
                    Tout outTarget, out Site<Tout> outResult);

                /// <summary>
                /// Performs an integer binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing <paramref
                /// name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the trip criteria's inflection point. The
                /// number of steps executed does not exceed log2((<paramref name="inTo"/> -<paramref name="inFrom"/>) / <paramref name="inMinDelta"/>),
                /// rounded up to the next integer. The search ends when the input value closest to the transition point (within the specified <paramref
                /// name="inMinDelta"/> resolution), but matching the criteria is reached. If no transition was found, the method returns <paramref
                /// name="inNotFoundResult"/>.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">Determines whether the logic is inverted. <c>false</c> means higher input values increase the likelihood of
                /// meeting the trip criteria. <c>true</c> means lower input values do.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <returns>The input value resulting in an output fulfilling the trip criteria and be closest to it's inflection point. The worst case
                /// deviation from the exact input is (<paramref name="inMinDelta"/>.</returns>
                public Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>, Site<Tout>> oneMeasurement,
                    Func<Tout, bool> outTripCriteria, int inNotFoundResult);

                /// <summary>
                /// Performs an integer binary search between <paramref name="inFrom"/> and <paramref name="inTo"/> by executing <paramref
                /// name="oneMeasurement"/> at each step. Determines the input resulting in an output closest to the trip criteria's inflection point. The
                /// number of steps executed does not exceed log2((<paramref name="inTo"/> -<paramref name="inFrom"/>) / <paramref name="inMinDelta"/>),
                /// rounded up to the next integer. The search ends when the input value closest to the transition point (within the specified <paramref
                /// name="inMinDelta"/> resolution), but matching the criteria is reached. If no transition was found, the method returns <paramref
                /// name="inNotFoundResult"/>.
                /// </summary>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The lower boundary of the search range. Must be less than <paramref name="inTo"/>.</param>
                /// <param name="inTo">The upper boundary of the search range. Must be greater than <paramref name="inFrom"/></param>
                /// <param name="inMinDelta">The minimum allowable difference between successive input values, used to determine when the search should stop.
                /// </param>
                /// <param name="invertingLogic">Determines whether the logic is inverted. <c>false</c> means higher input values increase the likelihood of
                /// meeting the trip criteria. <c>true</c> means lower input values do.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="outResult">Output - contains the output value of the input found.</param>
                /// <returns>The input value resulting in an output fulfilling the trip criteria and be closest to it's inflection point. The worst case
                /// deviation from the exact input is (<paramref name="inMinDelta"/>.</returns>
                public Site<int> BinarySearch<Tout>(int inFrom, int inTo, int inMinDelta, bool invertingLogic, Func<Site<int>, Site<Tout>> oneMeasurement,
                    Func<Tout, bool> outTripCriteria, int inNotFoundResult, out Site<Tout> outResult);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. The ramp ends after <paramref name="inCount"/> measurements are completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                public void LinearFullFromIncCount<Tin>(Tin inFrom, Tin inIncrement, int inCount, Action<Tin> oneMeasurement);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. The ramp ends with the last input less or equal <paramref name="inTo"/>.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                public void LinearFullFromToInc<Tin>(Tin inFrom, Tin inTo, Tin inIncrement, Action<Tin> oneMeasurement);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. The ramp ends after <paramref name="inCount"/> measurements are
                /// completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <returns>The calculated increment, for later use in the processing step (ignore if not needed).</returns>
                public Tin LinearFullFromToCount<Tin>(Tin inFrom, Tin inTo, int inCount, Action<Tin> oneMeasurement);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <param name="closestOut">Output - contains the output value of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or with the last input less or
                /// equal <paramref name="inTo"/>.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or with the last input less or
                /// equal <paramref name="inTo"/>. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the input resulting in an output closest to the (numeric)
                /// <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or with the last input less or
                /// equal <paramref name="inTo"/>. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <param name="closestOut">Output - contains the output value of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the input resulting in an output closest to the
                /// (numeric) <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the input resulting in an output closest to the
                /// (numeric) <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the input resulting in an output closest to the
                /// (numeric) <paramref name="outTarget"/>. The ramp stops prematurely when the target output is surpassed on all sites, or after <paramref
                /// name="inCount"/> measurements are completed. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTarget">The (numeric) target output value to be searched.</param>
                /// <param name="closestIndex">Output - contains the index of the input step found.</param>
                /// <param name="closestOut">Output - contains the output value of the input step found.</param>
                /// <returns>The input value resulting in an output closest to the target.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Tout outTarget, out Site<int> closestIndex, out Site<Tout> closestOut);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or after <paramref name="inCount"/> measurements are completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or after <paramref name="inCount"/> measurements are completed.
                /// Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or after <paramref name="inCount"/> measurements are completed.
                /// Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inCount">The total number of steps to execute.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <param name="tripOut">Output - contains the output value of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromIncCount<Tin, Tout>(Tin inFrom, Tin inIncrement, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or with the last input less or equal <paramref name="inTo"/>.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or with the last input less or equal <paramref name="inTo"/>.
                /// Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex);

                /// <summary>
                /// Performs a linear search from <paramref name="inFrom"/> by increasing with <paramref name="inIncrement"/>. Executes <paramref
                /// name="oneMeasurement"/> at each step including the start point. Determines the first input meeting the <paramref name="outTripCriteria"/>.
                /// The ramp stops prematurely when the trip criteria is met on all sites, or with the last input less or equal <paramref name="inTo"/>.
                /// Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inIncrement">The input increment value for every step.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <param name="tripOut">Output - contains the output value of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToInc<Tin, Tout>(Tin inFrom, Tin inTo, Tin inIncrement, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the first input meeting the
                /// <paramref name="outTripCriteria"/>. The ramp stops prematurely when the trip criteria is met on all sites, or after
                /// <paramref name="inCount"/> measurements are completed.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the first input meeting the
                /// <paramref name="outTripCriteria"/>. The ramp stops prematurely when the trip criteria is met on all sites, or after
                /// <paramref name="inCount"/> measurements are completed. Additionally provides the index of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex);

                /// <summary>
                /// Performs a linear search between <paramref name="inFrom"/> and <paramref name="inTo"/> with <paramref name="inCount"/> inputs. Executes
                /// <paramref name="oneMeasurement"/> at each step including the end points. Determines the first input meeting the
                /// <paramref name="outTripCriteria"/>. The ramp stops prematurely when the trip criteria is met on all sites, or after
                /// <paramref name="inCount"/> measurements are completed. Additionally provides the index and output value of the input step found.
                /// </summary>
                /// <typeparam name="Tin">The type of the input condition for the device.</typeparam>
                /// <typeparam name="Tout">The type of the device's output.</typeparam>
                /// <param name="inFrom">The starting point of the linear input ramp.</param>
                /// <param name="inTo">The end point of the linear input ramp.</param>
                /// <param name="inCount">The number of equally spaced steps to execute, including both endpoints exactly.</param>
                /// <param name="inOffset">The offset to correct the calculated input value. Use negative values to compensate propagation delays for output
                /// switching.</param>
                /// <param name="inNotFoundResult">The return value for the case when the trip criteria was never found.</param>
                /// <param name="oneMeasurement">The action to execute for every measurement.</param>
                /// <param name="outTripCriteria">A delegate indicating the output meets the condition required for the input value searched.</param>
                /// <param name="tripIndex">Output - contains the index of the input step found.</param>
                /// <param name="tripOut">Output - contains the output value of the input step found.</param>
                /// <returns>The first input value resulting in an output satisfying the trip criteria.</returns>
                public Site<Tin> LinearStopFromToCount<Tin, Tout>(Tin inFrom, Tin inTo, int inCount, Tin inOffset, Tin inNotFoundResult, Func<Tin,
                    Site<Tout>> oneMeasurement, Func<Tout, bool> outTripCriteria, out Site<int> tripIndex, out Site<Tout> tripOut);
            }
        }

        /// <summary>
        /// The interface for the Datalog branch.
        /// </summary>
        public interface IDatalog {

            /// <summary>
            /// Perform a functional datalog test.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="pattern">Optional. The pattern executed.</param>
            public void TestFunctional(Site<bool> result, string pattern = "");

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            public void TestParametric(Site<int> result, double forceValue = 0, string forceUnit = "");

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            public void TestParametric(Site<double> result, double forceValue = 0, string forceUnit = "");

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            public void TestParametric(PinSite<int> result, double forceValue = 0, string forceUnit = "");

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            public void TestParametric(PinSite<double> result, double forceValue = 0, string forceUnit = "");

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            /// <param name="sameLimitForAllSamples">Optional. Whether to use the same FlowLimit for all samples.</param>
            public void TestParametric(Site<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false);

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            /// <param name="sameLimitForAllSamples">Optional. Whether to use the same FlowLimit for all samples.</param>
            public void TestParametric(Site<Samples<double>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false);

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            /// <param name="sameLimitForAllSamples">Optional. Whether to use the same FlowLimit for all samples.</param>
            public void TestParametric(PinSite<Samples<int>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false);

            /// <summary>
            /// Perform a parametric datalog test by using FlowLimits.
            /// </summary>
            /// <param name="result">The result object to be datalogged.</param>
            /// <param name="forceValue">Optional. The force value applied for the result.</param>
            /// <param name="forceUnit">Optional. The force value's unit.</param>
            /// <param name="sameLimitForAllSamples">Optional. Whether to use the same FlowLimit for all samples.</param>
            public void TestParametric(PinSite<Samples<double>> result, double forceValue = 0, string forceUnit = "", bool sameLimitForAllSamples = false);

            /// <summary>
            /// Perform a flexible datalog test for ScanNetwork pattern results, with datalogging options set by <see cref="ScanNetworkDatalogOption"/>.
            /// </summary>
            /// <param name="result">The ScanNetwork pattern result object of type <see cref="ScanNetworkPatternResults"/></param>
            public void TestScanNetwork(ScanNetworkPatternResults result, ScanNetworkDatalogOption datalogOptions);
        }
    }
}

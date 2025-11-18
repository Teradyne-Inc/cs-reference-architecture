using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Csra.Interfaces;

namespace Csra.TheLib {
    public class Validate : ILib.IValidate {
        public void Dc(Pins pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
                    double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null,
                    Measure? meterMode = null, double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null,
                    double? sourceFoldLimit = null, double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null,
                    bool? voltageAltOutput = null, bool? bleederResistor = null, double? complianceBoth = null, double? compliancePositive = null,
                    double? complianceNegative = null, double? clampHiV = null, double? clampLoV = null, bool? highAccuracy = null, double? settlingTime = null,
                    double? hardwareAverage = null) {

            if (pins.ContainsType(InstrumentType.UP2200, out string up2200Pins)) {
                ValidatePpmu(up2200Pins, gate, mode, voltage, current, currentRange, meterMode, meterCurrentRange, clampHiV, clampLoV, highAccuracy,
                    settlingTime);
            }
            if (pins.ContainsType(InstrumentType.UVI264, out string uvi264Pins)) {
                ValidateDcvi(uvi264Pins, gate, mode, voltage, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange,
                    meterCurrentRange, meterBandwidth, bleederResistor, complianceBoth, compliancePositive, complianceNegative, hardwareAverage);
            }
            if (pins.ContainsType(InstrumentType.UVS256, out string uvs256Pins)) {
                ValidateDcvs(uvs256Pins, gate, mode, voltage, voltageAlt, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange,
                    meterCurrentRange, meterBandwidth, sourceFoldLimit, sinkFoldLimit, sourceOverloadLimit, sinkOverloadLimit, voltageAltOutput);
            }
            if (pins.ContainsType(InstrumentType.UVS64, out string uvs64Pins)) {
                ValidateDcvs(uvs64Pins, gate, mode, voltage, voltageAlt, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange,
                    meterCurrentRange, meterBandwidth, sourceFoldLimit, sinkFoldLimit, sourceOverloadLimit, sinkOverloadLimit, voltageAltOutput);
            }
        }

        public bool Enum<T>(string value, string argumentName, out T enumValue) where T : struct, Enum {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            enumValue = default;
            if (string.IsNullOrWhiteSpace(value)) {
                Api.Services.Alert.Error($"{messagePrefix}Provided value is null or a whitespace and cannot be parsed into an enum.", argumentIndex);
                return false;
            }
            if (System.Enum.TryParse(value, true, out enumValue)) {
                return true;
            } else {
                const int maxLength = 300;
                string options = string.Join(", ", System.Enum.GetNames(typeof(T)));
                string optionList = options.Length > maxLength ? $"{options.Substring(0, maxLength)}..." : options; // truncate in case of VERY LONG lists
                Api.Services.Alert.Error($"{messagePrefix}Provided value '{value}' could not be found within provided Enum '{typeof(T).Name}' - the valid " +
                    $"options are '{optionList}'.", argumentIndex);
                return false;
            }
        }

        public void Fail(string problemReasonResolutionMessage, string argumentName) {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (string.IsNullOrEmpty(problemReasonResolutionMessage)) {
                Api.Services.Alert.Error($"{messagePrefix}Provided string is null.", argumentIndex);
            }
            Api.Services.Alert.Error($"{messagePrefix}{problemReasonResolutionMessage}", argumentIndex);
        }

        public bool GreaterOrEqual<T>(T value, T boundary, string argumentName) where T : IComparable<T> {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (typeof(T) == typeof(string)) {
                if (EqualityComparer<T>.Default.Equals(value, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Value is null.", argumentIndex);
                    return false;
                } else if (EqualityComparer<T>.Default.Equals(boundary, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Boundary is null.", argumentIndex);
                    return false;
                }
            }
            if (value.CompareTo(boundary) < 0) {
                Api.Services.Alert.Error($"{messagePrefix}Value '{value}' must be greater or equal to Boundary '{boundary}'.", argumentIndex);
                return false;
            }
            return true;
        }

        public bool GreaterThan<T>(T value, T boundary, string argumentName) where T : IComparable<T> {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (typeof(T) == typeof(string)) {
                if (EqualityComparer<T>.Default.Equals(value, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Value is null.", argumentIndex);
                    return false;
                } else if (EqualityComparer<T>.Default.Equals(boundary, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Boundary is null.", argumentIndex);
                    return false;
                }
            }
            if (value.CompareTo(boundary) != 1) {
                Api.Services.Alert.Error($"{messagePrefix}Value '{value}' must be greater than Boundary '{boundary}'.", argumentIndex);
                return false;
            }
            return true;
        }

        public bool InRange<T>(T value, T from, T to, string argumentName) where T : IComparable<T> {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (from.CompareTo(to) > 0) {
                Api.Services.Alert.Error($"{messagePrefix}Reformat the provided boundaries so that 'from' ({from}) is less than 'to' ({to}).", argumentIndex);
                return false;
            }
            if (!(value.CompareTo(from) == 0 || value.CompareTo(from) > 0)) {
                Api.Services.Alert.Error($"{messagePrefix}Value is less than lower threshold '{from}'.", argumentIndex);
                return false;
            }
            if (!(value.CompareTo(to) == 0 || value.CompareTo(to) < 0)) {
                Api.Services.Alert.Error($"{messagePrefix}Value is greater than upper threshold '{to}'.", argumentIndex);
                return false;
            }
            return true;
        }

        public bool IsTrue(bool condition, string problemReasonResolutionMessage, string argumentName) {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (condition == false) {
                if (string.IsNullOrWhiteSpace(problemReasonResolutionMessage)) {
                    Api.Services.Alert.Error($"{messagePrefix}Provided 'ProblemReasonResolutionMessage' is null.", argumentIndex);
                    return false;
                }
                Api.Services.Alert.Error($"{messagePrefix}{problemReasonResolutionMessage}", argumentIndex);
                return false;
            }
            return true;
        }

        public bool LessOrEqual<T>(T value, T boundary, string argumentName) where T : IComparable<T> {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (typeof(T) == typeof(string)) {
                if (EqualityComparer<T>.Default.Equals(value, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Value is null.", argumentIndex);
                    return false;
                } else if (EqualityComparer<T>.Default.Equals(boundary, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Boundary is null.", argumentIndex);
                    return false;
                }
            }
            if (value.CompareTo(boundary) > 0) {
                Api.Services.Alert.Error($"{messagePrefix}Value '{value}' must be less or equal to Boundary '{boundary}'.", argumentIndex);
                return false;
            }
            return true;
        }

        public bool LessThan<T>(T value, T boundary, string argumentName) where T : IComparable<T> {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (typeof(T) == typeof(string)) {
                if (EqualityComparer<T>.Default.Equals(value, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Value is null.", argumentIndex);
                    return false;
                } else if (EqualityComparer<T>.Default.Equals(boundary, default)) {
                    Api.Services.Alert.Error($"{messagePrefix}Boundary is null.", argumentIndex);
                    return false;
                }
            }
            if (value.CompareTo(boundary) != -1) {
                Api.Services.Alert.Error($"{messagePrefix}Value '{value}' must be less than Boundary '{boundary}'.", argumentIndex);
                return false;
            }
            return true;
        }

        public bool MethodHandle<T>(string fullyQualifiedName, string argumentName, out MethodHandle<T> method) where T : Delegate {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (string.IsNullOrWhiteSpace(fullyQualifiedName)) {
                Api.Services.Alert.Error($"{messagePrefix}Provided value '{fullyQualifiedName}' is null (or empty) and cannot be matched to any existing methods.",
                    argumentIndex);
                method = null;
                return false;
            }
            try {
                method = new MethodHandle<T>(fullyQualifiedName);
                return true;
            } catch (Exception) {
                Api.Services.Alert.Error($"{messagePrefix}Unable to match '{fullyQualifiedName}' with existing methods. Please verify the method name, output " +
                    $"and parameters.", argumentIndex);
                method = null;
                return false;
            }
        }

        public bool MultiCondition<T>(string csv, Func<string, T> parser, string argumentName, out T[] conditions,
            int? referenceCount = null) {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (string.IsNullOrWhiteSpace(csv)) {
                Api.Services.Alert.Error($"{messagePrefix}Provided string contains only null or whitespace characters.", argumentIndex);
                conditions = [];
                return false;
            }
            try {
                conditions = csv.Split(',').Select(s => parser(s.Trim())).ToArray();
            } catch (Exception) {
                Api.Services.Alert.Error($"{messagePrefix}Invalid input string '{csv}' for parsing to type '{typeof(T).Name}'.", argumentIndex);
                conditions = [];
                return false;
            }
            if (referenceCount.HasValue && conditions.Length != 1 && conditions.Length != referenceCount) {
                Api.Services.Alert.Error($"{messagePrefix}{conditions.Length} values found in " +
                $"'{nameof(csv)}' ({csv}) - must either be 1 or match the provided reference count ({referenceCount}).", argumentIndex);
                return false;
            } else {
                return true;
            }
        }

        public bool MultiCondition<TEnum>(string csv, string argumentName, out TEnum[] conditions, int? referenceCount = null) where TEnum : struct,
            Enum {
            return MultiCondition(csv, EnumParser, argumentName, out conditions, referenceCount);

            TEnum EnumParser(string value) {
                if (System.Enum.TryParse(value, true, out TEnum result)) return result;
                GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
                const int maxLength = 300;
                string options = string.Join(", ", System.Enum.GetNames(typeof(TEnum)));
                string optionList = options.Length > maxLength ? $"{options.Substring(0, maxLength)}..." : options; // truncate in case of VERY LONG lists
                Api.Services.Alert.Error($"{messagePrefix}Provided value '{value}' could not be found within provided Enum '{typeof(TEnum).Name}' - the " +
                    $"valid options are '{optionList}'.", argumentIndex);
                return default; // This line will never be reached due to the AlertService.Error call above.
            }
        }

        public bool Pattern(Pattern pattern, string argumentName, out PatternInfo patternInfo, bool threading = true) {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            if (string.IsNullOrWhiteSpace(pattern)) {
                Api.Services.Alert.Error($"{messagePrefix}Provided pattern is null.", argumentIndex);
                patternInfo = null;
                return false;
            }
            try {
                TheHdw.Patterns(pattern).ValidatePatlist();
                patternInfo = new PatternInfo(pattern, threading);
                return true;
            } catch (Exception) {
                Api.Services.Alert.Error($"{messagePrefix}Provided pattern '{nameof(pattern)}' does not exist or cannot be found.", argumentIndex);
                patternInfo = null;
                return false;
            }
        }

        public bool Pins(PinList pinList, string argumentName, out Pins pins) {
            GetArgumentContext(argumentName, out int argumentIndex, out string messagePrefix);
            try {
                pins = new Pins(pinList);
                if (pins.Count() == 0) {
                    Api.Services.Alert.Error($"{messagePrefix}Pinlist could not be decompiled or does not exist.", argumentIndex);
                    pins = null;
                    return false;
                }
                return true;
            } catch (Exception) {
                string[] pinsArray = pinList.Value
                    .Split([','], StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
                var offenders = new List<string>();
                foreach (string pin in pinsArray) {
                    int success = TheExec.DataManager.DecomposePinList(pin, out string[] pinArray, out _);
                    if (success == 0) continue;
                    offenders.Add(pin);
                }
                Api.Services.Alert.Error($"{messagePrefix}Pin(s) '{string.Join(", ", offenders)}' could not be compiled into a Pins object.", argumentIndex);
                pins = null;
                return false;
            }
        }

        private void ValidatePpmu(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
             double? currentRange = null, Measure? meterMode = null, double? meterCurrentRange = null, double? clampHiV = null, double? clampLoV = null,
             bool? highAccuracy = null, double? settlingTime = null) {

            var ppmu = TheHdw.Pins(pins).PPMU;
            //if (gate.HasValue) // no check needed
            //if (mode.HasValue) // no check needed
            if (voltage.HasValue) {
                InRange(voltage.Value, ppmu.Voltage.Min, ppmu.Voltage.Max, "");
            }
            if (current.HasValue) {
                InRange(current.Value, ppmu.Current.Min, ppmu.Current.Max, "");
            }
            if (currentRange.HasValue) {
                // not sure how to check    
            }
            if (meterCurrentRange.HasValue) {
                // not sure how to check
            }
            //if (meterMode.HasValue) // no check needed
            if (clampHiV.HasValue) {
                InRange(clampHiV.Value, ppmu.ClampVHi.Min, ppmu.ClampVHi.Max, "");
            }
            if (clampLoV.HasValue) {
                InRange(clampLoV.Value, ppmu.ClampVLo.Min, ppmu.ClampVLo.Max, "");
            }
            //if (highAccuracy.HasValue) // no check needed
            if (settlingTime.HasValue) {
                // not sure how to check
            }
        }


        private void ValidateDcvi(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
            double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, bool? bleederResistor = null,
            double? complianceBoth = null, double? compliancePositive = null, double? complianceNegative = null, double? hardwareAverage = null) {

            var dcvi = TheHdw.Pins(pins).DCVI;
            //if (gate.HasValue) // no check needed
            //if (mode.HasValue) // no check needed
            if (voltageRange.HasValue) {
                InRange(voltageRange.Value, dcvi.VoltageRange.Min, dcvi.VoltageRange.Max, "");
            }
            if (currentRange.HasValue) {
                InRange(currentRange.Value, dcvi.CurrentRange.Min, dcvi.CurrentRange.Max, "");
            }
            if (voltage.HasValue) {
                InRange(voltage.Value, dcvi.VoltageRange.Min, dcvi.VoltageRange.Max, "");
            }
            if (current.HasValue) {
                InRange(current.Value, dcvi.CurrentRange.Min, dcvi.CurrentRange.Max, "");
            }
            //if (bleederResistor.HasValue) // no check needed
            if (complianceBoth.HasValue) {
                InRange(complianceBoth.Value, dcvi.ComplianceRange(tlDCVICompliance.Both).Min, dcvi.ComplianceRange(tlDCVICompliance.Both).Max,
                    "");
            }
            if (compliancePositive.HasValue) {
                InRange(compliancePositive.Value, dcvi.ComplianceRange(tlDCVICompliance.Positive).Min,
                    dcvi.ComplianceRange(tlDCVICompliance.Positive).Max, "");
            }
            if (complianceNegative.HasValue) {
                InRange(complianceNegative.Value, dcvi.ComplianceRange(tlDCVICompliance.Negative).Min,
                    dcvi.ComplianceRange(tlDCVICompliance.Negative).Max, "");
            }
            if (forceBandwidth.HasValue) {
                InRange(forceBandwidth.Value, dcvi.NominalBandwidth.Min, dcvi.NominalBandwidth.Max, "");
            }
            //if (meterMode.HasValue) // no check needed
            if (meterVoltageRange.HasValue) {
                InRange(meterVoltageRange.Value, dcvi.Meter.VoltageRange.Min, dcvi.Meter.VoltageRange.Max, "");
            }
            if (meterCurrentRange.HasValue) {
                InRange(meterCurrentRange.Value, dcvi.Meter.CurrentRange.Min, dcvi.Meter.CurrentRange.Max, "");
            }
            if (meterBandwidth.HasValue) {
                InRange(meterBandwidth.Value, dcvi.Meter.Filter.Min, dcvi.Meter.Filter.Max, "");
            }
            if (hardwareAverage.HasValue) {
                InRange(hardwareAverage.Value, dcvi.Meter.HardwareAverage.Min, dcvi.Meter.HardwareAverage.Max, "");
            }
        }

        private void ValidateDcvs(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
            double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, double? sourceFoldLimit = null,
            double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null, bool? voltageAltOutput = null) {

            var dcvs = TheHdw.Pins(pins).DCVS;
            //if (gate.HasValue) // no check needed 
            //if (meterMode.HasValue) // no check needed
            if (meterVoltageRange.HasValue) {
                InRange(meterVoltageRange.Value, dcvs.Meter.VoltageRange.Min, dcvs.Meter.VoltageRange.Max, "");
            }
            if (meterCurrentRange.HasValue) {
                InRange(meterCurrentRange.Value, dcvs.Meter.CurrentRange.Min, dcvs.Meter.CurrentRange.Max, "");
            }
            if (meterBandwidth.HasValue) {
                InRange(meterBandwidth.Value, dcvs.Meter.Filter.Min, dcvs.Meter.Filter.Max, "");
            }
            //if (mode.HasValue) // no check needed
            if (voltageRange.HasValue) {
                InRange(voltageRange.Value, dcvs.VoltageRange.Min, dcvs.VoltageRange.Max, "");
            }
            if (currentRange.HasValue) {
                InRange(currentRange.Value, dcvs.CurrentRange.Min, dcvs.CurrentRange.Max, "");
            }
            if (voltage.HasValue) {
                InRange(voltage.Value, dcvs.Voltage.Main.Min, dcvs.Voltage.Main.Max, "");
            }
            if (voltageAlt.HasValue) {
                InRange(voltageAlt.Value, dcvs.Voltage.Alt.Min, dcvs.Voltage.Alt.Max, "");
            }
            //if (voltageAltOutput.HasValue) // no check needed
            if (current.HasValue) {
                InRange(current.Value, dcvs.CurrentLimit.Source.FoldLimit.Level.Min, dcvs.CurrentLimit.Source.FoldLimit.Level.Max, "");
            }
            if (forceBandwidth.HasValue) {
                InRange(forceBandwidth.Value, dcvs.BandwidthSetting.Min, dcvs.BandwidthSetting.Max, "");
            }
            if (sourceFoldLimit.HasValue) {
                InRange(sourceFoldLimit.Value, dcvs.CurrentLimit.Source.FoldLimit.Level.Min,
                    dcvs.CurrentLimit.Source.FoldLimit.Level.Max, "");
            }
            if (sinkFoldLimit.HasValue) {
                InRange(sinkFoldLimit.Value, dcvs.CurrentLimit.Sink.FoldLimit.Level.Min, dcvs.CurrentLimit.Sink.FoldLimit.Level.Max,
                    "");
            }
            if (sourceOverloadLimit.HasValue) {
                InRange(sourceOverloadLimit.Value, dcvs.CurrentLimit.Source.OverloadLimit.Level.Min,
                    dcvs.CurrentLimit.Source.OverloadLimit.Level.Max, "");
            }
            if (sinkOverloadLimit.HasValue) {
                InRange(sinkOverloadLimit.Value, dcvs.CurrentLimit.Sink.OverloadLimit.Level.Min,
                    dcvs.CurrentLimit.Sink.OverloadLimit.Level.Max, "");
            }
        }

        private void GetArgumentContext(string argumentName, out int index, out string messagePrefix) {
            var stackTrace = new StackTrace();
            var frames = stackTrace.GetFrames();
            MethodBase callerMethod = null;
            if (frames != null) {
                foreach (var frame in frames) {
                    var method = frame.GetMethod();
                    if (method?.DeclaringType == null) continue;

                    // Check if the method has the [TestMethod] attribute
                    var hasTestMethodAttribute = method.GetCustomAttributes(false)
                        .Any(attr => attr.GetType().Name == "TestMethodAttribute" ||
                        attr.GetType().Name == "DataTestMethodAttribute"); // DataTestMethodAttribute for MSTest data-driven tests

                    if (hasTestMethodAttribute) {
                        callerMethod = method;
                        break;
                    }
                }
            }

            List<string> parameters = callerMethod?.GetParameters().Select(p => p.Name).ToList() ?? new List<string>();
            index = parameters.FindIndex(p => p == argumentName) + 1;
            messagePrefix = index > 0 ? $"Argument '{argumentName}': " : "";
        }
    }
}

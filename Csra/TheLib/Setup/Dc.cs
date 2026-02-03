using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Csra;
using Csra.Interfaces;

namespace Csra.TheLib.Setup {
    public class Dc : ILib.ISetup.IDc {

        public virtual void Connect(Pins pins, bool gateOn = false) {
            pins.Ppmu?.Connect(gateOn ? true : null);
            pins.Dcvi?.Connect(gateOn ? true : null);
            pins.Dcvs?.Connect(gateOn ? true : null);
        }

        public virtual void ConnectAllPins() => TheHdw.Digital.ApplyLevelsTiming(true);

        public virtual void Disconnect(Pins pins, bool gateOff = true) {
            pins.Ppmu?.Disconnect(gateOff ? false : null);
            pins.Dcvi?.Disconnect(gateOff ? false : null);
            pins.Dcvs?.Disconnect(gateOff ? false : null);
        }

        public virtual void Force(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, bool gateOn = true) {
            if (mode == TLibOutputMode.ForceVoltage) {
                ForceV(pins, forceValue, clampValue, forceRange, clampValue, true, gateOn);
            } else if (mode == TLibOutputMode.ForceCurrent) {
                ForceI(pins, forceValue, clampValue, forceRange, clampValue, true, gateOn);
            } else {
                ForceHiZ(pins);
            }
        }

        public virtual void Force(Pins[] pinGroups, TLibOutputMode[] modes, double[] forceValues, double[] forceRanges, double[] clampValues,
            bool[] gateOn = null) {
            TLibOutputMode[] outputModes = modes.ToArray();
            gateOn ??= new bool[] { true };
            bool modesUniform = outputModes.Length == 1;
            bool forceValuesUniform = forceValues.Length == 1;
            bool forceRangesUniform = forceRanges.Length == 1;
            bool clampValuesUniform = clampValues.Length == 1;
            bool gateOnUniform = gateOn.Length == 1;
            bool allUniform = modesUniform && forceValuesUniform && forceRangesUniform && clampValuesUniform && gateOnUniform;
            if (allUniform) {
                Api.TheLib.Setup.Dc.Force(Pins.Join(pinGroups), outputModes[0], forceValues[0], forceRanges[0], clampValues[0], gateOn[0]);
            } else {
                for (int i = 0; i < pinGroups.Length; i++) {
                    Api.TheLib.Setup.Dc.Force(pinGroups[i], outputModes.SingleOrAt(i), forceValues.SingleOrAt(i), forceRanges.SingleOrAt(i),
                        clampValues.SingleOrAt(i), gateOn.SingleOrAt(i));
                }
            }
        }

        public virtual void ForceHiZ(Pins pins) {
            pins.Ppmu?.ForceHiZ();
            pins.Dcvi?.ForceHiZ();
            pins.Dcvs?.ForceHiZ();
        }

        public virtual void ForceI(Pins pins, double forceCurrent, double? clampHiV = null, double? clampLoV = null, bool setCurrentMode = false,
            bool gateOn = true) { //clampVoltage has been added to reduce the risk of breaking the device
            pins.Ppmu?.ForceI(forceCurrent, clampHiV, clampLoV, gate: gateOn ? true : null); // assuming current range is already set
            pins.Dcvi?.ForceI(forceCurrent, clampHiV, setCurrentMode: setCurrentMode, gate: gateOn ? true : null);
            pins.Dcvs?.ForceI(forceCurrent, clampHiV, setCurrentMode: setCurrentMode, gate: gateOn ? true : null);
        }

        public virtual void ForceI(Pins pins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange = null,
            bool outputModeCurrent = false, bool gateOn = true, double? clampLoV = null) {
            pins.Ppmu?.ForceI(forceCurrent, clampHiV, clampLoV, currentRange, gateOn ? true : null);
            pins.Dcvi?.ForceI(forceCurrent, clampHiV, currentRange, voltageRange, outputModeCurrent, gateOn ? true : null);
            pins.Dcvs?.ForceI(forceCurrent, clampHiV, currentRange, voltageRange, outputModeCurrent, gateOn ? true : null);
        }

        public virtual void ForceV(Pins pins, double forceVoltage, double? clampCurrent = null, bool outputModeVoltage = false, bool gateOn = true) {
            pins.Ppmu?.ForceV(forceVoltage, clampCurrent, gateOn ? true : null);
            pins.Dcvi?.ForceV(forceVoltage, clampCurrent, setVoltageMode: outputModeVoltage, gate: gateOn ? true : null);
            pins.Dcvs?.ForceV(forceVoltage, clampCurrent, setVoltageMode: outputModeVoltage, gate: gateOn ? true : null);
        }

        public virtual void ForceV(Pins pins, double forceVoltage, double clampCurrent, double voltageRange, double? currentRange = null,
            bool outputModeVoltage = false, bool gateOn = true) {
            pins.Ppmu?.ForceV(forceVoltage, currentRange, gateOn ? true : null);
            pins.Dcvi?.ForceV(forceVoltage, clampCurrent, voltageRange, currentRange, outputModeVoltage, gateOn ? true : null);
            pins.Dcvs?.ForceV(forceVoltage, clampCurrent, voltageRange, currentRange, outputModeVoltage, gateOn ? true : null);
        }

        public virtual void Modify(Pins pins, DcParameters parameters) => Modify(pins, parameters.Gate, parameters.Mode, parameters.Voltage,
            parameters.VoltageAlt, parameters.Current, parameters.VoltageRange, parameters.CurrentRange, parameters.ForceBandwidth, parameters.MeterMode,
            parameters.MeterVoltageRange, parameters.MeterCurrentRange, parameters.MeterBandwidth, parameters.SourceFoldLimit, parameters.SinkFoldLimit,
            parameters.SourceOverloadLimit, parameters.SinkOverloadLimit, parameters.VoltageAltOutput, parameters.BleederResistor, parameters.ComplianceBoth,
            parameters.CompliancePositive, parameters.ComplianceNegative, parameters.ClampHiV, parameters.ClampLoV, parameters.HighAccuracy,
            parameters.SettlingTime, parameters.HardwareAverage);

        public virtual void Modify(Pins pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
            double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, double? sourceFoldLimit = null,
            double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null, bool? voltageAltOutput = null,
            bool? bleederResistor = null, double? complianceBoth = null, double? compliancePositive = null, double? complianceNegative = null,
            double? clampHiV = null, double? clampLoV = null, bool? highAccuracy = null, double? settlingTime = null, double? hardwareAverage = null) {

            if (pins.Ppmu != null) {
                ModifyPpmu(pins.Ppmu, gate, mode, voltage, current, currentRange, meterMode, meterCurrentRange, clampHiV, clampLoV, highAccuracy, settlingTime);
            }
            if (pins.Dcvi != null) {
                ModifyDcvi(pins.Dcvi, gate, mode, voltage, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange, meterCurrentRange,
                    meterBandwidth, bleederResistor, complianceBoth, compliancePositive, complianceNegative, hardwareAverage);
            }
            if (pins.Dcvs != null) {
                ModifyDcvs(pins.Dcvs, gate, mode, voltage, voltageAlt, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange,
                    meterCurrentRange, meterBandwidth, sourceFoldLimit, sinkFoldLimit, sourceOverloadLimit, sinkOverloadLimit, voltageAltOutput);
            }
        }

        public virtual void SetForceAndMeter(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode, double measureRange, bool gateOn = true) {
            if (mode == TLibOutputMode.ForceCurrent) {
                if (meterMode == Measure.Current) {
                    if (pins.Ppmu is not null) Api.Services.Alert.Warning("ForceISetMeterI is not supported on PPMU.");
                    pins.Dcvi?.ForceISetMeterI(forceValue, clampValue, measureRange, forceRange, gateOn ? true : null);
                    pins.Dcvs?.ForceISetMeterI(forceValue, clampValue, measureRange, forceRange, gateOn ? true : null);
                } else { // voltage
                    if (forceValue >= 0) pins.Ppmu?.ForceISetMeterV(forceValue, clampValue, pins.Ppmu.HardwareApi.ClampVLo.Min, forceRange, gateOn ? true : null);
                    else pins.Ppmu?.ForceISetMeterV(forceValue, pins.Ppmu.HardwareApi.ClampVHi.Max, clampValue, forceRange, gateOn ? true : null);
                    pins.Dcvi?.ForceISetMeterV(forceValue, clampValue, measureRange, forceRange, gateOn ? true : null);
                    pins.Dcvs?.ForceISetMeterV(forceValue, clampValue, forceRange, gateOn ? true : null);
                }
            } else if (mode == TLibOutputMode.ForceVoltage) {
                if (meterMode == Measure.Current) {
                    pins.Ppmu?.ForceVSetMeterI(forceValue, measureRange, gateOn ? true : null);
                    pins.Dcvi?.ForceVSetMeterI(forceValue, clampValue, measureRange, gateOn ? true : null);
                    pins.Dcvs?.ForceVSetMeterI(forceValue, clampValue, measureRange, gateOn ? true : null);
                } else { // voltage
                    pins.Ppmu?.ForceVSetMeterV(forceValue, measureRange, gateOn ? true : null);
                    pins.Dcvi?.ForceVSetMeterV(forceValue, clampValue, measureRange, gateOn ? true : null);
                    pins.Dcvs?.ForceVSetMeterV(forceValue, clampValue, measureRange, gateOn ? true : null);
                }
            } else { // HiZ
                pins.Ppmu?.ForceHiZ();
                pins.Dcvi?.ForceHiZ();
                pins.Dcvs?.ForceHiZ(clampValue);
                if (meterMode == Measure.Current) {
                    pins.Dcvi?.SetMeterI(measureRange);
                    pins.Dcvs?.SetMeterI(measureRange);
                } else { // Voltage
                    pins.Dcvi?.SetMeterV(measureRange);
                    pins.Dcvs?.SetMeterV(measureRange);
                }
            }
        }

        public virtual void SetMeter(Pins pins, Measure meterMode, double? rangeValue = null, double? filterValue = null, int? hardwareAverage = null,
            double? outputRangeValue = null) {
            if (meterMode == Measure.Current) {
                pins.Dcvi?.SetMeterI(rangeValue, hardwareAverage, filterValue);
                pins.Dcvs?.SetMeterI(rangeValue, filterValue, outputRangeValue);
            } else {
                pins.Dcvi?.SetMeterV(rangeValue, hardwareAverage, filterValue);
                pins.Dcvs?.SetMeterV(rangeValue, filterValue);
            }
        }

        public virtual void SetMeter(Pins[] pinGroups, Measure[] meterModes, double[] rangeValues, double[] filterValues = null, int[] hardwareAverages = null,
            double[] outputRangeValues = null) {
            Measure[] measureModes = meterModes.ToArray();
            bool meterModesUniform = measureModes.Length == 1;
            bool rangeValuesUniform = rangeValues.Length == 1;
            bool filterValuesUniform = filterValues is not null ? filterValues.Length == 1 : true;
            bool hardwareAveragesUniform = hardwareAverages is not null ? hardwareAverages.Length == 1 : true;
            bool outputRangeValuesUniform = outputRangeValues is not null ? outputRangeValues.Length == 1 : true;
            bool allUniform = meterModesUniform && rangeValuesUniform && filterValuesUniform && hardwareAveragesUniform && outputRangeValuesUniform;
            if (allUniform) {
                Api.TheLib.Setup.Dc.SetMeter(Pins.Join(pinGroups), measureModes[0], rangeValues[0], filterValues?[0], hardwareAverages?[0], outputRangeValues?[0]);
            } else {
                for (int i = 0; i < pinGroups.Length; i++) {
                    Api.TheLib.Setup.Dc.SetMeter(pinGroups[i], measureModes.SingleOrAt(i), rangeValues.SingleOrAt(i), filterValues?.SingleOrAt(i),
                        hardwareAverages?.SingleOrAt(i), outputRangeValues?.SingleOrAt(i));
                }
            }
        }

        private void ModifyPpmu(Tol.IPpmuPins ppmuPins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
             double? currentRange = null, Measure? meterMode = null, double? meterCurrentRange = null, double? clampHiV = null, double? clampLoV = null,
             bool? highAccuracy = null, double? settlingTime = null) {
            if (gate.HasValue) ppmuPins.Gate.Value = gate.Value;
            if (mode.HasValue) {
                switch (mode.Value) {
                    case TLibOutputMode.ForceVoltage: {
                            if (!voltage.HasValue) {
                                Api.Services.Alert.Error("Cannot configure ForceV and MeasureV or MeasureI on PPMU without a set value for VoltageValue.");
                            } else {
                                if (meterMode.HasValue && meterMode.Value == Measure.Voltage) {
                                    ppmuPins.HardwareApi.ForceVMeasureV(voltage.Value, meterCurrentRange ?? Type.Missing);
                                } else {
                                    ppmuPins.HardwareApi.ForceV(voltage.Value, meterCurrentRange ?? Type.Missing);
                                }
                            }
                            break;
                        }
                    case TLibOutputMode.ForceCurrent: {
                            if (!current.HasValue) {
                                Api.Services.Alert.Error("Cannot configure ForceI on PPMU without a set value for CurrentValue.");
                            } else {
                                ppmuPins.HardwareApi.ForceI(current.Value, currentRange ?? Type.Missing);
                            }
                            if (meterMode.HasValue && meterMode.Value == Measure.Current) {
                                Api.Services.Alert.Warning("ForceIMeasureI cannot be configured for PPMU. PPMU has been configured to ForceIMeasureV.");
                            }

                            break;
                        }
                    case TLibOutputMode.HighImpedance: {
                            ppmuPins.ForceI(0);
                            ppmuPins.Gate.Value = false;
                            break;
                        }
                }
            }
            if (!mode.HasValue && voltage.HasValue) ppmuPins.HardwareApi.ForceV(voltage.Value, meterCurrentRange ?? Type.Missing);
            if (!mode.HasValue && current.HasValue) ppmuPins.HardwareApi.ForceI(current.Value, currentRange ?? Type.Missing);
            if (!mode.HasValue && meterMode.HasValue) Api.Services.Alert.Error("Cannot configure MeterMode for PPMU without a set value for Mode.");
            if (clampHiV.HasValue) ppmuPins.ClampVHi.Value = clampHiV.Value;
            if (clampLoV.HasValue) ppmuPins.ClampVLo.Value = clampLoV.Value;
            if (highAccuracy.HasValue) TheHdw.PPMU.HighAccuracyMeasureVoltage.Enabled = highAccuracy.Value;
            if (settlingTime.HasValue) TheHdw.PPMU.HighAccuracyMeasureVoltage.SettlingTime = settlingTime.Value;
        }

        private void ModifyDcvi(Tol.IDcviPins dcviPins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
            double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, bool? bleederResistor = null,
            double? complianceBoth = null, double? compliancePositive = null, double? complianceNegative = null, double? hardwareAverage = null) {
            if (gate.HasValue) dcviPins.Gate.Value = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
            if (mode.HasValue) {
                tlDCVIMode forceMode = mode.Value switch {
                    TLibOutputMode.ForceCurrent => tlDCVIMode.Current,
                    TLibOutputMode.ForceVoltage => tlDCVIMode.Voltage,
                    _ => tlDCVIMode.HighImpedance,
                };
                if (forceMode == tlDCVIMode.HighImpedance) {
                    dcviPins.Gate.Value = tlDCVGate.GateOffHiZ;
                    dcviPins.Mode.Value = forceMode;
                } else {
                    dcviPins.Gate.Value = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                    dcviPins.Mode.Value = forceMode;
                    dcviPins.Gate.Value = tlDCVGate.GateOn;
                }
            }
            if (voltageRange.HasValue) dcviPins.VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) dcviPins.CurrentRange.Value = currentRange.Value;
            if (voltage.HasValue) dcviPins.Voltage.Value = voltage.Value;
            if (current.HasValue) dcviPins.Current.Value = current.Value;
            if (bleederResistor.HasValue) dcviPins.HardwareApi.BleederResistor.Mode = bleederResistor.Value ? tlDCVIBleederResistor.On : tlDCVIBleederResistor.Off;
            if (complianceBoth.HasValue) dcviPins.HardwareApi.ComplianceRange(tlDCVICompliance.Both).Value = complianceBoth.Value;
            if (compliancePositive.HasValue) dcviPins.HardwareApi.ComplianceRange(tlDCVICompliance.Positive).Value = compliancePositive.Value;
            if (complianceNegative.HasValue) dcviPins.HardwareApi.ComplianceRange(tlDCVICompliance.Negative).Value = complianceNegative.Value;
            if (forceBandwidth.HasValue) dcviPins.HardwareApi.NominalBandwidth.Value = forceBandwidth.Value;
            if (meterMode.HasValue) {
                dcviPins.Meter.Mode.Value = meterMode.Value switch {
                    Measure.Voltage => tlDCVIMeterMode.Voltage,
                    _ => tlDCVIMeterMode.Current,
                };
            }
            if (meterVoltageRange.HasValue) dcviPins.Meter.VoltageRange.Value = meterVoltageRange.Value;
            if (meterCurrentRange.HasValue) dcviPins.Meter.CurrentRange.Value = meterCurrentRange.Value;
            if (meterBandwidth.HasValue) dcviPins.Meter.Filter.Value = meterBandwidth.Value;
            if (hardwareAverage.HasValue) dcviPins.Meter.HardwareAverage.Value = hardwareAverage.Value;
        }

        private void ModifyDcvs(Tol.IDcvsPins dcvsPins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
            double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, double? sourceFoldLimit = null,
            double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null, bool? voltageAltOutput = null) {
            if (gate.HasValue) dcvsPins.Gate.Value = gate.Value;
            if (meterMode.HasValue) {
                dcvsPins.Meter.Mode.Value = meterMode.Value switch {
                    Measure.Voltage => tlDCVSMeterMode.Voltage,
                    _ => tlDCVSMeterMode.Current,
                };
            }
            if (meterVoltageRange.HasValue) dcvsPins.Meter.VoltageRange.Value = meterVoltageRange.Value;
            if (meterCurrentRange.HasValue) dcvsPins.Meter.CurrentRange.Value = meterCurrentRange.Value;
            if (meterBandwidth.HasValue) dcvsPins.Meter.Filter.Value = meterBandwidth.Value;
            if (mode.HasValue) {
                dcvsPins.Mode.Value = mode.Value switch {
                    TLibOutputMode.ForceCurrent => tlDCVSMode.Current,
                    TLibOutputMode.ForceVoltage => tlDCVSMode.Voltage,
                    _ => tlDCVSMode.HighImpedance,
                };
            }
            if (voltageRange.HasValue) dcvsPins.VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) dcvsPins.CurrentRange.Value = currentRange.Value;
            if (voltage.HasValue) dcvsPins.HardwareApi.Voltage.Main.Value = voltage.Value;
            if (voltageAlt.HasValue) dcvsPins.HardwareApi.Voltage.Alt.Value = voltageAlt.Value;
            if (voltageAltOutput.HasValue) dcvsPins.HardwareApi.Voltage.Output = voltageAltOutput.Value ? tlDCVSVoltageOutput.Alt : tlDCVSVoltageOutput.Main;
            if (current.HasValue) {
                double forceCurrent = current.Value;
                double sinkFoldLimitMax = dcvsPins.HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Max;
                dcvsPins.HardwareApi.CurrentLimit.Source.FoldLimit.Level.Value = forceCurrent;
                dcvsPins.HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Value = forceCurrent > sinkFoldLimitMax ? sinkFoldLimitMax : forceCurrent;
            }
            if (forceBandwidth.HasValue) dcvsPins.HardwareApi.BandwidthSetting.Value = forceBandwidth.Value;
            if (sourceFoldLimit.HasValue) dcvsPins.HardwareApi.CurrentLimit.Source.FoldLimit.Level.Value = sourceFoldLimit.Value;
            if (sinkFoldLimit.HasValue) dcvsPins.HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Value = sinkFoldLimit.Value;
            if (sourceOverloadLimit.HasValue) dcvsPins.HardwareApi.CurrentLimit.Source.OverloadLimit.Level.Value = sourceOverloadLimit.Value;
            if (sinkOverloadLimit.HasValue) dcvsPins.HardwareApi.CurrentLimit.Sink.OverloadLimit.Level.Value = sinkOverloadLimit.Value;
        }
    }
}

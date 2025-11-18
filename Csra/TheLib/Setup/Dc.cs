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
        public void Connect(Pins pins, bool gateOn = false) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                TheHdw.PPMU.Pins(ppmuPins).Connect();
                if (gateOn) { TheHdw.PPMU.Pins(ppmuPins).Gate = tlOnOff.On; }
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                TheHdw.DCVI.Pins(dcviPins).Connect();
                if (gateOn) { TheHdw.DCVI.Pins(dcviPins).Gate = tlDCVGate.GateOn; }
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                TheHdw.DCVS.Pins(dcvsPins).Connect();
                if (gateOn) { TheHdw.DCVS.Pins(dcvsPins).Gate = true; }
            }
        }

        public void ConnectAllPins() => TheHdw.Digital.ApplyLevelsTiming(true);

        public void Disconnect(Pins pins, bool gateOff = true) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                if (gateOff) { TheHdw.PPMU.Pins(ppmuPins).Gate = tlOnOff.Off; }
                TheHdw.PPMU.Pins(ppmuPins).Disconnect();
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                if (gateOff) { TheHdw.DCVI.Pins(dcviPins).Gate = tlDCVGate.GateOffHiZ; }
                TheHdw.DCVI.Pins(dcviPins).Disconnect();
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                if (gateOff) { TheHdw.DCVS.Pins(dcvsPins).Gate = false; }
                TheHdw.DCVS.Pins(dcvsPins).Disconnect();
            }
        }
        
        public void Force(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, bool gateOn = true) {
            if (mode == TLibOutputMode.ForceVoltage) {
                ForceV(pins, forceValue, clampValue, forceRange, clampValue, true, gateOn);
            } else if (mode == TLibOutputMode.ForceCurrent) {
                ForceI(pins, forceValue, clampValue, forceRange, clampValue, true, gateOn);
            } else {
                ForceHiZ(pins);
            }
        }

        public void Force(Pins[] pinGroups, TLibOutputMode[] modes, double[] forceValues, double[] forceRanges, double[] clampValues,
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
        
        public void ForceHiZ(Pins pins) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                ForceHiZPPMU(ppmuPins);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                ForceHiZDCVI(dcviPins);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                ForceHiZDCVS(dcvsPins);
            }
        }

        public void ForceI(Pins pins, double forceCurrent, double? clampHiV = null, double? clampLoV = null, bool outputModeCurrent = false,
            bool gateOn = true) { //clampVoltage has been added to reduce the risk of breaking the device
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) { // assuming current range is already set
                var ppmu = TheHdw.PPMU.Pins(ppmuPins);
                ppmu.ForceI(forceCurrent);
                if (clampHiV.HasValue) ppmu.ClampVHi.Value = clampHiV.Value;
                if (clampLoV.HasValue) ppmu.ClampVLo.Value = clampLoV.Value;
                if (gateOn) ppmu.Gate = tlOnOff.On;
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) { // assuming the mode is already set
                var dcvi = TheHdw.DCVI.Pins(dcviPins);
                if (outputModeCurrent) {
                    dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                    dcvi.Mode = tlDCVIMode.Current;   // set to force Current
                }
                if (clampHiV.HasValue) dcvi.Voltage.Value = clampHiV.Value;
                dcvi.Current.Value = forceCurrent;
                if (gateOn) dcvi.Gate = tlDCVGate.GateOn;
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) { // assuming that the voltage and the range is already set
                var dcvs = TheHdw.DCVS.Pins(dcvsPins);
                if (outputModeCurrent) dcvs.Mode = tlDCVSMode.Current;
                if (clampHiV.HasValue) dcvs.Voltage.Value = clampHiV.Value;
                double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
                dcvs.CurrentLimit.Source.FoldLimit.Level.Value = forceCurrent;
                dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = forceCurrent > sinkFoldLimit ? sinkFoldLimit : forceCurrent;
                if (gateOn) dcvs.Gate = true;
            }
        }

        public void ForceI(Pins pins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange = null,
            bool outputModeCurrent = false, bool gateOn = true, double? clampLoV = null) =>
            ForceIsub(pins, forceCurrent, clampHiV, currentRange, voltageRange, outputModeCurrent, clampLoV, gateOn);

        public void ForceV(Pins pins, double forceVoltage, double? clampCurrent = null, bool outputModeVoltage = false, bool gateOn = true) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                var ppmu = TheHdw.PPMU.Pins(ppmuPins);
                if (clampCurrent.HasValue) {
                    ppmu.ForceV(forceVoltage, clampCurrent);
                } else {
                    ppmu.ForceV(forceVoltage);
                }
                if (gateOn) ppmu.Gate = tlOnOff.On;
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                var dcvi = TheHdw.DCVI.Pins(dcviPins);
                if (outputModeVoltage) {
                    dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                    dcvi.Mode = tlDCVIMode.Voltage; // set the DCVI to force Voltage mode
                }
                dcvi.Voltage.Value = forceVoltage;
                if (clampCurrent.HasValue) dcvi.Current.Value = clampCurrent.Value;
                if (gateOn) dcvi.Gate = tlDCVGate.GateOn;
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                var dcvs = TheHdw.DCVS.Pins(dcvsPins);
                if (outputModeVoltage) dcvs.Mode = tlDCVSMode.Voltage;
                dcvs.Voltage.Value = forceVoltage;
                if (clampCurrent.HasValue) {
                    double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
                    dcvs.CurrentLimit.Source.FoldLimit.Level.Value = clampCurrent.Value;
                    dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = clampCurrent.Value > sinkFoldLimit ? sinkFoldLimit : clampCurrent.Value;
                }
                if (gateOn) dcvs.Gate = true;
            }
        }

        public void ForceV(Pins pins, double forceVoltage, double clampCurrent, double voltageRange, double? currentRange = null,
            bool outputModeVoltage = false, bool gateOn = true) =>
            ForceVsub(pins, forceVoltage, clampCurrent, voltageRange, currentRange, outputModeVoltage, gateOn);

        public void Modify(Pins pins, DcParameters parameters) => Modify(pins, parameters.Gate, parameters.Mode, parameters.Voltage,
            parameters.VoltageAlt, parameters.Current, parameters.VoltageRange, parameters.CurrentRange, parameters.ForceBandwidth, parameters.MeterMode,
            parameters.MeterVoltageRange, parameters.MeterCurrentRange, parameters.MeterBandwidth, parameters.SourceFoldLimit, parameters.SinkFoldLimit,
            parameters.SourceOverloadLimit, parameters.SinkOverloadLimit, parameters.VoltageAltOutput, parameters.BleederResistor, parameters.ComplianceBoth,
            parameters.CompliancePositive, parameters.ComplianceNegative, parameters.ClampHiV, parameters.ClampLoV, parameters.HighAccuracy,
            parameters.SettlingTime, parameters.HardwareAverage);

        public void Modify(Pins pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
            double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, double? sourceFoldLimit = null,
            double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null, bool? voltageAltOutput = null,
            bool? bleederResistor = null, double? complianceBoth = null, double? compliancePositive = null, double? complianceNegative = null,
            double? clampHiV = null, double? clampLoV = null, bool? highAccuracy = null, double? settlingTime = null, double? hardwareAverage = null) {

            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                ModifyPpmu(ppmuPins, gate, mode, voltage, current, currentRange, meterMode, meterCurrentRange, clampHiV, clampLoV, highAccuracy, settlingTime);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                ModifyDcvi(dcviPins, gate, mode, voltage, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange, meterCurrentRange,
                    meterBandwidth, bleederResistor, complianceBoth, compliancePositive, complianceNegative, hardwareAverage);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                ModifyDcvs(dcvsPins, gate, mode, voltage, voltageAlt, current, voltageRange, currentRange, forceBandwidth, meterMode, meterVoltageRange,
                    meterCurrentRange, meterBandwidth, sourceFoldLimit, sinkFoldLimit, sourceOverloadLimit, sinkOverloadLimit, voltageAltOutput);
            }
        }

        public void SetForceAndMeter(Pins pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode, double measureRange, bool gateOn = true) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                SetForceAndMeterPpmu(ppmuPins, mode, forceValue, forceRange, clampValue, meterMode, measureRange, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                SetForceAndMeterDcvi(dcviPins, mode, forceValue, forceRange, clampValue, meterMode, measureRange, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                SetForceAndMeterDcvs(dcvsPins, mode, forceValue, forceRange, clampValue, meterMode, measureRange, gateOn);
            }
        }

        public void SetMeter(Pins pins, Measure meterMode, double rangeValue, double? filterValue = null, int? hardwareAverage = null,
            double? outputRangeValue = null) {
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                MeterDcvi(dcviPins, meterMode, rangeValue, filterValue, hardwareAverage);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) {
                MeterDcvs(dcvsPins, meterMode, rangeValue, filterValue, outputRangeValue);
            }
        }

        public void SetMeter(Pins[] pinGroups, Measure[] meterModes, double[] rangeValues, double[] filterValues = null, int[] hardwareAverages = null,
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

        private void ForceHiZPPMU(string pins) {
            var ppmu = TheHdw.PPMU.Pins(pins);
            ppmu.ForceI(0);
            ppmu.Gate = tlOnOff.Off;
        }

        private void ForceHiZDCVI(string pins) {
            var dcvi = TheHdw.DCVI.Pins(pins);
            dcvi.Gate = tlDCVGate.GateOffHiZ;
            dcvi.Disconnect(tlDCVIConnectWhat.Default);
            dcvi.Mode = tlDCVIMode.HighImpedance;
            dcvi.Connect(tlDCVIConnectWhat.HighSense);
        }

        private void ForceHiZDCVS(string pins) {
            TheHdw.DCVS.Pins(pins).Mode = tlDCVSMode.HighImpedance;
        }

        private void ForceIsub(Pins pins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange, bool outputModeCurrent,
            double? clampLoV, bool gateOn) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                ForceCurrentPpmu(ppmuPins, forceCurrent, currentRange, clampHiV, clampLoV, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) { // assuming the mode is already set
                ForceCurrentDcvi(dcviPins, forceCurrent, clampHiV, currentRange, voltageRange, outputModeCurrent, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) { // assuming that the voltage and the mode is already set
                ForceCurrentDcvs(dcvsPins, forceCurrent, clampHiV, currentRange, voltageRange, outputModeCurrent, gateOn);
            }
        }

        private void ForceCurrentPpmu(string ppmuPins, double forceCurrent, double currentRange, double clampHiV, double? clampLoV, bool gateOn) {
            var ppmu = TheHdw.PPMU.Pins(ppmuPins);
            ppmu.ForceI(forceCurrent, currentRange); // this will automatically also change the mode to forceCurrent
            ppmu.ClampVHi.Value = clampHiV;
            if (clampLoV.HasValue) ppmu.ClampVLo.Value = clampLoV.Value;
            if (gateOn) ppmu.Gate = tlOnOff.On;
        }

        private void ForceCurrentDcvi(string dcviPins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange,
            bool outputModeCurrent, bool gateOn) {
            var dcvi = TheHdw.DCVI.Pins(dcviPins);
            if (outputModeCurrent) {
                dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                dcvi.Mode = tlDCVIMode.Current;   // set to force Current
            }
            if (voltageRange.HasValue) {
                dcvi.SetVoltageAndRange(clampHiV, voltageRange.Value);
            } else {
                dcvi.Voltage.Value = clampHiV;
            }
            dcvi.SetCurrentAndRange(forceCurrent, currentRange);
            if (gateOn) dcvi.Gate = tlDCVGate.GateOn;
        }

        private void ForceCurrentDcvs(string dcvsPins, double forceCurrent, double clampHiV, double currentRange, double? voltageRange,
            bool outputModeCurrent, bool gateOn) {
            var dcvs = TheHdw.DCVS.Pins(dcvsPins);
            if (outputModeCurrent) dcvs.Mode = tlDCVSMode.Current;
            dcvs.Voltage.Value = clampHiV;
            if (voltageRange.HasValue) dcvs.VoltageRange.Value = voltageRange.Value;
            dcvs.CurrentRange.Value = currentRange;
            double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
            dcvs.CurrentLimit.Source.FoldLimit.Level.Value = forceCurrent;
            dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = forceCurrent > sinkFoldLimit ? sinkFoldLimit : forceCurrent;
            if (gateOn) dcvs.Gate = true;
        }

        private void ForceVsub(Pins pins, double forceVoltage, double currentClamp, double voltageRange, double? currentRange,
            bool outputModeVoltage, bool gateOn) {
            if (pins.ContainsFeature(InstrumentFeature.Ppmu, out string ppmuPins)) {
                ForceVoltagePpmu(ppmuPins, forceVoltage, currentRange, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvi, out string dcviPins)) {
                ForceVoltageDcvi(dcviPins, forceVoltage, currentClamp, voltageRange, currentRange, outputModeVoltage, gateOn);
            }
            if (pins.ContainsFeature(InstrumentFeature.Dcvs, out string dcvsPins)) { // does not require anything different when in FVMV mode
                ForceVoltageDcvs(dcvsPins, forceVoltage, currentClamp, voltageRange, currentRange, outputModeVoltage, gateOn);
            }
        }

        private void ForceVoltagePpmu(string ppmuPins, double forceVoltage, double? currentRange, bool gateOn) {
            var ppmu = TheHdw.PPMU.Pins(ppmuPins);
            if (currentRange.HasValue) {
                ppmu.ForceV(forceVoltage, currentRange);
            } else {
                ppmu.ForceV(forceVoltage);
            }
            if (gateOn) ppmu.Gate = tlOnOff.On;
        }

        private void ForceVoltageDcvi(string dcviPins, double forceVoltage, double currentClamp, double voltageRange, double? currentRange,
            bool outputModeVoltage, bool gateOn) {
            var dcvi = TheHdw.DCVI.Pins(dcviPins);
            if (outputModeVoltage) {
                dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                dcvi.Mode = tlDCVIMode.Voltage; // set the DCVI to force Voltage mode
            }
            if (currentRange.HasValue) {
                dcvi.SetCurrentAndRange(currentClamp, currentRange.Value);
            } else {
                dcvi.Current.Value = currentClamp;
            }
            dcvi.SetVoltageAndRange(forceVoltage, voltageRange);
            if (gateOn) dcvi.Gate = tlDCVGate.GateOn;
        }

        private void ForceVoltageDcvs(string dcvsPins, double forceVoltage, double currentClamp, double voltageRange, double? currentRange,
            bool outputModeVoltage, bool gateOn) {
            var dcvs = TheHdw.DCVS.Pins(dcvsPins);
            if (outputModeVoltage) dcvs.Mode = tlDCVSMode.Voltage;
            if (currentRange.HasValue) dcvs.CurrentRange.Value = currentRange.Value;
            dcvs.VoltageRange.Value = voltageRange;
            dcvs.Voltage.Value = forceVoltage;
            double sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
            dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = currentClamp > sinkFoldLimit ? sinkFoldLimit : currentClamp;
            dcvs.CurrentLimit.Source.FoldLimit.Level.Value = currentClamp;
            if (gateOn) dcvs.Gate = true;
        }

        private void ModifyPpmu(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
             double? currentRange = null, Measure? meterMode = null, double? meterCurrentRange = null, double? clampHiV = null, double? clampLoV = null,
             bool? highAccuracy = null, double? settlingTime = null) {
            var ppmu = TheHdw.PPMU.Pins(pins);
            if (gate.HasValue) ppmu.Gate = gate.Value ? tlOnOff.On : tlOnOff.Off;
            if (mode.HasValue) {
                switch (mode.Value) {
                    case TLibOutputMode.ForceVoltage: {
                            if (!voltage.HasValue) {
                                Api.Services.Alert.Error("Cannot configure ForceV and MeasureV or MeasureI on PPMU without a set value for VoltageValue.");
                            } else {
                                if (meterMode.HasValue && meterMode.Value == Measure.Voltage) {
                                    ppmu.ForceVMeasureV(voltage.Value, meterCurrentRange ?? Type.Missing);
                                } else {
                                    ppmu.ForceV(voltage.Value, meterCurrentRange ?? Type.Missing);
                                }
                            }
                            break;
                        }
                    case TLibOutputMode.ForceCurrent: {
                            if (!current.HasValue) {
                                Api.Services.Alert.Error("Cannot configure ForceI on PPMU without a set value for CurrentValue.");
                            } else {
                                ppmu.ForceI(current.Value, currentRange ?? Type.Missing);
                            }
                            if (meterMode.HasValue && meterMode.Value == Measure.Current) {
                                Api.Services.Alert.Warning("ForceIMeasureI cannot be configured for PPMU. PPMU has been configured to ForceIMeasureV.");
                            }

                            break;
                        }
                    case TLibOutputMode.HighImpedance: {
                            ppmu.ForceI(0);
                            ppmu.Gate = tlOnOff.Off;
                            break;
                        }
                }
            }
            if (!mode.HasValue && voltage.HasValue) ppmu.ForceV(voltage.Value, meterCurrentRange ?? Type.Missing);
            if (!mode.HasValue && current.HasValue) ppmu.ForceI(current.Value, currentRange ?? Type.Missing);
            if (!mode.HasValue && meterMode.HasValue) Api.Services.Alert.Error("Cannot configure MeterMode for PPMU without a set value for Mode.");
            if (clampHiV.HasValue) ppmu.ClampVHi.Value = clampHiV.Value;
            if (clampLoV.HasValue) ppmu.ClampVLo.Value = clampLoV.Value;
            if (highAccuracy.HasValue) TheHdw.PPMU.HighAccuracyMeasureVoltage.Enabled = highAccuracy.Value;
            if (settlingTime.HasValue) TheHdw.PPMU.HighAccuracyMeasureVoltage.SettlingTime = settlingTime.Value;
        }

        private void ModifyDcvi(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? current = null,
            double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, bool? bleederResistor = null,
            double? complianceBoth = null, double? compliancePositive = null, double? complianceNegative = null, double? hardwareAverage = null) {
            var dcvi = TheHdw.DCVI.Pins(pins);
            if (gate.HasValue) dcvi.Gate = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
            if (mode.HasValue) {
                tlDCVIMode forceMode = mode.Value switch {
                    TLibOutputMode.ForceCurrent => tlDCVIMode.Current,
                    TLibOutputMode.ForceVoltage => tlDCVIMode.Voltage,
                    _ => tlDCVIMode.HighImpedance,
                };
                if (forceMode == tlDCVIMode.HighImpedance) {
                    dcvi.Gate = tlDCVGate.GateOffHiZ;
                    dcvi.Mode = forceMode;
                } else {
                    dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                    dcvi.Mode = forceMode;
                    dcvi.Gate = tlDCVGate.GateOn;
                }
            }
            if (voltageRange.HasValue) dcvi.VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) dcvi.CurrentRange.Value = currentRange.Value;
            if (voltage.HasValue) dcvi.Voltage.Value = voltage.Value;
            if (current.HasValue) dcvi.Current.Value = current.Value;
            if (bleederResistor.HasValue) dcvi.BleederResistor.Mode = bleederResistor.Value ? tlDCVIBleederResistor.On : tlDCVIBleederResistor.Off;
            if (complianceBoth.HasValue) dcvi.ComplianceRange(tlDCVICompliance.Both).Value = complianceBoth.Value;
            if (compliancePositive.HasValue) dcvi.ComplianceRange(tlDCVICompliance.Positive).Value = compliancePositive.Value;
            if (complianceNegative.HasValue) dcvi.ComplianceRange(tlDCVICompliance.Negative).Value = complianceNegative.Value;
            if (forceBandwidth.HasValue) dcvi.NominalBandwidth.Value = forceBandwidth.Value;
            if (meterMode.HasValue) {
                dcvi.Meter.Mode = meterMode.Value switch {
                    Measure.Voltage => tlDCVIMeterMode.Voltage,
                    _ => tlDCVIMeterMode.Current,
                };
            }
            if (meterVoltageRange.HasValue) dcvi.Meter.VoltageRange.Value = meterVoltageRange.Value;
            if (meterCurrentRange.HasValue) dcvi.Meter.CurrentRange.Value = meterCurrentRange.Value;
            if (meterBandwidth.HasValue) dcvi.Meter.Filter.Value = meterBandwidth.Value;
            if (hardwareAverage.HasValue) dcvi.Meter.HardwareAverage.Value = hardwareAverage.Value;
        }

        private void ModifyDcvs(string pins, bool? gate = null, TLibOutputMode? mode = null, double? voltage = null, double? voltageAlt = null,
            double? current = null, double? voltageRange = null, double? currentRange = null, double? forceBandwidth = null, Measure? meterMode = null,
            double? meterVoltageRange = null, double? meterCurrentRange = null, double? meterBandwidth = null, double? sourceFoldLimit = null,
            double? sinkFoldLimit = null, double? sourceOverloadLimit = null, double? sinkOverloadLimit = null, bool? voltageAltOutput = null) {
            var dcvs = TheHdw.DCVS.Pins(pins);
            if (gate.HasValue) dcvs.Gate = gate.Value;
            if (meterMode.HasValue) {
                dcvs.Meter.Mode = meterMode.Value switch {
                    Measure.Voltage => tlDCVSMeterMode.Voltage,
                    _ => tlDCVSMeterMode.Current,
                };
            }
            if (meterVoltageRange.HasValue) dcvs.Meter.VoltageRange.Value = meterVoltageRange.Value;
            if (meterCurrentRange.HasValue) dcvs.Meter.CurrentRange.Value = meterCurrentRange.Value;
            if (meterBandwidth.HasValue) dcvs.Meter.Filter.Value = meterBandwidth.Value;
            if (mode.HasValue) {
                dcvs.Mode = mode.Value switch {
                    TLibOutputMode.ForceCurrent => tlDCVSMode.Current,
                    TLibOutputMode.ForceVoltage => tlDCVSMode.Voltage,
                    _ => tlDCVSMode.HighImpedance,
                };
            }
            if (voltageRange.HasValue) dcvs.VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) dcvs.CurrentRange.Value = currentRange.Value;
            if (voltage.HasValue) dcvs.Voltage.Main.Value = voltage.Value;
            if (voltageAlt.HasValue) dcvs.Voltage.Alt.Value = voltageAlt.Value;
            if (voltageAltOutput.HasValue) dcvs.Voltage.Output = voltageAltOutput.Value ? tlDCVSVoltageOutput.Alt : tlDCVSVoltageOutput.Main;
            if (current.HasValue) {
                double forceCurrent = current.Value;
                double sinkFoldLimitMax = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;
                dcvs.CurrentLimit.Source.FoldLimit.Level.Value = forceCurrent;
                dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = forceCurrent > sinkFoldLimitMax ? sinkFoldLimitMax : forceCurrent;
            }
            if (forceBandwidth.HasValue) dcvs.BandwidthSetting.Value = forceBandwidth.Value;
            if (sourceFoldLimit.HasValue) dcvs.CurrentLimit.Source.FoldLimit.Level.Value = sourceFoldLimit.Value;
            if (sinkFoldLimit.HasValue) dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = sinkFoldLimit.Value;
            if (sourceOverloadLimit.HasValue) dcvs.CurrentLimit.Source.OverloadLimit.Level.Value = sourceOverloadLimit.Value;
            if (sinkOverloadLimit.HasValue) dcvs.CurrentLimit.Sink.OverloadLimit.Level.Value = sinkOverloadLimit.Value;
        }

        private void SetForceAndMeterPpmu(string pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode,
            double measureRange, bool gateOn) {
            var ppmu = TheHdw.PPMU.Pins(pins);
            if (mode == TLibOutputMode.ForceVoltage) {
                if (meterMode == Measure.Voltage) {
                    ppmu.ForceVMeasureV(forceValue, ppmu.MeasureCurrentRange.Max);
                } else {
                    ppmu.ForceV(forceValue, measureRange);
                }
            } else if (mode == TLibOutputMode.ForceCurrent) {
                ppmu.ForceI(forceValue, forceRange);
                if (meterMode == Measure.Current) {
                    Api.Services.Alert.Warning("ForceIMeasureI cannot be configured for PPMU. PPMU has been configured to ForceIMeasureV.");
                }
                if (forceValue >= 0) {
                    ppmu.ClampVHi.Value = clampValue > ppmu.ClampVHi.Max ? ppmu.ClampVHi.Max : clampValue;
                    ppmu.ClampVLo.Value = ppmu.ClampVLo.Min;
                } else {
                    ppmu.ClampVHi.Value = ppmu.ClampVHi.Max;
                    ppmu.ClampVLo.Value = clampValue < ppmu.ClampVLo.Min ? ppmu.ClampVLo.Min : clampValue;
                }
            } else { //HiZ
                ppmu.ForceI(0);
                ppmu.Gate = tlOnOff.Off;
            }
            if (mode != TLibOutputMode.HighImpedance && gateOn) ppmu.Gate = tlOnOff.On;
        }

        private void SetForceAndMeterDcvi(string pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode,
            double measureRange, bool gateOn) {
            var dcvi = TheHdw.DCVI.Pins(pins);
            if (mode == TLibOutputMode.ForceVoltage) {
                dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                dcvi.Mode = tlDCVIMode.Voltage;
                //dcvi.SetVoltageAndRange(forceValue, forceRange);
                //Since ApplyLevelsTiming does not configure the voltage range of the power supplies,
                //the voltage range should only be modified when necessary, using the Modify test block.
                dcvi.Voltage.Value = forceValue;
                dcvi.Current.Value = clampValue;
            } else if (mode == TLibOutputMode.ForceCurrent) {
                dcvi.Gate = tlDCVGate.GateOff; // this is to avoid glitches while changing the mode
                dcvi.Mode = tlDCVIMode.Current;
                dcvi.SetCurrentAndRange(forceValue, forceRange);
                dcvi.Voltage.Value = clampValue;
            } else { //HiZ
                dcvi.Gate = tlDCVGate.GateOffHiZ;
                dcvi.Disconnect(tlDCVIConnectWhat.Default);
                dcvi.Mode = tlDCVIMode.HighImpedance;
                dcvi.Connect(tlDCVIConnectWhat.HighSense);
            }
            if (meterMode == Measure.Voltage) {
                dcvi.Meter.Mode = tlDCVIMeterMode.Voltage;
                dcvi.Meter.VoltageRange.Value = measureRange;
            } else {
                dcvi.Meter.Mode = tlDCVIMeterMode.Current;
                dcvi.Meter.CurrentRange.Value = measureRange;
            }
            if (mode != TLibOutputMode.HighImpedance && gateOn) dcvi.Gate = tlDCVGate.GateOn;
        }

        private void SetForceAndMeterDcvs(string pins, TLibOutputMode mode, double forceValue, double forceRange, double clampValue, Measure meterMode,
            double measureRange, bool gateOn) {
            var dcvs = TheHdw.DCVS.Pins(pins);
            double sinkFoldLimit;
            if (meterMode == Measure.Voltage) {             // set the meter mode first to get the right measure ranges
                dcvs.Meter.Mode = tlDCVSMeterMode.Voltage;
            } else {
                dcvs.Meter.Mode = tlDCVSMeterMode.Current;
            }
            if (mode == TLibOutputMode.ForceVoltage) {
                dcvs.Mode = tlDCVSMode.Voltage;
                //dcvs.VoltageRange.Value = forceRange;
                //Since ApplyLevelsTiming does not configure the voltage range of the power supplies,
                //the voltage range should only be modified when necessary, using the Modify test block.
                dcvs.Voltage.Value = forceValue;
                if (meterMode == Measure.Voltage) {
                    dcvs.CurrentRange.Value = clampValue;
                } else {
                    dcvs.SetCurrentRanges(clampValue, measureRange);
                }
                sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;     // returned values depend on force mode and ranges
                dcvs.CurrentLimit.Source.FoldLimit.Level.Value = clampValue;
                dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = clampValue > sinkFoldLimit ? sinkFoldLimit : clampValue;
            } else if (mode == TLibOutputMode.ForceCurrent) {
                dcvs.Mode = tlDCVSMode.Current;
                dcvs.Voltage.Value = clampValue;
                if (meterMode == Measure.Voltage) {
                    dcvs.CurrentRange.Value = forceRange;
                } else {
                    dcvs.SetCurrentRanges(forceRange, measureRange);
                }
                sinkFoldLimit = dcvs.CurrentLimit.Sink.FoldLimit.Level.Max;     // returned values depend on force mode and ranges
                dcvs.CurrentLimit.Sink.FoldLimit.Level.Value = forceValue > sinkFoldLimit ? sinkFoldLimit : forceValue;
                dcvs.CurrentLimit.Source.FoldLimit.Level.Value = forceValue;
            } else {//HiZ
                dcvs.Mode = tlDCVSMode.HighImpedance;
                dcvs.Voltage.Value = clampValue;
            }
            if (gateOn) dcvs.Gate = true;
        }

        private void MeterDcvi(string pins, Measure meterMode, double rangeValue, double? filterValue = null, int? hardwareAverage = null) {
            var dcvi = TheHdw.DCVI.Pins(pins).Meter;
            if (meterMode == Measure.Voltage) {
                dcvi.Mode = tlDCVIMeterMode.Voltage;
                dcvi.VoltageRange.Value = rangeValue;

            } else {
                dcvi.Mode = tlDCVIMeterMode.Current;
                dcvi.CurrentRange.Value = rangeValue;
            }
            if (filterValue.HasValue) dcvi.Filter.Value = filterValue.Value;
            if (hardwareAverage.HasValue) dcvi.HardwareAverage.Value = hardwareAverage.Value;
        }
        private void MeterDcvs(string pins, Measure meterMode, double meterRangeValue, double? filterValue = null, double? outputRangeValue = null) {
            var dcvs = TheHdw.DCVS.Pins(pins);
            if (meterMode == Measure.Voltage) {
                dcvs.Meter.Mode = tlDCVSMeterMode.Voltage;
            } else {
                dcvs.Meter.Mode = tlDCVSMeterMode.Current;
                if (outputRangeValue.HasValue) dcvs.SetCurrentRanges(outputRangeValue.Value, meterRangeValue);
            }
            if (filterValue.HasValue) dcvs.Meter.Filter.Value = filterValue.Value;
        }
    }
}

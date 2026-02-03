using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    [Serializable]
    public class DcviPins : IDcviPins {

        [NonSerialized]
        private protected DriverDCVIPins _hardwareApi; // IG-XL object
        [NonSerialized]
        private IValuePerSite<tlDCVGate> _gate;
        [NonSerialized]
        private IValuePerSite<double> _voltage;
        [NonSerialized]
        private IValuePerSite<double> _current;
        [NonSerialized]
        private IValuePerSite<tlDCVIMode> _mode;
        [NonSerialized]
        private IValuePerSiteRange<double> _voltageRange;
        [NonSerialized]
        private IValuePerSiteRange<double> _currentRange;
        [NonSerialized]
        private IValuePerSiteRange<double> _complianceRangePositive;
        [NonSerialized]
        private IValuePerSiteRange<double> _complianceRangeNegative;
        [NonSerialized]
        private IValuePerSiteRange<double> _bandwidth;
        private MeterImpl _meter;
        private string _name;

        internal DcviPins(string pinList) : this(pinList, TheHdw.DCVI.Pins(pinList)) { }

        internal DcviPins(string pinList, DriverDCVIPins driverDCVIPins) {
            _name = pinList;
            _hardwareApi = driverDCVIPins;
            _ = Meter; // prevent lazy loading
            _ = Mode;
            _ = Voltage;
            _ = VoltageRange;
            _ = Current;
            _ = CurrentRange;
            _ = Gate;
            _ = ComplianceRangePositive;
            _ = ComplianceRangeNegative;
            _ = Bandwidth;
        }

        public DriverDCVIPins HardwareApi => _hardwareApi ??= TheHdw.DCVI.Pins(Name);

        public IValuePerSite<tlDCVIMode> Mode => _mode ??= new ValuePerSiteType<tlDCVIMode>(
                   setValue: value => { HardwareApi.Mode = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.Mode = siteValues[site]; });
                   });

        public IValuePerSite<double> Voltage => _voltage ??= new ValuePerSiteType<double>(
                   setValue: value => { HardwareApi.Voltage.Value = value; },
                   setValuePerSite: siteValues => { HardwareApi.Voltage.ValuePerSite = siteValues.ToSiteDouble(); });

        public IValuePerSiteRange<double> VoltageRange => _voltageRange ??= new ValuePerSiteRangeType<double>(
                   setValue: value => { HardwareApi.VoltageRange.Value = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.VoltageRange.Value = siteValues[site]; });
                   },
                   getMin: () => HardwareApi.VoltageRange.Min,
                   getMax: () => HardwareApi.VoltageRange.Max);

        public IValuePerSite<double> Current => _current ??= new ValuePerSiteType<double>(
                   setValue: value => { HardwareApi.Current.Value = value; },
                   setValuePerSite: siteValues => { HardwareApi.Current.ValuePerSite = siteValues.ToSiteDouble(); });

        public IValuePerSiteRange<double> CurrentRange => _currentRange ??= new ValuePerSiteRangeType<double>(
                   setValue: value => { HardwareApi.CurrentRange.Value = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.CurrentRange.Value = siteValues[site]; });
                   },
                   getMin: () => HardwareApi.CurrentRange.Min,
                   getMax: () => HardwareApi.CurrentRange.Max);

        public IValuePerSite<tlDCVGate> Gate => _gate ??= new ValuePerSiteType<tlDCVGate>(
                   setValue: value => { HardwareApi.Gate = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.Gate = siteValues[site]; });
                   });

        public IValuePerSiteRange<double> ComplianceRangePositive => _complianceRangePositive ??= new ValuePerSiteRangeType<double>(
                   setValue: value => { HardwareApi.ComplianceRange(tlDCVICompliance.Positive).Value = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.ComplianceRange(tlDCVICompliance.Positive).Value = siteValues[site]; });
                   },
                   getMin: () => HardwareApi.ComplianceRange(tlDCVICompliance.Positive).Min,
                   getMax: () => HardwareApi.ComplianceRange(tlDCVICompliance.Positive).Max);

        public IValuePerSiteRange<double> ComplianceRangeNegative => _complianceRangeNegative ??= new ValuePerSiteRangeType<double>(
                   setValue: value => { HardwareApi.ComplianceRange(tlDCVICompliance.Negative).Value = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.ComplianceRange(tlDCVICompliance.Negative).Value = siteValues[site]; });
                   },
                   getMin: () => HardwareApi.ComplianceRange(tlDCVICompliance.Negative).Min,
                   getMax: () => HardwareApi.ComplianceRange(tlDCVICompliance.Negative).Max);

        public IValuePerSiteRange<double> Bandwidth => _bandwidth ??= new ValuePerSiteRangeType<double>(
                   setValue: value => { HardwareApi.Bandwidth.Value = value; },
                   setValuePerSite: siteValues => {
                       ForEachSite(site => { HardwareApi.Bandwidth.Value = siteValues[site]; });
                   },
                   getMin: () => HardwareApi.Bandwidth.Min,
                   getMax: () => HardwareApi.Bandwidth.Max);

        public IDcviMeter Meter => _meter ??= new MeterImpl(Name, HardwareApi.Meter);

        [Serializable]
        private class MeterImpl : IDcviMeter {

            [NonSerialized]
            private DriverDCVIMeter _temp; // IG-XL object
            [NonSerialized]
            private IValuePerSite<tlDCVIMeterMode> _mode;
            [NonSerialized]
            private IValuePerSiteRange<double> _currentRange;
            [NonSerialized]
            private IValuePerSiteRange<double> _voltageRange;
            [NonSerialized]
            private IValuePerSiteRange<double> _filter;
            [NonSerialized]
            private IValuePerSiteRange<double> _hardwareAverage;
            private string _name;

            public MeterImpl(string name, DriverDCVIMeter driverDCVIMeter) {
                _name = name;
                _temp = driverDCVIMeter;
                _ = CurrentRange; // prevent lazy loading
                _ = Mode;
                _ = Filter;
                _ = HardwareAverage;
                _ = VoltageRange;
            }

            private DriverDCVIMeter _meter => _temp ??= TheHdw.DCVI.Pins(_name).Meter;

            public IValuePerSiteRange<double> CurrentRange => _currentRange ??= new ValuePerSiteRangeType<double>(
                       setValue: value => _meter.CurrentRange.Value = value,
                       setValuePerSite: siteValues => ForEachSite(site => { _meter.CurrentRange.Value = siteValues[site]; }),
                       getMin: () => _meter.CurrentRange.Min,
                       getMax: () => _meter.CurrentRange.Max);

            public IValuePerSiteRange<double> VoltageRange => _voltageRange ??= new ValuePerSiteRangeType<double>(
                       setValue: value => { _meter.VoltageRange.Value = value; },
                       setValuePerSite: siteValues => {
                           ForEachSite(site => { _meter.VoltageRange.Value = siteValues[site]; });
                       },
                       getMin: () => _meter.VoltageRange.Min,
                       getMax: () => _meter.VoltageRange.Max);

            public IValuePerSite<tlDCVIMeterMode> Mode => _mode ??= new ValuePerSiteType<tlDCVIMeterMode>(
                       setValue: value => { _meter.Mode = value; },
                       setValuePerSite: siteValues => {
                           ForEachSite(site => { _meter.Mode = siteValues[site]; });
                       });

            public IValuePerSiteRange<double> Filter => _filter ??= new ValuePerSiteRangeType<double>(
                       setValue: value => { _meter.Filter.Value = value; },
                       setValuePerSite: siteValues => {
                           ForEachSite(site => { _meter.Filter.Value = siteValues[site]; });
                       },
                       getMin: () => _meter.Filter.Min,
                       getMax: () => _meter.Filter.Max);

            public IValuePerSiteRange<double> HardwareAverage => _hardwareAverage ??= new ValuePerSiteRangeType<double>(
                       setValue: value => { _meter.HardwareAverage.Value = value; },
                       setValuePerSite: siteValues => {
                           ForEachSite(site => { _meter.HardwareAverage.Value = siteValues[site]; });
                       },
                       getMin: () => _meter.HardwareAverage.Min,
                       getMax: () => _meter.HardwareAverage.Max);
        }

        public string Name => _name;

        public virtual void Connect(bool? gate = null) {
            HardwareApi.Connect();
            if (gate.HasValue) Gate.Value = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
        }

        public virtual void Disconnect(bool? gate = null) {
            if (gate.HasValue) Gate.Value = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
            HardwareApi.Disconnect();
        }

        public void SetMeterI(double? currentRange = null, int? hardwareAverage = null, double? filter = null) {
            Meter.Mode.Value = tlDCVIMeterMode.Current;
            if (currentRange.HasValue) Meter.CurrentRange.Value = currentRange.Value;
            if (hardwareAverage.HasValue) Meter.HardwareAverage.Value = hardwareAverage.Value;
            if (filter.HasValue) Meter.Filter.Value = filter.Value;
        }

        public void SetMeterV(double? voltageRange = null, int? hardwareAverage = null, double? filter = null) {
            Meter.Mode.Value = tlDCVIMeterMode.Voltage;
            if (voltageRange.HasValue) Meter.VoltageRange.Value = voltageRange.Value;
            if (hardwareAverage.HasValue) Meter.HardwareAverage.Value = hardwareAverage.Value;
            if (filter.HasValue) Meter.Filter.Value = filter.Value;
        }

        public void Strobe(int sampleSize = 1, double sampleRate = -1) {
            HardwareApi.Meter.Strobe(sampleSize, sampleRate);
        }

        public PinSite<double> Read(int sampleSize = 1, double sampleRate = -1) {
            return HardwareApi.Meter.Read(tlStrobeOption.NoStrobe, sampleSize, sampleRate, tlDCVIMeterReadingFormat.Average).ToPinSite<double>();
        }

        public PinSite<Samples<double>> ReadSamples(int sampleSize, double sampleRate = -1) {
            IPinListData data = HardwareApi.Meter.Read(tlStrobeOption.NoStrobe, sampleSize, sampleRate, tlDCVIMeterReadingFormat.Array);
            return data.ToPinSiteSamplesDouble();
        }

        public PinSite<double> Measure(int sampleSize = 1, double sampleRate = -1) {
            return HardwareApi.Meter.Read(tlStrobeOption.Strobe, sampleSize, sampleRate, tlDCVIMeterReadingFormat.Average).ToPinSite<double>();
        }

        public PinSite<Samples<double>> MeasureSamples(int sampleSize, double sampleRate = -1) {
            IPinListData data = HardwareApi.Meter.Read(tlStrobeOption.Strobe, sampleSize, sampleRate, tlDCVIMeterReadingFormat.Array);
            return data.ToPinSiteSamplesDouble();
        }

        public void ForceI(double forceCurrent, double? clampVoltage = null, double? currentRange = null, double? voltageRange = null, bool setCurrentMode = true, bool? gate = null) {
            if (setCurrentMode) {
                Gate.Value = tlDCVGate.GateOff; // to avoid glitches while changing the mode
                Mode.Value = tlDCVIMode.Current;
            }
            if (clampVoltage.HasValue && voltageRange.HasValue) SetVoltageAndRange(clampVoltage.Value, voltageRange.Value);
            else if (clampVoltage.HasValue) Voltage.Value = clampVoltage.Value;
            else if (voltageRange.HasValue) VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) SetCurrentAndRange(forceCurrent, currentRange.Value);
            else Current.Value = forceCurrent;
            if (gate.HasValue) Gate.Value = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
        }

        public void SetCurrentAndRange(double current, double currentRange) {
            HardwareApi.SetCurrentAndRange(current, currentRange);
        }

        public void ForceV(double forceVoltage, double? clampCurrent = null, double? voltageRange = null, double? currentRange = null, bool setVoltageMode = true, bool? gate = null) {
            if (setVoltageMode) {
                Gate.Value = tlDCVGate.GateOff; // to avoid glitches while changing the mode
                Mode.Value = tlDCVIMode.Voltage;
            }
            if (clampCurrent.HasValue && currentRange.HasValue) SetCurrentAndRange(clampCurrent.Value, currentRange.Value);
            else if (clampCurrent.HasValue) Current.Value = clampCurrent.Value;
            else if (currentRange.HasValue) CurrentRange.Value = currentRange.Value;
            if (voltageRange.HasValue) SetVoltageAndRange(forceVoltage, voltageRange.Value);
            else Voltage.Value = forceVoltage;
            if (gate.HasValue) Gate.Value = gate.Value ? tlDCVGate.GateOn : tlDCVGate.GateOff;
        }

        public void SetVoltageAndRange(double voltage, double voltageRange) {
            HardwareApi.SetVoltageAndRange(voltage, voltageRange);
        }

        public void ForceISetMeterI(double forceCurrent, double clampVoltage, double measureCurrentRange, double forceCurrentRange, bool? gate = null) {
            SetMeterI(measureCurrentRange);
            ForceI(forceCurrent, clampVoltage, forceCurrentRange, gate: gate);
        }

        public void ForceISetMeterV(double forceCurrent, double clampVoltage, double measureVoltageRange, double forceCurrentRange, bool? gate = null) {
            SetMeterV(measureVoltageRange);
            ForceI(forceCurrent, clampVoltage, forceCurrentRange, gate: gate);
        }

        public void ForceVSetMeterV(double forceVoltage, double clampCurrent, double measureVoltageRange, bool? gate = null) {
            SetMeterV(measureVoltageRange);
            ForceV(forceVoltage, clampCurrent, gate: gate);
        }

        public void ForceVSetMeterI(double forceVoltage, double clampCurrent, double measureCurrentRange, bool? gate = null) {
            SetMeterI(measureCurrentRange);
            ForceV(forceVoltage, clampCurrent, gate: gate);
        }

        public void ForceHiZ() {
            HardwareApi.Gate = tlDCVGate.GateOffHiZ;
            HardwareApi.Disconnect(tlDCVIConnectWhat.Default);
            HardwareApi.Mode = tlDCVIMode.HighImpedance;
            HardwareApi.Connect(tlDCVIConnectWhat.HighSense);
        }

        public IDcviPins[] GetIndividualPins() {
            TheExec.DataManager.DecomposePinList(Name, out string[] individualPins, out _);
            return individualPins.Select(pin => new DcviPins(pin)).ToArray();
        }

        public IDcviPins[] GetPinListItem() {
            string[] pins = Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();
            if (pins.Count() == 1) return [this];
            return pins.Select(pin => new DcviPins(pin)).ToArray();
        }
    }
}

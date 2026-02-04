using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    [Serializable]
    public class DcvsPins : IDcvsPins {

        [NonSerialized]
        private protected DriverDCVSPins _hardwareApi; // IG-XL object
        [NonSerialized]
        private IValuePerSite<bool> _gate;
        [NonSerialized]
        private IValuePerSite<tlDCVSMode> _mode;
        [NonSerialized]
        private IValuePerSite<double> _current;
        [NonSerialized]
        private IValuePerSiteRange<double> _voltage;
        [NonSerialized]
        private IValuePerSiteRange<double> _voltageRange;
        [NonSerialized]
        private IValuePerSiteRange<double> _currentRange;
        [NonSerialized]
        private IValuePerSiteRange<double> _bandWidth;
        [NonSerialized]
        private IDcvsMeter _meter;
        private string _name;

        public DcvsPins(string pinList) : this(pinList, TheHdw.DCVS.Pins(pinList)) { }

        public DcvsPins(string pinList, DriverDCVSPins driverDCVSPins) {
            _name = pinList;
            _hardwareApi = driverDCVSPins;
            _ = Gate; // prevent lazy loading
            _ = Mode;
            _ = VoltageRange;
            _ = CurrentRange;
            _ = Bandwidth;
            _ = Meter;
            _ = Voltage;
        }

        public DriverDCVSPins HardwareApi => _hardwareApi ??= TheHdw.DCVS.Pins(Name);

        public IValuePerSite<bool> Gate => _gate ??= new ValuePerSiteType<bool>(
                    setValue: value => { HardwareApi.Gate = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.Gate = siteValues[site]; });
                    });

        public IValuePerSite<tlDCVSMode> Mode => _mode ??= new ValuePerSiteType<tlDCVSMode>(
                    setValue: value => { HardwareApi.Mode = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.Mode = siteValues[site]; });
                    });

        public IValuePerSiteRange<double> Voltage => _voltage ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.Voltage.Value = value; },
                    setValuePerSite: siteValues => { HardwareApi.Voltage.ValuePerSite = siteValues.ToSiteDouble(); },
                    getMin: () => HardwareApi.Voltage.Min,
                    getMax: () => HardwareApi.Voltage.Max);

        public IValuePerSiteRange<double> VoltageRange => _voltageRange ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.VoltageRange.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.VoltageRange.Value = siteValues[site]; });
                    },
                    getMin: () => HardwareApi.VoltageRange.Min,
                    getMax: () => HardwareApi.VoltageRange.Max);

        public IValuePerSite<double> Current => _current ??= new ValuePerSiteType<double>(
                    setValue: value => {
                        double maxSinkFoldLimit = HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Max;
                        HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Value = value > maxSinkFoldLimit ? maxSinkFoldLimit : value;
                        HardwareApi.CurrentLimit.Source.FoldLimit.Level.Value = value;
                    },
                    setValuePerSite: siteValues => {
                        double maxSinkFoldLimit = HardwareApi.CurrentLimit.Sink.FoldLimit.Level.Max;
                        Site<double> maxSink = siteValues.Select(values => values > maxSinkFoldLimit ? maxSinkFoldLimit : values);
                        HardwareApi.CurrentLimit.Sink.FoldLimit.Level.ValuePerSite = maxSink.ToSiteDouble();
                        HardwareApi.CurrentLimit.Source.FoldLimit.Level.ValuePerSite = siteValues.ToSiteDouble();
                    });

        public IValuePerSiteRange<double> CurrentRange => _currentRange ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.CurrentRange.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.CurrentRange.Value = siteValues[site]; });
                    },
                    getMin: () => HardwareApi.CurrentRange.Min,
                    getMax: () => HardwareApi.CurrentRange.Max);

        public IValuePerSiteRange<double> Bandwidth => _bandWidth ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.BandwidthSetting.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.BandwidthSetting.Value = siteValues[site]; });
                    },
                    getMin: () => HardwareApi.BandwidthSetting.Min,
                    getMax: () => HardwareApi.BandwidthSetting.Max);

        public IDcvsMeter Meter => _meter ??= new MeterImpl(HardwareApi.Meter);

        [Serializable]
        private class MeterImpl : IDcvsMeter {

            [NonSerialized]
            private DriverDCVSMeter _meter;
            [NonSerialized]
            private IValuePerSite<tlDCVSMeterMode> _mode;
            [NonSerialized]
            private IValuePerSiteRange<double> _currentRange;
            [NonSerialized]
            private IValuePerSiteRange<double> _filter;
            [NonSerialized]
            private IValuePerSiteRange<double> _voltageRange;

            public MeterImpl(DriverDCVSMeter driverDCVSMeter) {
                _meter = driverDCVSMeter;
                _ = Mode; // prevent lazy loading
                _ = CurrentRange;
                _ = Filter;
                _ = VoltageRange;
            }

            public IValuePerSiteRange<double> CurrentRange => _currentRange ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { _meter.CurrentRange.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { _meter.CurrentRange.Value = siteValues[site]; });
                    },
                    getMin: () => _meter.CurrentRange.Min,
                    getMax: () => _meter.CurrentRange.Max);

            public IValuePerSiteRange<double> Filter => _filter ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { _meter.Filter.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { _meter.Filter.Value = siteValues[site]; });
                    },
                    getMin: () => _meter.Filter.Min,
                    getMax: () => _meter.Filter.Max);

            public IValuePerSiteRange<double> VoltageRange => _voltageRange ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { _meter.VoltageRange.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { _meter.VoltageRange.Value = siteValues[site]; });
                    },
                    getMin: () => _meter.VoltageRange.Min,
                    getMax: () => _meter.VoltageRange.Max);

            public IValuePerSite<tlDCVSMeterMode> Mode => _mode ??= new ValuePerSiteType<tlDCVSMeterMode>(
                    setValue: value => { _meter.Mode = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { _meter.Mode = siteValues[site]; });
                    });
        }

        public string Name => _name;

        public virtual void Connect(bool? gate = null) {
            HardwareApi.Connect();
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public virtual void Disconnect(bool? gate = null) {
            if (gate.HasValue) Gate.Value = gate.Value;
            HardwareApi.Disconnect();
        }

        public void SetMeterI(double? currentRange = null, double? filter = null, double? outputRange = null) {
            Meter.Mode.Value = tlDCVSMeterMode.Current;
            if (currentRange.HasValue && outputRange.HasValue) SetCurrentRanges(outputRange.Value, currentRange.Value);
            else if (currentRange.HasValue) Meter.CurrentRange.Value = currentRange.Value;
            else if (outputRange.HasValue) CurrentRange.Value = outputRange.Value;
            if (filter.HasValue) Meter.Filter.Value = filter.Value;
        }

        public void SetCurrentRanges(double forceRange, double meterRange) {
            HardwareApi.SetCurrentRanges(forceRange, meterRange);
        }

        public void SetMeterV(double? voltageRange = null, double? filter = null) {
            Meter.Mode.Value = tlDCVSMeterMode.Voltage;
            if (voltageRange.HasValue) Meter.VoltageRange.Value = voltageRange.Value;
            if (filter.HasValue) Meter.Filter.Value = filter.Value;
        }

        public void Strobe(int sampleSize = 1, double sampleRate = -1) {
            HardwareApi.Meter.Strobe(sampleSize, sampleRate);
        }

        public PinSite<double> Read(int sampleSize = 1, double sampleRate = -1) {
            return HardwareApi.Meter.Read(tlStrobeOption.NoStrobe, sampleSize, sampleRate, tlDCVSMeterReadingFormat.Average).ToPinSite<double>();
        }

        public PinSite<Samples<double>> ReadSamples(int sampleSize, double sampleRate = -1) {
            IPinListData data = HardwareApi.Meter.Read(tlStrobeOption.NoStrobe, sampleSize, sampleRate, tlDCVSMeterReadingFormat.Array);
            return data.ToPinSiteSamplesDouble();
        }

        public PinSite<double> Measure(int sampleSize = 1, double sampleRate = -1) {
            return HardwareApi.Meter.Read(tlStrobeOption.Strobe, sampleSize, sampleRate, tlDCVSMeterReadingFormat.Average).ToPinSite<double>();
        }

        public PinSite<Samples<double>> MeasureSamples(int sampleSize, double sampleRate = -1) {
            IPinListData data = HardwareApi.Meter.Read(tlStrobeOption.Strobe, sampleSize, sampleRate, tlDCVSMeterReadingFormat.Array);
            return data.ToPinSiteSamplesDouble();
        }

        public void ForceI(double forceCurrent, double? clampVoltage = null, double? currentRange = null, double? voltageRange = null, bool setCurrentMode = true, bool? gate = null) {
            if (setCurrentMode) Mode.Value = tlDCVSMode.Current;
            if (clampVoltage.HasValue) Voltage.Value = clampVoltage.Value;
            if (voltageRange.HasValue) VoltageRange.Value = voltageRange.Value;
            if (currentRange.HasValue) CurrentRange.Value = currentRange.Value;
            Current.Value = forceCurrent;
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceV(double forceVoltage, double? clampCurrent = null, double? voltageRange = null, double? currentRange = null, bool setVoltageMode = false, bool? gate = null) {
            if (setVoltageMode) Mode.Value = tlDCVSMode.Voltage;
            if (currentRange.HasValue) CurrentRange.Value = currentRange.Value;
            if (clampCurrent.HasValue) Current.Value = clampCurrent.Value;
            if (voltageRange.HasValue) VoltageRange.Value = voltageRange.Value;
            Voltage.Value = forceVoltage;
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceISetMeterI(double forceCurrent, double clampVoltage, double measureCurrentRange, double forceCurrentRange, bool? gate = null) {
            SetMeterI(measureCurrentRange, outputRange: forceCurrentRange);
            ForceI(forceCurrent, clampVoltage, setCurrentMode: true, gate: gate);
        }

        public void ForceISetMeterV(double forceCurrent, double clampVoltage, double forceCurrentRange, bool? gate = null) {
            SetMeterV();
            ForceI(forceCurrent, clampVoltage, forceCurrentRange, setCurrentMode: true, gate: gate);
        }

        public void ForceVSetMeterI(double forceVoltage, double clampCurrent, double measureCurrentRange, bool? gate = null) {
            SetMeterI(measureCurrentRange, outputRange: clampCurrent);
            ForceV(forceVoltage, clampCurrent, setVoltageMode: true, gate: gate);
        }

        public void ForceVSetMeterV(double forceVoltage, double clampCurrent, double measureVoltageRange, bool? gate = null) {
            SetMeterV(measureVoltageRange);
            ForceV(forceVoltage, clampCurrent, measureVoltageRange, clampCurrent, true, gate);
        }

        public void ForceHiZ(double? clampValue = null) {
            Mode.Value = tlDCVSMode.HighImpedance;
            if (clampValue.HasValue) Voltage.Value = clampValue.Value;
        }

        public IDcvsPins[] GetIndividualPins() {
            TheExec.DataManager.DecomposePinList(Name, out string[] individualPins, out _);
            return individualPins.Select(pin => new DcvsPins(pin)).ToArray();
        }

        public IDcvsPins[] GetPinListItem() {
            string[] pins = Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();
            if (pins.Count() == 1) return [this];
            return pins.Select(pin => new DcvsPins(pin)).ToArray();
        }
    }
}

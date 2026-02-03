using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    [Serializable]
    public class PpmuPins : IPpmuPins {

        [NonSerialized]
        private protected tlDriverPPMUPins _hardwareApi; // IG-XL object
        [NonSerialized]
        private IValuePerSite<bool> _gate;
        [NonSerialized]
        private IValuePerSite<double> _voltage;
        [NonSerialized]
        private IValuePerSite<double> _current;
        [NonSerialized]
        private IValuePerSiteRange<double> _clampVLo;
        [NonSerialized]
        private IValuePerSiteRange<double> _clampVHi;
        private string _name;

        internal PpmuPins(string pinList) : this(pinList, TheHdw.PPMU.Pins(pinList)) { }

        internal PpmuPins(string pinList, tlDriverPPMUPins tlDriverPPMUPins) {
            _name = pinList;
            _hardwareApi = tlDriverPPMUPins;
            _ = Gate; // prevent lazy loading
            _ = ClampVHi;
            _ = ClampVLo;
        }

        public tlDriverPPMUPins HardwareApi => _hardwareApi ??= TheHdw.PPMU.Pins(Name);

        public IValuePerSite<bool> Gate => _gate ??= new ValuePerSiteType<bool>(
                    setValue: value => { HardwareApi.Gate = value ? tlOnOff.On : tlOnOff.Off; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.Gate = siteValues[site] ? tlOnOff.On : tlOnOff.Off; });
                    });

        public IValuePerSiteRange<double> ClampVHi => _clampVHi ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.ClampVHi.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.ClampVHi.Value = siteValues[site]; });
                    },
                    getMin: () => HardwareApi.ClampVHi.Min,
                    getMax: () => HardwareApi.ClampVHi.Max);

        public IValuePerSiteRange<double> ClampVLo => _clampVLo ??= new ValuePerSiteRangeType<double>(
                    setValue: value => { HardwareApi.ClampVLo.Value = value; },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.ClampVLo.Value = siteValues[site]; });
                    },
                    getMin: () => HardwareApi.ClampVLo.Min,
                    getMax: () => HardwareApi.ClampVLo.Max);

        public IValuePerSite<double> Voltage => _voltage ??= new ValuePerSiteType<double>(
                    setValue: value => { HardwareApi.ForceV(value); },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.ForceV(siteValues[site]); });
                    });

        public IValuePerSite<double> Current => _current ??= new ValuePerSiteType<double>(
                    setValue: value => { HardwareApi.ForceI(value); },
                    setValuePerSite: siteValues => {
                        ForEachSite(site => { HardwareApi.ForceI(siteValues[site]); });
                    });

        public string Name => _name;

        public virtual void Connect(bool? gate = null) {
            HardwareApi.Connect();
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public virtual void Disconnect(bool? gate = null) {
            if (gate.HasValue) Gate.Value = gate.Value;
            HardwareApi.Disconnect();
        }

        public void Strobe() {
            HardwareApi.Strobe();
        }

        public PinSite<double> Read() {
            return HardwareApi.ReadNoStrobe(tlPPMUReadWhat.Measurements).ToPinSite<double>();
        }

        public PinSite<double> Measure(int sampleSize = 1) {
            return HardwareApi.Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Average).ToPinSite<double>();
        }

        public PinSite<Samples<double>> MeasureSamples(int sampleSize) {
            IPinListData data = HardwareApi.Read(tlPPMUReadWhat.Measurements, sampleSize, tlPPMUReadingFormat.Array);
            return data.ToPinSiteSamplesDouble();
        }

        public void ForceI(double forceCurrent, double? clampVHi = null, double? clampVLo = null, double? currentRange = null, bool? gate = null) {
            if (clampVHi.HasValue) ClampVHi.Value = clampVHi.Value;
            if (clampVLo.HasValue) ClampVLo.Value = clampVLo.Value;
            if (currentRange.HasValue) HardwareApi.ForceI(forceCurrent, currentRange.Value);
            else HardwareApi.ForceI(forceCurrent);
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceV(double forceVoltage, double? measureCurrentRange = null, bool? gate = null) {
            if (measureCurrentRange.HasValue) HardwareApi.ForceV(forceVoltage, measureCurrentRange.Value);
            else HardwareApi.ForceV(forceVoltage);
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceISetMeterV(double forceCurrent, double clampVHi, double clampVLo, double forceCurrentRange, bool? gate = null) =>
            ForceI(forceCurrent, clampVHi, clampVLo, forceCurrentRange, gate);

        public void ForceVSetMeterI(double forceVoltage, double measureCurrentRange, bool? gate = null) {
            HardwareApi.ForceV(forceVoltage, measureCurrentRange);
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceVSetMeterV(double forceVoltage, double measureCurrentRange, bool? gate = null) {
            HardwareApi.ForceVMeasureV(forceVoltage, measureCurrentRange);
            if (gate.HasValue) Gate.Value = gate.Value;
        }

        public void ForceHiZ() {
            HardwareApi.ForceI(0);
            HardwareApi.Gate = tlOnOff.Off;
        }

        public IPpmuPins[] GetIndividualPins() {
            TheExec.DataManager.DecomposePinList(Name, out string[] individualPins, out _);
            return individualPins.Select(pin => new PpmuPins(pin)).ToArray();
        }

        public IPpmuPins[] GetPinListItem() {
            string[] pins = Name.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();
            if (pins.Count() == 1) return [this];
            return pins.Select(pin => new PpmuPins(pin)).ToArray();
        }
    }
}

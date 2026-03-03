namespace Csra {
    public class DcParameters {

        public bool? Gate { get; set; } = null;
        public TLibOutputMode? Mode { get; set; } = null;
        public double? Voltage { get; set; } = null;
        public double? VoltageAlt { get; set; } = null;
        public double? Current { get; set; } = null;
        public double? VoltageRange { get; set; } = null;
        public double? CurrentRange { get; set; } = null;
        public double? ForceBandwidth { get; set; } = null;
        public Measure? MeterMode { get; set; } = null;
        public double? MeterVoltageRange { get; set; } = null;
        public double? MeterCurrentRange { get; set; } = null;
        public double? MeterBandwidth { get; set; } = null;
        public double? SourceFoldLimit { get; set; } = null;
        public double? SinkFoldLimit { get; set; } = null;
        public double? SourceOverloadLimit { get; set; } = null;
        public double? SinkOverloadLimit { get; set; } = null;
        public bool? VoltageAltOutput { get; set; } = null;
        public bool? BleederResistor { get; set; } = null;
        public double? ComplianceBoth { get; set; } = null;
        public double? CompliancePositive { get; set; } = null;
        public double? ComplianceNegative { get; set; } = null;
        public double? ClampHiV { get; set; } = null;
        public double? ClampLoV { get; set; } = null;
        public bool? HighAccuracy { get; set; } = null;
        public double? SettlingTime { get; set; } = null;
        public double? HardwareAverage { get; set; } = null;
    }
}
